#region

using System;
using System.Threading;
using EasyNetQ;
using Newtonsoft.Json;
using R2V2.Infrastructure.GoogleAnalytics;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.GoogleAnalytics
{
    public class AnalyticsThread : ThreadBase, IR2V2Thread
    {
        private readonly AnalyticsQueueService _analyticsQueueService;
        private readonly AnalyticsService _analyticsService;
        private readonly ILog<AnalyticsThread> _log;
        private readonly IMessageQueueSettings _messageQueueSettings;
        private readonly IWindowsServiceSettings _windowsServiceSettings;

        public AnalyticsThread(ILog<AnalyticsThread> log, IMessageQueueSettings messageQueueSettings,
            AnalyticsService analyticsService,
            AnalyticsQueueService analyticsQueueService, IWindowsServiceSettings windowsServiceSettings)
        {
            _log = log;
            _messageQueueSettings = messageQueueSettings;
            _analyticsService = analyticsService;
            _analyticsQueueService = analyticsQueueService;
            _windowsServiceSettings = windowsServiceSettings;
            StopProcessing = false;
            _log.Debug("AnalyticsThread() initialized");
        }

        public void Start()
        {
            _log.Info("AnalyticsThread.OnStart() >>>");
            try
            {
                _thread = new Thread(StartProcessing) { Name = "analytics" };
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
            _log.Info("AnalyticsThread is now stopping...");
            StopProcessing = true;
            _log.Info("AnalyticsThread STOPPED");
        }

        public void StartProcessing()
        {
            _log.Info("StartProcessQueue() >>>");
            try
            {
                AnalyticsProcessor();
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

        private void AnalyticsProcessor()
        {
            _log.Debug("AnalyticsProcessor >>");
            _log.DebugFormat("MessageQueueSettings.EnvironmentConnectionString: {0}",
                _messageQueueSettings.EnvironmentConnectionString);
            _log.DebugFormat("MessageQueueSettings.AnalyticsQueueName: {0}", _messageQueueSettings.AnalyticsQueueName);

            using (var bus = RabbitHutch.CreateBus(_messageQueueSettings.EnvironmentConnectionString))
            {
                var queue = bus.Advanced.QueueDeclare(_messageQueueSettings.AnalyticsQueueName);

                bus.Advanced.Consume(queue,
                    x => x.Add<string>((message, info) =>
                        ProcessAnalyticsEvent(JsonConvert.DeserializeObject<GoogleRequestData>(message.Body))));

                while (!StopProcessing)
                {
                    Thread.Sleep(1000);
                }
            }

            _log.Info("STOP REQUESTED");
        }

        /// <summary>
        ///     Will attempt to log Event up to 5 times.
        /// </summary>
        private void ProcessAnalyticsEvent(GoogleRequestData googleRequestData)
        {
            _log.DebugFormat(">>>>> {0}", googleRequestData.ToDebugString());

            //Need to keep trying. Cannot send data out of order. Messes up shopping cart checkouts if impressions were not logged before adding to cart.
            if (!_analyticsService.ProcessGoogleRequestData(googleRequestData))
            {
                googleRequestData.FailedSaveAttempts++;
                _analyticsQueueService.WriteDataToMessageQueue(googleRequestData);
                _log.InfoFormat("Pause for {0:#,###} seconds",
                    _windowsServiceSettings.GoogleAnalyticsSecondsToPauseAfterException);
                SleepThreadAfterException(_windowsServiceSettings.GoogleAnalyticsSecondsToPauseAfterException);
                //Thread.Sleep(50);
            }

            _log.DebugFormat("<<<<< {0}", googleRequestData.ToDebugString());
        }
    }
}