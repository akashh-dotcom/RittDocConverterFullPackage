#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using R2Utilities.DataAccess.LogEvents;
using R2Utilities.Email.EmailBuilders;
using R2V2.Extensions;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.EmailTasks
{
    public class LogEventsReportEmailBuildService : InternalUtilitiesEmailBuildService
    {
        private readonly ILog<EmailBuildBaseService> _log;

        public LogEventsReportEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings)
            : base(log, emailSettings, contentSettings)
        {
            _log = log;
        }

        public new void InitEmailTemplates()
        {
            SetTemplates(MainHeaderFooterTemplate, LogEventsReportBodyTemplate, LogEventsReportBodyItemTemplate,
                LogEventsReportItemTemplate, false, LogEventsReportStepTemplate);
        }


        public EmailMessage BuildLogEventsReportReportEmail(string tableName, List<ReportConfiguration> logEventsLevels,
            DateTime startDate, DateTime endDate, string[] emails)
        {
            var messageHtml = GetEmailHtml(logEventsLevels, startDate, endDate, tableName);

            return string.IsNullOrWhiteSpace(messageHtml)
                ? null
                : BuildEmailMessage(emails, $"LogEvents Report - {tableName}", messageHtml);
        }

        private string GetEmailHtml(List<ReportConfiguration> reportConfigurations, DateTime startDate,
            DateTime endDate, string tableName)
        {
            var itemHtml = GetItemsHtml(reportConfigurations);

            var bodyHtml = BuildBodyHtml()
                    .Replace("{LogEvent_TableName}", tableName)
                    .Replace("{LogEvent_StartDate}", startDate.ToString())
                    .Replace("{LogEvent_EndDate}", endDate.ToString())
                    .Replace("{LogEvent_Body_Items}",
                        GetBodyItemsHtml(reportConfigurations, (endDate - startDate).Days))
                    .Replace("{LogEvent_Items}", itemHtml)
                ;

            return BuildMainHtml($"{tableName} LogEvents Report", bodyHtml, null);
        }

        private string GetBodyItemsHtml(List<ReportConfiguration> reportConfigurations, int reportDaysCount)
        {
            var bodyItemsBuilder = new StringBuilder();
            foreach (var reportConfiguration in reportConfigurations)
            {
                if (reportConfiguration.TotalLogEvents == 0)
                {
                    continue;
                }

                bodyItemsBuilder.Append(BodySubTemplate
                    .Replace("{LogEvent_Level_Name}", reportConfiguration.Name)
                    .Replace("{LogEvent_Level_Count}", $"{reportConfiguration.TotalLogEvents}")
                    .Replace("{Error_Count_Style}",
                        GetErrorCountStyle(reportConfiguration.TotalLogEvents,
                            reportConfiguration.PerDayAlertThreshHold * reportDaysCount))
                );
            }

            return bodyItemsBuilder.ToString();
        }

        private string GetErrorCountStyle(int errorCount, int reportAlertCount)
        {
            return errorCount > reportAlertCount ? "background-color:#ff0000; color: white" : "";
        }

        private string GetItemsHtml(IEnumerable<ReportConfiguration> reportConfigurations)
        {
            var itemBuilder = new StringBuilder();
            foreach (var reportConfiguration in reportConfigurations)
            {
                var fieldsToReportOn = ParseFields(reportConfiguration);

                if (!fieldsToReportOn.Any())
                {
                    continue;
                }

                var subItemBuilder = new StringBuilder();

                foreach (var logEvent in reportConfiguration.LogEvents)
                {
                    var sb = new StringBuilder();
                    object reportCount = null;
                    foreach (var s in fieldsToReportOn)
                    {
                        var propertyInfo = logEvent.GetType()
                            .GetProperty(s, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo != null)
                        {
                            if (s == "reportcount")
                            {
                                reportCount = propertyInfo.GetValue(logEvent, null);
                                continue;
                            }

                            if (sb.Length != 0)
                            {
                                sb.Append(" <br /> ");
                            }

                            var value = propertyInfo.GetValue(logEvent, null);

                            if (value != null && value.ToString().Length > 100)
                            {
                                value = $"{value.ToString().Substring(0, 100)}.....";
                            }

                            sb.Append($"<b>{s}</b>: {value}");
                        }
                    }

                    subItemBuilder.Append(SubItemTemplate
                        .Replace("{Step_Detail}", sb.ToString())
                        .Replace("{Step_Count}", reportCount != null ? reportCount.ToString() : "0")
                    );
                }

                itemBuilder.Append(ItemTemplate
                    .Replace("{LogEvent_Item_Steps}", subItemBuilder.ToString())
                    .Replace("{LogEvent_Count}", reportConfiguration.TotalLogEvents.ToString())
                    .Replace("{LogEvent_Level}", reportConfiguration.Name)
                );
            }

            return itemBuilder.ToString();
        }

        private List<string> ParseFields(ReportConfiguration reportConfiguration)
        {
            var fieldsToReportOn = new List<string>();

            var fields = reportConfiguration.FieldSelect.Split(',');
            if (fields.Length == 1 && fields.First() == "*")
            {
                return fieldsToReportOn;
            }

            foreach (var field in fields)
            {
                var fieldLowered = field.ToLower();
                if (fieldLowered.Contains("count("))
                {
                    var countField = fieldLowered.ReplaceWithRegExp("count((.*?)(?:)) as ", "").Replace("'", "");
                    if (countField.Contains("count("))
                    {
                        countField = fieldLowered.ReplaceWithRegExp(@"count\(\w+\) ", "").Replace("'", "");
                        fieldsToReportOn.Add(countField.Trim());
                    }
                    else
                    {
                        fieldsToReportOn.Add(countField.Trim());
                    }
                }
                else if (fieldLowered.Contains("max("))
                {
                    var countField = fieldLowered.ReplaceWithRegExp("max((.*?)(?:)) as ", "").Replace("'", "");
                    if (countField.Contains("max("))
                    {
                        countField = fieldLowered.ReplaceWithRegExp(@"max\(\w+\) ", "").Replace("'", "");
                        fieldsToReportOn.Add(countField.Trim());
                    }
                    else
                    {
                        fieldsToReportOn.Add(countField.Trim());
                    }
                }
                else
                {
                    fieldsToReportOn.Add(fieldLowered.Trim());
                }
            }


            return fieldsToReportOn;
        }
    }
}