#region

using System;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Email;

#endregion

namespace R2Utilities.Tasks.EmailTasks.WeeklyEmails
{
    public class ArchivedResourceTask : EmailTaskBase
    {
        private readonly ArchivedResourceEmailBuildService _archivedResourceEmailBuildService;
        private readonly EmailTaskService _emailTaskService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private int _failCount;


        private int _successCount;

        public ArchivedResourceTask(
            EmailTaskService emailTaskService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , ArchivedResourceEmailBuildService archivedResourceEmailBuildService
        )
            : base("ArchivedResourceTask", "-ArchivedResourceTask", "50", TaskGroup.CustomerEmails,
                "Archives resources and then send out an customer emails", true)
        {
            _emailTaskService = emailTaskService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _archivedResourceEmailBuildService = archivedResourceEmailBuildService;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "Archived Resource Email Task";
            var step = new TaskResultStep { Name = "ArchivedResourceTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _successCount = 0;
                _failCount = 0;

                //_archivedResourceEmailBuildService.SetTemplates(ResourceTemplate.Archived);

                var userIds = _emailTaskService.GetArchivedEmailUserIds();

                foreach (var userId in userIds)
                {
                    var user = _emailTaskService.GetUser(userId);
                    var archivedResourceIds = _emailTaskService.GetArchivedEmailResourceIds(userId).ToList();

                    var archivedResources = _emailTaskService.GetResources(archivedResourceIds);

                    if (archivedResources == null || !archivedResources.Any()) continue;

                    var emailMessage =
                        _archivedResourceEmailBuildService.BuildArchivedResourceEmail(user, archivedResources);
                    if (emailMessage != null)
                    {
                        var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                            _r2UtilitiesSettings.DefaultFromAddress,
                            _r2UtilitiesSettings.DefaultFromAddressName);

                        if (success)
                        {
                            _successCount++;
                            continue;
                        }
                    }

                    _failCount++;
                }

                if (!_r2UtilitiesSettings.EmailTestMode &&
                    _archivedResourceEmailBuildService.ProcessedArchivedResources != null)
                {
                    _emailTaskService.UpdateArchivedResourceEmailResources(_archivedResourceEmailBuildService
                        .ProcessedArchivedResources);
                }

                step.CompletedSuccessfully = true;

                var results = new StringBuilder()
                    .AppendFormat("<div>{0} Archived Resource Emails sent</div>", _successCount).AppendLine()
                    .AppendFormat("<div>{0} Archived Resource Emails failed</div>", _failCount).AppendLine()
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
    }
}