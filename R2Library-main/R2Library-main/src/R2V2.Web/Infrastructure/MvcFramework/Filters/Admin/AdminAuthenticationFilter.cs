#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Controllers;
using R2V2.Web.Infrastructure.Contexts;
using R2V2.Web.Models.Authentication;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters.Admin
{
    public class AdminAuthenticationFilter : ActionFilterAttribute, IR2V2Filter
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<AdminAuthenticationFilter> _log;

        public AdminAuthenticationFilter(ILog<AdminAuthenticationFilter> log,
            IAuthenticationContext authenticationContext)
        {
            _log = log;
            _authenticationContext = authenticationContext;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_authenticationContext.IsAuthenticated || filterContext.Controller is AuthenticationController)
            {
                return;
            }

            _log.InfoFormat("_authenticationContext.IsAuthenticated: {0}", _authenticationContext.IsAuthenticated);

            var urlHelper = new UrlHelper(filterContext.RequestContext);
            var redirectUrl = urlHelper.Action("NoAccess", "Authentication", new
            {
                Area = "",
                accessCode = AccessCode.Unauthenticated.ToLower(),
                //redirectUrl = filterContext.RequestContext.HttpContext.Request.Path
                redirectUrl = filterContext.RequestContext.HttpContext.Request.RawUrl
            });

            filterContext.Result = new RedirectResult(redirectUrl);
        }

        #region Implementation of IR2V2Filter

        public FilterScope FilterScope => FilterScope.Action;

        public bool CanProcess(ActionContext actionContext)
        {
            return actionContext.ControllerContext.Controller is R2AdminBaseController;
        }

        #endregion
    }
}