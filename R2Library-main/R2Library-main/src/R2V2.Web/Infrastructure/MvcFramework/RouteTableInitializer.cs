#region

using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Common.Logging;
using R2V2.Infrastructure.Initializers;
using R2V2.Web.Infrastructure.MvcFramework.Routing;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework
{
    public class RouteTableInitializer : IInitializer
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public void Initialize()
        {
            Log.Debug("Initialize() >>>");
            var namespaces = new[] { "R2V2.Web.Controllers" };

            var routes = RouteTable.Routes;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("oa/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("_static/{*pathInfo}");
            routes.IgnoreRoute("{*botdetect}", new { botdetect = @"(.*)BotDetectCaptcha\.ashx" });

            routes.MapRoute("LegacyUrl", "{aspxFilename}.aspx", new { controller = "Redirect", action = "Index" });
            routes.MapRoute("LegacyUrlWithDirectory", "{directory}/{aspxFilename}.aspx",
                new { controller = "Redirect", action = "Index" });

            routes.MapR2V2Route(
                "MyR2", // Route name
                "MyR2/{type}", // URL with parameters
                new { controller = "MyR2", action = "Index" }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "ResourceLink", // Route name
                "Resource/Link/{id}", // URL with parameters
                new { controller = "Resource", action = "Link" }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "ResourceDetail", // Route name
                "Resource/Detail/{isbn}/{section}/{showAllDictionaryTerms}", // URL with parameters
                new
                {
                    controller = "Resource", action = "Detail", showAllDictionaryTerms = UrlParameter.Optional
                }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "ResourceSection", // Route name
                "Resource/{action}/{isbn}/{section}", // URL with parameters
                new { controller = "Resource", action = "Title" }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "ReviewedTitle", // Route name
                "Resource/{action}/{isbn}", // URL with parameters
                new { controller = "Resource", action = "ReviewedTitle" }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "Resource", // Route name
                "Resource/{action}/{isbn}", // URL with parameters
                new { controller = "Resource", action = "Title" }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "Discover", // Route name
                "Discover/{title}", // URL with parameters
                new { controller = "Cms", action = "Index", title = "Academic" }, // Parameter defaults
                namespaces
            );

            routes.MapR2V2Route(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                namespaces
            );
            Log.Debug("Initialize() <<<");
        }
    }
}