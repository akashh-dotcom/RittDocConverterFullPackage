#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.EmailTasks.WeeklyEmails
{
    public class NewEditionTask : EmailTaskBase
    {
        private readonly IEmailSettings _emailSettings;
        private readonly EmailTaskService _emailTaskService;
        private readonly NewEditionResourceEmailBuildService _newEditionResourceEmailBuildService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public NewEditionTask(
            EmailTaskService emailTaskService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , IEmailSettings emailSettings
            , NewEditionResourceEmailBuildService newEditionResourceEmailBuildService)
            : base("NewEditionTask", "-SendNewEditionEmails", "42", TaskGroup.CustomerEmails,
                "Sends new edition emails to customers", true)
        {
            _newEditionResourceEmailBuildService = newEditionResourceEmailBuildService;
            _emailTaskService = emailTaskService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _emailSettings = emailSettings;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "New Edition Email Task";
            var step = new TaskResultStep { Name = "NewEditionTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            _newEditionResourceEmailBuildService.SetTemplates(false);

            var institutionCount = 0;
            var successCount = 0;
            var failCount = 0;
            var userCount = 0;
            var failureEmailAddress = new StringBuilder();

            var institutionIdsOfCartsUpdated = new List<int>();

            try
            {
                var users = _emailTaskService.GetUsersForNewEditionEmail();
                Log.InfoFormat("Processing {0} users", users.Count());

                var institutionIdAndUsers = new Dictionary<int, List<User>>();
                foreach (var user in users)
                {
                    if (institutionIdAndUsers.ContainsKey(user.InstitutionId.GetValueOrDefault()))
                    {
                        institutionIdAndUsers[user.InstitutionId.GetValueOrDefault()].Add(user);
                    }
                    else
                    {
                        institutionIdAndUsers.Add(user.InstitutionId.GetValueOrDefault(), new List<User> { user });
                    }
                }

                //Dictionary<int, User> institutionIdAndUsers = users.ToDictionary(x => x.InstitutionId, y => y);

                foreach (var institutionIdAndUser in institutionIdAndUsers)
                {
                    institutionCount++;
                    var institutionUserCount = institutionIdAndUser.Value.Count;
                    Log.InfoFormat("Processing {0} of {1} institutions - Id: {2}, users: {3}", institutionCount,
                        institutionIdAndUsers.Count(), institutionIdAndUser.Key, institutionUserCount);
                    var resources = _emailTaskService.GetNewEditionResourceEmailResources(institutionIdAndUser.Key);
                    if (resources == null || !resources.Any())
                    {
                        Log.InfoFormat("No New Edition Resources Found for Institution {0}", institutionIdAndUser.Key);
                        continue;
                    }

                    var institutionUserCounter = 0;
                    foreach (var user in institutionIdAndUser.Value)
                    {
                        userCount++;
                        institutionUserCounter++;
                        Log.InfoFormat("Processing {0} of {1} users - Id: {2}, username: {3}, email: {4}",
                            institutionUserCounter, institutionUserCount, user.Id, user.UserName, user.Email);
                        var emailMessage =
                            _newEditionResourceEmailBuildService.BuildNewEditionResourceEmail(resources, user);
                        if (emailMessage != null)
                        {
                            var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                _r2UtilitiesSettings.DefaultFromAddress,
                                _r2UtilitiesSettings.DefaultFromAddressName);

                            if (success)
                            {
                                if (!institutionIdsOfCartsUpdated.Contains(user.InstitutionId.GetValueOrDefault()))
                                {
                                    if (user.IsInstitutionAdmin())
                                    {
                                        institutionIdsOfCartsUpdated.Add(user.InstitutionId.GetValueOrDefault());
                                        _emailTaskService.AddNewResourcesToCart(user.InstitutionId.GetValueOrDefault(),
                                            resources);
                                    }
                                }

                                successCount++;
                                continue;
                            }
                        }

                        failCount++;
                        failureEmailAddress.AppendFormat(
                            "FailToSend: [InstitutionId:{0} | UserId:{1} | UserEmail: {2}] <br/>", user.Institution.Id,
                            user.Id, user.Email);
                    }
                }

                Log.DebugFormat("Is in Test Mode? : {0}", !_emailSettings.SendToCustomers);
                if (_emailSettings.SendToCustomers)
                {
                    _emailTaskService.UpdateNewEditionResourceEmailResources();
                }

                step.CompletedSuccessfully = failCount == 0;
                step.Results =
                    $"{userCount} users processed,  {successCount} new editition report emails sent, {failCount} emails failed to send. Failed Emails information: {failureEmailAddress}";
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