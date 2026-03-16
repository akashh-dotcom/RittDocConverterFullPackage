#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class AZIndexMap : BaseMap<AZIndex>
    {
        public AZIndexMap()
        {
            Table("tAtoZIndex");

            Id(x => x.Id).Column("iAtoZIndexId").GeneratedBy.Identity();

            Map(x => x.Name, "vchName");
            Map(x => x.AlphaKey, "chrAlphaKey");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.Isbn, "vchResourceISBN");
            Map(x => x.ChapterId, "vchChapterId");
            Map(x => x.SectionId, "vchSectionId");

            Map(x => x.Type, "iAtoZIndexTypeId").CustomType<int>();

            HasMany(x => x.ResourcePracticeAreas).KeyColumn("iResourceId").PropertyRef("ResourceId");
            HasMany(x => x.ResourceSpecialties).KeyColumn("iResourceId").PropertyRef("ResourceId");
            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("iResourceId").PropertyRef("ResourceId");
        }
    }
}