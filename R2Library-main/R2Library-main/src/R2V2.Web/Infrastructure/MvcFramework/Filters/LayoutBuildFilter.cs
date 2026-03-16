#region

using System;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers;
using R2V2.Web.Infrastructure.HttpModules;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class LayoutBuildFilter : R2V2ResultFilter
    {
        private readonly IClientSettings _clientSettings;
        private readonly ILog<LayoutBuildFilter> _log;
        private readonly IWebSettings _webSettings;

        public LayoutBuildFilter(IAuthenticationContext authenticationContext, IClientSettings clientSettings,
            IWebSettings webSettings, ILog<LayoutBuildFilter> log)
            : base(authenticationContext)
        {
            _clientSettings = clientSettings;
            _webSettings = webSettings;
            _log = log;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //_log.Debug("OnResultExecuting()");
            var model = filterContext.Controller.ViewData.Model;
            var baseModel = model as IR2V2Model;
            if (baseModel == null)
            {
                return;
            }

            baseModel.EnvironmentName = _webSettings.EnvironmentName;
            var debugInfo = new StringBuilder().Append(Environment.MachineName);
            try
            {
                if (filterContext.HttpContext.Session != null)
                {
                    debugInfo.AppendFormat(", {0}", filterContext.HttpContext.Session.SessionID);
                    //RequestLoggerData requestLoggerData = (RequestLoggerData)filterContext.HttpContext.Items["RequestLoggerData"];
                    var requestLoggerData = RequestLoggerModule.GetRequestLoggerData();
                    if (requestLoggerData != null)
                    {
                        debugInfo.AppendFormat(", {0}, {1}, {2}, {3}, {4:s}", requestLoggerData.IpAddress,
                            requestLoggerData.RequestId, requestLoggerData.InstitutionId, requestLoggerData.UserId,
                            requestLoggerData.StartTime);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendFormat("URL: {0}", filterContext.HttpContext.Request.RawUrl);
                msg.AppendFormat("Exception: {0}", ex.Message);
                _log.Error(msg.ToString(), ex);
                throw;
            }

            baseModel.DebugInformation = debugInfo.ToString();

            baseModel.Layout = new Layout
            {
                ControllerName = filterContext.RouteData.Values["controller"].ToString().ToLower(),
                ActionName = filterContext.RouteData.Values["action"].ToString().ToLower(),
                GoogleAnalyticsAccount = _clientSettings.GoogleAnalyticsAccount,
                DisableRightClick = _clientSettings.DisableRightClick,
                IsMarketingHome = filterContext.Controller is HomeController
            };
        }
    }
}