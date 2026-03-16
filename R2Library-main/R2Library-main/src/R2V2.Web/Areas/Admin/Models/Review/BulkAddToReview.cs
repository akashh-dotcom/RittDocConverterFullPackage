#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class BulkAddToReview : AdminBaseModel
    {
        private readonly IList<InstitutionResource> _excludedResources = new List<InstitutionResource>();

        private readonly IList<InstitutionResource> _resources = new List<InstitutionResource>();

        public BulkAddToReview()
        {
        }

        public BulkAddToReview(IAdminInstitution institution)
            : base(institution)
        {
        }

        public IEnumerable<InstitutionResource> Resources => _resources;

        public IEnumerable<InstitutionResource> ExcludedResources => _excludedResources;

        public CollectionManagementQuery ResourceQuery { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total { get; private set; }

        public int LicenseCount { get; private set; }

        public int ResourceCount => _resources.Count();

        public int ExcludedResourceCount => _excludedResources.Count();

        public string IsbnsNotFound { get; set; }

        public int IsbnsNotFoundCount { get; set; }

        public string KeepShoppingLink { get; set; }
        public string CollectionLink { get; set; }
        public string CartLink { get; set; }

        [Display(Name = @"Notes:")]
        [StringLength(1000, ErrorMessage = @"Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }

        public void AddResource(InstitutionResource resource)
        {
            _resources.Add(resource);
            LicenseCount += 1;
            Total += resource.DiscountPrice;
        }

        public void AddExcludedResource(InstitutionResource resource)
        {
            _excludedResources.Add(resource);
        }
    }
}