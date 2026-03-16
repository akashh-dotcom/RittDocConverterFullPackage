#region

using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework
{
    // in Global.asax.cs Application_Start you can insert these into the ViewEngine chain like so:
    //
    // ViewEngines.Engines.Insert(0, new MobileCapableRazorViewEngine());
    //
    // or
    //
    // ViewEngines.Engines.Insert(0, new MobileCapableRazorViewEngine("iPhone")
    // {
    //     ContextCondition = (ctx => ctx.Request.UserAgent.IndexOf(
    //         "iPhone", StringComparison.OrdinalIgnoreCase) >= 0)
    // });

    public class MobileCapableRazorViewEngine : RazorViewEngine
    {
        public MobileCapableRazorViewEngine() : this("Mobile", context => context.Request.Browser.IsMobileDevice)
        {
        }

        public MobileCapableRazorViewEngine(string viewModifier) : this(viewModifier,
            context => context.Request.Browser.IsMobileDevice)
        {
        }

        public MobileCapableRazorViewEngine(string viewModifier, Func<HttpContextBase, bool> contextCondition)
        {
            ViewModifier = viewModifier;
            ContextCondition = contextCondition;
        }

        public string ViewModifier { get; set; }
        public Func<HttpContextBase, bool> ContextCondition { get; set; }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName,
            string masterName, bool useCache)
        {
            return NewFindView(controllerContext, viewName, null, useCache, false);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName,
            bool useCache)
        {
            return NewFindView(controllerContext, partialViewName, null, useCache, true);
        }

        private ViewEngineResult NewFindView(ControllerContext controllerContext, string viewName, string masterName,
            bool useCache, bool isPartialView)
        {
            if (!ContextCondition(controllerContext.HttpContext))
            {
                return new ViewEngineResult(new string[] { }); // we found nothing and we pretend we looked nowhere
            }

            // Get the name of the controller from the path
            var controller = controllerContext.RouteData.Values["controller"].ToString();
            var area = "";
            try
            {
                area = controllerContext.RouteData.DataTokens["area"].ToString();
            }
            catch
            {
            }

            // Apply the view modifier
            var newViewName = $"{viewName}.{ViewModifier}";

            // Create the key for caching purposes          
            var keyPath = Path.Combine(area, controller, newViewName);

            var cacheLocation = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, keyPath);

            // Try the cache          
            if (useCache)
            {
                //If using the cache, check to see if the location is cached.                              
                if (!string.IsNullOrWhiteSpace(cacheLocation))
                {
                    return isPartialView
                        ? new ViewEngineResult(CreatePartialView(controllerContext, cacheLocation), this)
                        : new ViewEngineResult(CreateView(controllerContext, cacheLocation, masterName), this);
                }
            }

            var locationFormats = string.IsNullOrEmpty(area) ? ViewLocationFormats : AreaViewLocationFormats;

            // for each of the paths defined, format the string and see if that path exists. When found, cache it.          
            foreach (var rootPath in locationFormats)
            {
                var currentPath = string.IsNullOrEmpty(area)
                    ? string.Format(rootPath, newViewName, controller)
                    : string.Format(rootPath, newViewName, controller, area);

                if (FileExists(controllerContext, currentPath))
                {
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, keyPath, currentPath);

                    return isPartialView
                        ? new ViewEngineResult(CreatePartialView(controllerContext, currentPath), this)
                        : new ViewEngineResult(CreateView(controllerContext, currentPath, masterName), this);
                }
            }

            return new ViewEngineResult(new string[] { }); // we found nothing and we pretend we looked nowhere
        }
    }
}