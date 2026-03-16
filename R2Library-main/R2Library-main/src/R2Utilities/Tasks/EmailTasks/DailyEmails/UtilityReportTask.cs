#region

using System;
using System.Collections.Generic;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class UtilityReportTask : EmailTaskBase
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly TaskResultDataService _taskResultDataService;
        private readonly UtilitiesReportEmailBuildService _utilitiesReportEmailBuildService;

        public UtilityReportTask(
            IR2UtilitiesSettings r2UtilitiesSettings
            , UtilitiesReportEmailBuildService utilitiesReportEmailBuildService
        )
            : base("UtilityReportTask", "-UtilityReportTask", "61", TaskGroup.InternalSystemEmails,
                "Sends the utilities report emails", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _utilitiesReportEmailBuildService = utilitiesReportEmailBuildService;
            _taskResultDataService = new TaskResultDataService();
        }

        public override void Run()
        {
            TaskResult.Information = "Utility Report Task";
            var step = new TaskResultStep { Name = "UtilityReportTask Run", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                if (StartDate == DateTime.MinValue || EndDate == DateTime.MinValue)
                {
                    throw new Exception(
                        "Please set paramters: -start or -start and -end (Examples -start=1.0:0:0 or -start=12/15/2016 -end=12/18/2016)");
                }

                var taskResults = _taskResultDataService.GetTaskResultsFromDate(StartDate, EndDate, TaskResult.Id);

                var success = ProcessUtilityReport(taskResults, StartDate, EndDate);

                step.CompletedSuccessfully = success;
                step.Results = "Utility Report Task completed successfully";
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

        private bool ProcessUtilityReport(IEnumerable<TaskResult> taskResults, DateTime startDate, DateTime endDate)
        {
            TaskResult.Information = "Process Utility Report";
            var step = new TaskResultStep { Name = "ProcessUtilityReport (Build Email)", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                //_utilitiesReportEmailBuildService.SetTemplates(EmailTemplates.UtilityReportBody, EmailTemplates.UtilityReportItem, false, EmailTemplates.UtilityReportStep);
                _utilitiesReportEmailBuildService.InitEmailTemplates();

                var emailMessage = _utilitiesReportEmailBuildService.BuildUtilitiesReportEmail(taskResults, startDate,
                    endDate, EmailSettings.TaskEmailConfig.ToAddresses.ToArray());

                var success = false;
                if (emailMessage != null)
                {
                    // add cc & bcc
                    AddTaskCcToEmailMessage(emailMessage);
                    AddTaskBccToEmailMessage(emailMessage);

                    //MailMessage mailMessage = emailMessage.ToMailMessage(_r2UtilitiesSettings.DefaultFromAddress, _r2UtilitiesSettings.DefaultFromAddressName);
                    //success = _emailBuildService.SendTaskEmail(mailMessage);
                    success = EmailDeliveryService.SendTaskReportEmail(emailMessage,
                        _r2UtilitiesSettings.DefaultFromAddress,
                        _r2UtilitiesSettings.DefaultFromAddressName);
                }

                step.CompletedSuccessfully = success;
                step.Results = "Process Utility Report completed successfully";

                return success;
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