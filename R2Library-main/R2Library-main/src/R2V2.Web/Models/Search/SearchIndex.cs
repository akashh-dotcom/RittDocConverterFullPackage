#region

using R2V2.Web.Models.Shared;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchIndex : BaseModel
    {
        public string[] PageSizes = { "10", "25", "50" };

        public bool DisplayTocAvailable { get; set; }
        public string TocOnlyTitlesChecked { get; set; }

        public FilterGroup FieldFilterGroup { get; set; } =
            new FilterGroup { Name = "Show results from", Code = "field" };

        public FilterGroup FilterByFilterGroup { get; set; } =
            new FilterGroup { Name = "Filter by", Code = "filter-by" };

        public FilterGroup PracticeAreaFilterGroup { get; set; } =
            new FilterGroup { Name = "Practice Area", Code = "practice-area" };

        public FilterGroup PublicationDateFilterGroup { get; set; } =
            new FilterGroup { Name = "Publication Date", Code = "year" };

        public FilterGroup DisciplineFilterGroup { get; set; } =
            new FilterGroup { Name = "Discipline", Code = "discipline" };

        public EmailPage EmailPage { get; set; }

        public SearchQuery SearchQuery { get; set; }

        public bool DisplaySavedSearchResultLink { get; set; }

        public short Include { get; set; } = 1;

        public string PubMedSearchUrl { get; set; }
        public string MeshSearchUrl { get; set; }
    }
}