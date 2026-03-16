#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class FeaturedTitleService : IFeaturedTitleService
    {
        private const string FeaturedResourcesCacheKey = "FeaturedTitles.All";

        private readonly IApplicationWideStorageService _applicationWideStorageService;

        //private readonly ILog<FeaturedTitleService> _log;
        private readonly IQueryable<FeaturedTitle> _featuredTitles;
        private readonly ResourceCacheService _resourceCacheService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public FeaturedTitleService(IQueryable<FeaturedTitle> featuredTitles
            , IApplicationWideStorageService applicationWideStorageService
            , IUnitOfWorkProvider unitOfWorkProvider
            , ResourceCacheService resourceCacheService
        )
        {
            _featuredTitles = featuredTitles;
            _applicationWideStorageService = applicationWideStorageService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _resourceCacheService = resourceCacheService;
        }

        public IEnumerable<IFeaturedTitle> GetFeaturedTitles()
        {
            return GetFeaturedTitles(false, _resourceCacheService.GetResourceCache(false, false));
        }

        public IEnumerable<IFeaturedTitle> GetFeaturedTitles(bool forceReload, ResourceCache resourceCache)
        {
            var allFeaturedTitles =
                _applicationWideStorageService.Get<IList<IFeaturedTitle>>(FeaturedResourcesCacheKey);
            if (allFeaturedTitles == null || forceReload)
            {
                ClearFeaturedTitleCache();
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var featuredTitlesFromDb = _featuredTitles.Where(x => x.RecordStatus).ToList();

                    allFeaturedTitles = new List<IFeaturedTitle>();

                    foreach (var featuredTitle in featuredTitlesFromDb)
                    {
                        var resource = resourceCache.GetResourceById(featuredTitle.ResourceId);
                        var cachedFeaturedTitle = new CachedFeaturedTitle(featuredTitle, resource);

                        allFeaturedTitles.Add(cachedFeaturedTitle);
                    }

                    _applicationWideStorageService.Put(FeaturedResourcesCacheKey, allFeaturedTitles);

                    foreach (var featuredTitle in featuredTitlesFromDb.ToList())
                    {
                        uow.Evict(featuredTitle);
                    }
                }
            }

            return allFeaturedTitles;
        }

        public void SaveFeaturedTitle(FeaturedTitle featuredTitle)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(featuredTitle);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            ClearFeaturedTitleCache();
        }

        public void DeleteFeaturedTitle(FeaturedTitle featuredTitle)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Delete(featuredTitle);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            ClearFeaturedTitleCache();
        }

        public FeaturedTitle GetFeaturedTitleForEdit(int resourceId)
        {
            var title = _featuredTitles.FirstOrDefault(x => x.RecordStatus && x.ResourceId == resourceId);
            return title;
        }

        public IFeaturedTitle GetFeaturedTitle(int resourceId)
        {
            var titles = GetFeaturedTitles();
            return titles.FirstOrDefault(x => x.ResourceId == resourceId);
        }

        private void ClearFeaturedTitleCache()
        {
            var allFeaturedTitles =
                _applicationWideStorageService.Get<IList<IFeaturedTitle>>(FeaturedResourcesCacheKey);
            if (allFeaturedTitles != null)
            {
                allFeaturedTitles.Clear();
            }

            _applicationWideStorageService.Remove(FeaturedResourcesCacheKey);
        }
    }
}