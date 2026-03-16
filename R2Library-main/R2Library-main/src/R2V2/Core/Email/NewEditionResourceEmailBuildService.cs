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
    public class NewEditionResourceEmailBuildService : ResourceEmailBuildBaseService
    {
        private bool _isPdaEdition;

        public NewEditionResourceEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        public void SetTemplates(bool isPdaEdition)
        {
            _isPdaEdition = isPdaEdition;
            SetTemplates(_isPdaEdition ? NewEditionPdaBodyTemplate : NewEditionBodyTemplate,
                NewEditionItemTemplate);
        }

        public EmailMessage BuildNewEditionResourceEmail(IEnumerable<IResource> resources, User user)
        {
            var messageBody = GetNewEditionResourceEmailHtml(resources, user);
            var subject = _isPdaEdition
                ? "R2 Library New PDA Editions Now Available"
                : "R2 Library New Editions Now Available";
            return BuildEmailMessage(user, subject, messageBody);
        }

        public string GetNewEditionResourceEmailHtml(IEnumerable<IResource> resources, User user)
        {
            var itemBuilder = new StringBuilder();

            string lastSpecialtyName = null;

            foreach (var resource in resources)
            {
                var specialty = resource.Specialties != null
                    ? resource.Specialties.OrderBy(x => x.Name).FirstOrDefault()
                    : null;

                if (specialty != null)
                {
                    if (lastSpecialtyName != specialty.Name)
                    {
                        itemBuilder.Append(BuildSpecialtyHeader(resource, specialty));

                        lastSpecialtyName = specialty.Name;
                    }
                }

                var text = BuildItemHtml(resource, user.Institution.AccountNumber);

                text = text.Replace("{Resource_RetailPrice}", resource.ListPriceString(true))
                    .Replace("{Resource_DiscountPrice}",
                        resource.DiscountPriceString(resource.ListPrice -
                                                     user.Institution.Discount / 100 * resource.ListPrice));


                itemBuilder.Append(text);
            }

            var bodyBuilder = BuildBodyHtml(itemBuilder.ToString())
                .Replace("{CartLink}",
                    _isPdaEdition
                        ? GetPdaLink(user.InstitutionId.GetValueOrDefault())
                        : GetCartLink(user.InstitutionId.GetValueOrDefault()));

            var subject = _isPdaEdition ? "New Pda Editions Now Available" : "New Editions Now Available";

            var mainBuilder = BuildMainHtml(subject, bodyBuilder, user);

            return mainBuilder;
        }
    }
}