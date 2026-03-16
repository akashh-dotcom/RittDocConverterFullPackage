#region

using System;
using System.Messaging;
using System.Threading;
using EasyNetQ;
using R2V2.Core.RequestLogger;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.MessageQueue;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.RequestLogger
{
    public class RequestLoggerPrototypeThread : ThreadBase, IR2V2Thread
    {
        private readonly ILog<RequestLoggerPrototypeThread> _log;

        private readonly IMessageQueueService _messageQueueService;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly RequestLoggerDataService _requestLoggerService;

        private int _exceptionCount;

        private MessageQueue _messageQueue;

        /// <param name="requestLoggerService"> </param>
        public RequestLoggerPrototypeThread(ILog<RequestLoggerPrototypeThread> log
            , IMessageQueueService messageQueueService
            , IMessageQueueSettings messageQueueSettings
            , RequestLoggerDataService requestLoggerService
        )
        {
            _log = log;
            _messageQueueService = messageQueueService;
            _messageQueueSettings = messageQueueSettings;
            _requestLoggerService = requestLoggerService;
            StopProcessing = false;
            _log.Debug("RequestLoggerThread initialized");
        }

        public void Start()
        {
            _log.Info("RequestLoggerThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "requestLoggerProto" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("RequestLoggerThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("RequestLoggerThread is now stopping...");
            StopProcessing = true;
            _log.Info("RequestLoggerThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                _log.Debug("NEED TO IMPLEMENT LOGIC!!!");
                //_emailRelayService = Bootstrapper.Container.Resolve<EmailRelayService>();
                //_log.Info("_emailRelayService initialized");
                //_emailRelayService.EmailProcessor();

                RequestLoggerProcessor();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                _log.Info("StartProcessQueue() <<<");
            }
        }

        private void RequestLoggerProcessor()
        {
            _log.Debug("RequestLoggerProcessor >>");

            using (var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest"))
            {
                var queue = bus.Advanced.QueueDeclare("Q.Development.RequestData");

                bus.Advanced.Consume(queue,
                    x => x.Add<RequestData>((message, info) => HandleTextMessage(message.Body)));

                while (!StopProcessing)
                {
                    _log.DebugFormat("pausing for 15 seconds...");
                    Thread.Sleep(15000);
                }
            }
        }

        private void HandleTextMessage(RequestData requestData)
        {
            _log.Debug(requestData.ToDebugString());
            Thread.Sleep(3000);
            _log.Debug("done pausing for 3 seconds");
        }
    }
}