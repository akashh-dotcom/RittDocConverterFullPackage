#region

using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.Email
{
    public class EmailBuildBaseService : EmailTemplates
    {
        private readonly IContentSettings _contentSettings;
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<EmailBuildBaseService> _log;

        public EmailBuildBaseService(ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        )
        {
            _log = log;
            _emailSettings = emailSettings;
            _contentSettings = contentSettings;
        }

        public int? DebugMaxEmailsSent { get; set; }

        protected string MainTemplate { get; set; }
        protected string BodyTemplate { get; set; }
        protected string BodySubTemplate { get; set; }
        protected string ItemTemplate { get; set; }
        protected string SubItemTemplate { get; set; }
        protected string UnsubscribeTemplate { get; set; }

        public EmailMessage EmailMessage { get; set; }

        protected string GetTemplateFromFile(string fileName)
        {
            var emailTemplateDirectory = _emailSettings.TemplatesDirectory;
            string directoryName;
            if (emailTemplateDirectory.Contains("\\"))
            {
                directoryName = emailTemplateDirectory.Substring(emailTemplateDirectory.Length - 1) == "\\"
                    ? emailTemplateDirectory
                    : $"{emailTemplateDirectory}\\";
            }
            else
            {
                directoryName = emailTemplateDirectory.Substring(emailTemplateDirectory.Length - 1) == "/"
                    ? emailTemplateDirectory
                    : $"{emailTemplateDirectory}/";
            }

            var fullPathFile = $"{directoryName}{fileName}";
            //_log.DebugFormat("fullPathFile: {0}", fullPathFile);

            // check if the file exists
            var fileInfo = new FileInfo(fullPathFile);
            if (!fileInfo.Exists)
            {
                _log.ErrorFormat("Template file does not exist, '{0}'", fullPathFile);
                return string.Empty;
            }

            //_log.DebugFormat("fullPathFile: {0}", fullPathFile);
            return File.ReadAllText(fullPathFile);
        }

        protected string GetTemplateFromFile(string fileName, string folderName)
        {
            var emailTemplateDirectory = _emailSettings.TemplatesDirectory;
            string directoryName;
            if (emailTemplateDirectory.Contains("\\"))
            {
                directoryName = emailTemplateDirectory.Substring(emailTemplateDirectory.Length - 1) == "\\"
                    ? emailTemplateDirectory
                    : $"{emailTemplateDirectory}\\";
                directoryName = $@"{directoryName}\{folderName}\";
            }
            else
            {
                directoryName = emailTemplateDirectory.Substring(emailTemplateDirectory.Length - 1) == "/"
                    ? emailTemplateDirectory
                    : $"{emailTemplateDirectory}/";
                directoryName = $"{directoryName}/{folderName}";
            }

            var fullPathFile = $"{directoryName}{fileName}";
            _log.DebugFormat("fullPathFile: {0}", fullPathFile);

            // check if the file exists
            var fileInfo = new FileInfo(fullPathFile);
            if (!fileInfo.Exists)
            {
                _log.ErrorFormat("Template file does not exist, '{0}'", fullPathFile);
                return string.Empty;
            }

            _log.DebugFormat("fullPathFile: {0}", fullPathFile);
            return File.ReadAllText(fullPathFile);
        }

        protected string GetWebSiteBaseUrl()
        {
            return $"{_emailSettings.WebSiteBaseUrl}{(_emailSettings.WebSiteBaseUrl.EndsWith("/") ? "" : "/")}";
        }

        protected string GetResourceLink(string isbn, string accountNumber)
        {
            //http://www.r2library.com/Authentication/Title/?Query=1284029808&AccountNumber=XXXXXX
            return $"{GetWebSiteBaseUrl()}Authentication/Title/?Query={isbn}&AccountNumber={accountNumber}";
        }

        protected string GetAdminResourceLink(int resourceId)
        {
            return $"{GetWebSiteBaseUrl()}Admin/Resource/Detail/{resourceId}";
        }

        protected string GetCartLink(int institutionId)
        {
            return $"{GetWebSiteBaseUrl()}Admin/Cart/ShoppingCart/{institutionId}";
        }

        protected string GetSpecificCartLink(int institutionId, int cartId)
        {
            return $"{GetWebSiteBaseUrl()}Admin/Cart/ShoppingCart/{institutionId}?cartId={cartId}";
        }

        //?cartId=18186
        protected string GetPurchaseBooksLink(int institutionId)
        {
            return $"{GetWebSiteBaseUrl()}Admin/CollectionManagement/List/{institutionId}";
        }

        protected string GetPdaHistoryLink(int institutionId)
        {
            return
                $"{GetWebSiteBaseUrl()}Admin/CollectionManagement/List/{institutionId}?IncludePdaResources=True&IncludePdaHistory=True";
        }

        protected string GetPdaLink(int institutionId)
        {
            return $"{GetWebSiteBaseUrl()}Admin/CollectionManagement/List/{institutionId}?IncludePdaResources=True";
        }

        protected string GetResourceImageUrl(string imageFileName)
        {
            return $"{_contentSettings.BookCoverUrl}/{imageFileName}";
        }

        protected string GetDashboardLink(int institutionId)
        {
            return $"{GetWebSiteBaseUrl()}Admin/Dashboard/Index/{institutionId}";
        }

        protected string GetContactLink()
        {
            return $"{GetWebSiteBaseUrl()}Contact";
        }

        protected string GetDisciplineLink(int institutionId, int specialtyId)
        {
            return
                $"{GetWebSiteBaseUrl()}Admin/CollectionManagement/List/{institutionId}?SpecialtyFilter={specialtyId}";
        }

        protected string GetProfileLink()
        {
            return $"{GetWebSiteBaseUrl()}Profile";
        }

        protected static string PopulateField(string label, string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? $"<strong>{label}</strong>N/A"
                : $"<strong>{label}</strong>{value}";
        }

        protected void LogMessage(EmailMessage emailMessage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_emailSettings.OutputPath))
                {
                    return;
                }

                DirectoryHelper.VerifyDirectory(_emailSettings.OutputPath);
                var fileName = $"{DateTime.Now:yyyyMMdd-HHmmssfff}_{CleanFileName(emailMessage.Subject)}.htm";
                var fullPath = Path.Combine(_emailSettings.OutputPath, fileName);
                _log.DebugFormat("fullPath: {0}", fullPath);
                var msg = new StringBuilder();
                msg.AppendLine("<!--");
                msg.AppendFormat("To: {0}", emailMessage.ToRecipientsToString()).AppendLine();
                msg.AppendFormat("Cc: {0}", emailMessage.CcRecipientsToString()).AppendLine();
                msg.AppendFormat("Bcc: {0}", emailMessage.BccRecipientsToString()).AppendLine();
                msg.AppendFormat("Subject: {0}", emailMessage.Subject).AppendLine();
                msg.AppendFormat("From: {0}", emailMessage.FromAddress).AppendLine();
                msg.AppendLine("-->");
                msg.Append(emailMessage.Body);

                using (var file = new StreamWriter(fullPath))
                {
                    file.Write(msg.ToString());
                    file.Close();
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

        protected void SetTemplates(string bodyTemplate, bool includeUnsubscribe = true)
        {
            if (includeUnsubscribe)
            {
                SetUnsubscribeTemplate();
            }

            MainTemplate = GetTemplateFromFile(MainHeaderFooterTemplate);
            BodyTemplate = includeUnsubscribe
                ? GetTemplateFromFile(bodyTemplate).Replace("{Unsubscribe}", UnsubscribeTemplate)
                : GetTemplateFromFile(bodyTemplate);
        }

        protected void SetTemplates(string bodyTemplate, string itemTemplate, bool includeUnsubscribe = true,
            string subItemTemplate = null)
        {
            SetTemplates(MainHeaderFooterTemplate, bodyTemplate, itemTemplate, includeUnsubscribe, subItemTemplate);
        }


        protected void SetTemplates(string mainTemplate, string bodyTemplate, string bodySubTemplate,
            string itemTemplate, bool includeUnsubscribe = true, string subItemTemplate = null)
        {
            BodySubTemplate = GetTemplateFromFile(bodySubTemplate);
            SetTemplates(MainHeaderFooterTemplate, bodyTemplate, itemTemplate, includeUnsubscribe, subItemTemplate);
        }


        protected void SetTemplates(string mainTemplate, string bodyTemplate, string itemTemplate,
            bool includeUnsubscribe = true, string subItemTemplate = null)
        {
            if (includeUnsubscribe)
            {
                SetUnsubscribeTemplate();
            }

            MainTemplate = GetTemplateFromFile(mainTemplate);
            BodyTemplate = includeUnsubscribe
                ? GetTemplateFromFile(bodyTemplate).Replace("{Unsubscribe}", UnsubscribeTemplate)
                : GetTemplateFromFile(bodyTemplate);
            ItemTemplate = GetTemplateFromFile(itemTemplate);

            if (!string.IsNullOrWhiteSpace(subItemTemplate))
            {
                SubItemTemplate = GetTemplateFromFile(subItemTemplate);
            }
        }

        private void SetUnsubscribeTemplate()
        {
            UnsubscribeTemplate = GetTemplateFromFile("Unsubscribe.html").Replace("{Profile_Link}", GetProfileLink());
        }

        protected string BuildMainHtml(string title, string bodyHtml, User user)
        {
            return MainTemplate.Replace("{Title}", title)
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                .Replace("{User_Email}", user == null ? "" : user.Email)
                .Replace("{Institution_Name}(#{Institution_Number}) - ",
                    user == null
                        ? ""
                        : $"{user.Institution.Name}(#{user.Institution.AccountNumber}) - ")
                .Replace("{Body}", bodyHtml);
        }

        protected string BuildMainHtml(string title, string bodyHtml, string email, IInstitution institution)
        {
            return MainTemplate.Replace("{Title}", title)
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                .Replace("{User_Email}", email)
                .Replace("{Institution_Name}", institution != null ? institution.Name : "")
                .Replace("{Institution_Number}", institution != null ? institution.AccountNumber : "")
                .Replace("{Body}", bodyHtml);
        }

        protected string BuildMainHtml(string title, string bodyHtml, string email,
            AuthenticatedInstitution institution)
        {
            return MainTemplate.Replace("{Title}", title)
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                .Replace("{User_Email}", email)
                .Replace("{Institution_Name}", institution != null ? institution.Name : "")
                .Replace("{Institution_Number}", institution != null ? institution.AccountNumber : "")
                .Replace("{Body}", bodyHtml);
        }

        protected string BuildBodyHtml()
        {
            return BodyTemplate;
        }

        protected string BuildBodyHtml(string stringToReplace, string itemsHtml)
        {
            var bodyHtml = new StringBuilder()
                .Append(BodyTemplate.Replace(stringToReplace, itemsHtml))
                .ToString();
            return bodyHtml;
        }

        protected string GetResourceIcons(IResource resource, string specialIconUrl = null)
        {
            var sb = new StringBuilder();

            if (resource.CollectionIdsToArray().Contains((int)CollectionIdentifier.BradonHill))
            {
                sb.Append(GetIconBrandonHill());
            }

            if (resource.CollectionIdsToArray().Contains((int)CollectionIdentifier.Dct))
            {
                sb.Append(GetIconDoody());
            }
            else if (resource.CollectionIdsToArray().Contains((int)CollectionIdentifier.DctEssential))
            {
                sb.Append(GetIconDoodyEssential());
            }

            if (!string.IsNullOrWhiteSpace(specialIconUrl))
            {
                sb.Append(GetIconResourceSpecial(specialIconUrl));
            }

            return sb.ToString();
        }

        private string GetIconDoody()
        {
            return "<br/>".Append(GetTemplateFromFile("Icon_Doody.html"));
        }

        private string GetIconDoodyEssential()
        {
            return "<br/>".Append(GetTemplateFromFile("Icon_Doody_Essential.html"));
        }

        private string GetIconBrandonHill()
        {
            return "<br/>".Append(GetTemplateFromFile("Icon_Brandon_Hill.html"));
        }

        private string GetIconResourceSpecial(string iconUrl)
        {
            return "<br/>".Append(GetTemplateFromFile("Icon_Special_Resource.html")
                .Replace("{Special_IconName_Url}", iconUrl));
        }

        protected EmailMessage BuildEmailMessage(IUser user, string subject, string messageHtml)
        {
            return BuildEmailMessage(user.Email, null, subject, messageHtml, null);
        }

        protected EmailMessage BuildEmailMessage(User user, string[] ccAddresses, string subject, string messageHtml)
        {
            return BuildEmailMessage(user.Email, ccAddresses, subject, messageHtml, null);
        }

        protected EmailMessage BuildEmailMessage(string userEmail, string[] ccAddresses, string subject,
            string messageHtml, Attachment attachment)
        {
            try
            {
                SetEmailMessage(subject, messageHtml);

                EmailMessage.AddToRecipient(userEmail);

                if (ccAddresses != null)
                {
                    foreach (var address in ccAddresses)
                    {
                        if (!EmailMessage.AddCcRecipient(address))
                        {
                            _log.ErrorFormat("invalid CC email address <{0}>", address);
                        }
                    }
                }

                if (attachment != null)
                {
                    EmailMessage.ExcelAttachment = attachment;
                }

                LogMessage(EmailMessage);
                return EmailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        protected EmailMessage BuildEmailMessage(string[] emailAddresses, string subject, string messageHtml)
        {
            try
            {
                SetEmailMessage(subject, messageHtml);

                foreach (var email in emailAddresses)
                {
                    EmailMessage.AddToRecipient(email);
                }

                LogMessage(EmailMessage);
                return EmailMessage;
            }
            catch (Exception ex)
            {
                // swallow exception to prevent bad code from affecting user experience
                _log.Error(ex.Message, ex);
            }

            return null;
        }


        //Attachment

        protected void SetEmailMessage(string emailSubject, string messageBody)
        {
            EmailMessage = new EmailMessage
            {
                Subject =
                    $"{(_emailSettings.AddEnvironmentPrefixToSubject ? $"{Environment.MachineName} - " : "")}{emailSubject}",
                FromDisplayName = _emailSettings.DefaultFromName,
                FromAddress = _emailSettings.DefaultFromAddress,
                ReplyToAddress = _emailSettings.DefaultReplyToAddress,
                ReplyToDisplayName = _emailSettings.DefaultReplyToName,
                IsHtml = true,
                Body = messageBody
            };
        }
    }
}