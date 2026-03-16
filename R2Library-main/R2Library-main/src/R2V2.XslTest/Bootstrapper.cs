#region

using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using R2V2.Infrastructure.DependencyInjection;

#endregion

namespace R2V2.XslTest
{
    internal class Bootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        internal static void Initialize(string settingsConfigurationKey)
        {
            try
            {
                var assemblies = new List<Assembly>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Log.DebugFormat("assembly.FullName: {0}", assembly.FullName);
                    if (!assembly.FullName.ToLower().Contains("r2v2") &&
                        !assembly.FullName.ToLower().Contains("r2utilities"))
                        continue;
                    Log.DebugFormat("adding assembly: {0}", assembly.FullName);
                    assemblies.Add(assembly);
                }

                ServiceLocatorBuilder.Build(assemblies);
                //AutoDatabaseSettings.Initialize(settingsConfigurationKey);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}