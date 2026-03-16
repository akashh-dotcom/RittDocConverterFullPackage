#region

using System;
using System.Linq;
using System.Web.Mvc;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Contexts;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class FooterBuildFilter : R2V2ResultFilter
    {
        private readonly IAuthenticationContext _authenticationContext;

        public FooterBuildFilter(IAuthenticationContext authenticationContext) : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model;
            var baseModel = model as IR2V2Model;

            if (baseModel == null)
            {
                return;
            }

            baseModel.Footer = new Footer();

            baseModel.DisplayAskYourLibrarian = _authenticationContext.IsAuthenticated;

            //if (AuthenticationContext != null && AuthenticationContext.AuthenticatedInstitution != null && AuthenticationContext.AuthenticatedInstitution.User != null)
            if (_authenticationContext.IsAuthenticated && _authenticationContext.AuthenticatedInstitution.User != null)
            {
                var adminSettings = ServiceLocator.Current.GetInstance<AdminSettings>();
                if (adminSettings.AdminControllAccess.Any(x => string.Equals(x,
                        _authenticationContext.AuthenticatedInstitution.User.Email,
                        StringComparison.CurrentCultureIgnoreCase)))
                {
                    baseModel.DisplayConfigurationLink = true;
                }
            }
        }
    }
}