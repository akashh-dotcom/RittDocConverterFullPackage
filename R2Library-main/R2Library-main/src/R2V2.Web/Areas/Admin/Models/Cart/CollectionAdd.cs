#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Cart
{
    public class CollectionAdd : AdminBaseModel
    {
        public CollectionAdd()
        {
        }

        public CollectionAdd(IAdminInstitution institution)
            : base(institution)
        {
        }

        public CollectionAdd(IAdminInstitution institution, CollectionManagementQuery collmanManagementQuery)
            : base(institution)
        {
            ResourceQuery = collmanManagementQuery;
        }

        public int NumberOfLicenses { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total => NumberOfLicenses * InstitutionResource.DiscountPrice;

        public InstitutionResource InstitutionResource { get; set; }
        public int CartId { get; set; }
        public string KeepShoppingLink { get; set; }
        public string ViewMyPdaCollectionLink { get; set; }
        public CollectionManagementQuery ResourceQuery { get; set; }
        public int OriginalNumberOfLicenses { get; set; }
        public string ParentPageTitle { get; set; }
        public bool DisplayAddToSavedCart { get; set; }
        public IEnumerable<CachedCart> CachedCarts { get; set; }
        public bool IsBundlePurchase { get; set; }
        public string BaseImageUrl { get; set; }
    }

    public class CollectionAddSavedCarts : CollectionAdd
    {
        public IEnumerable<CachedCart> CachedCarts { get; set; }
    }
}