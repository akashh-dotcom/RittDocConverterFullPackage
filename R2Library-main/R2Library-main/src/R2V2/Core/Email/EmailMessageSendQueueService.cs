#region

using System;
using System.Diagnostics;
using EasyNetQ;
using EasyNetQ.Topology;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Email
{
    public class EmailMessageSendQueueService
    {
        private readonly ILog<EmailMessageSendQueueService> _log;
        private readonly MessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;

        public EmailMessageSendQueueService(ILog<EmailMessageSendQueueService> log,
            IMessageQueueSettings messageQueueSettings, MessageQueueService messageQueueService)
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _messageQueueService = messageQueueService;
        }

        public bool WriteEmailMessageToMessageQueue(EmailMessage emailMessage)
        {
            var queueName = _messageQueueSettings.EmailMessageQueueName;
            var routeKey = _messageQueueSettings.EmailMessageRouteKey;
            var exchangeName = _messageQueueSettings.EmailMessageExchangeName;
            if (emailMessage.SendAttempts >= 10)
            {
                queueName = _messageQueueService.GetFailedQueueName(queueName);
                routeKey = "Failed";
            }

            _log.DebugFormat("WriteEmailMessageToMessageQueue() >> MessageId: {0}", emailMessage.MessageId);
            try
            {
                long createBusTime;
                long queueDeclareTime;
                long declareTime;
                long bindTime;
                long publishTime;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
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

                    advancedBus.Publish(exchange, routeKey, true, new Message<EmailMessage>(emailMessage));
                    publishTime = stopwatch.ElapsedMilliseconds - createBusTime - queueDeclareTime - declareTime -
                                  bindTime;
                }

                stopwatch.Stop();

                var debugTimes =
                    $" --->>> createBusTime: {createBusTime}, queueDeclareTime: {queueDeclareTime}, declareTime: {declareTime}, bindTime: {bindTime}, publishTime: {publishTime}";

                if (emailMessage.SendAttempts >= 10)
                {
                    _log.ErrorFormat(
                        "Email message failed too many times. Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                        stopwatch.ElapsedMilliseconds,
                        emailMessage.ToJsonString(), debugTimes);
                }
                else
                {
                    if (stopwatch.ElapsedMilliseconds < 50)
                    {
                        _log.DebugFormat("Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                            stopwatch.ElapsedMilliseconds, emailMessage.ToJsonString(), debugTimes);
                    }
                    else if (stopwatch.ElapsedMilliseconds < 750)
                    {
                        _log.WarnFormat("Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                            stopwatch.ElapsedMilliseconds, emailMessage.ToJsonString(), debugTimes);
                    }
                    else
                    {
                        _log.ErrorFormat("Message sent to {0} in {1:00000#} ms - {2}{3}", queueName,
                            stopwatch.ElapsedMilliseconds, emailMessage.ToJsonString(), debugTimes);
                    }
                }

                _log.DebugFormat("WriteEmailMessageToMessageQueue() << MessageId: {0}", emailMessage.MessageId);
                return true;
            }
            catch (Exception ex)
            {
                _messageQueueService.WriteMessageToDisk(emailMessage, queueName);
                _log.Error(ex.Message, ex);
                _log.DebugFormat("WriteEmailMessageToMessageQueue() << MessageId: {0}", emailMessage.MessageId);
                return false;
            }
        }
    }
}