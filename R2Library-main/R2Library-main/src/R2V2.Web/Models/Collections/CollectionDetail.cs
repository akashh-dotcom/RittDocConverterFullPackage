#region

using System.Collections.Generic;
using R2V2.Core.Resource.Collection;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Models.Collections
{
    public class CollectionDetail
    {
        public CollectionDetail()
        {
        }

        public CollectionDetail(ICollection selectedCollection, IEnumerable<ResourceSummary> resources)
        {
            //selectedCollection.Id
            //selectedCollection.Description
            CollectionId = selectedCollection.Id;
            Name = selectedCollection.Name;
            Description = selectedCollection.Description;
            Resources = resources;
        }

        public int CollectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Selected { get; set; }
        public IEnumerable<ResourceSummary> Resources { get; set; }
    }
}