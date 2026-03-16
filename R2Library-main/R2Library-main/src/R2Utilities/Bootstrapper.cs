#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Autofac;
using Common.Logging;
using R2Utilities.Infrastructure;
using R2Utilities.Infrastructure.Settings;
using R2V2.Infrastructure.Settings;
using Module = Autofac.Module;

#endregion

namespace R2Utilities
{
    public static class Bootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public static IContainer Container;

        static Bootstrapper()
        {
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
                        assembly.FullName.ToLower().Contains("r2utilities"))
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

        public class InfrastructureModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                //builder.RegisterGeneric(typeof(SecurityLogger<>)).As(typeof(ISecurityLogger<>));
                //builder.RegisterGeneric(typeof(PerformanceLogger<>)).As(typeof(IPerformanceLogger<>));
                //builder.RegisterGeneric(typeof(FatalLogger<>)).As(typeof(IFatalLogger<>));
                //builder.RegisterGeneric(typeof(ErrorLogger<>)).As(typeof(IErrorLogger<>));
                //builder.RegisterGeneric(typeof(WarningLogger<>)).As(typeof(IWarningLogger<>));
                //builder.RegisterGeneric(typeof(InformationLogger<>)).As(typeof(IInformationLogger<>));
                //builder.RegisterGeneric(typeof(DebugLogger<>)).As(typeof(IDebugLogger<>));
            }
        }
    }
}