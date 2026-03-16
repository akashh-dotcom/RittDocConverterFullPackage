#region

using System;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using Common.Logging;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class RequireSslFilter : AuthorizeAttribute
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool _isLocalDevelopment;

        public RequireSslFilter()
        {
            // Check if we're in local development mode
            var isLocalDevConfig = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            _isLocalDevelopment = !string.IsNullOrEmpty(isLocalDevConfig) && 
                                  isLocalDevConfig.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException($"filterContext is null");
            }

            // Skip SSL requirement in local development mode
            if (_isLocalDevelopment)
            {
                //Log.DebugFormat("SSL requirement bypassed - local development mode");
                return;
            }

            var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();

            var request = filterContext.HttpContext.Request;

            if (requestLoggerData == null || requestLoggerData.IsSecureConnection || request == null ||
                request.Url == null)
            {
                //Log.DebugFormat("requestLoggerData.IsSecureConnection: {0}, request: {1}", requestLoggerData.IsSecureConnection, (request == null) ? "is null" : (request.Url == null) ? ".Url is null" : "is NOT null");
                return;
            }

            var webSettings = ServiceLocator.Current.GetInstance<IWebSettings>();
            if (!webSettings.RequireSsl)
            {
                //Log.DebugFormat("webSettings.RequireSsl: {0}", webSettings.RequireSsl);
                return;
            }

            var builder = new UriBuilder(request.Url)
            {
                Scheme = Uri.UriSchemeHttps,
                Port = 443
            };

            var redirectTo = builder.Uri.ToString();
            Log.DebugFormat("redirecting to SSL, URL: {0}", redirectTo);
            filterContext.Result = new RedirectResult(redirectTo);
        }
    }
}