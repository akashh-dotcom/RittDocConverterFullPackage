#region

using R2V2.Core.OrderHistory;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class OrderHistoryDiscountTypeMap : BaseMap<DbOrderHistoryDiscountType>
    {
        public OrderHistoryDiscountTypeMap()
        {
            Table("tDiscountType");
            Id(x => x.Id).Column("iDiscountTypeId").GeneratedBy.Identity();
            Map(x => x.Name, "vchDiscountTypeName");
            Map(x => x.Description, "vchDiscountTypeDescription");
        }
    }
}