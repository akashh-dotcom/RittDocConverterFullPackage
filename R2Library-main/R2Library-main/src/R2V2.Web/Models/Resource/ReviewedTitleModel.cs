#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Models.Resource
{
    public class ReviewedTitleModel : BaseModel
    {
        public ResourceDetail ResourceDetail { get; set; }
        public List<IResource> RecentlyReleasedResources { get; set; }
        public string Type { get; set; }
    }
}