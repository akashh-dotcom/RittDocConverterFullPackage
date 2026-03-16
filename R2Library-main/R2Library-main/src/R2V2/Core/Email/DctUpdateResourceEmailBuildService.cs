#region

using System.Collections.Generic;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class DctUpdateResourceEmailBuildService : ResourceEmailBuildBaseService
    {
        public DctUpdateResourceEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        public void SetTemplates()
        {
            SetTemplates(DctUpdateBodyTemplate, DctUpdateItemTemplate);
        }

        public EmailMessage BuildDctUpdateEmail(IEnumerable<IResource> resources, User user, string practiceArea)
        {
            //R2 Library: <DctType/> DCT and DCT Essentials Update
            var subject = $"R2Library: {practiceArea} DCT and DCT Essentials Update";
            var messageBody = GetNewEditionResourceEmailHtml(resources, user, subject);

            return BuildEmailMessage(user, subject, messageBody);
        }

        public string GetNewEditionResourceEmailHtml(IEnumerable<IResource> resources, User user, string subject)
        {
            var itemBuilder = new StringBuilder();

            foreach (var resource in resources)
            {
                var text = BuildItemHtml(resource, user.Institution.AccountNumber);
                text = text.Replace("{Resource_RetailPrice}", resource.ListPriceString(true))
                    .Replace("{Resource_DiscountPrice}",
                        resource.DiscountPriceString(resource.ListPrice -
                                                     user.Institution.Discount / 100 * resource.ListPrice));

                itemBuilder.Append(text);
            }

            var bodyBuilder = BuildBodyHtml(itemBuilder.ToString());

            var mainBuilder = BuildMainHtml(subject, bodyBuilder, user);

            return mainBuilder;
        }
    }
}