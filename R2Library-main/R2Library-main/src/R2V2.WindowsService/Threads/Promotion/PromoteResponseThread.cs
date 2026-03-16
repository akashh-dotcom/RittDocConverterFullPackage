#region

using System;
using System.Collections.Generic;
using System.Threading;
using EasyNetQ;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Promotion;
using R2V2.DataAccess;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.WindowsService.Threads.Promotion
{
    public class PromoteResponseThread : PromotionThreadBase, IR2V2Thread
    {
        private readonly ILog<PromoteResponseThread> _log;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly PdaRuleService _pdaRuleService;
        private readonly ResourcePromotionService _resourcePromotionService;

        public PromoteResponseThread(
            ILog<PromoteResponseThread> log
            , IMessageQueueSettings messageQueueSettings
            , ResourcePromotionService resourcePromotionService
            , PdaRuleService pdaRuleService
            , IUnitOfWorkProvider unitOfWorkProvider
        ) : base(unitOfWorkProvider)
        {
            _resourcePromotionService = resourcePromotionService;
            _pdaRuleService = pdaRuleService;
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            StopProcessing = false;
            _log.Debug("PromoteResponseThread initialized");
        }

        public void Start()
        {
            _log.Info("PromoteResponseThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "PromoteResponse" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Info("Start()");
                _log.Error(ex.Message, ex);
            }

            _log.Info("PromoteResponseThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("PromoteResponseThread is now stopping...");
            StopProcessing = true;
            _log.Info("PromoteResponseThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                PromoteResponseProcessor();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                Thread.Sleep(1000);
                throw;
            }
            finally
            {
                _log.Info("StartProcessQueue() <<<");
            }
        }


        private void PromoteResponseProcessor()
        {
            _log.Debug("PromoteResponseProcessor >>");

            using (var bus = RabbitHutch.CreateBus(_messageQueueSettings.ConnectionString))
            {
                var queue = bus.Advanced.QueueDeclare(_messageQueueSettings.PromoteResponseQueueName);
                bus.Advanced.Consume(queue,
                    x => x.Add<PromoteResponse>((message, info) => ProcessPromoteResponseMessage(message.Body)));

                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        /// <summary>
        ///     Handle promotion response requests
        /// </summary>
        private void ProcessPromoteResponseMessage(PromoteResponse promoteResponse)
        {
            _log.Debug(promoteResponse.ToDebugString());

            if (promoteResponse.ResourcePromoteQueue.BatchKey == null)
            {
                _log.ErrorFormat("Promotion BatchKey was null - {0}", promoteResponse.ToDebugString());
                throw new Exception($"Promotion BatchKey was null - {promoteResponse.ToDebugString()}");
            }

            var resourcePromoteQueues =
                _resourcePromotionService.GetResourcePromoteQueue(promoteResponse.ResourcePromoteQueue.BatchKey.Value);

            var promotedResourceIds = new List<int>();
            var completedResourceCount = 0;
            foreach (var resourcePromoteQueue in resourcePromoteQueues)
            {
                _log.Debug(resourcePromoteQueue.ToDebugString());
                if (resourcePromoteQueue.PromoteStatus == ResourcePromoteStatus.CompletedSuccessfully ||
                    resourcePromoteQueue.PromoteStatus == ResourcePromoteStatus.CompletedWithErrors)
                {
                    if (resourcePromoteQueue.PromoteStatus == ResourcePromoteStatus.CompletedSuccessfully)
                    {
                        promotedResourceIds.Add(resourcePromoteQueue.ResourceId);
                    }

                    completedResourceCount++;
                }
            }

            if (resourcePromoteQueues.Count == completedResourceCount)
            {
                // send message to ongoing PDA
                var ongoingPdaEventMessage = new OngoingPdaEventMessage
                {
                    EventType = OngoingPdaEventType.Promotion,
                    Id = CombGuidFactory.NewCombGuid()
                };
                ongoingPdaEventMessage.AddResourceIds(promotedResourceIds);

                _pdaRuleService.WriteOngoingPdaEventMessageToMessageQueue(ongoingPdaEventMessage);
            }
        }
    }
}