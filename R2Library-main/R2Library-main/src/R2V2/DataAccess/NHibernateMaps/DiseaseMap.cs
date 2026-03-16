#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DiseaseMap : BaseMap<Disease>
    {
        public DiseaseMap()
        {
            Table("tDiseaseName");

            Id(x => x.Id, "iDiseaseNameId").GeneratedBy.Identity();
            Map(x => x.Name, "vchDiseaseName");
            Map(x => x.Description, "vchDiseaseDesc");

            HasMany(x => x.DiseaseResources).KeyColumn("iDiseaseNameId");
        }
    }
}