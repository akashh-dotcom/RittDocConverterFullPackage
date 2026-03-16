#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class BulkDeletePda : AdminBaseModel
    {
        private readonly IList<InstitutionResource> _excludedResources = new List<InstitutionResource>();

        private readonly IList<InstitutionResource> _resources = new List<InstitutionResource>();

        public BulkDeletePda()
        {
        }

        public BulkDeletePda(IAdminInstitution institution, CollectionManagementQuery collectionManagementQuery)
            : base(institution)
        {
            CollectionManagementQuery = collectionManagementQuery;
        }

        public IEnumerable<InstitutionResource> Resources => _resources;

        public IEnumerable<InstitutionResource> ExcludedResources => _excludedResources;

        public CollectionManagementQuery CollectionManagementQuery { get; set; }

        public int ResourceCount => _resources.Count();

        public int ExcludedResourceCount => _excludedResources.Count();

        public string IsbnsNotFound { get; set; }

        public int IsbnsNotFoundCount { get; set; }

        public string KeepShoppingLink { get; set; }
        public string CollectionLink { get; set; }

        public void AddResource(InstitutionResource resource)
        {
            _resources.Add(resource);
        }

        public void AddExcludedResource(InstitutionResource resource)
        {
            _excludedResources.Add(resource);
        }
    }
}