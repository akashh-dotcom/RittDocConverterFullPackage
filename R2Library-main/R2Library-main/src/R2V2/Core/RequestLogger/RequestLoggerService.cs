#region

using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.RequestLogger
{
    public class RequestLoggerService
    {
        private readonly ILog<RequestLoggerService> _log;
        private readonly MessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly bool _isLocalDevelopment;

        public RequestLoggerService(ILog<RequestLoggerService> log, IMessageQueueSettings messageQueueSettings,
            MessageQueueService messageQueueService)
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _messageQueueService = messageQueueService;
            
            // Check if we're in local development mode
            var isLocalDevConfig = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            _isLocalDevelopment = !string.IsNullOrEmpty(isLocalDevConfig) && 
                                  isLocalDevConfig.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public bool WriteRequestDataToMessageQueue(RequestData requestData)
        {
            // Skip message queue entirely in local development
            if (_isLocalDevelopment)
            {
                _log.Debug($"Request logging disabled in local dev - Request: {requestData.Url}, Duration: {requestData.RequestDuration}ms");
                return true;
            }
            
            var queueName = _messageQueueSettings.RequestLoggingQueueName;
            var routeKey = _messageQueueSettings.RequestLoggingRouteKey;
            var exchangeName = _messageQueueSettings.RequestLoggingExchangeName;
            if (requestData.FailedSaveAttempts >= 10)
            {
                queueName = _messageQueueService.GetFailedQueueName(queueName);
                routeKey = "Failed";
            }

            try
            {
                long createBusTime = 0;
                long queueDeclareTime = 0;
                long declareTime = 0;
                long bindTime = 0;
                long publishTime = 0;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                // Add timeout for RabbitMQ operations in non-local environments
                var task = Task.Run(() =>
                {
                    using (var advancedBus =
                           RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString).Advanced)
                    {
                        createBusTime = stopwatch.ElapsedMilliseconds;

                        var queue = advancedBus.QueueDeclare(queueName);
                        queueDeclareTime = stopwatch.ElapsedMilliseconds - createBusTime;

                        var exchange = advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                        declareTime = stopwatch.ElapsedMilliseconds - createBusTime - queueDeclareTime;

                        advancedBus.Bind(exchange, queue, routeKey);
                        bindTime = stopwatch.ElapsedMilliseconds - createBusTime - queueDeclareTime - declareTime;

                        advancedBus.Publish(exchange, routeKey, true, new Message<RequestData>(requestData));
                        publishTime = stopwatch.ElapsedMilliseconds - createBusTime - queueDeclareTime - declareTime -
                                      bindTime;
                    }
                });
                
                if (!task.Wait(TimeSpan.FromSeconds(5)))
                {
                    _log.Warn($"Request logging timed out after 5 seconds for queue {queueName}");
                    return false;
                }

                stopwatch.Stop();

                var debugTimes =
                    $" --->>> createBusTime: {createBusTime}, queueDeclareTime: {queueDeclareTime}, declareTime: {declareTime}, bindTime: {bindTime}, publishTime: {publishTime}";

                if (requestData.FailedSaveAttempts >= 10)
                {
                    _log.ErrorFormat(
                        "Request data insert failed too many times. Message sent to {0} in {1:00000#} ms - {2}{3}",
                        queueName, stopwatch.ElapsedMilliseconds,
                        requestData.ToJsonString(), debugTimes);
                }
                else
                {
                    //Staging Server is a smaller EC2 and takes a little over a second to complete this request.
                    if (stopwatch.ElapsedMilliseconds < 1000)
                    {
                        _log.DebugFormat("Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                            stopwatch.ElapsedMilliseconds, requestData.ToJsonString(), debugTimes);
                    }
                    else if (stopwatch.ElapsedMilliseconds < 2000)
                    {
                        _log.WarnFormat("Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                            stopwatch.ElapsedMilliseconds, requestData.ToJsonString(), debugTimes);
                    }
                    else
                    {
                        _log.ErrorFormat("Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                            stopwatch.ElapsedMilliseconds, requestData.ToJsonString(), debugTimes);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _messageQueueService.WriteMessageToDisk(requestData, queueName);
                _log.Error(ex.Message, ex);
                _log.DebugFormat("WriteRequestDataToMessageQueue() << RequestId: {0}", requestData.RequestId);
                return false;
            }
        }
    }
}