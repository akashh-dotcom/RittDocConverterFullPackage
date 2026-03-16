#region

using System;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Email;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class FacultyRecommendationsTask : EmailTaskBase
    {
        private readonly RecommendationEmailBuildService _emailBuildService;
        private readonly EmailTaskService _emailTaskService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public FacultyRecommendationsTask(
            RecommendationEmailBuildService emailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , EmailTaskService emailTaskService
        )
            : base("FacultyRecommendationsTask", "-FacultyRecommendationsTask", "49", TaskGroup.CustomerEmails,
                "Task sends expert reviewer/faculty recommendations", true)
        {
            _emailBuildService = emailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _emailTaskService = emailTaskService;
        }

        public override void Run()
        {
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "ExpertReviewer Recommendations Task Run";
            var step = new TaskResultStep { Name = "FacultyRecommendationsTask Run", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var successEmails = 0;
            var failureEmails = 0;
            var failureEmailAddress = new StringBuilder();

            try
            {
                _emailBuildService.InitEmailTemplates();

                var institutionIds = _emailTaskService.GetInstitutionIdsForFacultyRecommentations();

                foreach (var institutionId in institutionIds)
                {
                    var id = institutionId;
                    var recommendations = _emailTaskService.GetRecommendations(id);

                    //go to next institution if there are no recommendations
                    if (!recommendations.Any())
                    {
                        continue;
                    }


                    var recommendationUsers = _emailTaskService.GetFacultyRecommendationUsers(institutionId);

                    //go to next institution if there are no users to recieve the recommendations
                    if (!recommendationUsers.Any())
                    {
                        continue;
                    }

                    var user = _emailTaskService.GetInstitutionAdministrator(institutionId);
                    var emails = recommendationUsers.Select(x => x.Email).ToArray();

                    var emailMessage = _emailBuildService.BuildRecommendationEmail(recommendations, user, emails);

                    if (emailMessage != null)
                    {
                        //MailMessage mailMessage = emailMessage.ToMailMessage(_r2UtilitiesSettings.DefaultFromAddress, _r2UtilitiesSettings.DefaultFromAddressName);
                        //bool success = _emailBuildService.SendTaskEmail(mailMessage);
                        var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                            _r2UtilitiesSettings.DefaultFromAddress,
                            _r2UtilitiesSettings.DefaultFromAddressName);

                        if (success)
                        {
                            successEmails++;
                            if (!_r2UtilitiesSettings.EmailTestMode)
                            {
                                _emailTaskService.SetRecommendationsAlertSentDate(recommendations.Select(x => x.Id)
                                    .ToArray());
                            }
                        }
                        else
                        {
                            failureEmails++;
                            failureEmailAddress.AppendFormat(
                                "[InstitutionId:{0} | UserId:{1} | UserEmail: {2} | Failed To Send] <br/>",
                                user.Institution.Id, user.Id, user.Email);
                        }
                    }
                    else
                    {
                        failureEmails++;
                        failureEmailAddress.AppendFormat(
                            "[InstitutionId:{0} | UserId:{1} | UserEmail: {2} | Failed To Build] <br/>",
                            user.Institution.Id, user.Id, user.Email);
                    }
                }

                step.CompletedSuccessfully = failureEmails == 0;
                step.Results =
                    $"{successEmails} recommendation emails sent, {failureEmails} recommendation emails failed to send/build. Failed Emails information: {(failureEmailAddress.Length == 0 ? "There were no failures" : failureEmailAddress.ToString())}";
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