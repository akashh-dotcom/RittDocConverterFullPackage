#region

using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.Email;
using R2Utilities.Infrastructure.Settings;
using R2V2.Infrastructure.Compression;

#endregion

//using ICSharpCode.SharpZipLib.Zip;

namespace R2Utilities.Tasks
{
    public abstract class TaskBase : R2UtilitiesBase, ITask
    {
        private bool _includeOkTaskStepsInSummaryEmail = true;
        private int _maxTaskStepsInSummaryEmail = 2500;
        private IR2UtilitiesSettings _r2UtilitiesSettings;
        private bool _showStepTotalsInSummaryEmail;

        private TaskResultDataService _taskResultDataService;

        protected TaskBase(string taskName, string taskSwitch, string taskSwitchSmall, TaskGroup taskGroup,
            string taskDescription, bool enabled)
        {
            TaskName = taskName;
            TaskDescription = taskDescription;
            TaskGroup = taskGroup;
            TaskSwitch = taskSwitch;
            TaskSwitchSmall = taskSwitchSmall;
            IsEnabled = enabled;
        }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        protected string EmailSubject { get; set; }

        protected TaskEmailSettings EmailSettings { get; private set; }

        public string TaskName { get; }
        public string TaskDescription { get; }
        public string TaskSwitch { get; }
        public string TaskSwitchSmall { get; }
        public TaskGroup TaskGroup { get; }
        public string[] CommandLineArguments { get; private set; }
        public bool IsEnabled { get; }

        public TaskResult TaskResult { get; private set; }

        public abstract void Run();

