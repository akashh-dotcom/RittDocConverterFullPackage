#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class ResourceLockEmailBuildService : EmailBuildBaseService
    {
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailBuildBaseService> _log;

        public ResourceLockEmailBuildService(ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            _log = log;
            _emailSettings = emailSettings;
        }

        public string BuildResourcePrintLockEmailItems(ResourceLockData lockData)
        {
            var itemTemplate = GetTemplateFromFile("ResourcePrintLock_Item.html");

            var items = new StringBuilder();

            foreach (var userData in lockData.UserData)
            {
                var item = itemTemplate.Replace("{UserFullName}", userData.UserFullName)
                    .Replace("{UserEmailAddress}", userData.EmailAddress)
                    .Replace("{UserIpAddress}", userData.IpAddress)
                    .Replace("{UserNumberOfPrintRequests}", userData.RequestCount.ToString())
                    .Replace("{UserSessionStartTime}", $"{userData.SessionStartTime:G}")
                    .Replace("{UserFirstPrintRequestTime}", $"{userData.FirstRequesTimestamp:G}")
                    .Replace("{UserLastPrintRequestTime}", $"{userData.LastRequesTimestamp:G}");
                items.AppendLine(item);
            }

            return items.ToString();
        }

        public string BuildResourceEmailLockEmailItems(ResourceLockData lockData,
            List<EmailAddressCount> emailAddressCounts)
        {
            var itemTemplate = GetTemplateFromFile("ResourceEmailLock_Item.html");

            var items = new StringBuilder();

            foreach (var userData in lockData.UserData)
            {
                var userInfo = string.IsNullOrEmpty(userData.UserFullName)
                    ? "N/A"
                    : string.IsNullOrEmpty(userData.EmailAddress)
                        ? userData.UserFullName
                        : $"{userData.UserFullName}<br/>{userData.EmailAddress}";

                var sessionEmailAddressCounts =
                    emailAddressCounts.Where(x => x.SessionId == userData.SessionId).ToList();

                var emailAddresses = new StringBuilder();
                foreach (var emailAddressCount in sessionEmailAddressCounts)
                {
                    emailAddresses.AppendFormat("{0}{1} [{2}]", emailAddresses.Length > 0 ? "<br/>" : string.Empty,
                        emailAddressCount.EmailAddress, emailAddressCount.Count);
                }

                var item = itemTemplate.Replace("{UserData}", userInfo)
                    .Replace("{EmailAddresses}", emailAddresses.ToString())
                    .Replace("{UserIpAddress}", userData.IpAddress)
                    .Replace("{UserNumberOfEmailRequests}", userData.RequestCount.ToString())
                    .Replace("{UserSessionStartTime}", $"{userData.SessionStartTime:G}")
                    .Replace("{UserFirstEmailRequestTime}", $"{userData.FirstRequesTimestamp:G}")
                    .Replace("{UserLastEmailRequestTime}", $"{userData.LastRequesTimestamp:G}");
                items.AppendLine(item);
            }

            return items.ToString();
        }

        public EmailMessage BuildResourcePrintLockEmail(User user, IResource resource, string items)
        {
            try
            {
                var mainTemplate = GetTemplateFromFile("Main_Header_Footer.html");
                var bodyTemplate = GetTemplateFromFile("ResourcePrintLock_Body.html");

                var messageBody = bodyTemplate.Replace("{Greeting}", $"Dear {user.FirstName} {user.LastName},");

                return BuildResourcePrintLockEmailBody(user, resource, items, messageBody, mainTemplate);
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        public EmailMessage BuildResourceEmailLockEmail(User user, IResource resource, string items)
        {
            try
            {
                var mainTemplate = GetTemplateFromFile("Main_Header_Footer.html");
                var bodyTemplate = GetTemplateFromFile("ResourceEmailLock_Body.html");

                var messageBody = bodyTemplate.Replace("{Greeting}", $"Dear {user.FirstName} {user.LastName},");

                return BuildResourceEmailLockEmailBody(user, resource, items, messageBody, mainTemplate);
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        public EmailMessage BuildResourcePrintLockInternalEmail(IResource resource, string items, List<User> adminUsers,
            string internalAddresses)
        {
            try
            {
                var mainTemplate = GetTemplateFromFile("Main_Header_Footer.html");
                var bodyTemplate = GetTemplateFromFile("ResourcePrintLock_Body.html");

                var greeting = new StringBuilder();
                greeting.Append("This message was sent to the following Admin Users:<br/>").AppendLine();

                foreach (var adminUser in adminUsers)
                {
                    greeting.AppendFormat("- {0} {1}, [{2}]<br/>", adminUser.FirstName, adminUser.LastName,
                        adminUser.Email).AppendLine();
                }

                var messageBody = bodyTemplate.Replace("{Greeting}", greeting.ToString());

                var emailMessage = BuildResourcePrintLockEmailBody(adminUsers.FirstOrDefault(), resource, items,
                    messageBody, mainTemplate);
                emailMessage.ToRecipients.Clear();
                emailMessage.AddToRecipients(internalAddresses, ';');
                return emailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        public EmailMessage BuildResourceEmailLockInternalEmail(IResource resource, string items, List<User> adminUsers,
            string internalAddresses)
        {
            try
            {
                var mainTemplate = GetTemplateFromFile("Main_Header_Footer.html");
                var bodyTemplate = GetTemplateFromFile("ResourceEmailLock_Body.html");

                var greeting = new StringBuilder();
                greeting.Append("This message was sent to the following Admin Users<br/>").AppendLine();

                foreach (var adminUser in adminUsers)
                {
                    greeting.AppendFormat(" + {0} {1}, [{2}]<br/>", adminUser.FirstName, adminUser.LastName,
                        adminUser.Email).AppendLine();
                }

                var messageBody = bodyTemplate.Replace("{Greeting}", greeting.ToString());
                var emailMessage = BuildResourceEmailLockEmailBody(adminUsers.FirstOrDefault(), resource, items,
                    messageBody, mainTemplate);
                emailMessage.ToRecipients.Clear();
                emailMessage.AddToRecipients(internalAddresses, ';');
                return emailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        private EmailMessage BuildResourcePrintLockEmailBody(User user, IResource resource, string items,
            string bodyTemplate, string mainTemplate)
        {
            try
            {
                var messageBody = bodyTemplate.Replace("{ResourceTitle}", resource.Title)
                    .Replace("{ResourceIsbn10}", resource.Isbn10)
                    .Replace("{ResourceIsbn13}", resource.Isbn13)
                    .Replace("{FrontendResourceUrl}", GetResourceLink(resource.Isbn10, user.Institution.AccountNumber))
                    .Replace("{BackendResourceUrl}", GetAdminResourceLink(resource.Id));

                messageBody = messageBody.Replace("{ResourcePrintLock_Items}", items);

                messageBody = mainTemplate.Replace("{Title}",
                        "Printing Disabled For One Of Your R2 Digital Library Resources.")
                    .Replace("{Year}", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture))
                    .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                    .Replace("{User_Email}", user.Email)
                    .Replace("{Institution_Name}", user.Institution.Name)
                    .Replace("{Institution_Number}", user.Institution.AccountNumber)
                    .Replace("{Body}", messageBody);

                var emailMessage = new EmailMessage
                {
                    Subject = $"R2Library.com - Printing Disabled for Resource: {resource.Title}",
                    FromDisplayName = _emailSettings.DefaultFromName,
                    FromAddress = _emailSettings.DefaultFromAddress,
                    ReplyToAddress = _emailSettings.DefaultReplyToAddress,
                    ReplyToDisplayName = _emailSettings.DefaultReplyToName,
                    IsHtml = true,
                    Body = messageBody
                };

                var toEmailAddress = user.Email;
                if (!_emailSettings.SendToCustomers)
                {
                    _log.WarnFormat("User email address overwritten to {0} from {1}", _emailSettings.TestEmailAddresses,
                        toEmailAddress);
                    toEmailAddress = _emailSettings.TestEmailAddresses;
                }

                if (!emailMessage.AddToRecipient(toEmailAddress))
                {
                    _log.ErrorFormat("invalid TO email address <{0}>", user.Email);
                }

                // log invalid email addresses
                foreach (var invalidEmailAddress in emailMessage.InvalidEmailAddresses)
                {
                    _log.ErrorFormat("invalid email address <{0}>", invalidEmailAddress);
                }

                LogMessage(emailMessage);
                return emailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        private EmailMessage BuildResourceEmailLockEmailBody(User user, IResource resource, string items,
            string bodyTemplate, string mainTemplate)
        {
            try
            {
                var messageBody = bodyTemplate.Replace("{ResourceTitle}", resource.Title)
                    .Replace("{ResourceIsbn10}", resource.Isbn10)
                    .Replace("{ResourceIsbn13}", resource.Isbn13)
                    .Replace("{FrontendResourceUrl}", GetResourceLink(resource.Isbn10, user.Institution.AccountNumber))
                    .Replace("{BackendResourceUrl}", GetAdminResourceLink(resource.Id));

                messageBody = messageBody.Replace("{ResourceEmailLock_Items}", items);

                messageBody = mainTemplate.Replace("{Title}",
                        "Email Is Disabled For One Of Your R2 Digital Library Resources.")
                    .Replace("{Year}", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture))
                    .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                    .Replace("{User_Email}", user.Email)
                    .Replace("{Institution_Name}", user.Institution.Name)
                    .Replace("{Institution_Number}", user.Institution.AccountNumber)
                    .Replace("{Body}", messageBody);

                var emailMessage = new EmailMessage
                {
                    Subject = $"R2Library.com - Email Is Disabled for Resource: {resource.Title}",
                    FromDisplayName = _emailSettings.DefaultFromName,
                    FromAddress = _emailSettings.DefaultFromAddress,
                    ReplyToAddress = _emailSettings.DefaultReplyToAddress,
                    ReplyToDisplayName = _emailSettings.DefaultReplyToName,
                    IsHtml = true,
                    Body = messageBody
                };

                var toEmailAddress = user.Email;
                if (!_emailSettings.SendToCustomers)
                {
                    _log.WarnFormat("User email address overwritten to {0} from {1}", _emailSettings.TestEmailAddresses,
                        toEmailAddress);
                    toEmailAddress = _emailSettings.TestEmailAddresses;
                }

                if (!emailMessage.AddToRecipient(toEmailAddress))
                {
                    _log.ErrorFormat("invalid TO email address <{0}>", user.Email);
                }

                // log invalid email addresses
                foreach (var invalidEmailAddress in emailMessage.InvalidEmailAddresses)
                {
                    _log.ErrorFormat("invalid email address <{0}>", invalidEmailAddress);
                }

                LogMessage(emailMessage);
                return emailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }
    }
}