#region

using Autofac;
using Autofac.Core;
using R2V2.DataAccess;
using R2V2.DataAccess.DtSearch;
using R2V2.Extensions;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

#endregion

namespace R2Utilities.Infrastructure
{
    public class ServiceLocatorBuilder
    {
        private static IContainer _container;

        public static IContainer Build(IEnumerable<Assembly> assemblies)
        {
            var builder = new ContainerBuilder();

            var arrayOfAssemblies = assemblies.ToArray();

            //Register Modules
            var modules = arrayOfAssemblies.FindImplementationsOf<IModule>();
            foreach (var module in modules)
            {
                builder.RegisterModule(module);
            }

            //Register Types
            builder.RegisterAssemblyTypes(arrayOfAssemblies.ToArray())
                .Where(x => x.GetCustomAttributes(typeof(DoNotRegisterWithContainerAttribute), false).IsEmpty() &&
                            !x.Inherits<IAutoSettings>())
                .DefaultRegistration();

            builder.RegisterAssemblyTypes(arrayOfAssemblies.ToArray())
                .Where(x => x.Inherits<IAutoSettings>()).AsSelf().AsImplementedInterfaces().SingleInstance();

            //Open Generic Registrations
            builder.RegisterGeneric(typeof(NhibernateQueryableFacade<>)).As(typeof(IQueryable<>));
            builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>));

            
            //Build Container
            _container = builder.Build();

            //Set Service Locator - using our own implementation
            ServiceLocator.SetContainer(_container);

            return _container;
        }

        // Provide static access to container for backward compatibility
        public static IContainer Container => _container;
    }
}