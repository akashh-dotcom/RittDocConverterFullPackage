#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Territory
{
    public interface ITerritoryService
    {
        IList<ITerritory> GetAllTerritories();
        ITerritory GetTerritory(int id);
        IList<ITerritory> GetTerritories(int[] territoryIds);
        IList<ITerritory> GetTerritories(string[] territoryCodes);
    }
}