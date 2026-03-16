#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Email;

#endregion

namespace R2Utilities.Tasks.EmailTasks.WeeklyEmails
{
    public class PdaHistoryTask : EmailTaskBase
    {
        private readonly EmailTaskService _emailTaskService;
        readonly StringBuilder _failureEmailAddress = new StringBuilder();
        private readonly PdaEmailBuildService _pdaEmailBuildService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private int _failCount;

        private int _successCount;

        public PdaHistoryTask(
            EmailTaskService emailTaskService
            , PdaEmailBuildService pdaEmailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
        )
            : base("PdaHistoryTask", "-PdaHistoryTask", "48", TaskGroup.CustomerEmails,
                "Sends PDA History Excel Reports.", true)
        {
            _emailTaskService = emailTaskService;
            _pdaEmailBuildService = pdaEmailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "PDA History Task";
            var step = new TaskResultStep { Name = "PdaHistoryTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _pdaEmailBuildService.InitEmailTemplatesForPdaHistory();

                _successCount = 0;
                _failCount = 0;

                var userIdsAndInstitutionIds = _emailTaskService.GetPdaHistoryUserIds();

                var institutionIds = userIdsAndInstitutionIds.Select(x => x.InstitutionId).Distinct();

                var pdaHistoryReports = new List<PdaHistoryReport>();
                foreach (var institutionId in institutionIds)
                {
                    var pdaResources = _emailTaskService.GetPdaHistoryResources(institutionId).ToList();
                    var report = _emailTaskService.GetPdaHistoryReport(institutionId, pdaResources);
                    if (report != null)
                    {
                        pdaHistoryReports.Add(report);
                    }
                }


                foreach (var userIdAndInstitutionId in userIdsAndInstitutionIds)
                {
                    var report =
                        pdaHistoryReports.FirstOrDefault(x => x.InstitutionId == userIdAndInstitutionId.InstitutionId);
                    if (report != null)
                    {
                        ProcessUser(userIdAndInstitutionId.UserId, report);
                    }
                }

                step.CompletedSuccessfully = true;

                var results = new StringBuilder()
                    .AppendFormat("<div>{0} PDA history reports sent</div>", _successCount).AppendLine()
                    .AppendFormat("<div>{0} PDA history reports failed</div>", _failCount).AppendLine()
                    .AppendFormat("<div>Emails that failed: {0}</div>", _failureEmailAddress).AppendLine()
                    .ToString();
                step.Results = results;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        public void ProcessUser(int userId, PdaHistoryReport pdaHistoryReport)
        {
            var user = _emailTaskService.GetUser(userId);

            var territoryusers = new List<User>();
            if (user.Institution != null)
            {
                if (user.Institution.Territory != null)
                {
                    territoryusers = _emailTaskService.GetTerritoryOwners(user.Institution.Territory.Id);
                }
            }

            var userArray = territoryusers.Any() ? territoryusers.Select(x => x.Email).ToArray() : null;

            var emailMessage = _pdaEmailBuildService.BuildPdaHistoryEmail(pdaHistoryReport, user, userArray);

            if (emailMessage != null)
            {
                var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                    _r2UtilitiesSettings.DefaultFromAddress,
                    _r2UtilitiesSettings.DefaultFromAddressName);

                if (success)
                {
                    _successCount++;
                    return;
                }
            }

            _failCount++;
            _failureEmailAddress.AppendFormat("FailToSend: [InstitutionId:{0} | UserId:{1} | UserEmail: {2}] <br/>",
                user.Institution != null ? user.Institution.Id : 0, user.Id, user.Email);
        }
    }
}