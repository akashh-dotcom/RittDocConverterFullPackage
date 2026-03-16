#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class PromotionProductMap : BaseMap<PromotionProduct>
    {
        public PromotionProductMap()
        {
            Table("tPromotionProduct");

            Id(x => x.Id).Column("iPromotionProductId").GeneratedBy.Identity();
            Map(x => x.PromotionId, "iPromotionId");
            Map(x => x.ProductId, "iProductId");
        }
    }
}