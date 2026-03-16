#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DiseaseSynonymMap : BaseMap<DiseaseSynonym>
    {
        public DiseaseSynonymMap()
        {
            Table("tDiseaseSynonym");

            Id(x => x.Id, "iDiseaseSynonymId").GeneratedBy.Identity();
            Map(x => x.Name, "vchDiseaseSynonym");

            HasMany(x => x.DiseaseSynonymResources).KeyColumn("iDiseaseSynonymId");

            References(x => x.Disease).Column("iDiseaseNameId");
        }
    }
}