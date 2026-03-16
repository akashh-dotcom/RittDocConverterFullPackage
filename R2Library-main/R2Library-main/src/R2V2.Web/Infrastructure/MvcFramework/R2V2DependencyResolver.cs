#region

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Autofac;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework
{
    public class R2V2DependencyResolver : IDependencyResolver
    {
        private readonly ILifetimeScope _container;

        public R2V2DependencyResolver(ILifetimeScope container)
        {
            _container = container;
        }

        //This was added to fix Autofac memory leak issue -DRJ
        private ILifetimeScope Scope
        {
            get
            {
                if (HttpContext.Current.Items["AutofacLifetimeScope"] == null)
                {
                    HttpContext.Current.Items["AutofacLifetimeScope"] = _container.BeginLifetimeScope();
                }

                return (ILifetimeScope)HttpContext.Current.Items["AutofacLifetimeScope"];
            }
        }

        public object GetService(Type serviceType)
        {
            return Scope.ResolveOptional(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = Scope.Resolve(enumerableServiceType);

            return (IEnumerable<object>)instance;
        }
    }
}