#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DrugResourceMap : BaseMap<DrugResource>
    {
        public DrugResourceMap()
        {
            Table("tDrugResource");

            Id(x => x.Id, "iDrugResourceId").GeneratedBy.Identity();
            Map(x => x.DrugId, "iDrugListId");
            Map(x => x.Isbn, "vchResourceISBN");
            Map(x => x.ChapterId, "vchChapterId");
            Map(x => x.SectionId, "vchSectionId");

            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("vchResourceISBN").Inverse().Cascade.None();
        }
    }
}