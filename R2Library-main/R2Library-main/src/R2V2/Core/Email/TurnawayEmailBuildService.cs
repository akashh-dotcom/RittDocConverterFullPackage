#region

using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class TurnawayEmailBuildService : ResourceEmailBuildBaseService
    {
        public TurnawayEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            SetTemplates(TurnawayBodyTemplate, TurnawayItemTemplate);
        }

        /// <param name="itemHtml"> </param>
        public EmailMessage BuildTurnawayEmail(User user, string itemHtml)
        {
            var messageBody = GetTurnawayEmailHtml(user, itemHtml);

            return BuildEmailMessage(user, "R2 Library Turnaways in the Last Day", messageBody);
        }

        /// <param name="itemHtml"> </param>
        private string GetTurnawayEmailHtml(User user, string itemHtml)
        {
            var bodyHtml = BuildBodyHtml(itemHtml);

            var mainHtml = BuildMainHtml("Turnaways in the Last Day", bodyHtml, user);

            return mainHtml;
        }

        public string BuildItemHtml(IResource resource, string turnawayString, int institutionId, string accountNumber)
        {
            return BuildItemHtml(resource, accountNumber)
                .Replace("{Resource_Turnaway}", turnawayString)
                .Replace("{PurchaseResource_Url}", $"{GetPurchaseBooksLink(institutionId)}?Query={resource.Isbn}");
        }

        public new string BuildSpecialtyHeader(IResource resource, ISpecialty specialty)
        {
            var specialtyString = GetTemplateFromFile("Resource_Specialty.html")
                .Replace("{Specialty_Header}", specialty.Name);

            return new StringBuilder()
                .Append(specialtyString)
                .ToString();
        }
    }
}