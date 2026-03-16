#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Author;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceCacheService
    {
        private const string AllResourcesCacheDirtyKey = "Resources.All.Dirty";
        private const string AllResourcesCacheCleanKey = "Resources.All.Clean";

        private static readonly object ResourceQueryLock = new object();
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        private readonly ILog<ResourceCacheService> _log;
        private readonly ResourceCacheDataService _resourceCacheDataService;

        public ResourceCacheService(
            ILog<ResourceCacheService> log
            , IApplicationWideStorageService applicationWideStorageService
            , ResourceCacheDataService resourceCacheDataService
        )
        {
            _log = log;
            _applicationWideStorageService = applicationWideStorageService;
            _resourceCacheDataService = resourceCacheDataService;
        }

        internal ResourceCache GetResourceCache(bool forceReload, bool waitForReload)
        {
            var resourceCache = _applicationWideStorageService.Get<ResourceCache>(AllResourcesCacheDirtyKey);
            if (!waitForReload && resourceCache != null && !forceReload)
            {
                return resourceCache;
            }

            _log.DebugFormat("GetResourceCache(forceReload: {0}, waitForReload: {1}) - (resourceCache == null): {2}",
                forceReload, waitForReload, resourceCache == null);
            if (forceReload)
            {
                _applicationWideStorageService.Remove(AllResourcesCacheCleanKey);
            }

            resourceCache = _applicationWideStorageService.Get<ResourceCache>(AllResourcesCacheCleanKey);
            if (resourceCache == null || forceReload)
            {
                _log.DebugFormat(
                    "waiting on lock to load resource cache - forceReload: {0}, (resourceCache == null): {1}",
                    forceReload, resourceCache == null);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                lock (ResourceQueryLock)
                {
                    resourceCache = _applicationWideStorageService.Get<ResourceCache>(AllResourcesCacheCleanKey);
                    if (resourceCache == null)
                    {
                        resourceCache = _resourceCacheDataService.GetResourceCache();
                        _applicationWideStorageService.Put(AllResourcesCacheCleanKey, resourceCache);

                        var dirtyResourceCache =
                            _applicationWideStorageService.Get<ResourceCache>(AllResourcesCacheDirtyKey);
                        dirtyResourceCache?.Clear();
                        _applicationWideStorageService.Put(AllResourcesCacheDirtyKey, resourceCache);
                    }
                }

                stopwatch.Stop();
                _log.DebugFormat("GetResourceCache() - cache reload time: {0}", stopwatch.ElapsedMilliseconds);
            }

            return resourceCache;
        }
    }

    public class ResourceCacheDataService
    {
        private static readonly object ResourceQueryLock = new object();
        private readonly IQueryable<IAuthor> _authors;
        private readonly IContentSettings _contentSettings;
        private readonly ILog<ResourceCacheService> _log;
        private readonly IQueryable<IPublisher> _publishers;
        private readonly IQueryable<ResourceCollection> _resourceCollections;
        private readonly IQueryable<ResourceFileDocIds> _resourceFileDocs;
        private readonly IQueryable<ResourcePracticeArea> _resourcePracticeAreas;
        private readonly IQueryable<Resource> _resources;
        private readonly IQueryable<ResourceSpecialty> _resourceSpecialties;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ResourceCacheDataService(
            ILog<ResourceCacheService> log
            , IQueryable<Resource> resources
            , IQueryable<IPublisher> publishers
            , IQueryable<IAuthor> authors
            , IQueryable<ResourcePracticeArea> resourcePracticeAreas
            , IQueryable<ResourceSpecialty> resourceSpecialties
            , IQueryable<ResourceCollection> resourceCollections
            , IQueryable<ResourceFileDocIds> resourceFileDocs
            , IContentSettings contentSettings
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _resources = resources;
            _publishers = publishers;
            _authors = authors;
            _resourcePracticeAreas = resourcePracticeAreas;
            _resourceSpecialties = resourceSpecialties;
            _resourceCollections = resourceCollections;
            _resourceFileDocs = resourceFileDocs;
            _contentSettings = contentSettings;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        internal ResourceCache GetResourceCache()
        {
            ResourceCache resourceCache;
            var stopwatch = new Stopwatch();
            var queryTimer = new Stopwatch();
            stopwatch.Start();
            var queryMsg = new StringBuilder().AppendLine();
            long totalQueryTime = 0;

            lock (ResourceQueryLock)
            {
                _log.Debug("loading resource cache");
                using (var uow = _unitOfWorkProvider.Start())
                {
                    _log.DebugFormat("uow.CacheMode: {0}", uow.CacheMode);

                    uow.CacheMode = CacheMode.Ignore;

                    queryTimer.Start();
                    var publishers = _publishers.OrderBy(x => x.Id).ToDictionary(x => x.Id);
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0:#,###} publishers returned in {1:#,###} ms", publishers.Count,
                        queryTimer.ElapsedMilliseconds).AppendLine();

                    queryTimer.Restart();
                    var authors = _authors.OrderBy(x => x.ResourceId).ToLookup(x => x.ResourceId);
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0:#,###} authors returned in {1:#,###} ms", authors.Count,
                        queryTimer.ElapsedMilliseconds).AppendLine();

                    queryTimer.Restart();
                    var resourcePracticeAreas = _resourcePracticeAreas
                        .Fetch(p => p.PracticeArea)
                        .OrderBy(x => x.ResourceId).ToLookup(x => x.ResourceId);
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0:#,###} resourcePracticeAreas returned in {1:#,###} ms",
                        resourcePracticeAreas.Count, queryTimer.ElapsedMilliseconds).AppendLine();

                    queryTimer.Restart();
                    var resourceSpecialties = _resourceSpecialties
                        .Fetch(s => s.Specialty)
                        .OrderBy(x => x.ResourceId).ToLookup(x => x.ResourceId);
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0:#,###} resourceSpecialties resourceSpecialties in {1:#,###} ms",
                        resourceSpecialties.Count, queryTimer.ElapsedMilliseconds).AppendLine();

                    queryTimer.Restart();
                    var resourceCollections = _resourceCollections
                        .Fetch(s => s.Collection)
                        .OrderBy(x => x.ResourceId).ToLookup(x => x.ResourceId);
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0:#,###} resourceCollections returned in {1:#,###} ms",
                        resourceCollections.Count, queryTimer.ElapsedMilliseconds).AppendLine();

                    queryTimer.Restart();
                    var resources = _resources
                        .OrderBy(x => x.Id).ToDictionary(x => x.Id);
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0} resources returned in {1:#,###} ms", resources.Count,
                        queryTimer.ElapsedMilliseconds).AppendLine();

                    queryTimer.Restart();
                    var fileDocIds = GetAllResourceFileDocIds();
                    queryTimer.Stop();
                    totalQueryTime += queryTimer.ElapsedMilliseconds;
                    queryMsg.AppendFormat("\t++++ {0} fileDocIds returned in {1:#,###} ms", fileDocIds.Count,
                        queryTimer.ElapsedMilliseconds).AppendLine();
                    queryMsg.AppendFormat("\t++++ total query time {0:#,###} ms -- THIS SHOULD BE UNDER 3 SECONDS!",
                        totalQueryTime).AppendLine();

                    _log.Info(queryMsg.ToString());

                    _log.Info(
                        "If the app crashes and this is the last statement in the log, there is most likely a consolidated publisher referencing itself.");

                    resourceCache = new ResourceCache(resources, fileDocIds, _contentSettings, publishers, authors,
                        resourcePracticeAreas,
                        resourceSpecialties, resourceCollections);

                    _log.DebugFormat("resource cache loaded with {0} resources, {1} are New Editions, in {2} ms",
                        resourceCache.GetAllResources().Count(),
                        resourceCache.GetAllNewEditionResources().Count(), stopwatch.ElapsedMilliseconds);

                    // SJS - 10/10/2013 - Added this to make sure these items were being removed from the nhibernate cache,
                    // but it does not see to be working.
                    uow.Clear();
                }

                _log.DebugFormat("after ResourceCache(), {0} ms", stopwatch.ElapsedMilliseconds);
            }

            stopwatch.Stop();
            _log.DebugFormat("GetResourceCache() - cache reload time: {0}", stopwatch.ElapsedMilliseconds);
            return resourceCache;
        }

        private Dictionary<int, ResourceFileDocIds> GetAllResourceFileDocIds()
        {
            return _resourceFileDocs.ToDictionary(r => r.Id);
        }
    }
}