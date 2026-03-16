#region

using System;
using System.Linq;
using System.Net.Mail;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Email
{
    public class EmailDeliveryService
    {
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailDeliveryService> _log;

        public EmailDeliveryService(ILog<EmailDeliveryService> log
            , IEmailSettings emailSettings
        )
        {
            _log = log;
            _emailSettings = emailSettings;
        }

        private int EmailsSentCount { get; set; }
        public int? DebugMaxEmailsSent { get; set; }

        protected string MainTemplate { get; set; }
        protected string BodyTemplate { get; set; }
        protected string ItemTemplate { get; set; }
        protected string SubItemTemplate { get; set; }
        protected string UnsubscribeTemplate { get; set; }

        public EmailMessage EmailMessage { get; set; }

        /// <summary>
        ///     Sends an email to a customer, NOT via the message queue.
        ///     If 'SendToCustomers' is set to false, emails are sent to email address defined in 'TestEmailAddresses'.
        ///     'SendToCustomers' should only send to true in production
        /// </summary>
        public bool SendCustomerTaskEmail(R2V2.Infrastructure.Email.EmailMessage emailMessage, string fromAddress,
            string fromAddressName)
        {
            try
            {
                if (!_emailSettings.SendToCustomers)
                {
                    _log.DebugFormat("Overwriting to email from <{0}> to <{1}>", emailMessage.ToRecipientsToString(),
                        _emailSettings.TestEmailAddresses);
                    if (emailMessage.CcRecipients.Count > 0)
                    {
                        _log.DebugFormat("CC Addresses cleared, <{0}>", emailMessage.CcRecipientsToString());
                        emailMessage.CcRecipients.Clear();
                    }

                    if (emailMessage.BccRecipients.Count > 0)
                    {
                        _log.DebugFormat("BCC Addresses cleared, <{0}>", emailMessage.BccRecipientsToString());
                        emailMessage.BccRecipients.Clear();
                    }

                    emailMessage.ToRecipients.Clear();
                    if (_emailSettings.TestEmailAddresses.Contains(';'))
                    {
                        emailMessage.AddToRecipients(_emailSettings.TestEmailAddresses, ';');
                    }
                    else
                    {
                        emailMessage.AddToRecipient(_emailSettings.TestEmailAddresses);
                    }


                    if (DebugMaxEmailsSent != null)
                    {
                        if (EmailsSentCount >= DebugMaxEmailsSent)
                        {
                            _log.DebugFormat("Max Debug Emails Reached, {0}, EMAIL NOT SENT!", EmailsSentCount);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }

            return SendEmail(emailMessage, fromAddress, fromAddressName);
        }

        public bool SendTaskReportEmail(R2V2.Infrastructure.Email.EmailMessage emailMessage, string fromAddress,
            string fromAddressName)
        {
            return SendEmail(emailMessage, fromAddress, fromAddressName);
        }

        /// <summary>
        ///     Will attemp to send three times if the SmtpClient has the error
        /// </summary>
        private bool SendEmail(R2V2.Infrastructure.Email.EmailMessage emailMessage, string fromAddress,
            string fromAddressName)
        {
            var attempCount = 0;
            while (attempCount < 3)
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        var mailMessage = emailMessage.ToMailMessage(fromAddress, fromAddressName);
                        client.Send(mailMessage);
                    }

                    EmailsSentCount++;
                    return true;
                }
                //Catches SMTP errors. We are looking for Timeouts specifically.
                catch (SmtpException sEx)
                {
                    attempCount++;
                    //Only log the 3rd exception.
                    if (attempCount > 2)
                    {
                        _log.Error(sEx.Message, sEx);
                    }
                    else
                    {
                        _log.Warn(sEx.Message, sEx);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    return false;
                }
            }

            return false;
        }
    }
}