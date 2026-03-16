#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SpecialMap : BaseMap<Special>
    {
        public SpecialMap()
        {
            //SELECT [iSpecialId]
            //      ,[vchName]
            //      ,[dtStartDate]
            //      ,[dtEndDate]
            //      ,[vchCreatorId]
            //      ,[dtCreationDate]
            //      ,[vchUpdaterId]
            //      ,[dtLastUpdate]
            //      ,[tiRecordStatus]
            //  FROM [dbo].[tSpecial]

            Table("tSpecial");
            Id(x => x.Id).Column("iSpecialId").GeneratedBy.Identity();
            Map(x => x.Name).Column("vchName");
            Map(x => x.StartDate).Column("dtStartDate");
            Map(x => x.EndDate).Column("dtEndDate");

            HasMany(x => x.Discounts).KeyColumn("iSpecialId").AsBag().Cascade.AllDeleteOrphan();
        }
    }
}