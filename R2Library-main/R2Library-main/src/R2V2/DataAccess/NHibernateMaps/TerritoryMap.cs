#region

using R2V2.Core.Territory;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class TerritoryMap : BaseMap<Territory>
    {
        public TerritoryMap()
        {
            Table("tTerritory");

            Id(x => x.Id).Column("iTerritoryId").GeneratedBy.Identity();

            Map(x => x.Code).Column("vchTerritoryCode");
            Map(x => x.Name).Column("vchTerritoryName");
        }
    }
}