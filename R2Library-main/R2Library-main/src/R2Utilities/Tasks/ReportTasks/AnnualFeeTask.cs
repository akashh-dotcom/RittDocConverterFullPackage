#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Email;
using R2V2.Core.Reports;

#endregion

namespace R2Utilities.Tasks.ReportTasks
{
    public class AnnualFeeTask : EmailTaskBase
    {
        private readonly AnnualFeeEmailBuildService _annualFeeEmailBuildService;
        private readonly EmailResultDataService _emailResultDataService;
        private readonly EmailTaskService _emailTaskService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IReportService _reportService;
        private readonly ReportServiceBase _reportServiceBase;

        private string _emailDescription;

        public AnnualFeeTask(
            ReportServiceBase reportServiceBase
            , IReportService reportService
            , AnnualFeeEmailBuildService annualFeeEmailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
            , EmailResultDataService emailResultDataService
            , EmailTaskService emailTaskService
        )
            : base("AnnualFeeTask", "-AnnualFeeTask", "55", TaskGroup.CustomerEmails,
                "Sends annual fee email to customers (IAs)", true)
        {
            _reportServiceBase = reportServiceBase;
            _reportService = reportService;
            _annualFeeEmailBuildService = annualFeeEmailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _emailResultDataService = emailResultDataService;
            _emailTaskService = emailTaskService;
        }

        public override void Run()
        {
            var step = new TaskResultStep { Name = "AnnualFeeTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _reportServiceBase.InitBase(new BaseReportQuery { Period = ReportPeriod.Today });
                var annualFeeReportItems = _reportService.GetAnnualFeeReportDataItems(_reportServiceBase.ReportRequest);
                TaskResult.Information = $"Annual Maintenance Fee Institutions Found = {annualFeeReportItems.Count}";

                var emailsSent = 0;
                var emailsFailed = 0;

                var users = _emailTaskService.GetAnnualMaintenanceFeeUsers();
                var annualFeeReportItemsCount = annualFeeReportItems.Count();
                if (annualFeeReportItemsCount > 0 && users.Any())
                {
                    SetDescription(annualFeeReportItems);
                    foreach (var user in users)
                    {
                        var emailMessage = _annualFeeEmailBuildService.BuildAnnualFeeEmail(annualFeeReportItems, user);
                        if (emailMessage != null)
                        {
                            var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                _r2UtilitiesSettings.DefaultFromAddress,
                                _r2UtilitiesSettings.DefaultFromAddressName);

                            if (success)
                            {
                                emailsSent++;
                                _emailResultDataService.InsertEmailResult(user, EmailType.AnnualFee, _emailDescription,
                                    _r2UtilitiesSettings.R2UtilitiesDatabaseName);
                            }
                            else
                            {
                                emailsFailed++;
                            }
                        }
                        else
                        {
                            emailsFailed++;
                        }
                    }
                }

                step.CompletedSuccessfully = true;
                step.Results =
                    $"{emailsSent} Emails sent. {emailsFailed} Emails failed. {annualFeeReportItems.Count} Institutions found for today. {users.Count()} users opt into the email.";
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

        private void SetDescription(IEnumerable<AnnualFeeReportDataItem> annualFeeReportItems)
        {
            var description = new StringBuilder()
                .Append("Email Contains the Renewals for the Following Institutions: ");
            foreach (var annualFeeReportDataItem in annualFeeReportItems)
            {
                description.AppendFormat("{0},", annualFeeReportDataItem.InstitutionId);
            }

            _emailDescription = description.ToString(0, description.Length - 1);
        }
    }
}