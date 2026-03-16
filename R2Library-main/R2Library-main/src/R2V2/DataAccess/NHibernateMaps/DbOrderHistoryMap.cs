#region

using R2V2.Core.CollectionManagement;
using R2V2.Core.OrderHistory;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DbOrderHistoryMap : BaseMap<DbOrderHistory>
    {
        public DbOrderHistoryMap()
        {
            Table("tOrderHistory");
            Id(x => x.Id).Column("iOrderHistoryId").GeneratedBy.Identity();
            Map(x => x.CartId, "iCartId");
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.OrderNumber, "vchOrderNumber");
            Map(x => x.PurchaseOrderNumber, "vchPurchaseOrderNumber");
            Map(x => x.PurchaseOrderComment, "vchPurchaseOrderComment");
            Map(x => x.Discount, "decDiscount");
            Map(x => x.PromotionCode, "vchPromotionCode");
            Map(x => x.PromotionDescription, "vchPromotionDescription");
            Map(x => x.PromotionId, "iPromotionId");
            Map(x => x.PurchaseDate, "dtPurchaseDate");
            Map(x => x.BillingMethod, "tiBillingMethod").CustomType<BillingMethodEnum>();
            Map(x => x.ForthcomingTitlesInvoicingMethod, "tiForthcomingTitlesInvoicingMethod")
                .CustomType<ForthcomingTitlesInvoicingMethodEnum>();
            Map(x => x.CartName, "vchCartName");
            Map(x => x.ResellerName, "vchResellerName");
            Map(x => x.ResellerDiscount, "decResellerDiscount");
            Map(x => x.OrderFile).Column("vchOrderFile").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.DiscountTypeId, "iDiscountTypeId");
            References(x => x.Reseller, "iResellerId");

            HasMany(x => x.OrderHistoryItems).KeyColumn("iOrderHistoryId").AsBag().Cascade.All();
        }
    }
}