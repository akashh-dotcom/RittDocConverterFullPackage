#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Autofac;
using Common.Logging;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Settings;
using R2V2.WindowsService.Infrastructure.Settings;
using Module = Autofac.Module;

#endregion

namespace R2V2.WindowsService
{
    public static class Bootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public static IContainer Container;

        static Bootstrapper()
        {
            //Empty static constructor causes the static Log variable to be initialized *after* the custom log file has been created.
            //This prevents (null).log file from getting created -DRJ
        }

        public static void Initialize()
        {
            try
            {
                var assemblies = new List<Assembly>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Log.DebugFormat("assembly.FullName: {0}", assembly.FullName);
                    if (assembly.FullName.ToLower().Contains("r2v2") ||
                        assembly.FullName.ToLower().Contains("r2library"))
                    {
                        Log.DebugFormat("adding assembly: {0}", assembly.FullName);
                        assemblies.Add(assembly);
                    }
                }

                Container = ServiceLocatorBuilder.Build(assemblies);

                SettingsInitializer.Initialize(
                    AutoDatabaseSettings.BuildAutoSettings(
                        ConfigurationManager.AppSettings["SettingsConfigurationKey"]));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }


    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>));
        }
    }
}