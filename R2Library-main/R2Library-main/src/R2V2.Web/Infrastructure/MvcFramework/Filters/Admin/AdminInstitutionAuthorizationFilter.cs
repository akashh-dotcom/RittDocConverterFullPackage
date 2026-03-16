#region

using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Infrastructure.Contexts;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models.Authentication;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters.Admin
{
    public class AdminInstitutionAuthorizationFilter : ActionFilterAttribute, IR2V2Filter
    {
        private readonly IAdminContext _adminContext;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<AdminInstitutionAuthorizationFilter> _log;

        public AdminInstitutionAuthorizationFilter(ILog<AdminInstitutionAuthorizationFilter> log
            , IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IInstitutionSettings institutionSettings
        )
        {
            _log = log;
            _authenticationContext = authenticationContext;
            _adminContext = adminContext;
            _institutionSettings = institutionSettings;
        }

        protected AuthenticatedInstitution AuthenticatedInstitution => _authenticationContext.AuthenticatedInstitution;

        protected int InstitutionId
        {
            get
            {
                var authenticatedInstitution = AuthenticatedInstitution;
                return authenticatedInstitution != null ? authenticatedInstitution.Id : 0;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller;
            var model = controller.ViewData.Model;
            var adminBaseModel = model as AdminBaseModel;
            if (adminBaseModel == null)
            {
                return;
            }

            var redirectUser = false;

            var authenticatedInstitution = AuthenticatedInstitution;
            if (authenticatedInstitution.IsRittenhouseAdmin() || authenticatedInstitution.IsSalesAssociate())
            {
                var controllerAttributes =
                    filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(
                        typeof(RequiresInstitutionId), false);
                if (!controllerAttributes.Any())
                {
                    return;
                }

                var isAdminAuthorized =
                    filterContext.ActionDescriptor.GetCustomAttributes(typeof(RequiresInstitutionId), false)
                        .FirstOrDefault() as RequiresInstitutionId;

                if ((adminBaseModel.InstitutionId == 0 && isAdminAuthorized == null) ||
                    (isAdminAuthorized != null && !isAdminAuthorized.IgnoreRedirect))
                {
                    redirectUser = true;
                }
                else
                {
                    return;
                }
            }


            if (!redirectUser)
            {
                var institutionId = adminBaseModel.InstitutionId;
                if (institutionId == 0 || institutionId == InstitutionId)
                {
                    return;
                }

                if (institutionId > 0)
                {
                    var institution = _adminContext.GetAdminInstitution(institutionId);
                    if (institution != null && institution.AccountNumber == _institutionSettings.GuestAccountNumber)
                    {
                        return;
                    }
                }
            }


            var urlHelper = new UrlHelper(filterContext.RequestContext);
            var redirectUrl = urlHelper.Action("NoAccess", "Authentication",
                new
                {
                    Area = "",
                    accessCode = redirectUser
                        ? AccessCode.UnknownParameters.ToLower()
                        : AccessCode.Unauthorized.ToLower()
                });
            _log.DebugFormat("redirectUrl: {0}", redirectUrl);
            filterContext.Result = new RedirectResult(redirectUrl);
        }

        #region Implementation of IR2V2Filter

        public FilterScope FilterScope => FilterScope.Action;

        public bool CanProcess(ActionContext actionContext)
        {
            return true;
        }

        #endregion
    }
}

public class RequiresInstitutionId : ActionFilterAttribute
{
    public bool IgnoreRedirect { get; set; }
}