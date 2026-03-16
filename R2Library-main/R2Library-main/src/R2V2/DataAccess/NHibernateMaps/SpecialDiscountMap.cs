#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SpecialDiscountMap : BaseMap<SpecialDiscount>
    {
        public SpecialDiscountMap()
        {
            //SELECT [iSpecialDiscountId]
            //      ,[iDiscountPercentage]
            //      ,[iSpecialId]
            //      ,[vchIconName]
            //      ,[vchCreatorId]
            //      ,[dtCreationDate]
            //      ,[vchUpdaterId]
            //      ,[dtLastUpdate]
            //      ,[tiRecordStatus]
            //  FROM [dbo].[tSpecialDiscount]
            Table("tSpecialDiscount");
            Id(x => x.Id).Column("iSpecialDiscountId").GeneratedBy.Identity();
            Map(x => x.DiscountPercentage).Column("iDiscountPercentage");
            Map(x => x.IconName).Column("vchIconName");
            Map(x => x.SpecialId).Column("iSpecialId");
            References(x => x.Special).Column("iSpecialId").ReadOnly().Cascade.None();
        }
    }
}