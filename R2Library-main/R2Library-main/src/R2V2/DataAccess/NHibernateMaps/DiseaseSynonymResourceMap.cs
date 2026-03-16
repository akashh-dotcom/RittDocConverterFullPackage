#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DiseaseSynonymResourceMap : BaseMap<DiseaseSynonymResource>
    {
        public DiseaseSynonymResourceMap()
        {
            Table("tDiseaseSynonymResource");

            Id(x => x.Id, "iDiseaseSynonymResourceId").GeneratedBy.Identity();
            Map(x => x.Isbn, "vchResourceISBN");
            Map(x => x.ChapterId, "vchChapterId");
            Map(x => x.SectionId, "vchSectionId");

            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("vchResourceISBN").Inverse().Cascade.None();
        }
    }
}