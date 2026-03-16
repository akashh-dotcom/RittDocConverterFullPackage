#region

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using EasyNetQ;
using EasyNetQ.Topology;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Promotion;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.OngoingPda
{
    public class OngoingPdaThread : ThreadBase, IR2V2Thread
    {
        private readonly ILog<OngoingPdaThread> _log;

        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly OngoingPdaService _ongoingPdaService;
        private readonly PdaRuleService _pdaRuleService;


        /// <summary>
        ///     -debug -service=r2v2.ongoingpdaservice
        /// </summary>
        /// <param name="ongoingPdaService"> </param>
        public OngoingPdaThread(ILog<OngoingPdaThread> log
            , IMessageQueueSettings messageQueueSettings
            , OngoingPdaService ongoingPdaService
            , PdaRuleService pdaRuleService
        )
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _ongoingPdaService = ongoingPdaService;
            _pdaRuleService = pdaRuleService;
            StopProcessing = false;
            _log.Debug("OngoingPdaThread() initialized");
        }

        public void Start()
        {
            _log.Info("OngoingPdaThread.OnStart() >>>");
            try
            {
                // testing
                //WriteTestOngoingPdaEventMessageToMessageQueue();

                _thread = new Thread(StartProcessing) { Name = "ongoingPda" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("OngoingPdaThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("OngoingPdaThread is now stopping...");
            StopProcessing = true;
            _log.Info("OngoingPdaThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                OngoingPdaProcessor();
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


        private void OngoingPdaProcessor()
        {
            _log.Debug("OngoingPdaProcessor >>");
            _log.DebugFormat("MessageQueueSettings.EnvironmentConnectionString: {0}",
                _messageQueueSettings.EnvironmentConnectionString);
            _log.DebugFormat("MessageQueueSettings.OngoingPdaQueueName: {0}",
                _messageQueueSettings.OngoingPdaQueueName);

            using (var bus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString))
            {
                var queue = bus.Advanced.QueueDeclare(_messageQueueSettings.OngoingPdaQueueName);

                bus.Advanced.Consume(queue,
                    x => x.Add<OngoingPdaEventMessage>((message, info) => ProcessOngoingPdaEventMessage(message.Body)));

                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        /// <summary>
        ///     Send a single promotion request message for each resource in the ResourcePromoteQueue.
        /// </summary>
        private void ProcessOngoingPdaEventMessage(OngoingPdaEventMessage ongoingPdaEventMessage)
        {
            _log.DebugFormat(">>>>> {0}", ongoingPdaEventMessage.ToDebugString());

            if (!_ongoingPdaService.ProcessOngoingPdaEvent(ongoingPdaEventMessage))
            {
                ongoingPdaEventMessage.ProcessCount++;

                _pdaRuleService.WriteOngoingPdaEventMessageToMessageQueue(ongoingPdaEventMessage, true);

                SleepThreadAfterException();
            }
            else
            {
                ClearExceptionCounters();
            }

            _log.DebugFormat("<<<<< {0}", ongoingPdaEventMessage.ToDebugString());
        }

        /// <summary>
        ///     This should be used for testing only and should NEVER be included in PRODUCTION CODE!!!
        /// </summary>
        public bool WriteTestOngoingPdaEventMessageToMessageQueue()
        {
            var ongoingPdaEventMessage = new OngoingPdaEventMessage
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(),
                EventType = OngoingPdaEventType.Promotion,
                ProcessCount = 0
            };

            var msg = new StringBuilder();
            msg.AppendLine("IF THIS IS PRODUCTION, PLEASE STOP THE ONGOING PDA SERVER ASAP!");
            msg.AppendLine("---------------------------------------------------------------");
            msg.AppendLine("----------- This should ONLY be used for testing!!! -----------");
            msg.AppendLine("----------- Adding ISBNs: 0128018887 & 1449600115   -----------");
            ongoingPdaEventMessage.AddIsbn("0128018887");
            ongoingPdaEventMessage.AddIsbn("1449600115");
            msg.AppendLine("---------------------------------------------------------------");
            _log.Error(msg.ToString());

            try
            {
                var queueName = _messageQueueSettings.OngoingPdaQueueName;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                using (var advancedBus =
                       RabbitHutch.CreateBus(_messageQueueSettings.ProductionConnectionString).Advanced)
                {
                    var queue = advancedBus.QueueDeclare(queueName);
                    var exchange = advancedBus.ExchangeDeclare(_messageQueueSettings.OngoingPdaExchangeName,
                        ExchangeType.Topic);
                    advancedBus.Bind(exchange, queue, _messageQueueSettings.OngoingPdaRouteKey);
                    //advancedBus.Publish(exchange, _messageQueueSettings.OngoingPdaRouteKey, true, false, new Message<OngoingPdaEventMessage>(ongoingPdaEventMessage));
                    advancedBus.Publish(exchange, _messageQueueSettings.OngoingPdaRouteKey, true,
                        new Message<OngoingPdaEventMessage>(ongoingPdaEventMessage));
                }

                stopwatch.Stop();

                _log.DebugFormat("Message sent to {0} in {1} ms\r\n{2}", queueName, stopwatch.ElapsedMilliseconds,
                    ongoingPdaEventMessage.ToDebugString());
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }
        }
    }
}