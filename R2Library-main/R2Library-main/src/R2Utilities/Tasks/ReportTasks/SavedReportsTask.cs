#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource;

#endregion

namespace R2Utilities.Tasks.ReportTasks
{
    public class SavedReportsTask : EmailTaskBase
    {
        private readonly SavedReportsEmailBuildService _emailBuildService;
        private readonly EmailTaskService _emailTaskService;

        private readonly IQueryable<Institution> _institutions;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IReportService _reportService;
        private readonly ReportServiceBase _reportServiceBase;
        private readonly IQueryable<Resource> _resources;
        private readonly IQueryable<User> _users;
        private ReportFrequency _frequency = ReportFrequency.Weekly;

        private IEnumerable<IResource> Resources;

        public SavedReportsTask(
            IQueryable<Institution> institutions
            , IReportService reportService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , SavedReportsEmailBuildService emailBuildService
            , IQueryable<Resource> resources
            , IQueryable<User> users
            , EmailTaskService emailTaskService
            , ReportServiceBase reportServiceBase
        )
            : base("SavedReportsTask", "-SavedReportsTask", "39", TaskGroup.CustomerEmails,
                "Sends saved reports emails to customers (IAs), -frequency=Weekly|BiWeekly|Monthly", true)
        {
            _institutions = institutions;
            _reportService = reportService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _emailBuildService = emailBuildService;
            _resources = resources;
            _users = users;
            _emailTaskService = emailTaskService;
            _reportServiceBase = reportServiceBase;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;
            _frequency = (ReportFrequency)Enum.Parse(typeof(ReportFrequency), GetArgument("frequency"));

            var step = new TaskResultStep { Name = $"SavedReportsTask - {_frequency}", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            TaskResult.Information = $"Frequency = {_frequency}";

            try
            {
                var reportsToRun = _reportService.GetSavedReports(_frequency);

                var appUserReportCount = 0;
                var resourceUserReportCount = 0;
                var invalidReportTypeCount = 0;
                var exceptionCount = 0;

                var emailCount = 0;
                var emailCountTotal = reportsToRun.Count();

                foreach (var savedReport in reportsToRun)
                {
                    emailCount++;
                    Log.InfoFormat("Processing {0} of {1}, Type: {2}, Email: {3}, UserId: {4}", emailCount,
                        emailCountTotal, savedReport.Type, savedReport.Email, savedReport.UserId);

                    //Only need to check the last updated because if null the report was never run and needs too.
                    if (savedReport.LastUpdate.HasValue)
                    {
                        if (savedReport.Frequency == (int)ReportFrequency.Monthly)
                        {
                            //Need to minus a few days to take into account shorter months
                            if (savedReport.LastUpdate.GetValueOrDefault().AddMonths(1).AddDays(-4).Date >
                                DateTime.Now.Date)
                            {
                                Log.InfoFormat(" -> Ignored, LastUpdate: {0}", savedReport.LastUpdate);
                                continue;
                            }
                        }
                        else
                        {
                            if (savedReport.LastUpdate.GetValueOrDefault().AddDays(savedReport.Frequency).AddDays(-2)
                                    .Date > DateTime.Now.Date)
                            {
                                Log.InfoFormat(" -> Ignored, LastUpdate: {0}", savedReport.LastUpdate);
                                continue;
                            }
                        }
                    }

                    try
                    {
                        switch (savedReport.Type)
                        {
                            case (int)ReportType.ApplicationUsageReport:
                                RunSavedApplicationUsage(savedReport);
                                appUserReportCount++;
                                Log.Info(" -> Application Usage Report Sent");
                                break;
                            case (int)ReportType.ResourceUsageReport:
                                RunSavedResourceUsage(savedReport);
                                resourceUserReportCount++;
                                Log.Info(" -> Resource Usage Report Sent");
                                break;
                            default:
                                Log.ErrorFormat(" -> Don't know how to handle this Report : {0}", savedReport);
                                invalidReportTypeCount++;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log.ErrorFormat("Error processing SavedReport : {0}", savedReport);
                        Log.Error($" -> Error processing SavedReport : {savedReport}", ex);
                        exceptionCount++;
                    }
                }

                step.CompletedSuccessfully = invalidReportTypeCount == 0 && exceptionCount == 0;
                step.Results =
                    $"{appUserReportCount} application usage report emails sent, {resourceUserReportCount} resource usage report emails sent, {invalidReportTypeCount} invalid report types, {exceptionCount} exceptions.";
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

        private void RunSavedApplicationUsage(SavedReport savedReport)
        {
            _reportServiceBase.InitBase(savedReport.InstitutionId, savedReport.Id);
            var institution = _institutions.FirstOrDefault(x => x.Id == _reportServiceBase.ReportRequest.InstitutionId);

            var applicationReportCounts = _reportService.GetApplicationReportCounts(_reportServiceBase.ReportRequest);

            var user = _users.FirstOrDefault(x => x.Id == savedReport.UserId);

            var emailMessage = _emailBuildService.BuildApplicationUsageReportEmail(applicationReportCounts, savedReport,
                _reportServiceBase.ReportRequest, institution, user);

            if (emailMessage != null)
            {
                var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                    _r2UtilitiesSettings.DefaultFromAddress,
                    _r2UtilitiesSettings.DefaultFromAddressName);
                if (success && !_r2UtilitiesSettings.EmailTestMode)
                {
                    _reportService.SaveSavedReport(_reportServiceBase.ReportRequest.DateRangeEnd, savedReport.Id);
                }
            }
        }

        private void RunSavedResourceUsage(SavedReport savedReport)
        {
            _reportServiceBase.InitBase(savedReport.InstitutionId, savedReport.Id);

            var institution = _institutions.FirstOrDefault(x => x.Id == _reportServiceBase.ReportRequest.InstitutionId);

            if (Resources == null)
            {
                Resources = _resources.ToList();
            }

            var items = _reportService.GetResourceReportItems(_reportServiceBase.ReportRequest, Resources.ToList());

            if (items.Count == 0)
            {
                return;
            }

            //_emailBuildService.SetTemplates(EmailTemplates.ResourceUsageBody);
            _emailBuildService.InitEmailTemplates();

            var emailMessage = _emailBuildService.BuildResourceUsageReportEmail(items, savedReport,
                _reportServiceBase.ReportRequest, institution);

            if (emailMessage != null)
            {
                var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                    _r2UtilitiesSettings.DefaultFromAddress,
                    _r2UtilitiesSettings.DefaultFromAddressName);

                if (success && !_r2UtilitiesSettings.EmailTestMode)
                {
                    _emailTaskService.UpdateSavedReportLastUpdate(_reportServiceBase.ReportRequest.DateRangeEnd,
                        savedReport.Id);
                }
            }
        }
    }
}