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
    public class NewResourceTask : EmailTaskBase
    {
        private readonly EmailTaskService _emailTaskService;
        private readonly NewResourceEmailBuildService _newResourceEmailBuildService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public NewResourceTask(
            EmailTaskService emailTaskService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , NewResourceEmailBuildService newResourceEmailBuildService
        )
            : base("NewResourceTask", "-SendNewResourceEmails", "40", TaskGroup.CustomerEmails,
                "Send new resource email to customers", true)
        {
            _emailTaskService = emailTaskService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _newResourceEmailBuildService = newResourceEmailBuildService;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "New Resource Email Task";
            var step = new TaskResultStep { Name = "NewResourceTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var userCount = 0;
            var successEmails = 0;
            var failureEmails = 0;
            var failureEmailAddress = new StringBuilder();

            try
            {
                var users = _emailTaskService.GetUsersForNewResourceEmail();

                var newResources = _emailTaskService.GetNewResourceEmailResources();
                if (newResources != null && newResources.Any())
                {
                    _newResourceEmailBuildService.SetNewResourceItemHtml(newResources, "REPLACE");

                    foreach (var user in users)
                    {
                        userCount++;
                        Log.InfoFormat("Processing {0} of {1} users - Id: {2}, username: {3}, email: {4}"
                            , userCount
                            , users.Count()
                            , user.Id
                            , user.UserName
                            , user.Email);
                        var emailMessage = _newResourceEmailBuildService.BuildNewResourceEmail(user);

                        if (emailMessage != null)
                        {
                            var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                _r2UtilitiesSettings.DefaultFromAddress,
                                _r2UtilitiesSettings.DefaultFromAddressName);

                            if (success)
                            {
                                successEmails++;
                                continue;
                            }
                        }

                        failureEmails++;
                    }

                    if (!_r2UtilitiesSettings.EmailTestMode)
                    {
                        _emailTaskService.UpdateNewResourceEmailResources();
                    }
                }

                step.CompletedSuccessfully = failureEmails == 0;
                step.Results =
                    $"{userCount} users processed,  {successEmails} new resource emails sent, {failureEmails} emails failed to send. Failed Emails information: {failureEmailAddress}";
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