#region

using System.Collections.Generic;
using System.Linq;
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
    public class ForthcomingResourceEmailBuildService : ResourceEmailBuildBaseService
    {
        public ForthcomingResourceEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
            SetTemplates(ForthcomingBodyTemplate, ForthcomingItemTemplate);
        }

        public EmailMessage BuildForthcomingResourceEmail(IEnumerable<IResource> resources, User user)
        {
            var messageBody = GetNewResourceEmailHtml(resources, user);

            return BuildEmailMessage(user, "R2 Library Purchased Titles Now Available", messageBody);
        }

        public string GetNewResourceEmailHtml(IEnumerable<IResource> resources, User user)
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

                itemBuilder.Append(
                    BuildItemHtml(resource, user.Institution.AccountNumber)
                        .Replace("{Resource_RetailPrice}", resource.ListPriceString()));
            }

            var bodyBuilder = BuildBodyHtml(itemBuilder.ToString());

            var mainBuilder = BuildMainHtml("Purchased Titles Now Available", bodyBuilder, user);

            return mainBuilder;
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