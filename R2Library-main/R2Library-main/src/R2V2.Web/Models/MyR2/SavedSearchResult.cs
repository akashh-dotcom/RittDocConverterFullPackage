#region

using System;
using R2V2.Web.Models.Search;

#endregion

namespace R2V2.Web.Models.MyR2
{
    [Serializable]
    public class SavedSearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public int ResultsCount { get; set; }

        public SavedSearchResultSet SavedSearchResultSet { get; set; }
    }
}