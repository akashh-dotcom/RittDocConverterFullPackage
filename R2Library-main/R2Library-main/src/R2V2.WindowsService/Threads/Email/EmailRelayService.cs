#region

using System;
using System.Collections.Generic;
using System.Messaging;
using System.Text;
using System.Threading;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.Email
{
    public class EmailRelayService
    {
        private readonly EmailSendService _emailService;
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailRelayService> _log;
        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;

        private int _exceptionCount;

        private MessageQueue _messageQueue;

        public EmailRelayService(ILog<EmailRelayService> log
            , IMessageQueueService messageQueueService
            , IMessageQueueSettings messageQueueSettings
            , IEmailSettings emailSettings
            , EmailSendService emailService)
        {
            _log = log;
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings;
            _emailSettings = emailSettings;
            _emailService = emailService;
            Stop = false;
        }

        public bool Stop { get; set; }


        public void EmailProcessor()
        {
            _log.Debug("EmailProcessor >>");

            _messageQueue = GetEmailMessageQueue();
            _log.DebugFormat("_messageQueue.Path: {0}", _messageQueue.Path);

            var formatter = new XmlMessageFormatter(new[] { typeof(EmailMessage) });
            _messageQueue.Formatter = formatter;


            while (!Stop)
            {
                _log.Debug("email message queue waiting ...");

                EmailMessage emailMessage = null;
                try
                {
                    var msg = _messageQueue.Peek();
                    if (msg == null)
                    {
                        _log.Warn("MESSAGE IS NULL!");
                        continue;
                    }

                    _log.DebugFormat("peek id: {0}", msg.Id);
                    emailMessage = (EmailMessage)msg.Body;

                    _log.DebugFormat("FromAddress: {0}", emailMessage.FromAddress);
                    _log.DebugFormat("Subject: {0}", emailMessage.Subject);
                    _log.DebugFormat("IsBodyHtml: {0}", emailMessage.IsHtml);

                    var recepientCount = GetValidEmailCount(emailMessage.ToRecipients, "TO")
                                         + GetValidEmailCount(emailMessage.CcRecipients, "CC")
                                         + GetValidEmailCount(emailMessage.BccRecipients, "BCC");

                    _log.DebugFormat("recepientCount: {0}", recepientCount);
                    if (recepientCount == 0)
                    {
                        _log.Error($"BAD EMAIL MESSAGE, ZERO RECEPIENTS - Subject: {emailMessage.Subject}");
                        emailMessage.AddToRecipients(_emailSettings.BccEmailAddresses, ';');
                        emailMessage.Subject = $"BAD EMAIL MESSAGE - Zero Recepients - {emailMessage.Subject}";
                    }

                    if (_emailService.SendViaSmtp(emailMessage))
                    {
                        var tx = new MessageQueueTransaction();
                        tx.Begin();
                        msg = _messageQueue.Receive(tx);
                        if (msg != null)
                        {
                            _log.DebugFormat("received id: {0}", msg.Id);
                            _log.DebugFormat("received Body: {0}", msg.Body);
                        }
                        else
                        {
                            _log.Warn("EMAIL MESSAGE QUEUE MESSASGE WAS NULL! (1)");
                        }

                        tx.Commit();
                        _exceptionCount = 0;
                    }
                    else
                    {
                        SendDebugMessage(emailMessage);
                        var tx = new MessageQueueTransaction();
                        tx.Begin();
                        msg = _messageQueue.Receive(tx);
                        if (msg != null)
                        {
                            // re-queue message, update send attempts
                            emailMessage = (EmailMessage)msg.Body;
                            emailMessage.SendAttempts++;
                            _messageQueue.Send(emailMessage, tx);

                            _log.DebugFormat("received id: {0}", msg.Id);
                        }
                        else
                        {
                            _log.Warn("EMAIL MESSAGE QUEUE MESSAGE WAS NULL! (2)");
                        }

                        tx.Commit();
                        SleepThreadAfterException();
                    }
                }
                catch (Exception ex)
                {
                    if (emailMessage == null)
                    {
                        _log.Error(ex.Message, ex);
                    }
                    else
                    {
                        var errorMsg = new StringBuilder();
                        errorMsg.AppendLine(ex.Message);
                        errorMsg.AppendLine();
                        errorMsg.Append(emailMessage.ToDebugString());
                        _log.Error(errorMsg.ToString(), ex);
                        SendDebugMessage(emailMessage);
                    }

                    var tx = new MessageQueueTransaction();
                    tx.Begin();
                    var msg = _messageQueue.Receive(tx);
                    if (msg != null)
                    {
                        // re-queue message, update send attempts
                        var emailMessageData = (EmailMessage)msg.Body;
                        emailMessageData.SendAttempts++;
                        _messageQueue.Send(emailMessageData, tx);

                        _log.DebugFormat("received id: {0}", msg.Id);
                        _messageQueue.Send(msg.Body, tx);
                    }
                    else
                    {
                        _log.Warn("EMAIL MESSAGE QUEUE MESSAGE WAS NULL! (3)");
                    }

                    tx.Commit();

                    SleepThreadAfterException();
                }
            }

            _log.Info("STOP REQUESTED");
        }

        private void SendDebugMessage(EmailMessage emailMessage)
        {
            try
            {
                var body = new StringBuilder();
                body.AppendLine("<h1>Original Message:</h1>");
                body.AppendFormat("<div>Subject: {0}</div>", emailMessage.Subject).AppendLine();
                body.AppendFormat("<div>TO Recipients: {0}</div>", string.Join(";", emailMessage.ToRecipients))
                    .AppendLine();
                body.AppendFormat("<div>CC Recipients: {0}</div>", string.Join(";", emailMessage.CcRecipients))
                    .AppendLine();
                body.AppendFormat("<div>BCC Recipients: {0}</div>", string.Join(";", emailMessage.BccRecipients))
                    .AppendLine();
                body.Append(emailMessage.Body);

                emailMessage.Subject = $"ERROR SENDING - {emailMessage.Subject}";
                emailMessage.Body = body.ToString();
                emailMessage.ClearAllRecipients();
                emailMessage.AddToRecipients(_emailSettings.BccEmailAddresses, ';');

                var wasDebugMessageSentSuccessfully = _emailService.SendViaSmtp(emailMessage);
                _log.WarnFormat("wasDebugMessageSentSuccessfully: {0}", wasDebugMessageSentSuccessfully);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }


        private int GetValidEmailCount(IEnumerable<string> addresses, string type)
        {
            var recepientCount = 0;
            if (null != addresses)
            {
                foreach (var mailAddress in addresses)
                {
                    if (!string.IsNullOrEmpty(mailAddress))
                    {
                        _log.DebugFormat("{1} Recipient: {0}", mailAddress, type);
                        recepientCount++;
                    }
                    else
                    {
                        _log.DebugFormat("{0} Recipients: empty", type);
                    }
                }
            }

            return recepientCount;
        }

        private MessageQueue GetEmailMessageQueue()
        {
            return _messageQueue ?? (_messageQueue =
                _messageQueueService.GetMessageQueue(_messageQueueSettings.EmailMessageQueue));
        }

        private void SleepThreadAfterException()
        {
            // this method will cause the thread to sleep for a period of time after an exception
            // giving any network related time to recover.
            // minimum sleep time, 1 minute, maximum sleept time 30 minutes.
            if (Stop)
            {
                _log.Debug("do not sleep, stop has been requested!");
                return;
            }

            _exceptionCount++;
            var pauseTimeSpan = new TimeSpan(0, _exceptionCount > 30 ? 30 : _exceptionCount, 0);

            _log.InfoFormat("Pausing thread after exception for {0} minutes ...", pauseTimeSpan.Minutes);
            Thread.Sleep(pauseTimeSpan);
        }
    }
}