#region

using System;
using System.Web;
using System.Web.Mvc;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.RequestLogger;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Contexts;
using R2V2.Web.Infrastructure.HttpModules;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class PassiveAuthorizationFilter : IR2V2Filter, IActionFilter
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<PassiveAuthorizationFilter> _log;

        public PassiveAuthorizationFilter(IAuthenticationContext authenticationContext
            , ILog<PassiveAuthorizationFilter> log
        )
        {
            _authenticationContext = authenticationContext;
            _log = log;
            _log.Debug("PassiveAuthorizationFilter() constructor");
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _log.Debug("OnActionExecuting() >>>");

            var actionContext =
                new ActionContext(filterContext.Controller.ControllerContext, filterContext.ActionDescriptor);
            var processPassive = CanProcess(actionContext);

            if (!processPassive)
            {
                _log.Debug("OnActionExecuting() <<<");
                return;
            }

            var httpContext = filterContext.HttpContext;


            var autoAuthenticate = httpContext?.Request.QueryString["Authenticate"] ?? "";

            if (autoAuthenticate.Contains("athens", StringComparison.CurrentCultureIgnoreCase))
            {
                HttpContext.Current.Session["AthensRequestedUrl"] = httpContext?.Request.Url;
                filterContext.Result = new RedirectResult("/Authentication/AthensLogin");
                return;
            }

            var authService = ServiceLocator.Current.GetInstance<IAuthenticationService>();
            var authResult = authService.AttemptPassiveAuthentication(httpContext.Request, httpContext.Response);

            //A Bad UrlReferrer will throw an error. Need to suppress it. It is already logged in a log.error so no need to send duplicate error emails. 
            try
            {
                if (!string.IsNullOrWhiteSpace(httpContext.Request.HttpReferrer()))
                {
                    _authenticationContext.SetAuthenticationReferrer(httpContext.Request.HttpReferrer());
                }
            }
            catch (Exception ex)
            {
                _log.Info(ex.Message, ex);
            }

            if (authResult.WasSuccessful)
            {
                _authenticationContext.Set(authResult.AuthenticatedInstitution);

                SetRequestLoggerData(authResult.AuthenticatedInstitution);

                if (httpContext.Request.FilePath == "/")
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    var url = urlHelper.Action("Index", "Browse");

                    if (authResult.AuthenticatedInstitution != null &&
                        authResult.AuthenticatedInstitution.HomePage == HomePage.AtoZIndex)
                    {
                        url = urlHelper.Action("Index", "AlphaIndex");
                    }

                    filterContext.Result = new RedirectResult(url);
                }
            }

            _log.Debug("OnActionExecuting() <<<");
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //TODO : Nothing
        }

        public bool AllowMultiple => true;

        public int Order => 90;

        public FilterScope FilterScope => FilterScope.Global;

        public bool CanProcess(ActionContext actionContext)
        {
            var controller = actionContext.ControllerContext.Controller;
            if (controller is PingController)
            {
                return false;
            }

            if (_authenticationContext.IsAuthenticated &&
                !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["accountNumber"]))
            {
                _log.Info("Forcing IP Plus re-authentication");
                return true;
            }

            var canProcess = !_authenticationContext.IsAuthenticated &&
                             (_authenticationContext.IsIpAuthRequired() ||
                              _authenticationContext.IsReferrerAuthRequired() ||
                              _authenticationContext.IsTrustedAuthRequired() ||
                              _authenticationContext.IsAthensAuthRequired());
            return canProcess;
        }

        private void SetRequestLoggerData(AuthenticatedInstitution institution)
        {
            var context = HttpContext.Current;
            var data = RequestLoggerModule.GetRequestLoggerData();
            if (data == null)
            {
                _log.ErrorFormat("'RequestLoggerData' was null, URL: {0}, IP: {1}", context?.Request.RawUrl ?? "null",
                    context?.Request.GetHostIpAddress() ?? "null");
            }
            else
            {
                data.InstitutionId = institution.Id;
                data.InstitutionName = institution.Name;
                data.InstitutionAccountNumber = institution.AccountNumber;
            }

            var requestData = (RequestData)context.Items[RequestLoggerFilter.RequestDataKey];
            if (requestData != null)
            {
                requestData.InstitutionId = institution.Id;
            }
            else
            {
                _log.InfoFormat(
                    "'{0}' was null - THIS IS ONLY OK IF PASSIVE AUTH WASN'T CALL YET AND THE RequestLoggerFilter._logRequest = false!",
                    RequestLoggerFilter.RequestDataKey);
            }
        }
    }
}