#region

using R2V2.Core.OrderHistory;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DbOrderHistoryItemMap : BaseMap<DbOrderHistoryItem>
    {
        public DbOrderHistoryItemMap()
        {
            Table("tOrderHistoryItem");
            Id(x => x.Id).Column("iOrderHistoryItemId").GeneratedBy.Identity();
            Map(x => x.OrderHistoryId, "iOrderHistoryId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.ProductId, "iProductId");
            Map(x => x.InstitutionResourceLicenseId, "iInstitutionResourceLicenseId");
            Map(x => x.NumberOfLicenses, "iNumberOfLicenses");
            Map(x => x.ListPrice, "decListPrice");
            Map(x => x.DiscountPrice, "decDiscountPrice");
            Map(x => x.Discount, "decDiscount");
            Map(x => x.SpecialText, "vchSpecialText");
            Map(x => x.SpecialIconName, "vchSpecialIconName");
            Map(x => x.SpecialDiscountId, "iSpecialDiscountId");
            Map(x => x.PdaPromotionId, "iPdaPromotionId");
            Map(x => x.DiscountTypeId, "iDiscountTypeId");
            Map(x => x.IsBundle, "tiBundle");
            Map(x => x.BundlePrice, "decBundlePrice");
        }
    }
}