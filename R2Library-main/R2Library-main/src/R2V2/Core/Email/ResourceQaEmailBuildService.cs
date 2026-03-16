#region

using System;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class ResourceQaEmailBuildService : ResourceEmailBuildBaseService
    {
        public ResourceQaEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        public EmailMessage BuildResourceQaEmail(IUser user, IResource resource, AuthenticatedInstitution institution)
        {
            SetTemplates(ResourceQaEmailBodyTemplate, false);
            var subject =
                $"Resource Ready for Promotion - {(resource.QaApprovalDate == null ? DateTime.Now : resource.QaApprovalDate.Value)}";
            var messageBody = GetResourceQaEmailHtml(user, resource, institution, subject);

            return BuildEmailMessage(user, subject, messageBody);
        }

        public string GetResourceQaEmailHtml(IUser user, IResource resource, AuthenticatedInstitution institution,
            string subject)
        {
            var bodyBuilder = BuildBodyHtml().Replace("{Resource_Title}", resource.Title)
                .Replace("{Resource_ISBN10}", resource.Isbn10)
                .Replace("{Resource_ISBN13}", resource.Isbn13)
                .Replace("{Resource_EISBN}", resource.EIsbn)
                .Replace("{Resource_Link}", GetResourceLink(resource.Isbn, institution.AccountNumber))
                .Replace("{Resource_AdminLink}", GetAdminResourceLink(resource.Id))
                .Replace("{User_Name}", $"{user.LastName}, {user.FirstName}")
                .Replace("{User_Email}", user.Email);

            var mainBuilder = BuildMainHtml(subject, bodyBuilder, user.Email, institution);

            return mainBuilder;
        }
    }
}