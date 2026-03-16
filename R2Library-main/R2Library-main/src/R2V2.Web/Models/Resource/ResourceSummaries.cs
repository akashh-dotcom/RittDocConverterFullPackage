#region

using System.Collections.Generic;
using R2V2.Web.Models.Browse;

#endregion

namespace R2V2.Web.Models.Resource
{
    public class ResourceSummaries : BaseModel, IPageable
    {
        public BrowseQuery BrowseQuery { get; set; }
        public IEnumerable<ResourceSummary> Resources { get; set; }
        public IEnumerable<PageLink> PageLinks { get; set; }
    }
}