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
    public class PurchasedForthcomingTask : EmailTaskBase
    {
        private readonly EmailTaskService _emailTaskService;
        private readonly ForthcomingResourceEmailBuildService _forthcomingResourceEmailBuildService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public PurchasedForthcomingTask(
            EmailTaskService emailTaskService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , ForthcomingResourceEmailBuildService forthcomingResourceEmailBuildService
        )
            : base("PurchasedForthcomingTask", "-SendPurchasedForthcomingEmails", "41", TaskGroup.CustomerEmails,
                "Sends ?? email to customers (IAs)", true)
        {
            _emailTaskService = emailTaskService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _forthcomingResourceEmailBuildService = forthcomingResourceEmailBuildService;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "Purchased Forthcoming Email Task";
            var step = new TaskResultStep { Name = "PurchasedForthcomingTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var userCount = 0;
            var successCount = 0;
            var failCount = 0;

            var failureEmailAddress = new StringBuilder();

            try
            {
                var users = _emailTaskService.GetUsersForPurchasedForthcomingEmail();

                foreach (var user in users)
                {
                    userCount++;
                    Log.InfoFormat("Processing {0} of {1} users - Id: {2}, username: {3}, email: {4}", userCount,
                        users.Count(), user.Id, user.UserName, user.Email);

                    var resources = _emailTaskService.GetPurchasedResourceEmailResources(user.Institution.Id);
                    if (resources == null || !resources.Any())
                    {
                        Log.Info("No Resources Found");
                        continue;
                    }

                    var emailMessage =
                        _forthcomingResourceEmailBuildService.BuildForthcomingResourceEmail(resources, user);

                    if (emailMessage != null)
                    {
                        var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                            _r2UtilitiesSettings.DefaultFromAddress,
                            _r2UtilitiesSettings.DefaultFromAddressName);

                        if (success)
                        {
                            successCount++;
                            continue;
                        }
                    }

                    failCount++;
                    failureEmailAddress.AppendFormat(
                        "FailToSend: [InstitutionId:{0} | UserId:{1} | UserEmail: {2}] <br/>", user.Institution.Id,
                        user.Id, user.Email);
                }

                if (!_r2UtilitiesSettings.EmailTestMode)
                {
                    _emailTaskService.UpdatePurchasedResourceEmailResources();
                }

                step.CompletedSuccessfully = failCount == 0;
                step.Results =
                    $"{userCount} users processed,  {successCount} purchased forthcoming emails sent, {failCount} emails failed to send. Failed Emails information: {failureEmailAddress}";
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