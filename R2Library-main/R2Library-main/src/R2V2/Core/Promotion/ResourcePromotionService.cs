#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Topology;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Promotion
{
    public class ResourcePromotionService
    {
        private const string ResourcePromotQueueKey = "R2v2.Resource.Promote.Queue";
        private readonly ILocalStorageService _localStorageService;

        private readonly ILog<ResourcePromotionService> _log;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly IQueryable<ResourcePromoteQueue> _resourcePromoteQueues;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserService _userService;

        public ResourcePromotionService(ILog<ResourcePromotionService> log
            , IMessageQueueSettings messageQueueSettings
            , IQueryable<ResourcePromoteQueue> resourcePromoteQueues
            , IUnitOfWorkProvider unitOfWorkProvider
            , ILocalStorageService localStorageService
            , UserService userService
        )
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _resourcePromoteQueues = resourcePromoteQueues;
            _unitOfWorkProvider = unitOfWorkProvider;
            _localStorageService = localStorageService;
            _userService = userService;
        }

        public bool AddResourceToPromoteQueue(int resourceId, string isbn, IUser user, PromotionType promotionType)
        {
            try
            {
                var resourcePromoteQueue = new ResourcePromoteQueue
                {
                    AddedByUserId = user.Id,
                    ResourceId = resourceId,
                    Isbn = isbn,
                    RecordStatus = true,
                    PromoteStatus = ResourcePromoteStatus.AddedToQueue
                };

                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        uow.Save(resourcePromoteQueue);
                        transaction.Commit();
                        uow.Commit();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public IList<ResourcePromoteQueue> GetResourcePromoteQueue()
        {
            var resourcePromoteQueues = _localStorageService.Get<IList<ResourcePromoteQueue>>(ResourcePromotQueueKey);
            if (resourcePromoteQueues == null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                resourcePromoteQueues = _resourcePromoteQueues.Where(x => x.PromoteInitDate == null)
                    .OrderBy(x => x.CreationDate)
                    .ToList();
                stopwatch.Stop();
                _localStorageService.Put(ResourcePromotQueueKey, resourcePromoteQueues);
                _log.DebugFormat("GetResourcePromoteQueue() << - resourcePromoteQueues.Count: {0} in {1} ms",
                    resourcePromoteQueues.Count, stopwatch.ElapsedMilliseconds);
            }

            return resourcePromoteQueues;
        }

        public IList<ResourcePromoteQueue> GetResourcePromoteQueue(Guid promotionBatchKey)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                IList<ResourcePromoteQueue> resourcePromoteQueues = _resourcePromoteQueues
                    .Where(x => x.BatchKey == promotionBatchKey)
                    .OrderBy(x => x.CreationDate)
                    .ToList();
                stopwatch.Stop();
                _log.DebugFormat("GetResourcePromoteQueue() << - resourcePromoteQueues.Count: {0} in {1} ms",
                    resourcePromoteQueues.Count,
                    stopwatch.ElapsedMilliseconds);

                foreach (var resourcePromoteQueue in resourcePromoteQueues)
                {
                    uow.Evict(resourcePromoteQueue);
                }

                return resourcePromoteQueues;
            }
        }

        public IList<ResourcePromoteQueue> GetResourcePromoteQueueHistory(int page, int pageSize)
        {
            return _resourcePromoteQueues.Where(x => x.PromoteInitDate != null)
                .OrderByDescending(x => x.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetResourcePromoteQueueSize()
        {
            return _resourcePromoteQueues.Count(x => x.PromoteInitDate != null);
        }

        public ResourcePromoteQueue GetLatestResourcePromoteQueue(int resourceId)
        {
            var resourcePromoteQueue = _resourcePromoteQueues.Where(x => x.ResourceId == resourceId)
                .OrderByDescending(x => x.PromoteInitDate).FirstOrDefault();
            return resourcePromoteQueue;
        }

        public bool RemoveResourceFromQueue(int resourceId)
        {
            var resourcePromoteQueue =
                _resourcePromoteQueues.FirstOrDefault(x => x.PromoteInitDate == null && x.ResourceId == resourceId);

            if (resourcePromoteQueue != null)
            {
                resourcePromoteQueue.RecordStatus = false;
                resourcePromoteQueue.PromoteStatus = ResourcePromoteStatus.DeleteFromQueue;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        uow.Save(resourcePromoteQueue);
                        transaction.Commit();
                        uow.Commit();
                    }
                }

                return true;
            }

            return false;
        }

        public bool InitialBatchPromotion(string batchName, int userId)
        {
            var batckKey = Guid.NewGuid();
            var initDate = DateTime.Now;

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var resourcePromoteQueues = GetResourcePromoteQueue();

                    foreach (var resourcePromoteQueue in resourcePromoteQueues)
                    {
                        resourcePromoteQueue.PromoteBatchName = batchName;
                        resourcePromoteQueue.PromoteInitDate = initDate;
                        //resourcePromoteQueue.PromoteStatus = "Batch Initiated";
                        resourcePromoteQueue.PromoteStatus = ResourcePromoteStatus.BatchInitialized;
                        resourcePromoteQueue.PromotedByUserId = userId;
                        resourcePromoteQueue.BatchKey = batckKey;
                        uow.Save(resourcePromoteQueue);
                    }

                    transaction.Commit();
                    uow.Commit();
                    return WriteResourcePromotionToMessageQueue(resourcePromoteQueues);
                }
            }
        }

        public bool IsBatchNameUnique(string batchName)
        {
            return !_resourcePromoteQueues.Any(x => x.PromoteBatchName.ToLower() == batchName.ToLower());
        }

        public bool WriteResourcePromotionToMessageQueue(IList<ResourcePromoteQueue> resourcePromoteQueues)
        {
            var raUsersWhoCanPromote = _userService.GetRaUsersWhoCanPromote();

            var batchKey = Guid.Empty;
            string batchName = null;
            var promoteRequests = new List<PromoteRequest>();
            foreach (var resourcePromoteQueue in resourcePromoteQueues)
            {
                if (batchName == null)
                {
                    batchName = resourcePromoteQueue.PromoteBatchName;
                    batchKey = resourcePromoteQueue.BatchKey == null ? Guid.Empty : resourcePromoteQueue.BatchKey.Value;
                }

                var promoteRequest = new PromoteRequest
                {
                    BatchKey = batchKey,
                    BatchName = batchName,
                    AddedByUser = GetPromoteUser(resourcePromoteQueue.AddedByUserId, raUsersWhoCanPromote),
                    ErrorCount = 0,
                    Isbn = resourcePromoteQueue.Isbn,
                    PromotedByUser =
                        GetPromoteUser(
                            resourcePromoteQueue.PromotedByUserId == null
                                ? 0
                                : resourcePromoteQueue.PromotedByUserId.Value, raUsersWhoCanPromote),
                    //RequestMessageKey = Guid.NewGuid(),
                    //RequestTimestamp = DateTime.Now,
                    ResourceId = resourcePromoteQueue.ResourceId
                };
                promoteRequests.Add(promoteRequest);
            }

            var resourcePromotionMessage = new InitiatePromotionBatch
            {
                BatchKey = batchKey,
                BatchName = batchName,
                StartTimestamp = DateTime.Now,
                PromoteRequests = promoteRequests.ToArray()
            };

            return WriteMessageToQueue(new Message<InitiatePromotionBatch>(resourcePromotionMessage),
                _messageQueueSettings.ResourceBatchPromotionQueueName,
                _messageQueueSettings.ResourceBatchPromotionExchangeName,
                _messageQueueSettings.ResourceBatchPromotionRouteKey);
        }

        public bool WritePromotionRequestToMessageQueue(PromoteRequest promoteRequest)
        {
            return WriteMessageToQueue(new Message<PromoteRequest>(promoteRequest),
                _messageQueueSettings.ResourceBatchPromotionQueueName,
                _messageQueueSettings.ResourceBatchPromotionExchangeName,
                _messageQueueSettings.ResourceBatchPromotionRouteKey);
        }

        public bool WritePromotionBatchCompleteToMessageQueue(PromotionBatchComplete promotionBatchComplete)
        {
            return WriteMessageToQueue(new Message<PromotionBatchComplete>(promotionBatchComplete),
                _messageQueueSettings.ResourceBatchPromotionQueueName,
                _messageQueueSettings.ResourceBatchPromotionExchangeName,
                _messageQueueSettings.ResourceBatchPromotionRouteKey);
        }

        private bool WriteMessageToQueue<T>(IMessage<T> message, string queueName, string exchangeName, string routeKey)
            where T : class
        {
            try
            {
                _log.DebugFormat("queueName: {0}, exchangeName: {1}, routeKey: {2}", queueName, exchangeName, routeKey);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                using (var advancedBus =
                       RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString).Advanced)
                {
                    var queue = advancedBus.QueueDeclare(queueName);
                    var exchange = advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                    advancedBus.Bind(exchange, queue, routeKey);
                    //advancedBus.Publish(exchange, routeKey, true, false, message);
                    advancedBus.Publish(exchange, routeKey, true, message);
                }

                stopwatch.Stop();
                _log.DebugFormat("Message sent to {0} in {1} ms", queueName, stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }
        }

        private PromoteUser GetPromoteUser(int userId, IEnumerable<User> raUsersWhoCanPromote)
        {
            var promoteUser = new PromoteUser { UserId = userId };
            var user = raUsersWhoCanPromote.FirstOrDefault(x => x.Id == userId);
            if (user != null)
            {
                promoteUser.UserEmailAddress = user.Email;
                promoteUser.UserNameFirst = user.FirstName;
                promoteUser.UserNameLast = user.LastName;
            }

            return promoteUser;
        }
    }
}