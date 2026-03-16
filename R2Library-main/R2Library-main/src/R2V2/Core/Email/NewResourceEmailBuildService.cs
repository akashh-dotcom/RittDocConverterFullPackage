#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class NewResourceEmailBuildService : ResourceEmailBuildBaseService
    {
        public NewResourceEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings)
            : base(log, emailSettings, contentSettings)
        {
            SetTemplates(NewResourceBodyTemplate, NewResourceItemTemplate);
        }

        private string NewResourceItemsHtml { get; set; }

        public EmailMessage BuildNewResourceEmail(User user)
        {
            var messageBody = GetNewResourceEmailHtml(user);

            return BuildEmailMessage(user, "R2 Library New Titles Available", messageBody);
        }

        public void SetNewResourceItemHtml(IEnumerable<IResource> newResources, string accountNumber)
        {
            var itemBuilder = new StringBuilder();

            string lastSpecialtyName = null;
            foreach (var resource in newResources)
            {
                var specialty = resource.Specialties != null
                    ? resource.Specialties.OrderBy(x => x.Name).FirstOrDefault()
                    : null;

                if (specialty != null)
                {
                    if (lastSpecialtyName != specialty.Name)
                    {
                        itemBuilder.Append(BuildSpecialtyHeader(resource, specialty, accountNumber)
                            .Replace("{Resource_RetailPrice}", resource.ListPriceString()));

                        lastSpecialtyName = specialty.Name;

                        continue;
                    }
                }

                itemBuilder.Append(BuildItemHtml(resource, accountNumber)
                    .Replace("{Resource_RetailPrice}", resource.ListPriceString()));
            }

            NewResourceItemsHtml = itemBuilder.ToString();
        }

        public string GetNewResourceEmailHtml(User user)
        {
            //Need to Repleace the REPLACE with AccountNumber
            var itemsHtml = NewResourceItemsHtml.Replace("REPLACE", user.Institution.AccountNumber);


            var bodyBuilder = BuildBodyHtml(itemsHtml);

            var mainBuilder = BuildMainHtml("New Titles Available", bodyBuilder, user);

            return mainBuilder;
        }
    }
}