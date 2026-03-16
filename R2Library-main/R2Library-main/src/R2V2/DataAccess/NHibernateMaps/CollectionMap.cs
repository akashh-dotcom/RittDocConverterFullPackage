#region

using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class CollectionMap : BaseMap<Collection>
    {
        public CollectionMap()
        {
            Table("tCollection");

            Id(x => x.Id).Column("iCollectionId").GeneratedBy.Assigned();
            Map(x => x.Name).Column("vchCollectionName");
            Map(x => x.HideInFilter).Column("tiHideInFilter");
            Map(x => x.Sequence).Column("iSequenceNumber");
            Map(x => x.IsSpecialCollection).Column("tiIsSpecialCollection");
            Map(x => x.SpecialCollectionSequence).Column("iSpecialCollectionSequence");
            Map(x => x.Description).Column("vchDescription").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.IsPublic).Column("tiIsPublic");
        }
    }
}