#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.Logging;
using R2V2.Core;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.Promotion;
using R2V2.Core.Resource;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.OngoingPda
{
    public class OngoingPdaService
    {
        private readonly EmailMessageSendQueueService _emailMessageSendQueueService;
        private readonly InstitutionService _institutionService;

        private readonly ILog _log;
        private readonly PdaEmailBuildService _pdaEmailBuildService;
        private readonly IQueryable<PdaRule> _pdaRules;
        private readonly PdaRuleService _pdaRuleService;

        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserService _userService;
        int _licenseCountAdded;
        int _totalEmailsSent;

        public OngoingPdaService(WindowsServiceSettings windowsServiceSettings
            , IResourceService resourceService
            , IQueryable<PdaRule> pdaRules
            , PdaRuleService pdaRuleService
            , InstitutionService institutionService
            , PdaEmailBuildService pdaEmailBuildService
            , UserService userService
            , IUnitOfWorkProvider unitOfWorkProvider
            , EmailMessageSendQueueService emailMessageSendQueueService
        )
        {
            if (windowsServiceSettings == null)
            {
                throw new ArgumentNullException("windowsServiceSettings");
            }

            var declaringType = MethodBase.GetCurrentMethod().DeclaringType;
            _log = LogManager.GetLogger(declaringType == null ? "OngoingPdaService" : declaringType.FullName);
            _resourceService = resourceService;
            _pdaRules = pdaRules;
            _pdaRuleService = pdaRuleService;
            _institutionService = institutionService;
            _pdaEmailBuildService = pdaEmailBuildService;
            _userService = userService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _emailMessageSendQueueService = emailMessageSendQueueService;
        }

        public bool ProcessOngoingPdaEvent(OngoingPdaEventMessage ongoingPdaEventMessage)
        {
            try
            {
                _licenseCountAdded = 0;
                _totalEmailsSent = 0;

                _log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                _log.InfoFormat(">>>>>>>>>>>> ProcessOngoingPdaEvent() - START - {0}",
                    ongoingPdaEventMessage.ToDebugString());
                _log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                var isbns = ongoingPdaEventMessage.Isbns.ToArray();
                var resources = _resourceService.GetResourcesForOngoingPda(isbns);
                
                _log.DebugFormat("resources.Count: {0}", resources.Count);

                var ongoingPdaEvent = InsertOngoingPdaEvent(ongoingPdaEventMessage, resources);

                var pdaRules = _pdaRules.Where(x => x.ExecuteForFuture).OrderBy(x => x.InstitutionId).ToList();

                if (ongoingPdaEventMessage.EventType == OngoingPdaEventType.DoodyUpdate)
                {
                    var dctPdaRules = pdaRules.Where(x => x.Collections.Any(y => y.CollectionId == 5))
                        .OrderBy(x => x.InstitutionId).ToList();
                    var dctResources = resources.Where(x => x.Collections.Any(y => y.Id == 5)).ToList();

                    var dctEssentialPdaRules = pdaRules.Where(x => x.Collections.Any(y => y.CollectionId == 6))
                        .OrderBy(x => x.InstitutionId).ToList();
                    var dctEssentialResources = resources.Where(x => x.Collections.Any(y => y.Id == 6)).ToList();

                    ProcessRules(dctPdaRules, dctResources, ongoingPdaEventMessage.Timestamp);
                    ProcessRules(dctEssentialPdaRules, dctEssentialResources, ongoingPdaEventMessage.Timestamp);
                }
                else
                {
                    ProcessRules(pdaRules, resources, ongoingPdaEventMessage.Timestamp);
                }

                ongoingPdaEvent.Processed = true;
                ongoingPdaEvent.LicenseCountAdded = _licenseCountAdded;
                UpdateOngoingPdaEvent(ongoingPdaEvent);

                _log.Info($"licenseCountAdded: {_licenseCountAdded}");
                _log.Info($"totalEmailsSent: {_totalEmailsSent}");
                _log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                _log.Info(
                    $"<<<<<<<<<<<< ProcessOngoingPdaEvent() - COMPLETE - {ongoingPdaEventMessage.ToDebugString()}");
                _log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }
        }

        private void ProcessRules(IList<PdaRule> rules, IList<IResource> resources, DateTime messageTimeStamp)
        {
            IUser user = new User { Id = -1, FirstName = "OngoingPDA" };
            var institutionIds = rules.Select(p => p.InstitutionId).Distinct().ToList();

            foreach (var institutionId in institutionIds)
            {
                _log.Debug($">>> >>> >>> >>> >>> >>> >>> >>> >>> >>> >>> >>> - institutionId: {institutionId}");
                var institution = _institutionService.GetInstitutionForAdminNotCached(institutionId);
                if (institution.AccountStatus != InstitutionAccountStatus.Active)
                {
                    _log.Debug($"Institution: {institution.Id} is not an Active Institution");
                    continue;
                }

                var adminInstitution = new AdminInstitution(institution);
                _log.Debug(adminInstitution.ToDebugString());

                var institutionPdaRules = rules.Where(p => p.InstitutionId == institutionId).ToList();

                var rulesWithNewLicenses = new Dictionary<PdaRule, IList<InstitutionResourceLicense>>();

                var numberOfPdaLicensesAdded = 0;
                foreach (var pdaRule in institutionPdaRules)
                {
                    _log.Debug($">>>++++++++++++++++++++++++++++++++++++++++>>> - pdaRule.Id: {pdaRule.Id}");
                    _log.Debug(pdaRule.ToDebugString());

                    var pdaRuleResult = _pdaRuleService.RunRuleOnResources(resources, pdaRule, adminInstitution, user);
                    numberOfPdaLicensesAdded += pdaRuleResult.PdaLicensesAdded.Count();
                    var pdaLicensesAdded = pdaRuleResult.PdaLicensesAdded.ToList();
                    if (pdaLicensesAdded.Any())
                    {
                        var logMsg = new StringBuilder();
                        logMsg.Append(
                            $"pdaRule.Id: {pdaRule.Id}, pdaRule.InstitutionId: {pdaRule.InstitutionId}, batchId: {pdaLicensesAdded.First().BatchId} resource Ids: [");
                        logMsg.Append(string.Join(",", pdaLicensesAdded.Select(x => x.ResourceId)));
                        logMsg.Append("]");
                        _log.Debug(logMsg);
                        rulesWithNewLicenses.Add(pdaRule, pdaRuleResult.PdaLicensesAdded.ToList());
                    }

                    _log.Debug($"<<<----------------------------------------<<< - pdaRule.Id: {pdaRule.Id}");
                }

                if (numberOfPdaLicensesAdded > 0)
                {
                    _licenseCountAdded += numberOfPdaLicensesAdded;
                    SendEmailsForInstitution(adminInstitution, rulesWithNewLicenses, resources, messageTimeStamp);
                }
            }
        }

        private void SendEmailsForInstitution(AdminInstitution adminInstitution,
            Dictionary<PdaRule, IList<InstitutionResourceLicense>> rulesWithNewLicenses,
            IList<IResource> resources, DateTime messageTimeStamp)
        {
            var numberOfEmailsSent = 0;

            var adminUsers = _userService.GetAdminUsers(adminInstitution.Id);
            foreach (var adminUser in adminUsers)
            {
                var emailMessage = _pdaEmailBuildService.BuildOngoingPdaResourceAddedEmail(adminUser,
                    rulesWithNewLicenses, adminInstitution, messageTimeStamp, resources);

                var messageSentSuccessfully =
                    _emailMessageSendQueueService.WriteEmailMessageToMessageQueue(emailMessage);
                if (messageSentSuccessfully)
                {
                    numberOfEmailsSent++;
                }
            }

            _log.DebugFormat("numberOfEmailsSent: {0}", numberOfEmailsSent);
            _log.DebugFormat("<<< <<< <<< <<< <<< <<< <<< <<< <<< <<< <<< <<< - institutionId: {0}",
                adminInstitution.Id);
            _totalEmailsSent += numberOfEmailsSent;
        }

        private OngoingPdaEvent InsertOngoingPdaEvent(OngoingPdaEventMessage ongoingPdaEventMessage,
            IList<IResource> resources)
        {
            var ongoingPdaEvent = ongoingPdaEventMessage.ToOngoingPdaEvent(resources);
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Save(ongoingPdaEvent);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                // we are going to log this exception as an error so that we are alerted about the issue
                // but we are going to swallow the exception so that an error does not stop the process.
                _log.Error(ex.Message, ex);
            }

            return ongoingPdaEvent;
        }

        private void UpdateOngoingPdaEvent(OngoingPdaEvent ongoingPdaEvent)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.SaveOrUpdate(ongoingPdaEvent);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                // we are going to log this exception as an error so that we are alerted about the issue
                // but we are going to swallow the exception so that an error does not stop the process.
                _log.Error(ex.Message, ex);
            }
        }
    }
}