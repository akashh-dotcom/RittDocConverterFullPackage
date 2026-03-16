#region

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Contexts;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class ImageFileModule : IHttpModule
    {
        private static readonly ILog<ImageFileModule> Log = new Log<ImageFileModule>();


        public void Init(HttpApplication application)
        {
            application.PostAcquireRequestState += Application_PostAcquireRequestState;
            application.PostMapRequestHandler += Application_PostMapRequestHandler;
        }

        void IHttpModule.Dispose()
        {
            Log.Debug("ImageFileModule Dispose() >> <<");
        }

        void Application_PostMapRequestHandler(object source, EventArgs e)
        {
            var app = (HttpApplication)source;

            if (app.Context.Handler is IRequiresSessionState)
            {
                // no need to replace the current handler
                return;
            }

            // swap the current handler
            app.Context.Handler = new MyHttpHandler(app.Context.Handler);
        }

        private void Application_PostAcquireRequestState(object source, EventArgs e)
        {
            var app = (HttpApplication)source;

            var resourceHttpHandler = HttpContext.Current.Handler as MyHttpHandler;

            if (resourceHttpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = resourceHttpHandler.OriginalHandler;
            }

            var request = app.Context.Request;

            var uri = request.Url;

            Log.Info("***ImageFileModule...");
            var url = uri.AbsoluteUri;
            if (!IsSecuredContent(url))
            {
                return;
            }

            try
            {
                if (HasAccess(url))
                {
                    Log.Info("Image Authentication worked.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Unauthorized Access to Content Image: {0}", ex.Message);
            }

            Log.InfoFormat("Blocking Image : {0}", uri);

            app.Context.Response.StatusCode = 401;
            app.Context.Response.Write(
                "HTTP Error 401.0 - Unauthorized - you must be authenticated to access protected images Please contact Rittenhouse Customer Service to resolve this issue.");
        }

        private static bool HasAccess(string url)
        {
            var authenticationContext = ServiceLocator.Current.GetInstance<IAuthenticationContext>();

            var authenticatedInstitution = authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution == null)
            {
                Log.Info("authenticatedInstitution == null");
                return false;
            }

            const string dirtyKey = "Resources.All.Dirty";
            const string cleanKey = "Resources.All.Clean";

            var applicationWideStorageService = ServiceLocator.Current.GetInstance<IApplicationWideStorageService>();
            var resourceCache = applicationWideStorageService.Get<ResourceCache>(dirtyKey) ??
                                applicationWideStorageService.Get<ResourceCache>(cleanKey);


            var remainingUrl = url.Substring(url.ToLower().IndexOf("images/"));
            var urlArray = remainingUrl.Split('/');

            var isbn = urlArray.FirstOrDefault(segment =>
                Regex.IsMatch(segment, "^[a-zA-Z0-9]{10}$") || Regex.IsMatch(segment, "^[a-zA-Z0-9]{13}$"));
            if (isbn == null)
            {
                Log.Info("isbn == null");
                return false;
            }

            var foundResource =
                resourceCache.GetAllResources()
                    .FirstOrDefault(resource =>
                        resource.Isbn == isbn || resource.Isbn10 == isbn || resource.Isbn13 == isbn ||
                        resource.EIsbn == isbn);
            if (foundResource == null)
            {
                Log.Info("foundResource == null");
                return false;
            }

            var haveLicense = authenticatedInstitution.Licenses.Any(license => license.ResourceId == foundResource.Id);
            if (!haveLicense)
            {
                Log.InfoFormat("Institution: {0} does not have license to this title: {1}", authenticatedInstitution.Id,
                    isbn);
            }

            return haveLicense;
        }

        private static bool IsSecuredContent(string url)
        {
            return IsImageRequest(url) && IsSecuredArea(url);
        }

        private static bool IsSecuredArea(string url)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    if (url.ToLower().Contains("images"))
                    {
                        var imageIndex = url.ToLower().IndexOf("images/");
                        if (imageIndex > 0)
                        {
                            var remainingUrl = url.Substring(url.ToLower().IndexOf("images/"));

                            var urlArray = remainingUrl.Split('/');
                            if (urlArray.Any() && urlArray.Count() > 2 && urlArray[1].Length == 10 ||
                                urlArray[1].Length == 13)
                            {
                                if (Regex.IsMatch(urlArray[1], "^[a-zA-Z0-9]{10}$") ||
                                    Regex.IsMatch(urlArray[1], "^[a-zA-Z0-9]{13}$"))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return false;
        }

        private static bool IsImageRequest(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                return url.Contains(".jpg") || url.Contains(".jpeg") || url.Contains(".gif") || url.Contains(".png");
            }

            return false;
        }

        public void OnLogRequest(object source, EventArgs e)
        {
            //custom logging logic can go here
        }
    }

    // a temp handler used to force the SessionStateModule to load session state
    public class MyHttpHandler : IHttpHandler, IRequiresSessionState
    {
        internal readonly IHttpHandler OriginalHandler;

        public MyHttpHandler(IHttpHandler originalHandler)
        {
            OriginalHandler = originalHandler;
        }

        public void ProcessRequest(HttpContext context)
        {
            // do not worry, ProcessRequest() will not be called, but let's be safe
            throw new InvalidOperationException("MyHttpHandler cannot process requests.");
        }

        public bool IsReusable =>
            // IsReusable must be set to false since class has a member!
            true;
    }
}