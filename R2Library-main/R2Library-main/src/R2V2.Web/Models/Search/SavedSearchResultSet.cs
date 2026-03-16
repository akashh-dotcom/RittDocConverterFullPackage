#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SavedSearchResultSet : BaseModel
    {
        public List<SearchResult> SearchResultList { get; set; }
        public SearchQuery SearchQuery { get; set; }

        public string Title { get; set; }
        public string PracticeArea { get; set; }
        public string Discipline { get; set; }

        public int ResultsCount { get; set; }

        public DateTime CreationDate { get; set; }
    }
}