#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class FeaturedTitleMap : BaseMap<FeaturedTitle>
    {
        public FeaturedTitleMap()
        {
            Table("tFeaturedTitle");
            Id(x => x.Id).Column("iFeaturedTitleId").GeneratedBy.Identity();
            Map(x => x.StartDate).Column("dtStartDate");
            Map(x => x.EndDate).Column("dtEndDate");
            Map(x => x.ResourceId).Column("iResourceId");
        }
    }
}