#region

using System;
using System.Messaging;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using R2V2.Core.OrderHistory;
using R2V2.Core.OrderRelay;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.OrderRelay
{
    public class OrderRelayService
    {
        private readonly ILog<OrderRelayService> _log;
        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly OrderHistoryService _orderHistoryService;
        private readonly PreludeOrderService _preludeOrderService;

        private int _exceptionCount;

        private MessageQueue _messageQueue;

        /// <summary>
        /// </summary>
        public OrderRelayService(ILog<OrderRelayService> log
            , IMessageQueueService messageQueueService
            , IMessageQueueSettings messageQueueSettings
            , PreludeOrderService preludeOrderService
            , OrderHistoryService orderHistoryService
        )
        {
            _log = log;
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings;
            _preludeOrderService = preludeOrderService;
            _orderHistoryService = orderHistoryService;
            Stop = false;
        }

        public bool Stop { get; set; }


        public void OrderProcessor()
        {
            _log.Debug("OrderProcessor >>");

            _messageQueue = GetOrderProcessingQueue();
            _log.DebugFormat("_messageQueue.Path: {0}", _messageQueue.Path);

            var formatter = new XmlMessageFormatter(new[] { typeof(string) });
            _messageQueue.Formatter = formatter;


            while (!Stop)
            {
                _log.Debug("order relay queue waiting ...");

                OrderMessage orderMessage = null;
                try
                {
                    var msg = _messageQueue.Peek();
                    if (msg == null)
                    {
                        _log.Warn("MESSAGE IS NULL!");
                        continue;
                    }

                    _log.DebugFormat("peek id: {0}", msg.Id);
                    var json = (string)msg.Body;
                    _log.DebugFormat("json: {0}", json);
                    orderMessage = JsonConvert.DeserializeObject<OrderMessage>(json);
                    _log.Debug(orderMessage.ToDebugString());

                    if (SendOrderToPrelude(orderMessage))
                    {
                        var tx = new MessageQueueTransaction();
                        tx.Begin();
                        msg = _messageQueue.Receive(tx);
                        if (msg != null)
                        {
                            _log.DebugFormat("received id: {0}", msg.Id);
                            _log.DebugFormat("received body: {0}", msg.Body);
                        }
                        else
                        {
                            _log.Warn("EMAIL MESSAGE QUEUE MESSASGE WAS NULL! (1)");
                        }

                        tx.Commit();
                        _exceptionCount = 0;
                    }
                    else
                    {
                        var tx = new MessageQueueTransaction();
                        tx.Begin();
                        msg = _messageQueue.Receive(tx);
                        if (msg != null)
                        {
                            // re-queue message, update send attempts
                            //orderMessage = (OrderMessage)msg.Body;
                            json = (string)msg.Body;
                            orderMessage = JsonConvert.DeserializeObject<OrderMessage>(json);
                            _log.Debug(orderMessage.ToDebugString());

                            orderMessage.SendAttemptCount++;
                            json = JsonConvert.SerializeObject(orderMessage);
                            _messageQueue.Send(json, tx);

                            _log.DebugFormat("received id: {0}", msg.Id);
                        }
                        else
                        {
                            _log.Warn("ORDER MESSAGE QUEUE MESSAGE WAS NULL! (2)");
                        }

                        tx.Commit();
                        SleepThreadAfterException();
                    }
                }

                catch (Exception ex)
                {
                    if (orderMessage == null)
                    {
                        _log.Error(ex.Message, ex);
                    }
                    else
                    {
                        var errorMsg = new StringBuilder();
                        errorMsg.AppendLine(ex.Message);
                        errorMsg.AppendLine();
                        errorMsg.Append(orderMessage.ToDebugString());
                        _log.Error(errorMsg.ToString(), ex);
                    }

                    var tx = new MessageQueueTransaction();
                    tx.Begin();
                    var msg = _messageQueue.Receive(tx);
                    if (msg != null)
                    {
                        // re-queue message, update send attempts
                        //OrderMessage orderMessageData = (OrderMessage)msg.Body;
                        var json = (string)msg.Body;
                        var orderMessageData = JsonConvert.DeserializeObject<OrderMessage>(json);
                        _log.Debug(orderMessageData.ToDebugString());
                        orderMessageData.SendAttemptCount++;

                        var orderMessageDataJson = JsonConvert.SerializeObject(orderMessageData);

                        _messageQueue.Send(orderMessageDataJson, tx);

                        _log.DebugFormat("received id: {0}", msg.Id);
                        _messageQueue.Send(msg.Body, tx);
                    }
                    else
                    {
                        _log.Warn("EMAIL MESSAGE QUEUE MESSAGE WAS NULL! (3)");
                    }

                    tx.Commit();

                    SleepThreadAfterException();
                }
            }

            _log.Info("STOP REQUESTED");
        }

        private bool SendOrderToPrelude(OrderMessage orderMessage)
        {
            try
            {
                // build order file
                var orderFileText = _preludeOrderService.GenerateOrderFile(orderMessage);
                _log.DebugFormat("orderFileText: {0}", orderFileText);

                var logFileText = new StringBuilder();
                logFileText.AppendLine(orderFileText);
                logFileText.AppendLine();
                logFileText.AppendLine(_preludeOrderService.GenerateOrderFileReadable(orderFileText));

                _orderHistoryService.SavePreludeMessageToOrderHistory(orderMessage.WebOrderNumber,
                    logFileText.ToString());

                // write order file to disk as a CYA - remove this step if the order contains credit card info
                var localFilePath = _preludeOrderService.LogOrderFileToDisk(orderFileText, orderMessage);
                if (localFilePath != null)
                {
                    // sFTP order file to Prelude server
                    if (_preludeOrderService.SendOrderFileToPrelude(orderFileText, orderMessage))
                    {
                        // send order email to rittenhouse
                        _preludeOrderService.SendInternalOrderEmail(orderFileText, orderMessage);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                var errorMsg = new StringBuilder();
                errorMsg.AppendLine(ex.Message);
                errorMsg.AppendLine();
                errorMsg.Append(orderMessage.ToDebugString());

                _log.Error(errorMsg, ex);
            }

            return false;
        }


        private MessageQueue GetOrderProcessingQueue()
        {
            return _messageQueue ?? (_messageQueue =
                _messageQueueService.GetMessageQueue(_messageQueueSettings.OrderProcessingQueue));
        }

        private void SleepThreadAfterException()
        {
            // this method will cause the thread to sleep for a period of time after an exception
            // giving any network related time to recover.
            // minimum sleep time, 1 minute, maximum sleept time 30 minutes.
            if (Stop)
            {
                _log.Debug("do not sleep, stop has been requested!");
                return;
            }

            _exceptionCount++;
            var pauseTimeSpan = new TimeSpan(0, _exceptionCount > 30 ? 30 : _exceptionCount, 0);

            _log.InfoFormat("Pausing thread after exception for {0} minutes ...", pauseTimeSpan.Minutes);
            Thread.Sleep(pauseTimeSpan);
        }
    }
}