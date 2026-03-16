#region

using System;
using R2V2.Core.Authentication;
using R2V2.Core.AutomatedCart;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class AutomatedCartInstitutionEmailBuildService : EmailBuildBaseService
    {
        //AutomatedCartBodyTemplate
        public AutomatedCartInstitutionEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            SetTemplates(AutomatedCartInstitutionBodyTemplate);
        }

        public EmailMessage BuildAutomatedCartEmail(DbAutomatedCart automatedCart, int cartId, User user)
        {
            var messageBody = GetAutomatedCartHtml(automatedCart, cartId, user);

            return BuildEmailMessage(user, automatedCart.EmailSubject, messageBody);
        }

        private string GetAutomatedCartHtml(DbAutomatedCart automatedCart, int cartId, User user)
        {
            var bodyBuilder = BuildBodyHtml()
                .Replace("{AutomatedCart_Text}", automatedCart.EmailText)
                .Replace("{AutomatedCart_Name}", automatedCart.CartName)
                .Replace("{CartUrl}", GetSpecificCartLink(user.InstitutionId.GetValueOrDefault(), cartId));

            var mainBuilder = BuildMainHtml(automatedCart.EmailTitle, bodyBuilder, user);

            return mainBuilder;
        }

        public string GetAutomatedCartExampleHtml(string cartName, string emailTitle, string emailText)
        {
            var bodyBuilder = BuildBodyHtml()
                .Replace("{AutomatedCart_Text}", emailText)
                .Replace("{AutomatedCart_Name}", cartName)
                .Replace("{CartUrl}", GetCartLink(1));

            return MainTemplate
                .Replace("{Title}", emailTitle)
                .Replace("{Year}", DateTime.Now.Year.ToString())
                .Replace("{WebsiteUrl}", GetWebSiteBaseUrl())
                .Replace("{User_Email}", "")
                .Replace("{Institution_Name}(#{Institution_Number}) - ", "")
                .Replace("{Body}", bodyBuilder);
        }
    }
}