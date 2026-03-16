#region

using System.Linq;
using System.Web.Mvc;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Infrastructure.Contexts;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models.Authentication;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters.Admin
{
    public class AdminAuthorizationFilter : ActionFilterAttribute
    {
        public RoleCode[] Roles { get; set; }

        public bool IsAdminAuthorizedArea { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as R2AdminBaseController;
            if (controller == null)
            {
                return;
            }

            var authenticationContext = controller.AuthenticationContext;
            if (!authenticationContext.IsAuthenticated)
            {
                return;
            }

            var log = ServiceLocator.Current.GetInstance<ILog<AdminAuthorizationFilter>>();

            var authenticatedInstitution = authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution != null)
            {
                if (IsAdminAuthorizedArea)
                {
                    var adminSettings = ServiceLocator.Current.GetInstance<AdminSettings>();
                    if (adminSettings.AdminControllAccess.Any(x =>
                            x.ToLower() == authenticatedInstitution.User.Email.ToLower()))
                    {
                        return;
                    }
                }
                else if (authenticatedInstitution.UserRole != null)
                {
                    if (Roles.Any(x => x == authenticatedInstitution.UserRole.Id))
                    {
                        return;
                    }
                }

                log.InfoFormat("Roles: {0}", Roles.Count() > 0 ? string.Join(",", Roles.ToArray()) : "empty");
            }
            else
            {
                log.Info("r2V2Principal is null");
            }

            var urlHelper = new UrlHelper(filterContext.RequestContext);
            var redirectUrl = urlHelper.Action("NoAccess", "Authentication",
                new
                {
                    Area = "",
                    accessCode = AccessCode.Unauthorized.ToLower(),
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