#region

using System.Collections.Generic;
using R2V2.Web.Models.Shared;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchResultsJson : JsonResponse
    {
        /// <summary>
        /// </summary>
        public SearchResultsJson()
        {
            FilterGroups = new Dictionary<string, FilterGroup>();
            OptionGroups = new Dictionary<string, OptionGroup>();
            HtmlSnippets = new Dictionary<string, string>();
        }

        public string TotalSearchTime { get; set; }
        public int TotalResults { get; set; }
        public bool TocSelected { get; set; }
        public Dictionary<string, FilterGroup> FilterGroups { get; set; }
        public Dictionary<string, OptionGroup> OptionGroups { get; set; }
        public Dictionary<string, string> HtmlSnippets { get; set; }
    }
}