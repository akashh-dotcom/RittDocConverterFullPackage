#region

using FluentNHibernate.Mapping;
using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceIsbnMap : ClassMap<ResourceIsbn>
    {
        public ResourceIsbnMap()
        {
            Table("tResourceIsbn");

            CompositeId().KeyProperty(x => x.ResourceId, "iResourceId")
                .KeyProperty(x => x.ResourceIsbnTypeId, "iResourceIsbnTypeId");
            //Map(x => x.ResourceIsbnTypeId).Column("iResourceIsbnTypeId");
            Map(x => x.Isbn).Column("vchIsbn");
            Map(x => x.IsTextMLIsbn).Column("bIsTextMLIsbn");
        }
    }
}