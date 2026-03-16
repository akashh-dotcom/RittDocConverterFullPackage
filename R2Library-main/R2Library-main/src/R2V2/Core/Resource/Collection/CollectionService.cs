#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Resource.Collection
{
    public class CollectionService : ICollectionService
    {
        private const string CollectionCacheKey = "Collections.All";
        private readonly IApplicationWideStorageService _applicationWideStorageService;

        private readonly IQueryable<Collection> _collections;
        private readonly ILog<CollectionService> _log;
        private readonly IQueryable<ResourceCollection> _resourceCollections;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public CollectionService(
            IQueryable<Collection> collections
            , IQueryable<ResourceCollection> resourceCollections
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService
            , IResourceService resourceService
            , ILog<CollectionService> log)
        {
            _collections = collections;
            _resourceCollections = resourceCollections;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
            _resourceService = resourceService;
            _log = log;
        }

        public void ClearCache()
        {
            _applicationWideStorageService.Remove(CollectionCacheKey);
        }

        public IEnumerable<ICollection> GetAllCollections()
        {
            var collections = _applicationWideStorageService.Get<IEnumerable<ICollection>>(CollectionCacheKey);

            if (collections == null)
            {
                List<ICollection> list;
                using (_unitOfWorkProvider.Start())
                {
                    var items = _collections.OrderBy(x => x.Name).ToList();
                    list = items.Select(collection => new CachedCollection(collection)).Cast<ICollection>().ToList();

                    collections = list.OrderBy(x => x.Sequence);
                    _applicationWideStorageService.Put(CollectionCacheKey, collections);
                }

                if (list.Any())
                {
                    foreach (var collection in list)
                    {
                        _log.InfoFormat("Collection Id: {0} || Collection Name: {1}", collection.Id, collection.Name);
                    }
                }
            }

            return collections;
        }

        public ICollection GetCollectionById(int collectionId)
        {
            return GetAllCollections().SingleOrDefault(x => x.Id == collectionId);
        }

        public ICollection GetCollectionById(string collectionId)
        {
            int.TryParse(collectionId, out var id);
            return id > 0 ? GetAllCollections().SingleOrDefault(x => x.Id == id) : null;
        }

        public Collection GetCollection(int collectionId)
        {
            return _collections.FirstOrDefault(x => x.Id == collectionId);
        }

        #region "Collection List Management"

        public List<ICollection> GetCollectionLists()
        {
            return GetAllCollections().Where(x => x.IsSpecialCollection).OrderBy(x => x.SpecialCollectionSequence)
                .ToList();
        }

        public ICollection GetPublicCollection()
        {
            var collections = GetCollectionLists();
            return collections.Find(x => x.IsPublic);
        }

        public List<ICollection> GetAllPublicCollections()
        {
            var collections = GetCollectionLists();
            return collections.Where(x => x.IsPublic).ToList();
        }

        public int AddCollection(string collectionName)
        {
            var collection = new Collection();
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        collection.Name = collectionName;
                        collection.RecordStatus = true;
                        collection.IsSpecialCollection = true;

                        var lastSpecialCollectionSequence = _collections
                            .OrderByDescending(x => x.SpecialCollectionSequence).FirstOrDefault();
                        if (lastSpecialCollectionSequence != null)
                        {
                            collection.SpecialCollectionSequence =
                                lastSpecialCollectionSequence.SpecialCollectionSequence + 1;
                        }

                        //Need to manually assign ID because CollectionId is not auto generated. 
                        _unitOfWorkProvider.IncludeSoftDeletedValues();
                        var lastSpecialCollection = _collections.OrderByDescending(x => x.Id).FirstOrDefault();
                        if (lastSpecialCollection != null)
                        {
                            collection.Id = lastSpecialCollection.Id + 1;
                        }

                        _unitOfWorkProvider.ExcludeSoftDeletedValues();

                        uow.Save(collection);
                        uow.Commit();
                        transaction.Commit();

                        if (_applicationWideStorageService.Has(CollectionCacheKey))
                        {
                            _applicationWideStorageService.Remove(CollectionCacheKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return collection.Id;
        }

        public void UpdateCollection(ICollection editedCollection)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var dbCollection = _collections.FirstOrDefault(x => x.Id == editedCollection.Id);
                        if (dbCollection != null)
                        {
                            dbCollection.Name = editedCollection.Name;
                            dbCollection.Description = editedCollection.Description;
                            uow.Save(dbCollection);
                            uow.Commit();
                            transaction.Commit();

                            if (_applicationWideStorageService.Has(CollectionCacheKey))
                            {
                                _applicationWideStorageService.Remove(CollectionCacheKey);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void DeleteSpecialCollection(int collectionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var dbCollection = _collections.FirstOrDefault(x => x.Id == collectionId);
                        if (dbCollection != null)
                        {
                            var resourceCollections =
                                _resourceCollections.Where(x => x.CollectionId == collectionId).ToList();
                            if (resourceCollections.Any())
                            {
                                foreach (var resourceCollection in resourceCollections)
                                {
                                    uow.Delete(resourceCollection);
                                }
                            }

                            uow.Delete(dbCollection);
                            uow.Commit();
                            transaction.Commit();

                            if (_applicationWideStorageService.Has(CollectionCacheKey))
                            {
                                _applicationWideStorageService.Remove(CollectionCacheKey);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void RemoveResourceFromCollection(int collectionId, int resourceId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var resourceCollection = _resourceCollections.FirstOrDefault(x =>
                            x.ResourceId == resourceId && x.CollectionId == collectionId);
                        if (resourceCollection != null)
                        {
                            //resourceCollection.RecordStatus = false;
                            uow.Delete(resourceCollection);
                            uow.Commit();
                            transaction.Commit();

                            if (_applicationWideStorageService.Has(CollectionCacheKey))
                            {
                                _applicationWideStorageService.Remove(CollectionCacheKey);
                            }

                            _resourceService.GetAllResources(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void SaveCollectionListSequence(int[] orderedSequence)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var collections = _collections.Where(x => x.IsSpecialCollection);
                        for (var i = 0; i < orderedSequence.Count(); i++)
                        {
                            var dbCollection = collections.FirstOrDefault(x => x.Id == orderedSequence[i]);
                            if (dbCollection != null)
                            {
                                dbCollection.SpecialCollectionSequence = i + 1;
                                uow.Save(dbCollection);
                            }
                        }

                        uow.Commit();
                        transaction.Commit();
                        if (_applicationWideStorageService.Has(CollectionCacheKey))
                        {
                            _applicationWideStorageService.Remove(CollectionCacheKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void BulkAddResourcesToSpecialCollection(int collectionId, int[] resourceIds)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        foreach (var resourceId in resourceIds)
                        {
                            var resourceCollection = new ResourceCollection
                            {
                                RecordStatus = true,
                                ResourceId = resourceId,
                                CollectionId = collectionId
                            };

                            uow.Save(resourceCollection);
                            uow.Evict(resourceCollection);
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }

                    _resourceService.GetAllResources(true);
                }
            }
        }

        #endregion
    }
}