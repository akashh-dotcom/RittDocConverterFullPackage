#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using R2V2.DataAccess;
using R2V2.Extensions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Infrastructure.DependencyInjection
{
    public class ServiceLocatorBuilder
    {
        private static IContainer _container;

        public static IContainer Build(IEnumerable<Assembly> assemblies)
        {
            var builder = new ContainerBuilder();

            //Register Modules
            var modules = assemblies.FindImplementationsOf<IModule>();
            foreach (var module in modules)
            {
                builder.RegisterModule(module);
            }

            //Register Types
            builder.RegisterAssemblyTypes(assemblies.ToArray())
         .Where(x => x.GetCustomAttributes(typeof(DoNotRegisterWithContainerAttribute), false).IsEmpty() &&
         !x.Inherits<IAutoSettings>() &&
                x.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any()) // Exclude types without public constructors (e.g., static factory pattern classes)
                 .DefaultRegistration();

            builder.RegisterAssemblyTypes(assemblies.ToArray())
          .Where(x => x.Inherits<IAutoSettings>()).AsSelf().AsImplementedInterfaces().SingleInstance();

            //Open Generic Registrations
            builder.RegisterGeneric(typeof(NhibernateQueryableFacade<>)).As(typeof(IQueryable<>));
            builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>));

            //AutoDbSettings.Initialize(settingsConfigurationKey);

            //Build Container
            _container = builder.Build();

            //Set Service Locator - now using our own implementation
            ServiceLocator.SetContainer(_container);

            return _container;
        }

        // Provide static access to container for backward compatibility
        public static IContainer Container => _container;
    }
}