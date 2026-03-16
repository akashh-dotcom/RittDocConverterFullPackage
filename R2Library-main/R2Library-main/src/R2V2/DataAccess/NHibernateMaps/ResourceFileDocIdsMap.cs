#region

using FluentNHibernate.Mapping;
using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceFileDocIdsMap : ClassMap<ResourceFileDocIds>
    {
        public ResourceFileDocIdsMap()
        {
            // -- select iResourceId, iMinDocumentId, iMaxDocumentId from vResourceFileDocIds 
            Table("vResourceFileDocIds");
            Id(x => x.Id).Column("iResourceId");
            Map(x => x.MinDocumentId).Column("iMinDocumentId");
            Map(x => x.MaxDocumentId).Column("iMaxDocumentId");
        }
    }
}