#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Models.Browse
{
    public class BrowseQuery
    {
        public string Id { get; set; }

        public BrowseType Type { get; set; }

        public Include Include { get; set; }

        public string PracticeArea { get; set; }
        public string PublisherId { get; set; }
        public string DisciplineId { get; set; }
        public string SortBy { get; set; }

        public string Alpha { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool ShowAll { get; set; }

        public bool TocAvailable { get; set; }

        public int CollectionId { get; set; }
    }
}