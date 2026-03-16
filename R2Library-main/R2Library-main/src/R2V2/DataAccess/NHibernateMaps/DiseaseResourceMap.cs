#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DiseaseResourceMap : BaseMap<DiseaseResource>
    {
        public DiseaseResourceMap()
        {
            Table("tDiseaseResource");

            Id(x => x.Id, "iDiseaseResourceId").GeneratedBy.Identity();
            Map(x => x.DiseaseId, "iDiseaseNameId");
            Map(x => x.Isbn, "vchResourceISBN");
            Map(x => x.ChapterId, "vchChapterId");
            Map(x => x.SectionId, "vchSectionId");

            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("vchResourceISBN").Inverse().Cascade.None();
        }
    }
}