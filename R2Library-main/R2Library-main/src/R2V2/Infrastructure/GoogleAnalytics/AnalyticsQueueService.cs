#region

using System;
using System.Configuration;
using System.Diagnostics;
using EasyNetQ;
using EasyNetQ.Topology;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.GoogleAnalytics
{
    public class AnalyticsQueueService
    {
        private readonly ILog<AnalyticsQueueService> _log;
        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly bool _isLocalDevelopment;
        private readonly bool _messageQueueEnabled;

        public AnalyticsQueueService(ILog<AnalyticsQueueService> log, IMessageQueueSettings messageQueueSettings,
            IMessageQueueService messageQueueService)
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _messageQueueService = messageQueueService;

            var isLocalDevConfig = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            _isLocalDevelopment = !string.IsNullOrEmpty(isLocalDevConfig)
                                  && isLocalDevConfig.Equals("true", StringComparison.OrdinalIgnoreCase);

            var messageQueueEnabledConfig = ConfigurationManager.AppSettings["MessageQueue.Enabled"];
            _messageQueueEnabled = string.IsNullOrEmpty(messageQueueEnabledConfig)
                                   || !messageQueueEnabledConfig.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        public bool WriteDataToMessageQueue(GoogleRequestData googleRequestData)
        {
            if (_isLocalDevelopment || !_messageQueueEnabled)
            {
                _log.Debug("Analytics queue disabled in local dev - skipping analytics queue publish.");
                return true;
            }

            var queueName = _messageQueueSettings.AnalyticsQueueName;
            var routeKey = _messageQueueSettings.AnalyticsRouteKey;
            var exchangeName = _messageQueueSettings.AnalyticsExchangeName;
            if (googleRequestData.FailedSaveAttempts >= 10)
            {
                queueName = _messageQueueService.GetFailedQueueName(queueName);
                routeKey = "Failed";
            }

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                using (var advancedBus =
                       RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString).Advanced)
                {
                    var queue = advancedBus.QueueDeclare(queueName);
                    var exchange = advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                    //advancedBus.Bind(exchange, queue, _messageQueueSettings.AnalyticsRouteKey);
                    advancedBus.Bind(exchange, queue, routeKey);
                    //advancedBus.Publish(exchange, _messageQueueSettings.AnalyticsRouteKey, true, false, new Message<string>(googleRequestData.ToJsonString()));
                    advancedBus.Publish(exchange, routeKey, true,
                        new Message<string>(googleRequestData.ToJsonString()));
                }

                stopwatch.Stop();

                if (googleRequestData.FailedSaveAttempts >= 5)
                {
                    _log.ErrorFormat("Request data insert failed too many times. Message sent to {0} in {1} ms - {2}",
                        queueName, stopwatch.ElapsedMilliseconds, googleRequestData.ToJsonString());
                }
                else
                {
                    _log.DebugFormat("Message sent to {0} in {1} ms - {2}", queueName, stopwatch.ElapsedMilliseconds,
                        googleRequestData.ToJsonString());
                }

                return true;
            }
            catch (Exception ex)
            {
                _messageQueueService.WriteMessageToDisk(googleRequestData, queueName);
                _log.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
