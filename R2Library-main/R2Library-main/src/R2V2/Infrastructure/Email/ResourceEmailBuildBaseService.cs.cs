#region

using System.Text;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.Email
{
    public class ResourceEmailBuildBaseService : EmailBuildBaseService
    {
        public ResourceEmailBuildBaseService(
            ILog<EmailBuildBaseService> log
            , IEmailSettings emailSettings
            , IContentSettings contentSettings
        ) : base(log, emailSettings, contentSettings)
        {
        }

        protected string BuildSpecialtyHeader(IResource resource, ISpecialty specialty, string accountNumber)
        {
            var specialtyString = GetTemplateFromFile("Resource_Specialty.html")
                .Replace("{Specialty_Header}", specialty.Name);

            return new StringBuilder()
                .Append(specialtyString)
                .Append(BuildItemHtml(resource, accountNumber))
                .ToString();
        }

        protected string BuildSpecialtyHeader(IResource resource, ISpecialty specialty)
        {
            var specialtyString = GetTemplateFromFile("Resource_Specialty.html")
                .Replace("{Specialty_Header}", specialty.Name);

            return new StringBuilder()
                .Append(specialtyString)
                .ToString();
        }

        protected string BuildItemHtml(IResource resource, string accountNumber)
        {
            return ItemTemplate
                    .Replace("{Resource_Url}", GetResourceLink(resource.Isbn, accountNumber))
                    .Replace("{Resource_Title}", resource.Title)
                    .Replace("{Resource_Author}", resource.Authors)
                    .Replace("{Resource_PracticeArea}",
                        PopulateField("Practice Area: ", resource.PracticeAreasToString()))
                    .Replace("{Resource_Publisher}", PopulateField("Publisher: ", resource.Publisher.ToName()))
                    .Replace("{Resource_Specialties}", PopulateField("Discipline: ", resource.SpecialtiesToString()))
                    .Replace("{Resource_PublicationYear}",
                        PopulateField("Publication Date: ",
                            resource.PublicationDate.GetValueOrDefault().Year.ToString()))
                    .Replace("{Resource_ISBN10}", PopulateField("ISBN 10: ", resource.Isbn10))
                    .Replace("{Resource_ReleaseDate}",
                        PopulateField("R2 Release Date: ",
                            resource.ReleaseDate.GetValueOrDefault().ToShortDateString()))
                    .Replace("{Resource_ISBN13}", PopulateField("ISBN 13: ", resource.Isbn13))
                    .Replace("{Resource_EISBN}", PopulateField("EISBN: ", resource.EIsbn))
                    .Replace("{Resource_Edition}", PopulateField("Edition: ", resource.Edition))
                    .Replace("{Resource_ImageUrl}", GetResourceImageUrl(resource.ImageFileName))
                    .Replace("{Resource_Icons}", GetResourceIcons(resource))
                    .Replace("{Resource_Affiliation}", PopulateField("Affiliation: ", resource.Affiliation))
                ;
        }

        protected string BuildBodyHtml(string itemsHtml)
        {
            return BuildBodyHtml("{Resource_Body}", itemsHtml);
        }
    }
}