#region

using R2V2.Core.Resource;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceSpecialtyMap : BaseMap<ResourceSpecialty>
    {
        public ResourceSpecialtyMap()
        {
            Table("tResourceSpecialty");
            Id(x => x.Id).Column("iResourceSpecialtyId").GeneratedBy.Identity();
            References(x => x.Specialty).Column("iSpecialtyId").ReadOnly();
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.SpecialtyId).Column("iSpecialtyId");
        }
    }
}