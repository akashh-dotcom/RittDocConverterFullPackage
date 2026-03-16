#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class CartMap : BaseMap<Cart>
    {
        public CartMap()
        {
            Table("tCart");

            Id(x => x.Id).Column("iCartId").GeneratedBy.Identity();
            Map(x => x.PurchaseOrderNumber, "vchPurchaseOrderNumber");
            Map(x => x.PurchaseOrderComment, "vchPurchaseOrderComment");
            Map(x => x.PurchaseDate, "dtPurchaseDate");
            Map(x => x.BillingMethod, "tiBillingMethod").CustomType<BillingMethodEnum>();
            Map(x => x.ForthcomingTitlesInvoicingMethod, "tiForthcomingTitlesInvoicingMethod")
                .CustomType<ForthcomingTitlesInvoicingMethodEnum>();
            Map(x => x.Discount, "decInstDiscount");
            Map(x => x.Processed, "tiProcessed");
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.OrderNumber, "vchOrderNumber");
            Map(x => x.PromotionCode, "vchPromotionCode");
            Map(x => x.PromotionDiscount, "decPromotionDiscount");
            Map(x => x.CartType, "iCartTypeId").CustomType<CartTypeEnum>();
            Map(x => x.CartName, "vchCartName");
            Map(x => x.ConvertDate, "dtCartTypeConvertDate");

            References(x => x.Reseller).Column("iResellerId");

            HasMany(x => x.CartItems).KeyColumn("iCartId").AsBag().Inverse().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}