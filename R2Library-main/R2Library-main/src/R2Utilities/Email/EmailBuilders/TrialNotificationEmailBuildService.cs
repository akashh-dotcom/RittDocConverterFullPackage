#region

using R2Utilities.DataAccess;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Email.EmailBuilders
{
    public class TrialNotificationEmailBuildService : EmailBuildBaseService
    {
        public TrialNotificationEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        private string Title { get; set; }

        public void InitEmailTemplates(TrialNotice trialNotice)
        {
            var bodyTemplate = "";
            switch (trialNotice)
            {
                case TrialNotice.First:
                    bodyTemplate = TrialFirstNoticeBodyTemplate;
                    break;
                case TrialNotice.Second:
                    bodyTemplate = TrialSecondNoticeBodyTemplate;
                    break;
                case TrialNotice.Final:
                    bodyTemplate = TrialFinalNoticeBodyTemplate;
                    break;
                case TrialNotice.Extension:
                    bodyTemplate = TrialExtensionNoticeBodyTemplate;
                    break;
            }

            SetTemplates(bodyTemplate, false);
            Title = trialNotice.ToTitle();
        }


        public R2V2.Infrastructure.Email.EmailMessage BuildTrialNotificationEmail(User user)
        {
            var messageBody = GetTrialNotificationEmailHtml(user);

            return BuildEmailMessage(user, $"R2 Library {Title}", messageBody);
        }

        private string GetTrialNotificationEmailHtml(User user)
        {
            var bodyBuilder = BuildBodyHtml();

            var mainBuilder = BuildMainHtml(Title, bodyBuilder, user);
            return mainBuilder;
        }
    }
}