#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EasyNetQ;
using EasyNetQ.Topology;
using NHibernate.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Promotion;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaRuleService
    {
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly ILog<PdaRuleService> _log;
        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly IResourceService _resourceService;
        private readonly IQueryable<PdaRule> _rules;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PdaRuleService(
            ILog<PdaRuleService> log
            , IQueryable<PdaRule> rules
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , IResourceService resourceService
            , IMessageQueueSettings messageQueueSettings
            , ICollectionManagementSettings collectionManagementSettings
            , IMessageQueueService messageQueueService
        )
        {
            _log = log;
            _rules = rules;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutionResourceLicenses = institutionResourceLicenses;
            _resourceService = resourceService;
            _messageQueueSettings = messageQueueSettings;
            _collectionManagementSettings = collectionManagementSettings;
            _messageQueueService = messageQueueService;
        }

        public List<PdaRule> GetInstitutionPdaRules(IAdminInstitution adminInstitution)
        {
            var rules =
                _rules.Where(x => x.InstitutionId == adminInstitution.Id)
                    .OrderByDescending(x => x.LastUpdated.GetValueOrDefault(DateTime.Now.AddMonths(-2)))
                    .ThenByDescending(x => x.CreationDate)
                    .ToList();

            PopulateResourceCountsForRules(rules, adminInstitution);

            return rules.ToList();
        }

        public PdaRule GetInstitutionRule(int institutionId, int ruleId)
        {
            _rules.FetchMany(x => x.PracticeAreas).ToFuture();
            _rules.FetchMany(x => x.Collections).ToFuture();
            _rules.FetchMany(x => x.Specialties).ToFuture();

            return _rules.FirstOrDefault(x => x.InstitutionId == institutionId && x.Id == ruleId);
        }

        public bool DoesRuleNameExist(int ruleId, int institutionId, string ruleName)
        {
            return ruleId > 0
                ? _rules.Any(x => x.InstitutionId == institutionId && x.Id != ruleId && x.Name == ruleName)
                : _rules.Any(x => x.InstitutionId == institutionId && x.Name == ruleName);
        }

        public int SaveInstitutionRule(PdaRule rule)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.SaveOrUpdate(rule);
                        uow.Commit();
                        transaction.Commit();
                        return rule.Id;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return 0;
        }

        public bool DeleteInstitutionRule(PdaRule rule)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.Delete(rule);
                        uow.Commit();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }

        private IEnumerable<IResource> FilterResourcesForRule(PdaRule rule, IList<IResource> resources,
            IList<InstitutionResourceLicense> licenses, IAdminInstitution adminInstitution)
        {
            var resourcesToReturn = FilterResourcesForRule(rule, resources, adminInstitution);

            var pdaLicenseResourceIds = licenses
                .Where(x => x.LicenseTypeId == (int)LicenseType.Pda && !(x.PdaRuleId > 0)).Select(y => y.ResourceId);
            var purchasedLicenseResourceIds = licenses.Where(x => x.LicenseTypeId == (int)LicenseType.Purchased)
                .Select(y => y.ResourceId);

            if (rule.IncludeNewEditionFirm || rule.IncludeNewEditionPda)
            {
                List<IResource> allLicenseResources;
                if (rule.IncludeNewEditionFirm && rule.IncludeNewEditionPda)
                {
                    _log.Debug("IncludeNewEditionFirm & IncludeNewEditionPda");
                    allLicenseResources = resources.Where(r =>
                        pdaLicenseResourceIds.Contains(r.Id) || purchasedLicenseResourceIds.Contains(r.Id)).ToList();
                }
                else if (rule.IncludeNewEditionFirm)
                {
                    _log.Debug("IncludeNewEditionFirm");
                    allLicenseResources = resources.Where(r => purchasedLicenseResourceIds.Contains(r.Id)).ToList();
                }
                else
                {
                    _log.Debug("Other");
                    allLicenseResources = resources.Where(r => pdaLicenseResourceIds.Contains(r.Id)).ToList();
                }

                var latestEditionIds = allLicenseResources.Where(x => x.LatestEditResourceId != null)
                    .Select(x => x.LatestEditResourceId).Distinct().ToList();
                var filteredResources = resourcesToReturn.Where(x => latestEditionIds.Contains(x.Id)).ToList();
                return filteredResources.Where(r => !licenses.Select(x => x.ResourceId).Contains(r.Id)).ToList();
            }

            if (licenses.Any())
            {
                var ownedLicenseIds = adminInstitution.Licenses.Where(z =>
                        z.LicenseType == LicenseType.Pda &&
                        (z.PdaViewCount == z.PdaMaxViews ||
                         z.PdaViewCount < z.PdaMaxViews && z.PdaDeletedDate == null ||
                         z.PdaRuleId > 0 && z.PdaDeletedDate == null)
                        ||
                        z.LicenseType == LicenseType.Purchased
                    )
                    .Select(y => y.ResourceId);

                resourcesToReturn = resourcesToReturn.Where(x => !ownedLicenseIds.Contains(x.Id)).ToList();
            }

            return resourcesToReturn;
        }

        /// <summary>
        ///     This method does not take licenses into account. Only use when counting all titles rule could account for
        ///     OR use a base filter then apply licensing to the resources.
        /// </summary>
        private IEnumerable<IResource> FilterResourcesForRule(PdaRule rule, IEnumerable<IResource> resources,
            IAdminInstitution adminInstitution)
        {
            var practiceAreaIds = rule.PracticeAreas != null
                ? rule.PracticeAreas.Select(x => x.PracticeAreaId).ToArray()
                : null;
            var specialtyIds = rule.Specialties != null ? rule.Specialties.Select(x => x.SpecialtyId).ToArray() : null;
            var collectionIds =
                rule.Collections != null ? rule.Collections.Select(x => x.CollectionId).ToArray() : null;

            var resourcesToReturn = resources
                .PracticeAreaFilterBy(practiceAreaIds)
                .SpecialtyFilterBy(specialtyIds)
                .CollectionFilterBy(collectionIds)
                .MaxPriceFilterBy(rule.MaxPrice.GetValueOrDefault(0), adminInstitution.Discount)
                .Where(x => x.StatusId == (int)ResourceStatus.Active && !x.NotSaleable && !x.IsFreeResource)
                .ToList();

            _log.Info(rule.ToDebugString());
            return resourcesToReturn;
        }

        private void PopulateResourceCountsForRules(List<PdaRule> rules, IAdminInstitution adminInstitution)
        {
            if (rules != null && rules.Any())
            {
                var resources = _resourceService.GetAllResources().ToList();


                var licenses = _institutionResourceLicenses.Where(x => x.InstitutionId == adminInstitution.Id).ToList();

                foreach (var rule in rules)
                {
                    PopulateResourceCountsForRule(rule, resources, licenses, adminInstitution);
                }
            }
        }

        public void PopulateResourceCountsForVerify(PdaRule rule, IAdminInstitution adminInstitution)
        {
            var licenses = _institutionResourceLicenses.Where(x => x.InstitutionId == adminInstitution.Id).ToList();
            var resources = _resourceService.GetAllResources().ToList();

            PopulateResourceCountsForRule(rule, resources, licenses, adminInstitution);
        }

        private void PopulateResourceCountsForRule(PdaRule rule, List<IResource> resources,
            List<InstitutionResourceLicense> licenses, IAdminInstitution adminInstitution)
        {
            var resourcesAddedFromRule = licenses.Count(x => x.PdaRuleId == rule.Id);
            //var resourcesAddedFromRule = licenses.Count(x => x.PdaRuleId == rule.Id && x.PdaDeletedDate == null);

            rule.ResourcesAdded = resourcesAddedFromRule;

            var resourcesExcludeLicenses = FilterResourcesForRule(rule, resources, licenses, adminInstitution);

            var resourcesToAdd = resourcesExcludeLicenses.Count();
            rule.ResourcesToAdd = resourcesToAdd;
        }

        public IEnumerable<IResource> GetBackFillResources(IAdminInstitution adminInstitution, int ruleId)
        {
            var rule = GetInstitutionRule(adminInstitution.Id, ruleId);
            return GetBackFillResources(adminInstitution, rule);
        }

        public IEnumerable<IResource> GetBackFillResources(IAdminInstitution adminInstitution, PdaRule rule)
        {
            return GetBackFillResourcesBase(adminInstitution, rule);
        }


        private IEnumerable<IResource> GetBackFillResourcesBase(IAdminInstitution adminInstitution, PdaRule rule)
        {
            var resources = _resourceService.GetAllResources().ToList();
            var licenses = _institutionResourceLicenses.Where(x => x.InstitutionId == rule.InstitutionId).ToList();

            return FilterResourcesForRule(rule, resources, licenses, adminInstitution);
        }

        public int RunRuleNow(IAdminInstitution adminInstitution, IUser user, int ruleId, int pdaMaxViews)
        {
            var resources = GetBackFillResources(adminInstitution, ruleId);

            return AddUpdateInstitutionResourceLicenses(adminInstitution, user, ruleId, pdaMaxViews, resources,
                Guid.NewGuid());
        }

        public PdaRuleResult RunRuleOnResources(IList<IResource> resources, PdaRule rule,
            IAdminInstitution adminInstitution, IUser user)
        {
            var pdaRuleResult = new PdaRuleResult(resources, rule, adminInstitution, user,
                _collectionManagementSettings.PatronDriveAcquisitionMaxViews);

            var licenses = _institutionResourceLicenses.Where(x => x.InstitutionId == rule.InstitutionId).ToList();
            IList<IResource> filteredResources =
                FilterResourcesForRule(rule, resources, licenses, adminInstitution).ToList();
            _log.InfoFormat("filteredResources.Count: {0}", filteredResources.Count);

            var ids = new StringBuilder();
            foreach (var filteredResource in filteredResources)
            {
                ids.AppendFormat("{0}{1}", ids.Length > 0 ? ", " : "", filteredResource.Id);
            }

            if (filteredResources.Any())
            {
                _log.DebugFormat("Add PDA license for institution id: {0}, rule id: {1}, resource Ids: {2}",
                    adminInstitution.Id, rule.Id, ids);
                var numberOfPdaLicensesAdded = AddUpdateInstitutionResourceLicenses(adminInstitution, user, rule.Id,
                    pdaRuleResult.PdaMaxViews, filteredResources, pdaRuleResult.BatchId);

                var institutionResourceLicenses = GetInstitutionResourceLicenseByBatchId(pdaRuleResult.BatchId);
                _log.InfoFormat("Added {0} tInstitutionResourceLicense records", institutionResourceLicenses.Count);
                foreach (var institutionResourceLicense in institutionResourceLicenses)
                {
                    _log.Debug(institutionResourceLicense.ToDebugString());
                }

                pdaRuleResult.AddPdaLicenses(institutionResourceLicenses);
                if (numberOfPdaLicensesAdded != pdaRuleResult.PdaLicensesAdded.Count())
                {
                    var msg = new StringBuilder();
                    msg.AppendLine("numberOfPdaLicensesAdded != pdaRuleResult.PdaLicensesAdded.Count()");
                    msg.AppendFormat("{0} != {1}", numberOfPdaLicensesAdded, pdaRuleResult.PdaLicensesAdded.Count())
                        .AppendLine().AppendLine();
                    msg.Append("filteredResources.Ids: [");
                    foreach (var filteredResource in filteredResources)
                    {
                        msg.AppendFormat("{0},", filteredResource.Id);
                    }

                    msg.AppendLine("]").Append("pdaRuleResult.PdaAddedResources.Ids: [");
                    foreach (var institutionResourceLicense in pdaRuleResult.PdaLicensesAdded)
                    {
                        msg.AppendFormat("{0},", institutionResourceLicense.Id);
                    }

                    msg.AppendLine("]");
                    msg.AppendLine(rule.ToDebugString());
                    _log.Error(msg);
                }

                _log.DebugFormat(pdaRuleResult.ToDebugString());
            }
            else
            {
                _log.DebugFormat(
                    "Rule does not apply to resource, rule id: {0}, institution id: {1}, resource ids: {2}", rule.Id,
                    adminInstitution.Id, ids);
            }

            return pdaRuleResult;
        }

        public int AddUpdateInstitutionResourceLicenses(IAdminInstitution adminInstitution, IUser user, int ruleId,
            int pdaMaxViews, IEnumerable<IResource> resources, Guid batchId)
        {
            var resourceIds = resources.Select(x => x.Id).ToArray();

            if (resourceIds.Length == 0)
            {
                _log.Debug("no resources ids to add");
                return 0;
            }

            var inClauseResourceIds = string.Join(",", resourceIds);

            using (var uow = _unitOfWorkProvider.Start())
            {
                //Update Licenses for PDAs that have been manually deleted
                var sql = new StringBuilder()
                        .Append("UPDATE tInstitutionResourceLicense ")
                        .Append(
                            $"SET dtPdaDeletedDate = null, vchPdaDeletedById = null, tiRecordStatus = 1, dtLastUpdate = getdate(), vchUpdaterId = 'user Id: {user.Id} [{user.FirstName}]' ")
                        .Append($", dtPdaRuleDateAdded = getdate(), iPdaRuleId = {ruleId}, guidBatchId = '{batchId}' ")
                        //TODO: 10/18/2018 SquishList #1085 - PDA Resources added previously by rule than deleted, not able to be added via wizard again.
                        //.AppendFormat("WHERE iInstitutionId = {1} and iPdaRuleId is null and iResourceid in ({0}) ", inClauseResourceIds, adminInstitution.Id);
                        .Append(
                            $"WHERE iInstitutionId = {adminInstitution.Id} and (iPdaRuleId is null or (iPdaRuleId is not null and dtPdaDeletedDate is not null))")
                        .Append($" and iResourceid in ({inClauseResourceIds}) ")
                    ;
                var updateQuery = uow.Session.CreateSQLQuery(sql.ToString());
                var updatedRows = updateQuery.ExecuteUpdate();

                //Inserting New Licenses
                sql = new StringBuilder()
                    .Append("INSERT INTO tInstitutionResourceLicense ")
                    .Append(
                        "(iInstitutionId, iResourceId, iLicenseCount, tiLicenseTypeId, tiLicenseOriginalSourceId, dtPdaAddedDate ")
                    .Append(
                        ", iPdaMaxViews, dtCreationDate, vchCreatorId, tiRecordStatus, dtPdaRuleDateAdded, iPdaRuleId, guidBatchId) ")
                    .AppendFormat("SELECT {0}, r.iResourceId, 0, {1}, {2}, getdate() ", adminInstitution.Id,
                        (int)LicenseType.Pda, (int)LicenseOriginalSource.Pda)
                    .AppendFormat(" , {0}, getdate(), 'user Id: {1} [{2}]', 1 ", pdaMaxViews, user.Id, user.FirstName)
                    .AppendFormat(", getdate(), {0}, '{1}' ", ruleId, batchId)
                    .Append("FROM tResource r ")
                    .AppendFormat(
                        "left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and irl.iInstitutionId = {0} ",
                        adminInstitution.Id)
                    .AppendFormat("where irl.iInstitutionResourceLicenseId is null and r.iResourceId in ({0}) ",
                        inClauseResourceIds);

                var insertQuery = uow.Session.CreateSQLQuery(sql.ToString());
                var insertedRows = insertQuery.ExecuteUpdate();

                _log.InfoFormat("updatedRows: {0}, insertedRows: {1}", updatedRows, insertedRows);
                if (updatedRows + insertedRows == 0)
                {
                    _log.Warn(
                        "Zero tInstitutionResourceLicense inserted or updated! - This should NOT occur on the web site when a single rule is executed at time. It is OK for this to occur in the service when running multiple rules since a previous rule could have already added the PDA license.");
                    return 0;
                }

                sql = new StringBuilder()
                    .AppendFormat(
                        "insert into tInstitutionResourceAudit(iInstitutionId, iResourceId, iUserId, iInstitutionResourceAuditTypeId, iLicenseCount")
                    .AppendFormat(
                        "         , decSingleLicensePrice, vchCreatorId, dtCreationDate, vchEventDescription, bLegacy) ")
                    .AppendFormat("    select {0}, irl.iResourceId, {1}, {2}, 0 ", adminInstitution.Id, user.Id,
                        (int)InstitutionResourceAuditType.PdaResourceAddedViaProfile)
                    .AppendFormat("         , 0.00, 'user Id: {0} [{1}]', getdate(), 'Pda Profile Rule #{2}', 0 ",
                        user.Id, user.FirstName, ruleId)
                    .AppendFormat("    from   tInstitutionResourceLicense irl ")
                    .AppendFormat("    where  irl.guidBatchId = '{0}' ", batchId);

                var auditInsert = uow.Session.CreateSQLQuery(sql.ToString());
                var auditRows = auditInsert.ExecuteUpdate();

                _log.InfoFormat("auditRows: {0}", auditRows);
                if (auditRows != updatedRows + insertedRows)
                {
                    _log.ErrorFormat(
                        "Updated Rows({0}) + Inserted Rows({1}) does not equal the Audit Rows({2}). THIS SHOULD NEVER HAPPEN.",
                        updatedRows, insertedRows, auditRows);
                }

                return updatedRows + insertedRows;
            }
        }

        public Guid SendOngoingPdaEventToMessageQueue(string isbn, OngoingPdaEventType ongoingPdaEventType)
        {
            var ongoingPdaEventMessage = new OngoingPdaEventMessage(ongoingPdaEventType);
            ongoingPdaEventMessage.AddIsbn(isbn);

            ongoingPdaEventMessage.Timestamp = DateTime.Now;

            var success = WriteOngoingPdaEventMessageToMessageQueue(ongoingPdaEventMessage);
            if (!success)
            {
                _log.ErrorFormat("Error sending R2 order to the message queue, {0}",
                    ongoingPdaEventMessage.ToDebugString());
            }

            return ongoingPdaEventMessage.Id;
        }

        public Guid SendOngoingPdaEventToMessageQueue(IEnumerable<string> isbns,
            OngoingPdaEventType ongoingPdaEventType)
        {
            //_authenticationContext.isAuthenicated Needs to be Populated
            var ongoingPdaEventMessage = new OngoingPdaEventMessage(ongoingPdaEventType);
            ongoingPdaEventMessage.AddIsbns(isbns);

            ongoingPdaEventMessage.Timestamp = DateTime.Now;

            var success = WriteOngoingPdaEventMessageToMessageQueue(ongoingPdaEventMessage);
            if (!success)
            {
                _log.ErrorFormat("Error sending R2 order to the message queue, {0}",
                    ongoingPdaEventMessage.ToDebugString());
            }

            return ongoingPdaEventMessage.Id;
        }

        public bool WriteOngoingPdaEventMessageToMessageQueue(OngoingPdaEventMessage ongoingPdaEventMessage,
            bool sendToFailedQueue = false)
        {
            var queueName = _messageQueueSettings.OngoingPdaQueueName;
            var routeKey = _messageQueueSettings.OngoingPdaRouteKey;
            var exchangeName = _messageQueueSettings.OngoingPdaExchangeName;
            if (sendToFailedQueue)
            {
                queueName = _messageQueueService.GetFailedQueueName(queueName);
                routeKey = "Failed";
            }

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                using (var advancedBus =
                       RabbitHutch.CreateBus(_messageQueueSettings.ProductionConnectionString).Advanced)
                {
                    var queue = advancedBus.QueueDeclare(queueName);
                    var exchange = advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                    advancedBus.Bind(exchange, queue, routeKey);
                    advancedBus.Publish(exchange, routeKey, true,
                        new Message<OngoingPdaEventMessage>(ongoingPdaEventMessage));
                }

                stopwatch.Stop();

                _log.DebugFormat("Message sent to {0} in {1} ms\r\n{2}", queueName, stopwatch.ElapsedMilliseconds,
                    ongoingPdaEventMessage.ToDebugString());
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }
        }

        private List<InstitutionResourceLicense> GetInstitutionResourceLicenseByBatchId(Guid batchId)
        {
            var licenses = _institutionResourceLicenses.Where(x => x.BatchId == batchId).ToList();
            return licenses;
        }
    }
}