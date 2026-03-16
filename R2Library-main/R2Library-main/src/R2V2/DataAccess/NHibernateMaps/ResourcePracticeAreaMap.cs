#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourcePracticeAreaMap : BaseMap<ResourcePracticeArea>
    {
        public ResourcePracticeAreaMap()
        {
            Table("tResourcePracticeArea");
            Id(x => x.Id).Column("iResourcePracticeAreaId").GeneratedBy.Identity();
            References(x => x.PracticeArea).Column("iPracticeAreaId").ReadOnly();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.PracticeAreaId).Column("iPracticeAreaId");
        }
    }
}