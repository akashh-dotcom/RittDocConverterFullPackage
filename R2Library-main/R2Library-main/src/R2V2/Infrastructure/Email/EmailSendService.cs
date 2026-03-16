#region

using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.Email
{
    public class EmailSendService
    {
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailSendService> _log;

        public EmailSendService(ILog<EmailSendService> log, IEmailSettings emailSettings)
        {
            _log = log;
            _emailSettings = emailSettings;
        }


        /// <summary>
        ///     DO NOT USE THIS FROM WITHIN THE WEB APP
        /// </summary>
        public bool SendViaSmtp(EmailMessage emailMessage)
        {
            try
            {
                var message = GetMailMessage(emailMessage);

                if (_emailSettings.SendToCustomers)
                {
                    using (var client = new CustomerSmtpClient())
                    {
                        emailMessage.SendAttempts++;
                        _log.Debug(
                            $"Host: {client.Host}, SendAttempts: {emailMessage.SendAttempts}, client.Timeout: {client.Timeout}, client.Port: {client.Port}, client.EnableSsl: {client.EnableSsl}, client.DeliveryMethod: {client.DeliveryMethod}");
                        client.Send(message);
                        _log.Info(
                            $"Message successfully sent to <{emailMessage.ToRecipientsToString()}>, subject: {emailMessage.Subject}");
                        return true;
                    }
                }

                _log.Info(
                    $"SendToCustomers: {_emailSettings.SendToCustomers} -- WhiteListedEmails: {_emailSettings.WhiteListedEmails}");
                var toRecipients = emailMessage.ToRecipients.Where(x => _emailSettings.WhiteListedEmails.Contains(x))
                    .ToList();

                if (toRecipients.Any())
                {
                    emailMessage.ClearAllRecipients();
                    emailMessage.AddToRecipients(toRecipients);
                    var whiteListMessage = GetMailMessage(emailMessage);
                    using (var client = new CustomerSmtpClient())
                    {
                        emailMessage.SendAttempts++;
                        _log.Debug(
                            $"Host: {client.Host}, SendAttempts: {emailMessage.SendAttempts}, client.Timeout: {client.Timeout}, client.Port: {client.Port}, client.EnableSsl: {client.EnableSsl}, client.DeliveryMethod: {client.DeliveryMethod}");
                        client.Send(whiteListMessage);
                        _log.Info(
                            $"Message successfully sent to <{emailMessage.ToRecipientsToString()}>, subject: {emailMessage.Subject}");
                        return true;
                    }
                }

                using (var client = new TestSmtpClient())
                {
                    emailMessage.SendAttempts++;
                    _log.Debug(
                        $"Host: {client.Host}, SendAttempts: {emailMessage.SendAttempts}, client.Timeout: {client.Timeout}, client.Port: {client.Port}, client.EnableSsl: {client.EnableSsl}, client.DeliveryMethod: {client.DeliveryMethod}");
                    client.Send(message);
                    _log.Info(
                        $"Message successfully sent to <{emailMessage.ToRecipientsToString()}>, subject: {emailMessage.Subject}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ex.Message}, {emailMessage.ToDebugString()}";
                _log.Error(errorMsg, ex);
                return false;
            }
        }

        private MailMessage GetMailMessage(EmailMessage emailMessage)
        {
            var message = new MailMessage();

            // To addresses
            if (null != emailMessage.ToRecipients)
            {
                foreach (var address in emailMessage.ToRecipients)
                {
                    if (!string.IsNullOrEmpty(address))
                    {
                        var cleanedAddress =
                            address.Replace(",", ".").Trim()
                                .Replace(" ",
                                    ""); // Added to handle invalid email address with comma instead of periods.  (The IU was supposed to fix this!) SJS-4/26/2011
                        _log.DebugFormat("Adding TO address: <{0}>", cleanedAddress);
                        message.To.Add(cleanedAddress);
                    }
                }
            }

            // CC addresses
            if (null != emailMessage.CcRecipients)
            {
                foreach (var address in emailMessage.CcRecipients)
                {
                    if (!string.IsNullOrEmpty(address))
                    {
                        _log.DebugFormat("Adding CC address: <{0}>", address);
                        message.CC.Add(address);
                    }
                }
            }

            // bcc address
            if (null != emailMessage.BccRecipients)
            {
                foreach (var address in emailMessage.BccRecipients)
                {
                    if (!string.IsNullOrEmpty(address))
                    {
                        _log.DebugFormat("Adding BCC address: <{0}>", address);
                        message.Bcc.Add(address);
                    }
                }
            }


            // from address
            if (!string.IsNullOrEmpty(emailMessage.FromAddress))
            {
                _log.DebugFormat("Adding FROM address: <{0}>, '{1}'", emailMessage.FromAddress,
                    emailMessage.FromDisplayName);
                message.From = string.IsNullOrEmpty(emailMessage.FromDisplayName)
                    ? new MailAddress(emailMessage.FromAddress)
                    : new MailAddress(emailMessage.FromAddress, emailMessage.FromDisplayName);
            }
            else
            {
                message.From = new MailAddress(_emailSettings.DefaultFromAddress, _emailSettings.DefaultFromName);
            }

            // reply-to address
            if (!string.IsNullOrEmpty(emailMessage.ReplyToAddress))
            {
                _log.DebugFormat("Adding REPLY-TO address: <{0}>, '{1}'", emailMessage.ReplyToAddress,
                    emailMessage.ReplyToDisplayName);
                message.ReplyToList.Add(string.IsNullOrEmpty(emailMessage.ReplyToDisplayName)
                    ? new MailAddress(emailMessage.ReplyToAddress)
                    : new MailAddress(emailMessage.ReplyToAddress, emailMessage.ReplyToDisplayName));
                //message.ReplyTo = (string.IsNullOrEmpty(emailMessage.ReplyToDisplayName)) ? new MailAddress(emailMessage.ReplyToAddress) : new MailAddress(emailMessage.ReplyToAddress, emailMessage.ReplyToDisplayName);
            }

            message.Subject = emailMessage.Subject;
            message.Body = emailMessage.Body;
            message.IsBodyHtml = emailMessage.IsHtml;

            return message;
        }
    }

    public class CustomerSmtpClient : SmtpClient
    {
        public CustomerSmtpClient()
        {
            var section = (SmtpSection)ConfigurationManager.GetSection("mailSettings/smtp_normal_mode");
            if (section != null)
            {
                if (section.Network != null)
                {
                    Host = section.Network.Host;
                    Port = section.Network.Port;
                    UseDefaultCredentials = section.Network.DefaultCredentials;

                    Credentials = new NetworkCredential(section.Network.UserName, section.Network.Password,
                        section.Network.ClientDomain);
                    EnableSsl = section.Network.EnableSsl;

                    if (section.Network.TargetName != null)
                    {
                        TargetName = section.Network.TargetName;
                    }
                }

                DeliveryMethod = section.DeliveryMethod;
                if (section.SpecifiedPickupDirectory?.PickupDirectoryLocation != null)
                {
                    PickupDirectoryLocation = section.SpecifiedPickupDirectory.PickupDirectoryLocation;
                }
            }
        }
    }

    public class TestSmtpClient : SmtpClient
    {
        public TestSmtpClient()
        {
            var section = (SmtpSection)ConfigurationManager.GetSection("mailSettings/smtp_test_mode");
            if (section != null)
            {
                if (section.Network != null)
                {
                    Host = section.Network.Host;
                    Port = section.Network.Port;
                    UseDefaultCredentials = section.Network.DefaultCredentials;

                    Credentials = new NetworkCredential(section.Network.UserName, section.Network.Password,
                        section.Network.ClientDomain);
                    EnableSsl = section.Network.EnableSsl;

                    if (section.Network.TargetName != null)
                    {
                        TargetName = section.Network.TargetName;
                    }
                }

                DeliveryMethod = section.DeliveryMethod;
                if (section.SpecifiedPickupDirectory?.PickupDirectoryLocation != null)
                {
                    PickupDirectoryLocation = section.SpecifiedPickupDirectory.PickupDirectoryLocation;
                }
            }
        }
    }
}