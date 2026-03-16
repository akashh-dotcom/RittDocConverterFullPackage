#region

using Autofac;
using FluentNHibernate.Cfg;
using log4net;

using R2V2.Infrastructure.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;
using R2V2.DataAccess.NHibernateMaps;
using R2V2.Extensions;
using R2V2.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Module = Autofac.Module;

#endregion

namespace R2V2.Infrastructure.Modules
{
    public class NHibernateModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => BuildConfiguration(x)).As<Configuration>().SingleInstance();
            builder.Register(x =>
                {
                    var cfg = x.Resolve<Configuration>();
                    return BuildSessionFactory(cfg);
                })
                .As<ISessionFactory>().SingleInstance();

            // Register IUnitOfWorkProvider which implements IUnitOfWork
            // This is required for NhibernateQueryableFacade<T> to resolve IUnitOfWork
            builder.RegisterType<UnitOfWorkProvider>()
                .As<IUnitOfWorkProvider>()
                .As<IUnitOfWork>()
                .InstancePerLifetimeScope();
        }

        private ISessionFactory BuildSessionFactory(Configuration normalConfig)
        {
            try
            {
                return Fluently.Configure(normalConfig).Mappings(m =>
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("R2V2"));

                    var autoMaps =
                        assemblies.SelectMany(a => a.GetTypes().Where(x =>
                            x.Name.Contains("Map") &&
                            x.Namespace.Contains("NHibernateMaps")));

                    autoMaps.ForEach(x => m.FluentMappings.Add(x));

                    //Filters
                    m.FluentMappings.Add(typeof(SoftDeleteFilter));
                }).BuildSessionFactory();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (var exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    var exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }

                    sb.AppendLine();
                }

                var errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);

                var log = LogManager.GetLogger(typeof(NHibernateModule));
                log.ErrorFormat(errorMessage, ex);
            }

            return null;
        }

        private static Configuration BuildConfiguration(IComponentContext context)
        {
            var path = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/hibernate.config")
                : Assembly.GetCallingAssembly().Location.ToLower().Replace("r2v2.dll", "hibernate.config");

            var normalConfig = new Configuration().Configure(path);

            var preInsertEventListeners = context.Resolve<IEnumerable<IPreInsertEventListener>>();
            if (preInsertEventListeners.Any())
                normalConfig.EventListeners.PreInsertEventListeners = preInsertEventListeners.ToArray();

            var preUpdateEventListeners = context.Resolve<IEnumerable<IPreUpdateEventListener>>();
            if (preUpdateEventListeners.Any())
                normalConfig.EventListeners.PreUpdateEventListeners = preUpdateEventListeners.ToArray();

            var saveOrUpdateEventListeners = context.Resolve < IEnumerable<ISaveOrUpdateEventListener>>();
            if (saveOrUpdateEventListeners.Any())
                normalConfig.EventListeners.SaveEventListeners = saveOrUpdateEventListeners.ToArray();

            var mergeEventListeners = context.Resolve < IEnumerable<IMergeEventListener>>();
            if (mergeEventListeners.Any())
            {
                var listeners = mergeEventListeners.ToArray();
                normalConfig.EventListeners.MergeEventListeners = listeners;
                //normalConfig.EventListeners.SaveOrUpdateCopyEventListeners = listeners;
            }

            var deleteEventListeners = context.Resolve<IEnumerable<IDeleteEventListener>>();
            if (deleteEventListeners.Any())
                normalConfig.EventListeners.DeleteEventListeners = deleteEventListeners.ToArray();

            return normalConfig;
        }
    }
}