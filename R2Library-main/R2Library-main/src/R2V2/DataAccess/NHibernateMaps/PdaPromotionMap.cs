#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PdaPromotionMap : BaseMap<PdaPromotion>
    {
        public PdaPromotionMap()
        {
            Table("tPdaPromotion");

            Id(x => x.Id).Column("iPdaPromotionId").GeneratedBy.Identity();
            Map(x => x.Name, "vchPdaPromotionName");
            Map(x => x.Description, "vchPdaPromotionDiscription");
            Map(x => x.Discount, "iDiscountPercentage");
            Map(x => x.StartDate, "dtStartDate");
            Map(x => x.EndDate, "dtEndDate");
        }
    }
}