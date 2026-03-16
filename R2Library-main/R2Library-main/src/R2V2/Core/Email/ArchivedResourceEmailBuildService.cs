#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class ArchivedResourceEmailBuildService : ResourceEmailBuildBaseService
    {
        public List<IResource> ProcessedArchivedResources;

        public ArchivedResourceEmailBuildService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        )
            : base(log, emailSettings, contentSettings)
        {
            SetTemplates(ArchivedBodyTemplate, ArchivedItemTemplate);
        }

        public EmailMessage BuildArchivedResourceEmail(User user, List<IResource> archivedResources)
        {
            var messageBody = GetArchivedResourceEmailHtml(user, archivedResources);

            return BuildEmailMessage(user, "R2 Library Archived Titles", messageBody);
        }


        private string GetArchivedResourceEmailHtml(User user, List<IResource> archivedResources)
        {
            var itemBuilder = new StringBuilder();

            string lastSpecialtyName = null;

            foreach (var item in archivedResources)
            {
                if (item == null) continue;
                var license = "";
                var institutionResourceLicense =
                    user.Institution.InstitutionResourceLicenses.FirstOrDefault(x => x.ResourceId == item.Id);
                if (institutionResourceLicense != null)
                {
                    license = !institutionResourceLicense.FirstPurchaseDate.HasValue &&
                              institutionResourceLicense.OriginalSourceId == (int)LicenseOriginalSource.Pda
                        ? "PDA"
                        : institutionResourceLicense.LicenseCount.ToString();
                }

                var specialty = item.Specialties != null
                    ? item.Specialties.OrderBy(x => x.Name).FirstOrDefault()
                    : null;

                if (specialty != null)
                {
                    if (lastSpecialtyName != specialty.Name)
                    {
                        itemBuilder.Append(BuildSpecialtyHeader(item, specialty));

                        lastSpecialtyName = specialty.Name;
                    }
                }

                itemBuilder.Append(
                    BuildItemHtml(item, user.Institution.AccountNumber)
                        .Replace("{Resource_License}", item.GetLicenseCount(license))
                );
            }

            var bodyBuilder = BuildBodyHtml(itemBuilder.ToString());

            var mainBuilder = BuildMainHtml("Archived Titles", bodyBuilder, user);

            MergeArchivedResources(archivedResources);

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

        private void MergeArchivedResources(IEnumerable<IResource> resources)
        {
            if (ProcessedArchivedResources == null)
            {
                ProcessedArchivedResources = new List<IResource>();
            }

            foreach (var resource in resources.Where(resource => !ProcessedArchivedResources.Contains(resource)))
            {
                ProcessedArchivedResources.Add(resource);
            }
        }
    }
}