#region

using System.Collections.Generic;
using R2V2.Web.Models.Shared;

#endregion

namespace R2V2.Web.Models
{
    public class FacetedResultsJson : JsonResponse
    {
        public Dictionary<string, FilterGroup> FilterGroups { get; set; } = new Dictionary<string, FilterGroup>();
        public Dictionary<string, SortGroup> SortGroups { get; set; } = new Dictionary<string, SortGroup>();
        public Dictionary<string, string> HtmlSnippets { get; set; } = new Dictionary<string, string>();
    }
}