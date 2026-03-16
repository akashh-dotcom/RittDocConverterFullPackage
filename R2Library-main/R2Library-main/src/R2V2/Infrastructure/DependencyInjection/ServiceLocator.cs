#region

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

#endregion

namespace R2V2.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Custom service locator implementation to replace Microsoft.Practices.ServiceLocation
    /// This provides backward compatibility while removing the dependency on obsolete packages
    /// </summary>
    public static class ServiceLocator
    {
        private static IContainer _container;

        /// <summary>
        /// Sets the container used by the service locator
        /// </summary>
        public static void SetContainer(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <summary>
        /// Gets the current service locator instance
        /// </summary>
        public static IServiceLocator Current => new AutofacServiceLocator(_container);

        /// <summary>
        /// Service locator interface for dependency resolution
        /// </summary>
        public interface IServiceLocator
        {
            T GetInstance<T>();
            object GetInstance(Type serviceType);
            IEnumerable<T> GetAllInstances<T>();
            IEnumerable<object> GetAllInstances(Type serviceType);
        }

        /// <summary>
        /// Autofac-based implementation of IServiceLocator
        /// </summary>
        private class AutofacServiceLocator : IServiceLocator
        {
            private readonly IContainer _container;

            public AutofacServiceLocator(IContainer container)
            {
                _container = container ?? throw new ArgumentNullException(nameof(container));
            }

            public T GetInstance<T>()
            {
                return _container.Resolve<T>();
            }

            public object GetInstance(Type serviceType)
            {
                return _container.Resolve(serviceType);
            }

            public IEnumerable<T> GetAllInstances<T>()
            {
                return _container.Resolve<IEnumerable<T>>();
            }

            public IEnumerable<object> GetAllInstances(Type serviceType)
            {
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
                var resolved = _container.Resolve(enumerableType);
                return ((System.Collections.IEnumerable)resolved).Cast<object>();
            }
        }
    }
}
