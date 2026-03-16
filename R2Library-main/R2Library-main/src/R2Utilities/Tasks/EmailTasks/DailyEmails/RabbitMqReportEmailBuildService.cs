#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Utilities.DataAccess;
using R2Utilities.Email.EmailBuilders;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class RabbitMqReportEmailBuildService : InternalUtilitiesEmailBuildService
    {
        public RabbitMqReportEmailBuildService(ILog<EmailBuildBaseService> log, IEmailSettings emailSettings,
            IContentSettings contentSettings) : base(log, emailSettings, contentSettings)
        {
        }

        public new void InitEmailTemplates()
        {
            SetTemplates(RabbitMqReportBodyTemplate, RabbitMqReportHostTemplate, false,
                RabbitMqReportHostQueueTemplate);
        }

        public EmailMessage BuildRabbitMqReportEmail(Dictionary<string, List<RabbitMqQueueDetails>> detailsDictionary,
            string[] emails)
        {
            var messageHtml = GetRabbitMqReportEmailHtml(detailsDictionary);

            return string.IsNullOrWhiteSpace(messageHtml)
                ? null
                : BuildEmailMessage(emails, "R2 Library RabbitMQ Queues", messageHtml);
        }

        private string GetRabbitMqReportEmailHtml(Dictionary<string, List<RabbitMqQueueDetails>> detailsDictionary)
        {
            var itemBuilder = new StringBuilder();
            var queuesWithMessagesList = new List<RabbitMqQueueDetails>();
            foreach (var detailsItem in detailsDictionary)
            {
                var itemHtml = GetRabbitMqHostDetails(detailsItem.Key, detailsItem.Value);
                itemBuilder.Append(itemHtml);
                var queuesWithMessages = detailsItem.Value.Where(x => x.messages > 0);
                queuesWithMessagesList.AddRange(queuesWithMessages);
            }

            var bodyHtml = BuildBodyHtml()
                    .Replace("{Date_Run}", DateTime.Now.ToString("g"))
                    .Replace("{Instances_Count}", detailsDictionary.Keys.Count.ToString())
                    .Replace("{Queues_Count}", detailsDictionary.Sum(x => x.Value.Count).ToString())
                    .Replace("{Queues_With_Messages_Count}", queuesWithMessagesList.Count.ToString())
                    .Replace("{Queues_Error_Count_Style}", GetErrorCountStyle(queuesWithMessagesList.Count))
                    .Replace("{Host_Items}", itemBuilder.ToString())
                ;

            var mainHtml = BuildMainHtml("RabbitMq Queues", bodyHtml, null);

            return mainHtml;
        }

        private string GetRabbitMqHostDetails(string hostName, List<RabbitMqQueueDetails> details)
        {
            var queueBuilder = new StringBuilder();

            details = details.OrderByDescending(x => x.messages).ToList();
            foreach (var test in details)
            {
                queueBuilder.Append(SubItemTemplate
                    .Replace("{Queue_Name}", test.name)
                    .Replace("{Queue_Last_Run}", GetIdleInEst(test.idle_since))
                    .Replace("{Queue_Messages}", test.messages.ToString())
                    .Replace("{Queue_Number_Sent}", test.message_stats?.publish.ToString("N0") ?? "")
                    .Replace("{Queues_Error_Count_Style}", test.messages > 0 ? GetErrorCountStyle(test.messages) : "")
                );
            }

            return ItemTemplate
                .Replace("{Host_Name}", hostName)
                .Replace("{Queue_Items}", queueBuilder.ToString()
                );
        }

        private string GetIdleInEst(string utcTime)
        {
            if (!string.IsNullOrWhiteSpace(utcTime))
            {
                try
                {
                    var parsedTime = DateTime.SpecifyKind(DateTime.Parse(utcTime), DateTimeKind.Utc);

                    if (parsedTime != DateTime.MinValue)
                    {
                        var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        var targetTime = TimeZoneInfo.ConvertTime(parsedTime, est);
                        return targetTime.ToString("g");
                    }
                }
                catch
                {
                }
            }

            return null;
        }


        private string GetErrorCountStyle(int errorCount)
        {
            return errorCount > 0 ? "background-color:#ff0000; color: white" : "background-color:#00FF00; color: white";
        }
    }
}