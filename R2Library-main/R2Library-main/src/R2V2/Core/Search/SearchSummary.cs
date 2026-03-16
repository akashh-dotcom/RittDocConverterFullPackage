namespace R2V2.Core.Search
{
    public class SearchSummary
    {
        public string Term { get; set; }

        public string Author { get; set; }
        public string BookTitle { get; set; }
        public string Publisher { get; set; }
        public string Editor { get; set; }

        public bool Active { get; set; }
        public bool Archive { get; set; }
        public string PracticeAreaCode { get; set; }
        public string SpecialtyCode { get; set; }
        public SearchFields Field { get; set; }
        public bool DrugMonograph { get; set; }
        public string Years { get; set; }
        public bool IncludeTocResouces { get; set; }

        public SearchSortBy SortBy { get; set; }

        public string[] Isbns { get; set; }
        public string[] SearchWithinIsbns { get; set; }

        public int ResultsCount { get; set; }
        public bool Advanced { get; set; }
        public int ReserveShelfId { get; set; }
        public int PageSize { get; set; }
    }
}