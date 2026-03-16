#region

using System;
using System.Threading;
using EasyNetQ;
using Newtonsoft.Json;
using R2V2.Core.AutomatedCart;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.AutomatedCart
{
    public class AutomatedCartThread : ThreadBase, IR2V2Thread
    {
        private readonly AutomatedCartQueueService _automatedCartQueueService;
        private readonly AutomatedCartService _automatedCartService;
        private readonly ILog<AutomatedCartThread> _log;
        private readonly IMessageQueueSettings _messageQueueSettings;

        public AutomatedCartThread(
            ILog<AutomatedCartThread> log
            , IMessageQueueSettings messageQueueSettings
            , AutomatedCartService automatedCartService
            , AutomatedCartQueueService automatedCartQueueService
        )
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _automatedCartService = automatedCartService;
            _automatedCartQueueService = automatedCartQueueService;
        }

        public void Start()
        {
            _log.Info("AutomatedCartThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "automatedcart" };
                _log.Info("initialized _thread");
                _thread.Start();
                _log.Info("started _thread");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("AnalyticsThread.OnStart() <<<");
        }

        public void Stop()
        {
            _log.Info("AutomatedCartThread is now stopping...");
            StopProcessing = true;
            _log.Info("AutomatedCartThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                AutomatedCartProcessor();
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


        private void AutomatedCartProcessor()
        {
            _log.Debug("AutomatedCartProcessor >>");
            _log.DebugFormat("MessageQueueSettings.EnvironmentConnectionString: {0}",
                _messageQueueSettings.EnvironmentConnectionString);
            _log.DebugFormat("MessageQueueSettings.AutomatedCartQueueName: {0}",
                _messageQueueSettings.AutomatedCartQueueName);

            using (var bus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString))
            {
                var queue = bus.Advanced.QueueDeclare(_messageQueueSettings.AutomatedCartQueueName);

                bus.Advanced.Consume(queue,
                    x => x.Add<string>((message, info) =>
                        ProcessAutomatedCartMessage(
                            JsonConvert.DeserializeObject<AutomatedCartMessage>(message.Body))));


                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        private void ProcessAutomatedCartMessage(AutomatedCartMessage automatedCartMessage)
        {
            _log.Debug($">>>>> {automatedCartMessage.ToJsonString()}");

            if (!_automatedCartService.ProcessAutomatedCartMessage(automatedCartMessage))
            {
                automatedCartMessage.FailedSaveAttempts++;
                _automatedCartQueueService.WriteDataToMessageQueue(automatedCartMessage);
                SleepThreadAfterException(60 * 10 * automatedCartMessage.FailedSaveAttempts);
            }

            _log.Debug($"<<<<< {automatedCartMessage.ToJsonString()}");
        }
    }
}