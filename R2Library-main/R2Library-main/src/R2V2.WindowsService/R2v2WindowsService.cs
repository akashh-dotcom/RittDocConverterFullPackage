#region

using System;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Autofac;
using log4net;
using log4net.Config;
using log4net.Util;

using R2V2.Infrastructure.DependencyInjection;
using R2Library.Data.ADO.Config;
using R2V2.Extensions;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.WindowsService.Infrastructure.Settings;
using R2V2.WindowsService.Threads;
using R2V2.WindowsService.Threads.AutomatedCart;
using R2V2.WindowsService.Threads.Email;
using R2V2.WindowsService.Threads.GoogleAnalytics;
using R2V2.WindowsService.Threads.OngoingPda;
using R2V2.WindowsService.Threads.OrderRelay;
using R2V2.WindowsService.Threads.Promotion;
using R2V2.WindowsService.Threads.RequestLogger;
using ILog = Common.Logging.ILog;
using LogManager = Common.Logging.LogManager;

#endregion

namespace R2V2.WindowsService
{
    public partial class R2v2WindowsService : ServiceBase
    {
        private static ILog _log;
        private static string _serviceName = "undefined";
        private IR2V2Thread _workerThread;

        public R2v2WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Init();
        }

        public void Init()
        {
            try
            {
                Console.WriteLine("Init() >>");

                var args = Environment.GetCommandLineArgs();

                foreach (var arg in args)
                {
                    Console.WriteLine(arg);
                }

                _serviceName = GetServiceName(args);
                Console.WriteLine("_serviceName: " + _serviceName);

                GlobalContext.Properties["LogName"] = _serviceName ?? "R2v2.WindowsService";
                XmlConfigurator.Configure();
                LogLog.InternalDebugging = true;
                _log = LogManager.GetLogger(typeof(Program));
                _log.ErrorFormat("R2v2 Windows [{1}] Started on {0}", Environment.MachineName,
                    _serviceName ?? "R2v2.WindowsService");

                _log.DebugFormat("Start Parameters: {0}", string.Join(", ", args));

                if (_serviceName == null)
                {
                    throw new Exception("Missing required start parameter: -service=[serviceName]");
                }

                Bootstrapper.Initialize();

                var initializers = ServiceLocator.Current.GetAllInstances<SettingInitializer>();
                _log.Debug("After ServiceLocator.Current.GetAllInstances<SettingInitializer>();");
                initializers.ForEach(i => i.Initialize());
                _log.Debug("After initializers.Initialize() loop");

                var messageQueueSettings = Bootstrapper.Container.Resolve<IMessageQueueSettings>();
                _log.DebugFormat("messageQueueSettings.EmailMessageQueue: {0}", messageQueueSettings.EmailMessageQueue);
                _log.DebugFormat("messageQueueSettings.OrderProcessingQueue: {0}",
                    messageQueueSettings.OrderProcessingQueue);
                _log.DebugFormat("messageQueueSettings.ProductionConnectionString:  {0}",
                    messageQueueSettings.ProductionConnectionString);
                _log.DebugFormat("messageQueueSettings.EnvironmentConnectionString: {0}",
                    messageQueueSettings.EnvironmentConnectionString);
                _log.DebugFormat("messageQueueSettings.SendErrorDirectoryPath: {0}",
                    messageQueueSettings.SendErrorDirectoryPath);
                _log.DebugFormat("Analytics - QueueName: {0}, RouteKey: {1}, ExchangeName: {2}",
                    messageQueueSettings.AnalyticsQueueName,
                    messageQueueSettings.AnalyticsRouteKey, messageQueueSettings.AnalyticsExchangeName);
                _log.DebugFormat("OngoingPda - QueueName: {0}, RouteKey: {1}, ExchangeName: {2}",
                    messageQueueSettings.OngoingPdaQueueName,
                    messageQueueSettings.OngoingPdaRouteKey, messageQueueSettings.OngoingPdaExchangeName);
                _log.DebugFormat("RequestLogging - QueueName: {0}, RouteKey: {1}, ExchangeName: {2}",
                    messageQueueSettings.RequestLoggingQueueName,
                    messageQueueSettings.RequestLoggingRouteKey, messageQueueSettings.RequestLoggingExchangeName);
                _log.DebugFormat("ResourceBatchPromotion - QueueName: {0}, RouteKey: {1}, ExchangeName: {2}",
                    messageQueueSettings.ResourceBatchPromotionQueueName,
                    messageQueueSettings.ResourceBatchPromotionRouteKey,
                    messageQueueSettings.ResourceBatchPromotionExchangeName);

                var windowsServiceSettings = Bootstrapper.Container.Resolve<WindowsServiceSettings>();
                DbConfigSettings.Settings = new R2DbConfigSettings(
                    windowsServiceSettings.RIT001ProductionConnectionString,
                    windowsServiceSettings.R2UtilitiesProductionConnectionString,
                    windowsServiceSettings.R2ReportsConnectionString);

                // the audit id is used by the UtilitiesAuthenticationContent, which is then used by AuditablePersistenceDecorator to set the CreatedBy or UpdatedBy fields
                var applicationWideStorage = Bootstrapper.Container.Resolve<IApplicationWideStorageService>();
                applicationWideStorage.Put("AuthenticationContext.AuditId", _serviceName);

                _workerThread = GetWorkerThread(_serviceName);
                _workerThread.Start();
                _log.DebugFormat("{0} started ...", _workerThread.GetType().Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        private static string GetServiceName(string[] args)
        {
            var arg = args.SingleOrDefault(a => a.StartsWith("-service="));

            if (arg != null)
            {
                return arg.Split('=')[1];
            }

            return null;
        }

        private static IR2V2Thread GetWorkerThread(string serviceName)
        {
            var name = serviceName.ToLower();
            switch (name)
            {
                case "r2v2.emailmessageservice":
                    return Bootstrapper.Container.Resolve<EmailThread>();
                case "r2v2.ongoingpdaservice":
                    return Bootstrapper.Container.Resolve<OngoingPdaThread>();
                case "r2v2.orderprocessingservice":
                    return Bootstrapper.Container.Resolve<OrderRelayThread>();
                case "r2v2.promotionservice":
                    return Bootstrapper.Container.Resolve<ResourcePromotionThread>();
                case "r2v2.requestloggingservice":
                    return Bootstrapper.Container.Resolve<RequestLoggerThread>();
                case "r2v2.analyticsservice":
                    return Bootstrapper.Container.Resolve<AnalyticsThread>();
                case "r2v2.rabbitmqemailservice":
                    return Bootstrapper.Container.Resolve<RabbitMqEmailThread>();
                case "r2v2.automatedcartservice":
                    return Bootstrapper.Container.Resolve<AutomatedCartThread>();
                default:
                    throw new Exception($"Invalid service name specified: {serviceName}");
            }
        }

        protected override void OnStop()
        {
            _log.InfoFormat("Service '{0}' is now stopping...", _serviceName);
            //_stop = true;
            _workerThread.Stop();
            Thread.Sleep(1000);
            _log.ErrorFormat(
                "Service '{0}' has stopped! --- Is this a plan outage? If no, everyone who get this message should be investigating why this service stopped ASAP!",
                _serviceName);
        }

        public void LogThreadStatus(Thread thread)
        {
            _log.InfoFormat("{0} - IsAlive: {1}", thread.Name, thread.IsAlive);
            if (!thread.IsAlive)
            {
                var msg = new StringBuilder()
                    .AppendLine("THREAD IS DEAD!")
                    .AppendFormat("Thread Name: {0}", thread.Name).AppendLine()
                    .AppendLine("This windows service needs to be restarted in order for this thread to restart.")
                    .AppendLine("One or more messages within the queue is probably invalid.");
                _log.Error(msg.ToString());
            }
        }
    }
}