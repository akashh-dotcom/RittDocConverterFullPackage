#region

using System;
using System.Collections.Generic;
using log4net.Appender;
using log4net.Core;

#endregion

namespace R2V2.Infrastructure.Logging
{
    public class NoBufferSmtpAppender : SmtpAppender
    {
        public string SubjectPrefix { get; set; } = "";

        protected override void SendBuffer(LoggingEvent[] events)
        {
            PrepareSubject(events);
            base.SendBuffer(events);
        }

        /// <summary>
        ///     Customize subject before call base.
        /// </summary>
        protected virtual void PrepareSubject(ICollection<LoggingEvent> events)
        {
            try
            {
                Subject = null;
                foreach (var evt in events)
                {
                    var msg = evt.ExceptionObject == null ? evt.RenderedMessage : evt.ExceptionObject.Message;

                    Subject = $"{SubjectPrefix} - {GetTruncatedMessage(msg, 40)} - {DateTime.Now:yyMMdd-HHmmssfff}";
                    Console.WriteLine(Subject);
                    break;
                }

                if (Subject == null)
                {
                    Subject = SubjectPrefix;
                }
            }
            catch (Exception ex)
            {
                Subject = $"PrepareSubject() exception: {ex.Message}";
            }
        }

        private string GetTruncatedMessage(string message, int length)
        {
            try
            {
                if (message.Length <= length)
                {
                    return message;
                }

                var spaceIndex = message.IndexOf(" ", length, StringComparison.CurrentCulture);
                if (spaceIndex <= 0 || message.Length <= spaceIndex + 4)
                {
                    return message;
                }

                return $"{message.Substring(0, spaceIndex)} ...".Replace('\r', '\0').Replace('\n', ' ');
            }
            catch (Exception ex)
            {
                return $"GetTruncatedMessage() - exception: {ex.Message}";
            }
        }
    }
}