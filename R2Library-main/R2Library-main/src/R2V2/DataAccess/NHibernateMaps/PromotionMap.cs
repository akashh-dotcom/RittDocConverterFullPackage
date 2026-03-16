#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PromotionMap : BaseMap<Promotion>
    {
        public PromotionMap()
        {
            Table("tPromotion");

            Id(x => x.Id).Column("iPromotionId").GeneratedBy.Identity();
            Map(x => x.Code, "vchPromotionCode");
            Map(x => x.Name, "vchPromotionName");
            Map(x => x.Description, "vchPromotionDiscription");
            Map(x => x.Discount, "iDiscountPercentage");
            Map(x => x.StartDate, "dtStartDate");
            Map(x => x.EndDate, "dtEndDate");
            Map(x => x.OrderSource, "vchOrderSource");
            Map(x => x.MaximumUses, "iMaximumUses");
            Map(x => x.EnableCartAlert, "tiEnableCartAlert");
            HasMany(x => x.PromotionProducts).KeyColumn("iPromotionId").AsBag().Cascade.AllDeleteOrphan();
        }
    }
}