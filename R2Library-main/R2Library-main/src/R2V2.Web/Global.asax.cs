#region

using log4net;
using log4net.Config;
using Newtonsoft.Json;
using NHibernate.Cfg.XmlHbmBinding;
using R2V2.Core.Resource;
using R2V2.DataAccess.DtSearch;
using R2V2.Extensions;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Initializers;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Infrastructure;
using R2V2.Web.Infrastructure.Settings;
using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;

#endregion

namespace R2V2.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        protected void Application_Start()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var file = HttpRuntime.AppDomainAppVirtualPath + "/log4net.config";
            file = Server.MapPath(file);
            var fi = new FileInfo(file);
            XmlConfigurator.ConfigureAndWatch(fi);
            GlobalContext.Properties["version"] = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Log.InfoFormat("Application_Start() - log4net init time: {0} ms", stopwatch.ElapsedMilliseconds);

            AreaRegistration.RegisterAllAreas();
            Log.DebugFormat("After RegisterAllAreas() - run time: {0} ms", stopwatch.ElapsedMilliseconds);

            RegisterGlobalFilters(GlobalFilters.Filters);
            Log.DebugFormat("After RegisterGlobalFilters() - run time: {0} ms", stopwatch.ElapsedMilliseconds);

            Bootstrapper.Initialize();


            Log.DebugFormat("After Bootstrapper.Initialize() - run time: {0} ms", stopwatch.ElapsedMilliseconds);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            var initializers = ServiceLocator.Current.GetAllInstances<IInitializer>();
            Log.DebugFormat("After ServiceLocator.Current.GetAllInstances<IInitializer>() - run time: {0} ms",
                stopwatch.ElapsedMilliseconds);

            foreach (var initializer in initializers)
            {
                Log.DebugFormat("Before initializer.Initialize() for {1} - run time: {0} ms",
                    stopwatch.ElapsedMilliseconds, initializer);
                initializer.Initialize();
                Log.DebugFormat("After initializer.Initialize() for {1} - run time: {0} ms",
                    stopwatch.ElapsedMilliseconds, initializer);
            }

            Log.DebugFormat("After initializers.Initialize() loop - run time: {0} ms", stopwatch.ElapsedMilliseconds);

            // Toggle dtSearch at startup via appSettings["DtSearchEnabled"]
            var enabledSetting = ConfigurationManager.AppSettings["DtSearchEnabled"];
            var dtSearchEnabled = string.Equals(enabledSetting, "true", StringComparison.OrdinalIgnoreCase)
                                  || string.IsNullOrEmpty(enabledSetting); // default = enabled


            if (dtSearchEnabled)
            {
                ISearchInitializer searchInitializer = ServiceLocator.Current.GetInstance<SearchInitializer>();
                Log.DebugFormat("After ServiceLocator.Current.GetInstance<SearchInitializer>() - run time: {0} ms",
                    stopwatch.ElapsedMilliseconds);
                try
                {
                    searchInitializer.Init();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }
            else
            {
                Log.DebugFormat("Turning off DTSearch for debugging - run time: {0} ms", stopwatch.ElapsedMilliseconds);
            }


            Log.DebugFormat("After searchInitializer.Init() - run time: {0} ms", stopwatch.ElapsedMilliseconds);
            var contentSettings = ServiceLocator.Current.GetInstance<ContentSettings>();
            LogAccessToDirectory(contentSettings.ContentLocation, "ContentLocation");
            LogAccessToDirectory(contentSettings.DtSearchIndexLocation, "DtSearchIndexLocation");
            LogAccessToDirectory(contentSettings.NewContentLocation, "NewContentLocation");
            LogAccessToDirectory(contentSettings.DtSearchBinLocation, "DtSearchBinLocation");
            LogAccessToDirectory(contentSettings.XslLocation, "XslLocation");
            Log.DebugFormat("After LogAccessToDirectory() - run time: {0} ms", stopwatch.ElapsedMilliseconds);

            var windowsIdentity = WindowsIdentity.GetCurrent();
            Log.DebugFormat("System.Security.Principal.WindowsIdentity.GetCurrent().Name: {0}", windowsIdentity.Name);
            Log.DebugFormat("System.Threading.Thread.CurrentPrincipal.Identity.Name: {0}",
                Thread.CurrentPrincipal.Identity.Name);

            var resourceAccessService = ServiceLocator.Current.GetInstance<IResourceAccessService>();
            resourceAccessService.CleanupResourceLocks();
            Log.DebugFormat("After CleanupResourceLocks() - run time: {0} ms", stopwatch.ElapsedMilliseconds);

            var resourceService = ServiceLocator.Current.GetInstance<IResourceService>();
            resourceService.ValidateAllResourceIsbns();

            var clientSettings = ServiceLocator.Current.GetInstance<ClientSettings>();
            var clientSettingsJson = JsonConvert.SerializeObject(clientSettings, Formatting.Indented);
            Log.Debug($"clientSettingsJson: {clientSettingsJson}");

            var messageQueueSettings = ServiceLocator.Current.GetInstance<IMessageQueueSettings>();
            var messageQueueSettingsJson = JsonConvert.SerializeObject(messageQueueSettings, Formatting.Indented);
            Log.Debug($"messageQueueSettingsJson: {messageQueueSettingsJson}");

            if (dtSearchEnabled)
            {
                try
                {
                    var search = ServiceLocator.Current.GetInstance<ISearch>();
                    var indexStatus = search.GetIndexStatus();
                    Log.Debug(indexStatus.ToDebugString());
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            stopwatch.Stop();
  Log.InfoFormat(">--------> APPLICATION STARTED >--------> - Application_Start() run time: {0} ms",
    stopwatch.ElapsedMilliseconds);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            foreach (var item in Context.Items.OfType<IDisposable>())
            {
                Log.DebugFormat("disposing: {0}", item);
                item.As<IDisposable>().Dispose();
            }

            ServiceLocator.Current.GetInstance<ILocalStorageService>().Dispose();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex != null)
            {
                var typeException = ex.GetType();
                if (typeException == typeof(SerializationException) || typeException == typeof(ProviderException))
                {
                    Log.Error(ex.Message, ex);
                }
                else
                {
                    Log.Warn(ex.Message, ex);
                }
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            Log.DebugFormat("Session_End() - SessionID: {0}", Session.SessionID);
            SessionCleanupService.Clean(Session);
            foreach (string key in Session.Keys)
            {
                try
                {
                    var value = Session[key];
                    if (value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }

            Session.Clear();
        }

        private static void LogAccessToDirectory(string directory, string key)
        {
            var log = ServiceLocator.Current.GetInstance<ILog<MvcApplication>>();

            try
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.Exists)
                {
                    log.InfoFormat("directory '{0}' for key: {1} was found.", directory, key);
                    return;
                }

                log.ErrorFormat("directory '{0}' for key: {1} was NOT found.", directory, key);
            }
            catch (Exception ex)
            {
                var msg = $"{ex.Message}, directory: {directory}, key: {key}";
                log.Error(msg, ex);
            }
        }

        protected void Application_End()
        {
         Log.Info("<--------< APPLICATION ENDING <--------<");
            try
            {
                var runtime =
                    (HttpRuntime)
                    typeof(HttpRuntime).InvokeMember("_theRuntime",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

                if (runtime == null)
                {
                    return;
                }

                var shutDownMessage =
                    (string)
                    runtime.GetType()
                        .InvokeMember("_shutDownMessage",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime,
                            null);

                var shutDownStack =
                    (string)
                    runtime.GetType()
                        .InvokeMember("_shutDownStack",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime,
                            null);

                Log.InfoFormat("_shutDownMessage={0}", shutDownMessage);
                Log.InfoFormat("_shutDownStack={0}", shutDownStack);
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message, ex);
            }
        }
    }
}