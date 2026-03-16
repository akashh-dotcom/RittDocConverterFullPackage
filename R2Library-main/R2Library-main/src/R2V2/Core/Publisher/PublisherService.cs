#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Publisher
{
    public class PublisherService : IPublisherService
    {
        private const string PublisherCacheKey = "Publisher.All";
        private readonly IApplicationWideStorageService _applicationWideStorageService;
        private readonly ILog<PublisherService> _log;

        private readonly IQueryable<Publisher> _publishers;
        private readonly IQueryable<PublisherUser> _publisherUsers;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PublisherService(IQueryable<Publisher> publishers
            , IQueryable<PublisherUser> publisherUsers
            , IUnitOfWorkProvider unitOfWorkProvider
            , IApplicationWideStorageService applicationWideStorageService
            , IResourceService resourceService
            , IUnitOfWork unitOfWork
            , ILog<PublisherService> log
        )
        {
            _publishers = publishers;
            _publisherUsers = publisherUsers;
            _unitOfWorkProvider = unitOfWorkProvider;
            _applicationWideStorageService = applicationWideStorageService;
            _resourceService = resourceService;
            _unitOfWork = unitOfWork;
            _log = log;
        }

        public void ClearPublisherCache()
        {
            if (_applicationWideStorageService.Has(PublisherCacheKey))
            {
                _applicationWideStorageService.Remove(PublisherCacheKey);
            }
        }


        /// <summary>
        ///     Get all publisher from cache
        /// </summary>
        public IList<IPublisher> GetActivePublishers(int resourceStatusId)
        {
            var allPublishers = GetPublishers();

            if (resourceStatusId == (int)ResourceStatus.Forthcoming)
            {
                return allPublishers.Where(x => x.RecordStatus)
                    .Where(x => x.ConsolidatedPublisher == null).ToList();
            }

            return allPublishers;
        }

        /// <summary>
        ///     Get all publisher from cache
        /// </summary>
        public IList<IPublisher> GetPublishers()
        {
            var publishers = _applicationWideStorageService.Get<IList<IPublisher>>(PublisherCacheKey);
            if (publishers == null)
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    _unitOfWorkProvider.IncludeSoftDeletedValues();
                    var publishersFromDb = _publishers
                        .Fetch(x => x.ConsolidatedPublisher)
                        .Where(x => x.Name != null && x.Name != "")
                        .Where(x => x.RecordStatus)
                        .OrderBy(x => x.Name.TrimStart())
                        .ToList();

                    publishers = new List<IPublisher>();

                    var resources = _resourceService.GetAllResources().ToList();
                    foreach (var publisher in publishersFromDb)
                    {
                        var cachedPublisher = new CachedPublisher(publisher);
                        cachedPublisher.ResourceCount =
                            resources.Count(x => x.PublisherId == cachedPublisher.Id && !x.NotSaleable);
                        publishers.Add(cachedPublisher);
                    }

                    foreach (var publisher in publishers)
                    {
                        var cachedPublisher = (CachedPublisher)publisher;
                        SetPublisherChildrenResourceCount(cachedPublisher, publishers);
                        SetPublisherParentResourceCount(cachedPublisher, publishers);
                    }

                    _applicationWideStorageService.Put(PublisherCacheKey, publishers);

                    foreach (var publisher in publishersFromDb.ToList())
                    {
                        uow.Evict(publisher);
                    }

                    _unitOfWorkProvider.ExcludeSoftDeletedValues();
                }
            }

            return publishers;
        }


        public Publisher GetPublisherForAdmin(int id)
        {
            var publisher = _publishers.FirstOrDefault(x => x.Id == id);
            if (publisher != null)
            {
                var resources = _resourceService.GetAllResources().ToList();

                var cachedPublisher = new CachedPublisher(publisher);
                cachedPublisher.ResourceCount = resources.Count(x => x.PublisherId == cachedPublisher.Id);

                var publishersFromCache = GetPublishers();


                SetPublisherChildrenResourceCount(cachedPublisher, publishersFromCache);
                SetPublisherParentResourceCount(cachedPublisher, publishersFromCache);

                publisher.ResourceCount = cachedPublisher.ResourceCount;
            }

            return publisher;
        }

        public IPublisher GetPublisher(int id)
        {
            return GetPublishers().FirstOrDefault(x => x.Id == id);
        }


        public void MarkPublisherNotSaleable(int[] publisherIds, IUser currentUser)
        {
            var sql = new StringBuilder()
                .Append(" update tPublisher ")
                .Append(" set dtNotSaleableDate = GetDate() ")
                .AppendFormat(", vchUpdaterId = 'user Id: {0}, [{1}]' ", currentUser.Id, currentUser.FirstName)
                .Append(", dtLastUpdate = GetDate() ")
                .AppendFormat(" where iPublisherId in (");

            for (var i = 0; i < publisherIds.Count(); i++)
            {
                if (i == 0)
                {
                    sql.Append(publisherIds[i]);
                }
                else
                {
                    sql.AppendFormat(",{0}", publisherIds[i]);
                }
            }

            sql.Append(")");

            _log.Debug(sql.ToString());
            _unitOfWork.Session.CreateSQLQuery(sql.ToString()).List();


            sql = new StringBuilder()
                .Append(" update tResource ")
                .Append(" set dtNotSaleableDate = GetDate() ")
                .Append(", NotSaleable = 1 ")
                .AppendFormat(", vchUpdaterId = 'user Id: {0}, [{1}]' ", currentUser.Id, currentUser.FirstName)
                .Append(", dtLastUpdate = GetDate() ")
                .AppendFormat(" where iPublisherId in (");

            for (var i = 0; i < publisherIds.Count(); i++)
            {
                if (i == 0)
                {
                    sql.Append(publisherIds[i]);
                }
                else
                {
                    sql.AppendFormat(",{0}", publisherIds[i]);
                }
            }

            sql.Append(")");

            _log.Debug(sql.ToString());
            _unitOfWork.Session.CreateSQLQuery(sql.ToString()).List();

            ClearPublisherCache();
            _resourceService.GetAllResources(true);
        }

        public IList<IPublisher> GetAdminPublishers()
        {
            var publishers = new List<IPublisher>();

            IList<Publisher> publishersFromDb = _publishers.Fetch(x => x.ConsolidatedPublisher)
                .Where(x => x.Name != null && x.Name != "" && x.RecordStatus).OrderBy(x => x.Name.TrimStart())
                .ToList();

            var resources = _resourceService.GetAllResources().ToList();
            foreach (var cachedPublisher in publishersFromDb.Select(publisher => new CachedPublisher(publisher)))
            {
                cachedPublisher.ResourceCount = resources.Count(x => x.PublisherId == cachedPublisher.Id);
                publishers.Add(cachedPublisher);
            }

            foreach (var publisher in publishers)
            {
                var cachedPublisher = (CachedPublisher)publisher;
                if (cachedPublisher.Id == 79)
                {
                    var test = 1;
                }

                SetPublisherChildrenResourceCount(cachedPublisher, publishers);
            }

            return publishers;
        }

        public void SetPublisherChildrenResourceCount(CachedPublisher cachedPublisher, IList<IPublisher> publishers)
        {
            if (cachedPublisher.ConsolidatedPublisher == null && publishers.Any(x =>
                    x.ConsolidatedPublisher != null && x.ConsolidatedPublisher.Id == cachedPublisher.Id))
            {
                var consolidatedPublishers = publishers.Where(x =>
                    x.ConsolidatedPublisher != null && x.ConsolidatedPublisher.Id == cachedPublisher.Id);
                cachedPublisher.ChildrenResourceCount = consolidatedPublishers.Sum(x => x.ResourceCount);
            }
        }

        public void SetPublisherParentResourceCount(CachedPublisher cachedPublisher, IList<IPublisher> publishers)
        {
            if (cachedPublisher.ConsolidatedPublisher !=
                null) // && publishers.Any(x => x.ConsolidatedPublisher.Id == cachedPublisher.Id))
            {
                var consolidatedPublishers =
                    publishers.Where(x =>
                        x.ConsolidatedPublisher != null &&
                        x.ConsolidatedPublisher.Id == cachedPublisher.ConsolidatedPublisher.Id &&
                        x.Id != cachedPublisher.Id);

                cachedPublisher.ParentResourceCount = consolidatedPublishers.Sum(x => x.ResourceCount);
            }
        }

        public IPublisher AddPublisher(Publisher publisher)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Save(publisher);
                    transaction.Commit();
                    uow.Commit();
                }
            }

            ClearPublisherCache();

            return publisher;
        }

        public bool DeletePublisher(Publisher publisher)
        {
            var consolidatedPublishers = _publishers.Where(x => x.ConsolidatedPublisher.Id == publisher.Id);
            var baseResources = _resourceService.GetAllResources();
            var resources = baseResources.Where(x => x.PublisherId == publisher.Id);
            var publisherUsers = _publisherUsers.Where(x => x.Publisher.Id == publisher.Id);


            if (consolidatedPublishers.Any() || resources.Any())
            {
                return false;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    publisher.RecordStatus = false;

                    uow.Update(publisher);

                    foreach (var user in publisherUsers)
                    {
                        user.RecordStatus = false;
                        uow.Update(user);
                    }

                    transaction.Commit();
                    uow.Commit();
                }
            }

            return true;
        }

        public IList<IPublisher> GetNonConsolidatedPublishers()
        {
            return GetAdminPublishers().Where(x => x.ConsolidatedPublisher == null).OrderBy(x => x.Name).ToList();
        }

        public IList<IPublisher> GetChildPublishers(int consolidatedPublisherId)
        {
            return GetAdminPublishers()
                .Where(x => x.ConsolidatedPublisher != null && x.ConsolidatedPublisher.Id == consolidatedPublisherId)
                .OrderBy(x => x.Name).ToList();
        }

        public List<PublisherUser> GetConsolidatedPublisherUsers(int[] ids)
        {
            _unitOfWorkProvider.IncludeSoftDeletedValues();
            return _publisherUsers.Where(x => ids.Contains(x.Publisher.Id)).ToList();
        }

        public PublisherUser GetConsolidatedPublisherUser(int publisherUserId)
        {
            _unitOfWorkProvider.IncludeSoftDeletedValues();
            return _publisherUsers.FirstOrDefault(x => x.Id == publisherUserId);
        }

        public IPublisher GetFeaturedPublisher()
        {
            return GetPublishers().FirstOrDefault(x => x.IsFeaturedPublisher);
        }

        public void SavePublisherUser(PublisherUser publisherUser)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(publisherUser);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void DeletePublisherUser(PublisherUser publisherUser)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Delete(publisherUser);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void SaveUpdatePublisher(Publisher publisher)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(publisher);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            ClearPublisherCache();
        }

        public void DeletePublisherConsolidation(int publisherId)
        {
            var sql = new StringBuilder()
                .Append("UPDATE tPublisher ")
                .Append("SET iConsolidatedPublisherId = null ")
                .AppendFormat("WHERE iPublisherId = {0}", publisherId)
                .ToString();

            _unitOfWork.Session.CreateSQLQuery(sql).List();

            ClearPublisherCache();
        }
    }
}