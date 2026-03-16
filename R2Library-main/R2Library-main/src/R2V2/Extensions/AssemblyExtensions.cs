#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

#endregion

namespace R2V2.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<T> FindImplementationsOf<T>(this Assembly assembly)
        {
            var scanner = new ImplementationScanner<T>(assembly);

            return scanner.Implementations;
        }

        public static IEnumerable<T> FindImplementationsOf<T>(this IEnumerable<Assembly> assemblies)
        {
            var scanner = new ImplementationScanner<T>(assemblies.ToArray());

            return scanner.Implementations;
        }
    }

    public class ImplementationScanner<T>
    {
        public ImplementationScanner(params Assembly[] assemblies)
        {
            try
            {
                var scanningBuilder = new ContainerBuilder();

                scanningBuilder.RegisterAssemblyTypes(assemblies)
                    .Where(t => typeof(T).IsAssignableFrom(t))
                    .As<T>();

                using (var scanningContainer = scanningBuilder.Build())
                {
                    Implementations = scanningContainer.Resolve<IEnumerable<T>>().ToList();
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                throw new Exception("Scanner Exception: \n {0}\nLoaderExceptions: {1}".Args(
                    e.ToString(),
                    e.LoaderExceptions.AsString(x => x.ToString(), ",\n"), e)
                );
            }
        }

        public IEnumerable<T> Implementations { get; }
    }
}