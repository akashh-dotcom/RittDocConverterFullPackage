#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Extensions;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.Email
{
    public class EmailSiteService
    {
        private readonly EmailQueueService _emailQueueService;
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailSiteService> _log;
        private readonly RequestInformation _requestInformation;

        public EmailSiteService(ILog<EmailSiteService> log, IEmailSettings emailSettings,
            EmailQueueService emailQueueService, RequestInformation requestInformation)
        {
            _log = log;
            _emailSettings = emailSettings;
            _emailQueueService = emailQueueService;
            _requestInformation = requestInformation;
        }

        public bool SendEmailMessageToQueue(string message, EmailPage emailPage)
        {
            var emailSendStatus = SendEmailMessageToQueue(message, emailPage, _emailSettings.DefaultReplyToAddress);
            return emailSendStatus.SendToQueueSuccessfully;
        }

        public EmailSendStatus PutEmailMessageToQueue(string message, EmailPage emailPage)
        {
            var emailSendStatus = SendEmailMessageToQueue(message, emailPage, _emailSettings.DefaultReplyToAddress);
            return emailSendStatus;
        }

        public bool SendEmailMessageToQueue(string message, EmailPage emailPage, IUser replyToUser)
        {
            var emailSendStatus = SendEmailMessageToQueue(message, emailPage, replyToUser.Email);
            return emailSendStatus.SendToQueueSuccessfully;
        }

        private EmailSendStatus SendEmailMessageToQueue(string message, EmailPage emailPage, string replyToAddress)
        {
            message = message.Replace("[[From]]",
                !string.IsNullOrWhiteSpace(emailPage.From)
                    ? $"<p class=\"email-info\">This page was sent to you by: {emailPage.From}</p>"
                    : "");
            message = message.Replace("[[Comment]]",
                !string.IsNullOrWhiteSpace(emailPage.Comments)
                    ? $"<p class=\"email-info\">Message from sender: {emailPage.Comments}</p>"
                    : "");

            LogMessage(message, emailPage);

            var emailMessage = new EmailMessage
            {
                Subject = emailPage.Subject,
                FromDisplayName = _emailSettings.DefaultFromName,
                FromAddress = _emailSettings.DefaultFromAddress,
                ReplyToAddress = replyToAddress,
                ReplyToDisplayName = replyToAddress,
                IsHtml = true,
                Body = message
            };

            if (_emailSettings.AddEnvironmentPrefixToSubject)
            {
                emailMessage.Subject = $"{_requestInformation.Host} - {emailPage.Subject}";
            }

            SetEmailAddresses(emailMessage, emailPage);

            var status = new EmailSendStatus
            {
                EmailMessage = emailMessage,
                SendToQueueSuccessfully = _emailQueueService.QueueEmailMessage(emailMessage)
            };
            return status;
        }

        private void LogMessage(string message, EmailPage emailPage)
        {
            _log.DebugFormat("message: {0}",
                message.Length > 500 ? $"{message.Substring(0, 500)} ... truncated!" : message);
            _log.DebugFormat("emailPage.Subject: {0}", emailPage.Subject);
            _log.DebugFormat("emailPage.To: {0}", emailPage.To);
            _log.DebugFormat("emailPage.Cc: {0}", emailPage.Cc);

            try
            {
                if (!string.IsNullOrWhiteSpace(_emailSettings.OutputPath))
                {
                    DirectoryHelper.VerifyDirectory(_emailSettings.OutputPath);
                    var filename = string.Format("{0}{1}{2:yyyyMMdd-HHmmssfff}_{3}.htm", _emailSettings.OutputPath,
                        _emailSettings.OutputPath.EndsWith("\\") ? "" : "\\", DateTime.Now,
                        CleanFileName(emailPage.Subject));
                    _log.DebugFormat("filename: {0}", filename);

                    var msg = new StringBuilder();
                    msg.AppendLine("<!--");
                    msg.AppendFormat("To: {0}", emailPage.To).AppendLine();
                    msg.AppendFormat("Cc: {0}", emailPage.Cc).AppendLine();
                    msg.AppendFormat("Bcc: {0}", emailPage.Bcc).AppendLine();
                    msg.AppendFormat("Subject: {0}", emailPage.Subject).AppendLine();
                    msg.AppendFormat("From: {0}", emailPage.From).AppendLine();
                    msg.AppendFormat("Comments: {0}", emailPage.Comments).AppendLine();
                    msg.AppendLine("-->");
                    msg.Append(message);

                    using (var file = new StreamWriter(filename))
                    {
                        file.Write(msg.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // swallow exception, don't let bad debug logic affect production
                _log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Remove illegal characters from filename. Most of this code comes from heere
        ///     http://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
        /// </summary>
        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName.Replace(" ", "").Replace(":", "-"),
                (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private void SetEmailAddresses(EmailMessage emailMessage, EmailPage emailPage)
        {
            if (!string.IsNullOrWhiteSpace(emailPage.To))
            {
                var toAddresses = emailPage.To.Split(';');
                foreach (var address in toAddresses)
                {
                    if (!emailMessage.AddToRecipient(address))
                    {
                        _log.WarnFormat("invalid TO email address <{0}>", address);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(emailPage.Cc))
            {
                var ccAddresses = emailPage.Cc.Split(';');
                foreach (var address in ccAddresses)
                {
                    if (!emailMessage.AddCcRecipient(address))
                    {
                        _log.WarnFormat("invalid CC email address <{0}>", address);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(emailPage.Bcc))
            {
                var bccAddresses = emailPage.Bcc.Split(';');
                foreach (var address in bccAddresses)
                {
                    if (!emailMessage.AddBccRecipient(address))
                    {
                        _log.WarnFormat("invalid BCC email address <{0}>", address);
                    }
                }
            }

            if (_emailSettings.BccAllMessages && !string.IsNullOrWhiteSpace(_emailSettings.BccEmailAddresses))
            {
                var bccAddresses = _emailSettings.BccEmailAddresses.Split(';');
                foreach (var address in bccAddresses)
                {
                    if (!emailMessage.AddBccRecipient(address))
                    {
                        _log.WarnFormat("invalid BCC email address <{0}>", address);
                    }
                }
            }
        }

        /// <summary>
        ///     Do NOT include slashes with fileName. This function will check and add it if needed.
        /// </summary>
        public string GetTemplateFromFile(string directory, string fileName)
        {
            string fullPathFile;
            if (directory.Contains("\\"))
            {
                fullPathFile = directory.Substring(directory.Length - 1) == "\\" ? directory : $"{directory}\\";
            }
            else
            {
                fullPathFile = directory.Substring(directory.Length - 1) == "/" ? directory : $"{directory}/";
            }

            return File.ReadAllText($"{fullPathFile}{fileName}");
        }
    }

    public class EmailSendStatus
    {
        public bool SendToQueueSuccessfully { get; set; }
        public EmailMessage EmailMessage { get; set; }
    }
}
