#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using R2V2.Infrastructure.MessageQueue;

#endregion

namespace R2V2.Infrastructure.Email
{
    public class EmailMessage : IR2V2Message
    {
        private const string EmailPatternStrict =
            @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";

        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsHtml { get; set; }
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }
        public string ReplyToAddress { get; set; }
        public string ReplyToDisplayName { get; set; }
        public DateTime DateSentToQueue { get; set; }
        public int SendAttempts { get; set; }

        public List<string> ToRecipients { get; } = new List<string>();
        public List<string> CcRecipients { get; } = new List<string>();
        public List<string> BccRecipients { get; } = new List<string>();
        public List<string> InvalidEmailAddresses { get; } = new List<string>();

        //This property will will suppress the Reflection error when sending to Que
        [XmlIgnore] public Attachment ExcelAttachment { get; set; }

        public Guid MessageId { get; set; } = Guid.NewGuid();

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public bool AddToRecipient(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                address = address.Trim();
            }

            if (IsValidEmail(address))
            {
                ToRecipients.Add(address);
                return true;
            }

            InvalidEmailAddresses.Add(address);
            return false;
        }

        public int AddToRecipients(string addresses, char delimiter)
        {
            var recipients = addresses.Split(delimiter);
            return AddToRecipients(recipients);
        }

        public int AddToRecipients(IEnumerable<string> addresses)
        {
            return addresses.Where(AddToRecipient).Count();
        }


        public bool AddCcRecipient(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                address = address.Trim();
            }

            if (IsValidEmail(address))
            {
                CcRecipients.Add(address);
                return true;
            }

            return false;
        }

        public int AddCcRecipients(string addresses, char delimiter)
        {
            var recipients = addresses.Split(delimiter);
            return AddCcRecipients(recipients);
        }

        public int AddCcRecipients(IEnumerable<string> addresses)
        {
            return addresses.Where(AddCcRecipient).Count();
        }

        public bool AddBccRecipient(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                address = address.Trim();
            }

            if (IsValidEmail(address))
            {
                BccRecipients.Add(address);
                return true;
            }

            return false;
        }

        public int AddBccRecipients(string addresses, char delimiter)
        {
            var recipients = addresses.Split(delimiter);
            return AddBccRecipients(recipients);
        }

        public int AddBccRecipients(IEnumerable<string> addresses)
        {
            return addresses.Where(AddBccRecipient).Count();
        }

        public void ClearAllRecipients()
        {
            ToRecipients.Clear();
            CcRecipients.Clear();
            BccRecipients.Clear();
        }

        public static bool IsValidEmail(string email)
        {
            var regex = new Regex(EmailPatternStrict);

            return !string.IsNullOrWhiteSpace(email) && regex.IsMatch(email);
        }


        public string ToDebugString()
        {
            var debug = new StringBuilder();

            debug.AppendLine("EmailMessage = [");
            debug.AppendFormat("\t  Subject: {0}", Subject).AppendLine();
            debug.AppendFormat("\t, IsHtml: {0}", IsHtml).AppendLine();
            debug.AppendFormat("\t, FromAddress: {0}", FromAddress).AppendLine();
            debug.AppendFormat("\t, FromDisplayName: {0}", FromDisplayName).AppendLine();
            debug.AppendFormat("\t, ReplyToAddress: {0}", ReplyToAddress).AppendLine();
            debug.AppendFormat("\t, ReplyToDisplayName: {0}", ReplyToDisplayName).AppendLine();
            debug.AppendFormat("\t, DateSentToQueue: {0}", DateSentToQueue).AppendLine();
            debug.AppendFormat("\t, SendAttempts: {0}", SendAttempts).AppendLine();

            debug.AppendFormat("\t, ToRecipients: {0}", GetRecipientsAsString(ToRecipients)).AppendLine();
            debug.AppendFormat("\t, CcRecipients: {0}", GetRecipientsAsString(CcRecipients)).AppendLine();
            debug.AppendFormat("\t, BccRecipients: {0}", GetRecipientsAsString(BccRecipients)).AppendLine();

            debug.AppendFormat("\t, Body: {0}", Body).AppendLine();
            debug.Append("]");

            return debug.ToString();
        }

        public string ToRecipientsToString()
        {
            return GetRecipientsAsString(ToRecipients);
        }

        public string CcRecipientsToString()
        {
            return GetRecipientsAsString(CcRecipients);
        }

        public string BccRecipientsToString()
        {
            return GetRecipientsAsString(BccRecipients);
        }

        private string GetRecipientsAsString(List<string> recipients)
        {
            if (recipients == null || recipients.Count == 0)
            {
                return "null";
            }

            return string.Join(";", recipients);
        }
    }

    public static class EmailMessageExtensions
    {
        public static MailMessage ToMailMessage(this EmailMessage emailMessage, string defaultFromAddress,
            string defaultFromAddressName)
        {
            var mailMessage = new MailMessage();

            // To addresses
            if (null != emailMessage.ToRecipients)
            {
                foreach (var address in emailMessage.ToRecipients)
                {
                    var addressesToAdd = address.Contains(";") ? address.Split(';') : new[] { address };

                    foreach (var addressToAdd in addressesToAdd)
                    {
                        if (!string.IsNullOrEmpty(addressToAdd))
                        {
                            var cleanedAddress = addressToAdd.Replace(",", ".").Trim().Replace(" ", "");
                            mailMessage.To.Add(cleanedAddress);
                        }
                    }
                }
            }

            // CC addresses
            if (null != emailMessage.CcRecipients)
            {
                foreach (var address in emailMessage.CcRecipients.Where(address => !string.IsNullOrEmpty(address)))
                {
                    //log.DebugFormat("Adding CC address: <{0}>", address);
                    mailMessage.CC.Add(address);
                }
            }

            // Bcc address
            if (null != emailMessage.BccRecipients)
            {
                foreach (var address in emailMessage.BccRecipients.Where(address => !string.IsNullOrEmpty(address)))
                {
                    //log.DebugFormat("Adding BCC address: <{0}>", address);
                    mailMessage.Bcc.Add(address);
                }
            }

            if (null != emailMessage.ExcelAttachment)
            {
                mailMessage.Attachments.Add(emailMessage.ExcelAttachment);
            }

            // from address
            if (!string.IsNullOrEmpty(emailMessage.FromAddress))
            {
                //log.DebugFormat("Adding FROM address: <{0}>, '{1}'", emailMessage.FromAddress, emailMessage.FromDisplayName);
                mailMessage.From = string.IsNullOrEmpty(emailMessage.FromDisplayName)
                    ? new MailAddress(emailMessage.FromAddress)
                    : new MailAddress(emailMessage.FromAddress, emailMessage.FromDisplayName);
            }
            else
            {
                mailMessage.From = new MailAddress(defaultFromAddress, defaultFromAddressName);
            }

            // reply-to address
            if (!string.IsNullOrEmpty(emailMessage.ReplyToAddress))
            {
                //log.DebugFormat("Adding REPLY-TO address: <{0}>, '{1}'", emailMessage.ReplyToAddress, emailMessage.ReplyToDisplayName);
                mailMessage.ReplyToList.Add(string.IsNullOrEmpty(emailMessage.ReplyToDisplayName)
                    ? new MailAddress(emailMessage.ReplyToAddress)
                    : new MailAddress(emailMessage.ReplyToAddress, emailMessage.ReplyToDisplayName));
            }

            mailMessage.Subject = emailMessage.Subject;
            mailMessage.Body = emailMessage.Body;
            mailMessage.IsBodyHtml = emailMessage.IsHtml;

            return mailMessage;
        }
    }
}