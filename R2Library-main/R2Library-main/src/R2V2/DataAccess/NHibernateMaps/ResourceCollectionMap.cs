#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceCollectionMap : BaseMap<ResourceCollection>
    {
        public ResourceCollectionMap()
        {
            Table("tResourceCollection");

            Id(x => x.Id).Column("iResourceCollectionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.CollectionId).Column("iCollectionId");

            References(x => x.Collection).Column("iCollectionId").ReadOnly();

            Map(x => x.DataString).Column("vchData");
        }
    }
}