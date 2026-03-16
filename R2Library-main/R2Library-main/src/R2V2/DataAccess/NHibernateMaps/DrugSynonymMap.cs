#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DrugSynonymMap : BaseMap<DrugSynonym>
    {
        public DrugSynonymMap()
        {
            Table("tDrugSynonym");

            Id(x => x.Id, "iDrugSynonymId").GeneratedBy.Identity();
            Map(x => x.Name, "vchDrugSynonymName");

            HasMany(x => x.DrugSynonymResources).KeyColumn("iDrugSynonymId");

            References(x => x.Drug).Column("iDrugListId");
        }
    }
}