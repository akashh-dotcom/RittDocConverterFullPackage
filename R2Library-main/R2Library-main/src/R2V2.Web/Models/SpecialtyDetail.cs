#region

using System.Collections.Generic;
using R2V2.Web.Models.Browse;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Models
{
    public class SpecialtyDetail : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BrowseQuery BrowseQuery { get; set; }
        public IEnumerable<ResourceSummary> Resources { get; set; }
    }
}