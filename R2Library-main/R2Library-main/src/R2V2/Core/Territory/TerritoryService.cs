#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Territory
{
    public class TerritoryService : ITerritoryService
    {
        private const string TerritoriesAllKey = "Territories.All";

        private static readonly object LockObject = new object();
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        private readonly ILog<TerritoryService> _log;
        private readonly IQueryable<Territory> _territories;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public TerritoryService(ILog<TerritoryService> log
            , IQueryable<Territory> territories
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService)
        {
            _log = log;
            _territories = territories;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
        }

        public IList<ITerritory> GetAllTerritories()
        {
            var territories = _applicationWideStorageService.Get<IList<ITerritory>>(TerritoriesAllKey);

            if (territories == null)
            {
                _log.Debug("waiting on lock to load territories cache");
                lock (LockObject)
                {
                    territories = _applicationWideStorageService.Get<IList<ITerritory>>(TerritoriesAllKey);
                    if (territories == null)
                    {
                        _log.Debug("loading territories cache");
                        territories = new List<ITerritory>();
                        using (var uow = _unitOfWorkProvider.Start())
                        {
                            var list = _territories.Where(x => x.RecordStatus);

                            foreach (var territory in list.ToList())
                            {
                                territories.Add(new CachedTerritory(territory));
                            }
                        }

                        _applicationWideStorageService.Put(TerritoriesAllKey, territories);
                    }
                }
            }

            return territories;
        }

        public ITerritory GetTerritory(int id)
        {
            var territories = GetAllTerritories();
            return territories.FirstOrDefault(x => x.Id == id);
        }

        public IList<ITerritory> GetTerritories(int[] territoryIds)
        {
            var territories = GetAllTerritories();
            return territories.Where(x => territoryIds.Contains(x.Id)).ToList();
        }

        public IList<ITerritory> GetTerritories(string[] territoryCodes)
        {
            var territories = GetAllTerritories();
            return territories.Where(x => territoryCodes.Contains(x.Code)).ToList();
        }
    }
}