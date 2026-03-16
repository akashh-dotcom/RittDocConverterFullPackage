#region

using System;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class ExpertReviewerRequestEmailBuildService : EmailBuildBaseService
    {
        public ExpertReviewerRequestEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        public string BuildRequestEmailBody(IUser user, AuthenticatedInstitution institution)
        {
            SetTemplates(FacultyRequestBodyTemplate);
            return GetRequestEmailHtml(user, institution);
        }


        public string GetRequestEmailHtml(IUser user, AuthenticatedInstitution institution)
        {
            var bodyBuilder = BuildBodyHtml().Replace("{User_FirstName}", user.FirstName)
                .Replace("{User_LastName}", user.LastName)
                .Replace("{User_UserName}", user.UserName)
                .Replace("{User_FacultyDateRequest}", DateTime.Now.ToShortDateString());

            var mainBuilder = BuildMainHtml("Expert Reviewer User Request", bodyBuilder, user.Email, institution);

            return mainBuilder;
        }
    }
}