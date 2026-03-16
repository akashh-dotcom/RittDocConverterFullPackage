#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using log4net;
using R2V2.Infrastructure.DependencyInjection;
using R2Library.Data.ADO.Config;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks;
using R2V2.Core;
using R2V2.Extensions;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2Utilities
{
    public class Program
    {
        private static ILog _log;

        public static int Main(string[] args)
        {
            GlobalContext.Properties["LogName"] = GetLogFileName(args);
            var programStatus = "WITH NO STATUS TO REPORT!";
            var exitCode = 0;

            Bootstrapper.Initialize();

            _log = LogManager.GetLogger(typeof(Program));
            _log.InfoFormat("R2Utilities {0} -->> STARTED", string.Join(" ", args));

            var initializers = ServiceLocator.Current.GetAllInstances<SettingInitializer>();
            _log.Debug("After ServiceLocator.Current.GetAllInstances<SettingInitializer>();");
            initializers.ForEach(i => i.Initialize());
            _log.Debug("After initializers.Initialize() loop");

            var r2UtilitiesSettings = Bootstrapper.Container.Resolve<IR2UtilitiesSettings>();


            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                LogSetting();
            }

            DbConfigSettings.Settings = new R2DbConfigSettings(r2UtilitiesSettings.R2DatabaseConnection,
                r2UtilitiesSettings.R2UtilitiesDatabaseConnection, r2UtilitiesSettings.R2ReportsConnection);

            ValidateDatabaseConnections();

            var applicationWideStorage = Bootstrapper.Container.Resolve<IApplicationWideStorageService>();

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            Console.WriteLine("");
            Console.WriteLine("R2 Library Utilities - Version: {0}", version);
            Console.WriteLine("==============================================");

            var tasks = Bootstrapper.Container.Resolve<IEnumerable<ITask>>().OrderBy(t => t.TaskGroup)
                .ThenBy(t => t.TaskSwitchSmall).ToList();

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Task Not Specified");
                    programStatus = "WITH ERROR(S)";
                    exitCode = -1;
                }

                var arg = args[0];

                ITask taskToRun = null;

                foreach (var task in tasks)
                {
                    _log.DebugFormat("task.TaskName: {0}", task.TaskName);
                    if ((task.TaskName != null && task.TaskName.Equals(arg, StringComparison.OrdinalIgnoreCase)) ||
                        (task.TaskSwitch != null && task.TaskSwitch.Equals(arg, StringComparison.OrdinalIgnoreCase)) ||
                        (task.TaskSwitchSmall != null &&
                         task.TaskSwitchSmall.Equals(arg, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!task.IsEnabled)
                        {
                            _log.DebugFormat("Task is disabled - {0}", task.TaskName);
                            Console.WriteLine("TASK IS DISABLED! - {0}", task.TaskName);
                            exitCode = -2;
                            break;
                        }

                        taskToRun = task;
                        break;
                    }
                }

                LogSetting();


                if (taskToRun != null)
                {
                    try
                    {
                        // the audit id is used by the UtilitiesAuthenticationContent, which is then used by AuditablePersistenceDecorator to set the CreatedBy or UpdatedBy fields
                        var auditId = $"{taskToRun.TaskSwitchSmall} - {taskToRun.TaskName}";
                        applicationWideStorage.Put("AuthenticationContext.AuditId", auditId);

                        taskToRun.Init(args);
                        taskToRun.Run();
                        taskToRun.TaskResult.CompletedSuccessfully = true;
                        taskToRun.TaskResult.Results = "Successfully Completed.";
                        programStatus = "SUCCESSFULLY";

                        foreach (var step in taskToRun.TaskResult.Steps)
                        {
                            if (!step.CompletedSuccessfully)
                            {
                                taskToRun.TaskResult.CompletedSuccessfully = false;
                                taskToRun.TaskResult.Results = $"Step {step.Id} failed.";
                                programStatus = "WITH ERROR(S)";
                                exitCode = -3;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        taskToRun.TaskResult.Results = $"EXCEPTION: {ex.Message}";
                        taskToRun.TaskResult.CompletedSuccessfully = false;
                        _log.Error(ex.Message, ex);
                        programStatus = $"WITH EXCEPTION: {ex}";
                        exitCode = -4;
                    }
                    finally
                    {
                        taskToRun.Cleanup();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                if (ex.InnerException != null)
                {
                    _log.Error(ex.InnerException.Message, ex.InnerException);
                }

                exitCode = -5;
            }

            Console.WriteLine();
            _log.InfoFormat("R2Utilities {0} COMPLETED {1}", string.Join(" ", args), programStatus);
            return exitCode;
        }

        private static void LogSetting()
        {
            var contentSettings = Bootstrapper.Container.Resolve<IContentSettings>();
            _log.DebugFormat("contentSettings.ContentLocation: {0}", contentSettings.ContentLocation);
            _log.DebugFormat("contentSettings.DtSearchBinLocation: {0}", contentSettings.DtSearchBinLocation);
            _log.DebugFormat("contentSettings.DtSearchIndexLocation: {0}", contentSettings.DtSearchIndexLocation);
            _log.DebugFormat("contentSettings.NewContentLocation: {0}", contentSettings.NewContentLocation);
            _log.DebugFormat("contentSettings.XslLocation: {0}", contentSettings.XslLocation);

            var r2UtilitiesSettings = Bootstrapper.Container.Resolve<IR2UtilitiesSettings>();
            _log.DebugFormat("r2UtilitiesSettings.DefaultFromAddress: {0}", r2UtilitiesSettings.DefaultFromAddress);
            _log.DebugFormat("r2UtilitiesSettings.DefaultFromAddressName: {0}",
                r2UtilitiesSettings.DefaultFromAddressName);
            _log.DebugFormat("r2UtilitiesSettings.EmailConfigDirectory: {0}", r2UtilitiesSettings.EmailConfigDirectory);
            _log.DebugFormat("r2UtilitiesSettings.EnvironmentName: {0}", r2UtilitiesSettings.EnvironmentName);
            _log.DebugFormat("r2UtilitiesSettings.HtmlIndexerBatchSize: {0}", r2UtilitiesSettings.HtmlIndexerBatchSize);
            _log.DebugFormat("r2UtilitiesSettings.HtmlIndexerMaxIndexBatches: {0}",
                r2UtilitiesSettings.HtmlIndexerMaxIndexBatches);
            _log.DebugFormat("r2UtilitiesSettings.IndexEnumerableFields: {0}",
                string.Join(",", r2UtilitiesSettings.IndexEnumerableFields));
            _log.DebugFormat("r2UtilitiesSettings.IndexStoredFields: {0}",
                string.Join(",", r2UtilitiesSettings.IndexStoredFields));
            _log.DebugFormat("r2UtilitiesSettings.R2DatabaseConnection: {0}", r2UtilitiesSettings.R2DatabaseConnection);
            _log.DebugFormat("r2UtilitiesSettings.R2UtilitiesDatabaseConnection: {0}",
                r2UtilitiesSettings.R2UtilitiesDatabaseConnection);
        }

        private static void ValidateDatabaseConnections()
        {
            var r2UtilitiesSettings = Bootstrapper.Container.Resolve<IR2UtilitiesSettings>();
            _log.DebugFormat("r2UtilitiesSettings.R2DatabaseConnection: {0}", r2UtilitiesSettings.R2DatabaseConnection);
            _log.DebugFormat("r2UtilitiesSettings.R2UtilitiesDatabaseConnection: {0}",
                r2UtilitiesSettings.R2UtilitiesDatabaseConnection);

            var pingService = Bootstrapper.Container.Resolve<PingService>();
            var pingStatusCode = pingService.GetPingValue();
            _log.DebugFormat("pingStatusCode: {0}", pingStatusCode);
        }

        private static string GetLogFileName(string[] args)
        {
            var logFileName = args.Length > 0 ? args[0].Replace("-", "") : "_PickList_";
            var logFileSuffix = args.FirstOrDefault(a =>
                a.StartsWith("-logFileSuffix=", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(logFileSuffix))
            {
                logFileName = $"{logFileName}_{logFileSuffix.Replace("-logFileSuffix=", "")}";
            }

            return logFileName;
        }
    }
}