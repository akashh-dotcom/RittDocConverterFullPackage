#region

using R2V2.Core.Resource.Topic;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class DrugMap : BaseMap<Drug>
    {
        public DrugMap()
        {
            Table("tDrugsList");

            Id(x => x.Id, "iDrugListId").GeneratedBy.Identity();
            Map(x => x.Name, "vchDrugName");

            HasMany(x => x.DrugResources).KeyColumn("iDrugListId");
        }
    }
}