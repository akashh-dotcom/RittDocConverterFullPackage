#region

using System.Text;
using System.Web.Script.Serialization;
using R2V2.Core.Search;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchResult : ResourceSummaryBase, IAccessInfo
    {
        public SearchResult()
        {
        }

        public SearchResult(SearchResultsItem item, int licenseCount, AuthenticatedInstitution authenticatedInstitution)
        {
            var resource = item.SearchResource.Resource;

            BookTitle =
                $"{resource.Title}{(string.IsNullOrEmpty(resource.Edition) ? string.Empty : $", {resource.Edition}")}";
            ItemNumber = item.ItemNumber;

            //ImageUrl = resource.ImageFileName.ToImageUrl(contentSettings);
            ImageUrl = resource.ImageUrl;

            Gist = item.GetGist(250);
            IsFullTextAvailable = item.SearchResource.FullTextAvailable;
            IsArchive = item.SearchResource.Resource.IsArchive();
            IsForthcoming = item.SearchResource.Resource.IsForthcoming();

            SetShowLicenseCount(authenticatedInstitution);
            LicenseCount = licenseCount;

            if (item.BookHit)
            {
                Title = BookTitle;
                Description = resource.PublicationDate == null
                    ? $"{resource.Authors}, {item.Publisher}"
                    : $"{resource.Authors}, {item.Publisher}, {resource.PublicationDate.Value.Year}";
                Url = $"/Resource/Title/{resource.Isbn}";
            }
            else
            {
                Title = GetChapterSectionTitle(item);
                Description = resource.PublicationDate == null
                    ? $"{BookTitle}; {resource.Authors}, {item.Publisher}"
                    : $"{BookTitle}; {resource.Authors}, {item.Publisher}, {resource.PublicationDate.Value.Year}";

                Url = $"/Resource/detail/{resource.Isbn}/{item.Chapter}";
            }


            Editor = resource.Edition;
            Publisher = item.Publisher;
            Year = resource.Copyright;
            Isbn10 = resource.Isbn10;
            Isbn13 = resource.Isbn13;
            ChapterTitle = item.ChapterTitle;
        }

        public string BookTitle { get; }
        public int ItemNumber { get; private set; }

        // federated search - json
        public string Editor { get; private set; }
        public string Publisher { get; private set; }
        public string Year { get; private set; }
        public string Isbn10 { get; private set; }
        public string Isbn13 { get; private set; }
        public string ChapterTitle { get; private set; }

        // IAccessInfo
        public bool IsFullTextAvailable { get; }
        public bool IsArchive { get; }
        public bool IsForthcoming { get; set; }
        public bool IsPdaResource { get; set; }

        private static string GetChapterSectionTitle(ISearchResultsItem item)
        {
            var title = new StringBuilder();

            // chapter number
            if (!string.IsNullOrEmpty(item.ChapterNumber))
            {
                // change CHAPTER to Chapter
                title.AppendFormat("{0}{1}",
                    !item.ChapterNumber.ToLower().StartsWith("chapter") ? "Chapter " : string.Empty,
                    item.ChapterNumber.Replace("CHAPTER", "Chapter"));
            }

            // chapter title
            if (!string.IsNullOrEmpty(item.ChapterTitle))
            {
                if (title.Length > 0)
                {
                    title.Append(" - ");
                }

                title.Append(item.ChapterTitle);
            }

            // section title
            if (!string.IsNullOrEmpty(item.SectionTitle))
            {
                title.AppendFormat("{0}Section: {1}", title.Length == 0 ? string.Empty : " | ",
                    item.SectionTitle.Replace("SECTION", "Section"));
            }

            return title.Length == 0 ? "CHAPTER & SECTION TITLE MISSING" : title.ToString();
        }

        public string ToJson()
        {
            var scriptSerializer = new JavaScriptSerializer();
            return scriptSerializer.Serialize(this);
        }
    }
}