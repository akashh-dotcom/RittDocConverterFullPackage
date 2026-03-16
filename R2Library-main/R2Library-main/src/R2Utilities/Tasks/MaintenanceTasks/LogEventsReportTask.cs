#region

using System;
using System.Linq;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess.LogEvents;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.EmailTasks;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class LogEventsReportTask : EmailTaskBase, ITask
    {
        private readonly ILog<LogEventsReportTask> _log;
        private readonly LogEventsReportEmailBuildService _logEventsReportEmailBuildService;
        private readonly LogEventsService _logEventsService;


        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private string _file;

        public LogEventsReportTask(
            IR2UtilitiesSettings r2UtilitiesSettings
            , LogEventsService logEventsService
            , ILog<LogEventsReportTask> log
            , LogEventsReportEmailBuildService logEventsReportEmailBuildService
        ) : base(
            "LogEventsReportTask", "-LogEventsReportTask", "30", TaskGroup.DiagnosticsMaintenance,
            "Task will Send Email on LogEvents based on Period Provided", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _logEventsService = logEventsService;
            _log = log;
            _logEventsReportEmailBuildService = logEventsReportEmailBuildService;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);
            _file = GetArgument("file");
        }

        /// <summary>
        ///     <para>-LogEventsReportTask -start=1.0.0.0</para>
        ///     1 Day
        /// </summary>
        public override void Run()
        {
            TaskResult.Information = "This task will send an email with all LogEvents based on the configuration file.";
            var step = new TaskResultStep { Name = "LogEventsReportTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _logEventsReportEmailBuildService.InitEmailTemplates();
                _log.Info("Templates Set");
                var emailsSent = 0;

                var logEventsConfigurations = _logEventsService.GetLogEventsConfigruation(_file);
                _log.Info($"{logEventsConfigurations.Count} LogEventsConfigurations Found in file: {_file}");
                foreach (var logEventsConfiguration in logEventsConfigurations)
                {
                    _log.Info(
                        $"{logEventsConfiguration.ReportConfigurations.Count} ReportConfigurations Found for Table {logEventsConfiguration.TableName}");
                    foreach (var reportConfiguration in logEventsConfiguration.ReportConfigurations)
                    {
                        var logEvents = _logEventsService.GetLogEvents(reportConfiguration,
                            logEventsConfiguration.TableName, StartDate, EndDate);

                        reportConfiguration.TotalLogEvents = logEvents.Count;
                        if (logEvents.Count > reportConfiguration.ReportedItems)
                        {
                            logEvents = logEvents.Take(reportConfiguration.ReportedItems).ToList();
                        }

                        reportConfiguration.LogEvents = logEvents;
                        _log.Info(
                            $"{reportConfiguration.TotalLogEvents} LogEvents found for {reportConfiguration.Name}");
                    }

                    var emailMessage = _logEventsReportEmailBuildService.BuildLogEventsReportReportEmail(
                        logEventsConfiguration.TableName, logEventsConfiguration.ReportConfigurations, StartDate,
                        EndDate, EmailSettings.TaskEmailConfig.ToAddresses.ToArray());
                    _log.Info("Email Built");
                    if (emailMessage != null)
                    {
                        AddTaskCcToEmailMessage(emailMessage);
                        AddTaskBccToEmailMessage(emailMessage);

                        var success = EmailDeliveryService.SendTaskReportEmail(emailMessage,
                            _r2UtilitiesSettings.DefaultFromAddress, _r2UtilitiesSettings.DefaultFromAddressName);
                        _log.Info($"Email Sent {(success ? "Successfully" : "Failed")}");
                        if (success)
                        {
                            emailsSent++;
                        }
                    }
                }

                step.Results = $"Log Event Emails sent: {emailsSent}";
                step.CompletedSuccessfully = emailsSent == logEventsConfigurations.Count;
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