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
    public class DoodyUpdateResourceTask : EmailTaskBase
    {
        private readonly DctUpdateResourceEmailBuildService _dctUpdateResourceEmailBuildService;
        private readonly EmailTaskService _emailTaskService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public DoodyUpdateResourceTask(
            EmailTaskService emailTaskService
            , DctUpdateResourceEmailBuildService dctUpdateResourceEmailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
        )
            : base("DoodyUpdateResourceTask", "-DoodyUpdateResourceEmailTask", "52", TaskGroup.CustomerEmails,
                "Sends Doody update email to ???", true)
        {
            _emailTaskService = emailTaskService;
            _dctUpdateResourceEmailBuildService = dctUpdateResourceEmailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;
            _dctUpdateResourceEmailBuildService.SetTemplates();

            TaskResult.Information = "Doody Update Email Resource Task";
            var step = new TaskResultStep { Name = "DoodyUpdateResourceTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var medicalEmailCount = 0;
            var nursingEmailCount = 0;
            var alliedHealthEmailCount = 0;

            var failCount = 0;
            try
            {
                var medicineResources = _emailTaskService.GetDctUpdateResourcesForEmail((int)PracticeArea.Medicine);
                var nursingResources = _emailTaskService.GetDctUpdateResourcesForEmail((int)PracticeArea.Nursing);
                var alliedHealthResources =
                    _emailTaskService.GetDctUpdateResourcesForEmail((int)PracticeArea.AlliedHealth);

                if (medicineResources.Any())
                {
                    var users = _emailTaskService.GetUsersForDctUpdateEmails((int)PracticeArea.Medicine);
                    foreach (var user in users)
                    {
                        var emailMessage =
                            _dctUpdateResourceEmailBuildService.BuildDctUpdateEmail(medicineResources, user,
                                "Medicine");
                        if (emailMessage != null)
                        {
                            var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                _r2UtilitiesSettings.DefaultFromAddress, _r2UtilitiesSettings.DefaultFromAddressName);

                            if (success)
                            {
                                medicalEmailCount++;
                                break;
                            }

                            failCount++;
                        }
                    }
                }

                if (nursingResources.Any())
                {
                    var users = _emailTaskService.GetUsersForDctUpdateEmails((int)PracticeArea.Nursing);
                    foreach (var user in users)
                    {
                        var emailMessage =
                            _dctUpdateResourceEmailBuildService.BuildDctUpdateEmail(nursingResources, user, "Nursing");
                        if (emailMessage != null)
                        {
                            var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                _r2UtilitiesSettings.DefaultFromAddress, _r2UtilitiesSettings.DefaultFromAddressName);
                            if (success)
                            {
                                nursingEmailCount++;
                                break;
                            }

                            failCount++;
                        }
                    }
                }

                if (alliedHealthResources.Any())
                {
                    var users = _emailTaskService.GetUsersForDctUpdateEmails((int)PracticeArea.AlliedHealth);
                    foreach (var user in users)
                    {
                        var emailMessage =
                            _dctUpdateResourceEmailBuildService.BuildDctUpdateEmail(alliedHealthResources, user,
                                "Allied Health");
                        if (emailMessage != null)
                        {
                            var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                _r2UtilitiesSettings.DefaultFromAddress, _r2UtilitiesSettings.DefaultFromAddressName);
                            if (success)
                            {
                                alliedHealthEmailCount++;
                                break;
                            }

                            failCount++;
                        }
                    }
                }

                var results = new StringBuilder()
                    .AppendFormat("DCT Medicine Emails Sent: {0} <br />", medicalEmailCount).AppendLine()
                    .AppendFormat("DCT Nursing Emails Sent: {0} <br />", nursingEmailCount).AppendLine()
                    .AppendFormat("DCT Allied Health Emails Sent: {0} <br />", alliedHealthEmailCount).AppendLine()
                    .AppendFormat("FAILED Emails: {0} <br />", failCount).AppendLine()
                    .ToString();

                step.CompletedSuccessfully = failCount == 0;
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

    public enum PracticeArea
    {
        Medicine = 1,
        Nursing = 2,
        AlliedHealth = 3
    }
}