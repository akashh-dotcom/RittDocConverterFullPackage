#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SpecialResourceMap : BaseMap<SpecialResource>
    {
        public SpecialResourceMap()
        {
            //SELECT [iSpecialResourceId]
            //      ,[iSpecialDiscountId]
            //      ,[iResourceId]
            //      ,[vchCreatorId]
            //      ,[dtCreationDate]
            //      ,[vchUpdaterId]
            //      ,[dtLastUpdate]
            //      ,[tiRecordStatus]
            //  FROM [dbo].[tSpecialResource]
            Table("tSpecialResource");
            Id(x => x.Id).Column("iSpecialResourceId").GeneratedBy.Identity();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.DiscountId).Column("iSpecialDiscountId");
            References(x => x.Discount).Column("iSpecialDiscountId").ReadOnly();
        }
    }
}