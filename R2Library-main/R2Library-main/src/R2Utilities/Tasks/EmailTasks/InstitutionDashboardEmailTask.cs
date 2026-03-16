#region

using System;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Cms;
using R2V2.Core.Email;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2Utilities.Tasks.EmailTasks
{
    public class InstitutionDashboardEmailTask : EmailTaskBase
    {
        private readonly CmsService _cmsService;
        private readonly DashboardEmailBuildService _dashboardEmailBuildService;
        private readonly DashboardService _dashboardService;
        private readonly EmailTaskService _emailTaskService;
        private readonly ILog<InstitutionDashboardEmailTask> _log;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public InstitutionDashboardEmailTask(
            EmailTaskService emailTaskService
            , DashboardService dashboardService
            , ILog<InstitutionDashboardEmailTask> log
            , IR2UtilitiesSettings r2UtilitiesSettings
            , DashboardEmailBuildService dashboardEmailBuildService
            , CmsService cmsService
        )
            : base("InstitutionDashboardEmailTask", "-InstitutionStatisticsEmailTask", "51", TaskGroup.CustomerEmails,
                "Sends institutional dashboard emails to customers (IAs)", true)
        {
            _emailTaskService = emailTaskService;
            _dashboardService = dashboardService;
            _log = log;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _dashboardEmailBuildService = dashboardEmailBuildService;
            _cmsService = cmsService;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "Institution Dashboard Email Task";
            var step = new TaskResultStep { Name = "InstitutionDashboardEmailTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var emailBuildCount = 0;
            var emailFailBuildCount = 0;

            var emailSendCount = 0;
            var emailFailedCount = 0;

            var failToBuildEmails = new StringBuilder();
            var failToSendEmails = new StringBuilder();
            try
            {
                var taskResultDataService = new TaskResultDataService();
                var taskResult = taskResultDataService.GetPreviousTaskResult("InstitutionStatisticsTask");

                //This is used to test. under normal circumstances it should be set to datetime.now
                var dashboardDateRunDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                //Just make sure enddate is after first of month.
                var firstOfMonth = new DateTime(dashboardDateRunDate.Year, dashboardDateRunDate.Month, 1);

                var resources = _emailTaskService.GetResources();
                var featuredTitles = _emailTaskService.GetFeaturedTitles(dashboardDateRunDate, 4);
                var specials = _emailTaskService.GetSpecials(dashboardDateRunDate, 4);
                var specialies = _emailTaskService.GetSpecialties();

                var notes = _r2UtilitiesSettings.OverRideDashboardEmailQuickNotes
                    ? null
                    : _cmsService.GetDashboardQuickNotes();
                //: _dashboardService.GetQuickNoteTextList(_r2UtilitiesSettings.CmsHtmlContentUrl);

                var dashboardDate =
                    new DateTime(dashboardDateRunDate.Year, dashboardDateRunDate.Month, 1).AddMonths(-1);
                var institutions = _emailTaskService.GetDashboardInstitutions();

                foreach (var institution in institutions)
                {
                    var users = _emailTaskService.GetDashboardUsers(institution.Id);

                    if (users.Count > 0)
                    {
                        var recommendations = _emailTaskService.GetRecommendations(institution.Id, 4);

                        var stats = _dashboardService.GetInstitutionEmailStatistics(institution.Id, dashboardDate);

                        stats.PopulateResources(resources);
                        stats.PopulateFeaturedTitles(resources,
                            featuredTitles.Count > 4 ? featuredTitles.Take(4).ToList() : featuredTitles,
                            institution.Discount);
                        stats.PopulateRecommendations(resources,
                            recommendations.Count > 4 ? recommendations.Take(4).ToList() : recommendations);
                        stats.PopulateSpecialResources(resources,
                            specials.Count > 4 ? specials.Take(4).ToList() : specials);

                        if (!_r2UtilitiesSettings.OverRideDashboardEmailQuickNotes)
                        {
                            stats.QuickNotes = notes;
                        }

                        stats.PopulateSpecialtyIds(specialies);

                        var emailBodyBase = _dashboardEmailBuildService.GetDashboardBodyBase(stats, institution);

                        var userCount = 0;
                        foreach (var user in users)
                        {
                            var emailMessage =
                                _dashboardEmailBuildService.BuildDashboardEmail(stats, user, emailBodyBase);

                            emailMessage.IsHtml = true;

                            if (emailMessage.Body != null)
                            {
                                emailBuildCount++;
                                var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                    _r2UtilitiesSettings.DefaultFromAddress,
                                    _r2UtilitiesSettings.DefaultFromAddressName);

                                if (success)
                                {
                                    emailSendCount++;
                                }
                                else
                                {
                                    emailFailedCount++;
                                    failToSendEmails.AppendFormat("[UserId: {0} | Email: {1}]", user.Id, user.Email)
                                        .AppendLine();
                                    failToSendEmails.Append("\r\n");
                                }
                            }
                            else
                            {
                                emailFailBuildCount++;
                                failToBuildEmails.Append(stats.ToDebugString(user)).AppendLine();
                            }

                            userCount++;
                        }

                        _log.InfoFormat("Institution : {0} || {1} emails sent", institution.Id, userCount);
                    }
                }

                var resultsBuilder = new StringBuilder();
                resultsBuilder.AppendFormat("{0} sent", emailSendCount).AppendLine();
                resultsBuilder.AppendFormat("{0} built", emailBuildCount).AppendLine();
                resultsBuilder.AppendFormat("{0} FAIL to send", emailFailedCount).AppendLine();
                resultsBuilder.AppendFormat("{0} FAIL to build", emailFailBuildCount).AppendLine();
                resultsBuilder.AppendFormat("FAIL to send Information: {0}", failToSendEmails).AppendLine();
                resultsBuilder.AppendFormat("FAIL to build Information: {0}", failToBuildEmails).AppendLine();

                step.CompletedSuccessfully = true;
                step.Results = resultsBuilder.ToString();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
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