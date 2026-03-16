#region

using System;
using R2V2.Core.Institution;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CollectionManagementResource : IDiscountResource
    {
        private Product _product;


        private IResource _resource;

        public CollectionManagementResource()
        {
        }

        public CollectionManagementResource(IResource resource, int cartId)
        {
            Resource = resource;
            CartId = cartId;
        }

        public IResource Resource
        {
            get => _resource;
            set
            {
                ResourceId = value.Id;
                _resource = value;
            }
        }

        public Product Product
        {
            get => _product;
            set
            {
                ProductId = value.Id;
                _product = value;
            }
        }

        public int LicenseCount { get; set; }
        public LicenseType LicenseType { get; set; }
        public int CartLicenseCount { get; set; }

        public LicenseOriginalSource OriginalSource { get; set; }

        public string OriginalSourceString
        {
            get
            {
                switch (OriginalSource)
                {
                    case LicenseOriginalSource.FirmOrder:
                        return "Firm Order";
                    case LicenseOriginalSource.Pda:
                        return "PDA title selection";
                }

                return null;
            }
        }

        public DateTime? PdaDeletedDate { get; set; }

        public DateTime? PdaAddedDate { get; set; }
        public DateTime? PdaAddedToCartDate { get; set; }
        public DateTime? PdaCartDeletedDate { get; set; }
        public string PdaCartDeletedByName { get; set; }
        public int PdaViewCount { get; set; }
        public int PdaMaxViews { get; set; }
        public DateTime? FirstPurchaseDate { get; set; }

        public DateTime? ResourceNotSaleableDate { get; set; }

        public DateTime? PdaRuleAddedDate { get; set; }
        public int ConcurrentTurnawayCount { get; set; }
        public bool FreeLicenseInCart { get; set; }

        public decimal ListPrice
        {
            get => Resource.IsFreeResource ? 0 : Resource.ListPrice;
            set => throw new NotImplementedException();
        }

        public decimal BundlePrice { get; set; }
        public bool IsBundle { get; set; }

        public decimal Discount { get; set; }
        //public decimal PromotionDiscount { get; set; }

        public int? ResourceId { get; set; }
        public int? ProductId { get; set; }
        public int CartId { get; set; }
        public decimal DiscountPrice { get; set; }

        //public CachedSpecialResource SpecialDiscount { get; set; }

        public string SpecialText { get; set; }
        public string SpecialIconName { get; set; }
        public bool PdaPromotionApplied { get; set; }
        public short OriginalSourceId { get; set; }
        public int? PdaPromotionId { get; set; }
        public int? SpecialDiscountId { get; set; }
    }
}