#region

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Infrastructure.MvcFramework;
using R2V2.Web.Infrastructure.Settings;
//using R2V2.Infrastructure.Logging.Loggers;
using Module = Autofac.Module;

#endregion

namespace R2V2.Web.Infrastructure
{
    public static class Bootstrapper
    {
        public static void Initialize()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.ToLower().Contains("r2v2"))
                .Union(new[] { Assembly.GetExecutingAssembly() });
            var container = ServiceLocatorBuilder.Build(assemblies);

            //Pass in container.BeginLifetimeScope() to fix Autofac memory leak issue -DRJ
            DependencyResolver.SetResolver(new R2V2DependencyResolver(container.BeginLifetimeScope()));

            SettingsInitializer.Initialize(
                AutoDatabaseSettings.BuildAutoSettings(ConfigurationManager.AppSettings["SettingsConfigurationKey"]));
        }
    }

    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
        }
    }
}