        public void Init(string[] commandLineArguments)
        {
            CommandLineArguments = commandLineArguments;

            _r2UtilitiesSettings = Bootstrapper.Container.Resolve<IR2UtilitiesSettings>();
            _taskResultDataService = new TaskResultDataService();
            try
            {
                EmailSettings = new TaskEmailSettings(TaskName, _r2UtilitiesSettings.EmailConfigDirectory);
                TaskResult = new TaskResult { Name = TaskName, StartTime = DateTime.Now, Results = "Init Complete" };
                _taskResultDataService.InsertTaskResult(TaskResult);
                SetDates();
                Log.Debug(TaskResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void Cleanup()
        {
            TaskResult.EndTime = DateTime.Now;
            UpdateTaskResult();
            SendCompleteEmail();
        }

        protected void SetSummaryEmailSetting(bool includeOkTaskSteps, bool showStepTotals, int maxTaskSteps)
        {
            _includeOkTaskStepsInSummaryEmail = includeOkTaskSteps;
            _showStepTotalsInSummaryEmail = showStepTotals;
            _maxTaskStepsInSummaryEmail = maxTaskSteps;
            Log.InfoFormat(
                "_includeOkTaskStepsInSummaryEmail: {0}, _showStepTotalsInSummaryEmail: {1}, _maxTaskStepsInSummaryEmail: {2}",
                _includeOkTaskStepsInSummaryEmail, _showStepTotalsInSummaryEmail, _maxTaskStepsInSummaryEmail);
        }

        private void SetDates()
        {
            var startArgument = CommandLineArguments.FirstOrDefault(x => x.StartsWith("-start"));
            if (!string.IsNullOrWhiteSpace(startArgument))
            {
                var startString = startArgument.Replace("-start=", "");

                var endArgument = CommandLineArguments.FirstOrDefault(x => x.StartsWith("-end"));
                if (!string.IsNullOrWhiteSpace(endArgument))
                {
                    var endString = endArgument.Replace("-end=", "");
                    var parsedStartDate = DateTime.Parse(startString);
                    var parsedEndDate = DateTime.Parse(endString);

                    StartDate = new DateTime(parsedStartDate.Year, parsedStartDate.Month, parsedStartDate.Day);
                    EndDate = new DateTime(parsedEndDate.Year, parsedEndDate.Month, parsedEndDate.Day).AddDays(1)
                        .AddMilliseconds(-5);
                }
                else
                {
                    var timeSpan = TimeSpan.Parse(startString);
                    StartDate = DateTime.Now.Add(timeSpan);
                    EndDate = DateTime.Now;

                    if (StartDate < EndDate)
                    {
                        var dayDifference = (EndDate - StartDate).Days;

                        if (dayDifference >= 28 && dayDifference <= 31)
                        {
                            StartDate = EndDate.AddMonths(-1);
                        }
                    }
                    else
                    {
                        var dayDifference = (StartDate - EndDate).Days;

                        if (dayDifference >= 28 && dayDifference <= 31)
                        {
                            StartDate = EndDate.AddMonths(1);
                        }
                    }
                }
            }
        }

        public void UpdateTaskResult()
        {
            var taskResultDataService = new TaskResultDataService();
            taskResultDataService.UpdateTaskResult(TaskResult);
            foreach (var taskResultStep in TaskResult.Steps)
            {
                taskResultDataService.SaveTaskResultStep(taskResultStep);
            }
        }

        private void SendCompleteEmail()
        {
            var emailBody = new StringBuilder()
                .AppendLine("<html><body><title></title>")
                .AppendLine("<style type=\"text/css\">")
                .AppendLine("body { font-family: Sans-serif, Arial, Verdana; font-size: 100%; }")
                .AppendLine("h1 { font-size: 1.4em; font-weight:bold; }")
                .AppendLine("h2 { font-size: 1.2em; font-weight:bold; }")
                .AppendLine(".status { font-size: 1.1em; font-weight:bold; }")
                .AppendLine(".step { font-size: 1.1em; font-weight:bold; padding-top: 15px;}")
                .AppendLine(".ok { color:green; }")
                .AppendLine(".stepData { font-size: 0.8em; padding-left: 20px; }")
                .AppendLine(".error { color:red; }")
                .AppendLine(".warning { color:orange; }")
                .AppendLine("</style></head>")
                .AppendLine("<body>")
                .AppendLine("<h1>R2 Library v2 Utilities</h1>")
                .AppendFormat("<h2>Task Name: {0}</h2>", TaskResult.Name).AppendLine()
                .AppendFormat("<div class=\"status\">Status: <span class=\"{1}\">{0}</span></div>", TaskResult.Status,
                    TaskResult.Status.ToLower())
                .AppendLine()
                .AppendFormat("<div>Information: {0}</div>", TaskResult.Information).AppendLine()
                .AppendFormat("<div>Task Id: {0}</div>", TaskResult.Id).AppendLine()
                .AppendFormat("<div>Run Time: {0:c} - ({1:M/d/yy hh:mm:ss.fff tt} to {2:M/d/yy hh:mm:ss.fff tt})</div>",
                    TaskResult.GetRunTime(), TaskResult.StartTime, TaskResult.EndTime)
                .AppendFormat("<div>Machine Name: {0}</div>", Environment.MachineName).AppendLine()
                .AppendFormat("<div>User: {0}</div>", Environment.UserName).AppendLine()
                .AppendLine();

            emailBody.AppendLine("<table border=0 cellpadding=0 cellspacing=0>");
            AppendTaskReportText(emailBody);

            emailBody.AppendLine("</table>");
            emailBody.AppendLine("</body>");

            var subject = new StringBuilder();
            subject.AppendFormat("{0} - {1}", TaskResult.Status, TaskResult.Name);
            if (!string.IsNullOrWhiteSpace(EmailSubject))
            {
                subject.AppendFormat(" - {0}", EmailSubject);
            }

            subject.AppendFormat(" - R2Utilities on {0} - {1}", Environment.MachineName,
                _r2UtilitiesSettings.EnvironmentName);

            Log.DebugFormat("Send Complete Email: {0}", EmailSettings.SuccessEmailConfig.Send);
            Log.DebugFormat("subject: {0}\n{1}", subject, emailBody);

            if (!EmailSettings.SuccessEmailConfig.Send)
            {
                var warningMsg = new StringBuilder()
                    .AppendLine().AppendLine()
                    .AppendLine(
                        "----------------------------------------------------------------------------------------------------")
                    .AppendLine(
                        "----------------------------------------------------------------------------------------------------")
                    .AppendLine(
                        "------- STATUS EMAIL MESSAGES FOR THIS TASK ARE DISABLED IN THE EMAIL CONFIGURATION XML FILE -------")
                    .AppendLine(
                        "----------------------------------------------------------------------------------------------------")
                    .AppendLine(
                        "----------------------------------------------------------------------------------------------------")
                    .AppendLine();
                Log.Warn(warningMsg);
                return;
            }

            SendCompleteEmail(subject.ToString(), emailBody.ToString(),
                TaskResult.CompletedSuccessfully ? EmailSettings.SuccessEmailConfig : EmailSettings.ErrorEmailConfig);
        }


        protected void SendCompleteEmail(string subject, string body, EmailConfiguration emailConfiguration)
        {
            var emailMessageData = new EmailMessage(_r2UtilitiesSettings)
            {
                IsBodyHtml = true,
                Subject = subject,
                MessageBody = body,
                ToRecipients = emailConfiguration.ToAddresses.ToArray(),
                CcRecipients = emailConfiguration.CcAddresses.ToArray(),
                BccRecipients = emailConfiguration.BccAddresses.ToArray()
            };


            if (!string.IsNullOrEmpty(TaskResult.EmailAttachmentData))
            {
                var zipFilePath = CompressStringToFile(TaskResult.EmailAttachmentData);
                var attachment = new Attachment(zipFilePath);
                emailMessageData.ExcelAttachment = attachment;
            }

            var messageSendOk = emailMessageData.Send();
            Log.InfoFormat("messageSendOk: {0}", messageSendOk);
        }

        private void AppendTaskReportText(StringBuilder text)
        {
            var okSteps = TaskResult.Steps.Count(x => x.CompletedSuccessfully);
            var errorSteps = TaskResult.Steps.Count(x => !x.CompletedSuccessfully);

            var okStepsText = $"<span class=\"ok\">{okSteps} Ok</span> steps";
            var errorStepsText = string.Format("<span class=\"{1}\">{0} ERROR</span> steps", errorSteps,
                errorSteps > 0 ? "error" : "");

            if (!_includeOkTaskStepsInSummaryEmail)
            {
                text.AppendLine("<tr><td colspan=\"2\">&nbsp;</td></tr>");
                text.AppendLine(
                    "<tr><td colspan=\"2\"><div class=\"step\">This task is configured to ONLY show ERRORS.</div></td></tr>");
                text.AppendFormat(
                    "<tr><td style=\"width:20px;\" rowspan=\"2\">&nbsp;</td><td><div class=\"stepData\">{0} total steps, {1}, {2}.</div></td></tr>",
                    TaskResult.Steps.Count, okStepsText, errorStepsText).AppendLine();
            }
            else if (_showStepTotalsInSummaryEmail)
            {
                text.AppendLine("<tr><td colspan=\"2\">&nbsp;</td></tr>");
                text.AppendLine("<tr><td colspan=\"2\"><div class=\"step\">Step Summary.</div></td></tr>");
                text.AppendFormat(
                    "<tr><td style=\"width:20px;\" rowspan=\"2\">&nbsp;</td><td><div class=\"stepData\">{0} total steps, {1}, {2}.</div></td></tr>",
                    TaskResult.Steps.Count, okStepsText, errorStepsText).AppendLine();
            }

            var stepDisplayCount = 0;
            for (var i = TaskResult.Steps.Count - 1; i >= 1; i--)
            {
                var step = TaskResult.Steps[i];

                if (_includeOkTaskStepsInSummaryEmail || !step.CompletedSuccessfully)
                {
                    AppendTaskReportText(text, step);
                    stepDisplayCount++;
                }

                if (stepDisplayCount >= _maxTaskStepsInSummaryEmail)
                {
                    text.AppendLine("<tr><td colspan=\"2\">&nbsp;</td></tr>");
                    text.AppendFormat(
                        "<tr><td colspan=\"2\"><div class=\"step warning\">This task is configured to display {0} steps, this message was be truncated.</div></td></tr>",
                        _maxTaskStepsInSummaryEmail);
                    text.AppendFormat(
                        "<tr><td style=\"width:20px;\" rowspan=\"2\">&nbsp;</td><td><div class=\"stepData\">{0} total steps, {1} <span class=\"ok\">Ok steps, {2} <span class=\"error\">ERROR steps, </span></div></td></tr>",
                        TaskResult.Steps.Count, okSteps, errorSteps).AppendLine();

                    break;
                }
            }

            if (TaskResult.Steps.Count > 0)
            {
                // always display the first step
                AppendTaskReportText(text, TaskResult.Steps[0]);
            }
        }

        private void AppendTaskReportText(StringBuilder text, TaskResultStep step)
        {
            text.AppendLine("<tr><td colspan=\"2\">&nbsp;</td></tr>");

            text.AppendFormat("<tr><td colspan=\"2\"><div class=\"step\">{0}</div></td></tr>", step.Name).AppendLine();
            text.AppendFormat(
                "<tr><td style=\"width:20px;\" rowspan=\"2\">&nbsp;</td><td><div class=\"stepData\">{0}</div></td></tr>",
                step.Results.Replace("\r\n", "\r\n<br />")).AppendLine();
            text.AppendFormat(
                "<tr><td><div class=\"stepData\">Status: <span class=\"{1}\">{0}</span>, Run Time: {2:c}, Step Id: {3}</div></td></tr>",
                step.Status, step.Status.ToLower(), step.GetRunTime(),
                step.Id).AppendLine();
        }

        protected string GetArgument(string name)
        {
            return (
                from arg in CommandLineArguments
                let argName = $"-{name}=".ToLower()
                where arg.ToLower().StartsWith(argName)
                select Regex.Replace(arg, argName, string.Empty, RegexOptions.IgnoreCase)
            ).FirstOrDefault();
        }

        protected bool GetArgumentBoolean(string name, bool defaultValue)
        {
            var textValue = GetArgument(name);
            if (string.IsNullOrWhiteSpace(textValue))
            {
                return defaultValue;
            }

            return bool.TryParse(textValue, out var returnValue) ? returnValue : defaultValue;
        }

        protected DateTime GetArgumentDateTime(string name, DateTime defaultValue)
        {
            var textValue = GetArgument(name);
            if (string.IsNullOrWhiteSpace(textValue))
            {
                return defaultValue;
            }

            return DateTime.TryParse(textValue, out var returnValue) ? returnValue : defaultValue;
        }

        protected int GetArgumentInt32(string name, int defaultValue)
        {
            var textValue = GetArgument(name);
            if (string.IsNullOrWhiteSpace(textValue))
            {
                return defaultValue;
            }

            return int.TryParse(textValue, out var returnValue) ? returnValue : defaultValue;
        }


        protected string CompressStringToFile(string data)
        {
            var textFileName = $"{TaskResult.Id:00000000}-EmailAttachment.txt";
            var zipFileName = $"{TaskResult.Id:00000000}-EmailAttachment.zip";
            var textFilePath = Path.Combine(_r2UtilitiesSettings.TaskCompressionTempDirectory, textFileName);
            File.WriteAllText(textFilePath, data);

            var zipFilePath = Path.Combine(_r2UtilitiesSettings.TaskCompressionTempDirectory, zipFileName);


            var outFileInfo = new FileInfo(zipFilePath);
            //FileInfo inFileInfo = new FileInfo(textFilePath);

            // Create the output directory if it does not exist
            if (outFileInfo.Directory != null && !Directory.Exists(outFileInfo.Directory.FullName))
            {
                Directory.CreateDirectory(outFileInfo.Directory.FullName);
            }

            ZipHelper.CompressFile(textFilePath, zipFilePath);
            return zipFilePath;
        }
    }
}