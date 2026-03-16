#region

using System;
using System.Diagnostics;
using EasyNetQ;
using EasyNetQ.Topology;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class AutomatedCartQueueService
    {
        private readonly ILog<AutomatedCartQueueService> _log;
        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;

        public AutomatedCartQueueService(
            ILog<AutomatedCartQueueService> log
            , IMessageQueueSettings messageQueueSettings
            , IMessageQueueService messageQueueService)
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _messageQueueService = messageQueueService;
        }

        public bool WriteDataToMessageQueue(AutomatedCartMessage automatedCartMessage)
        {
            var queueName = _messageQueueSettings.AutomatedCartQueueName;
            var routeKey = _messageQueueSettings.AutomatedCartRouteKey;
            var exchangeName = _messageQueueSettings.AutomatedCartExchangeName;
            if (automatedCartMessage.FailedSaveAttempts >= 10)
            {
                queueName = _messageQueueService.GetFailedQueueName(queueName);
                routeKey = "Failed";
            }

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                using (var advancedBus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString)
                           .Advanced)
                {
                    var queue = advancedBus.QueueDeclare(queueName);
                    var exchange = advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                    advancedBus.Bind(exchange, queue, routeKey);
                    advancedBus.Publish(exchange, routeKey, true,
                        new Message<string>(automatedCartMessage.ToJsonString()));
                }

                stopwatch.Stop();

                if (automatedCartMessage.FailedSaveAttempts >= 5)
                {
                    _log.ErrorFormat("Request data insert failed too many times. Message sent to {0} in {1} ms - {2}",
                        queueName,
                        stopwatch.ElapsedMilliseconds, automatedCartMessage.ToJsonString());
                }
                else
                {
                    _log.DebugFormat("Message sent to {0} in {1} ms - {2}", queueName, stopwatch.ElapsedMilliseconds,
                        automatedCartMessage.ToJsonString());
                }

                return true;
            }
            catch (Exception ex)
            {
                _messageQueueService.WriteMessageToDisk(automatedCartMessage, queueName);
                _log.Error(ex.Message, ex);
                return false;
            }
        }
    }
}