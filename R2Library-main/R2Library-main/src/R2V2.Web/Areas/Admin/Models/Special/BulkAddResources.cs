#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class BulkAddResources : AdminBaseModel
    {
        private readonly IList<IResource> _excludedResources = new List<IResource>();
        private readonly IList<IResource> _resources = new List<IResource>();
        private readonly IList<string> _resourcesNotFound = new List<string>();
        public int SpecialId { get; set; }
        public int SpecialDiscountId { get; set; }

        public string Isbns { get; set; }

        public IEnumerable<IResource> Resources => _resources;

        public IEnumerable<IResource> ExcludedResources => _excludedResources;

        public IEnumerable<string> ResourcesNotFound => _resourcesNotFound;

        public string ResourceString { get; set; }

        public void AddResource(IResource resource)
        {
            _resources.Add(resource);
        }

        public void AddExcludedResource(IResource resource)
        {
            _excludedResources.Add(resource);
        }

        public void AddResourceNotFound(string isbn)
        {
            _resourcesNotFound.Add(isbn);
        }

        public int[] GetResourceIds()
        {
            var resourceIds = string.IsNullOrWhiteSpace(ResourceString)
                ? new string[0]
                : ResourceString.Split(',');

            var ids = new List<int>();
            foreach (var id in resourceIds)
            {
                int resourceId;
                int.TryParse(id, out resourceId);
                ids.Add(resourceId);
            }

            return ids.ToArray();
        }
    }
}