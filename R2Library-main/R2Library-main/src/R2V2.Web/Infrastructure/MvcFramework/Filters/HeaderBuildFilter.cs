#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class HeaderBuildFilter : R2V2ResultFilter
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IInstitutionSettings _institutionSettings;

        public HeaderBuildFilter(IAuthenticationContext authenticationContext, IInstitutionSettings institutionSettings)
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _institutionSettings = institutionSettings;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model;
            var baseModel = model as IR2V2Model;

            if (baseModel == null)
            {
                return;
            }

            var isInstitutionUser = _authenticationContext.IsInstitutionNoUser();
            var showLoginLink = !_authenticationContext.IsAuthenticated || isInstitutionUser;
            var redirectUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;

            var header = new Header(showLoginLink, redirectUrl);
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution != null &&
                !string.IsNullOrWhiteSpace(authenticatedInstitution.BrandingLogoFileName))
            {
                header.BrandingLogoFileName = authenticatedInstitution.BrandingLogoFileName;
                header.BrandingLogoDisplayUrl = $"{_institutionSettings.LogoLocation}{header.BrandingLogoFileName}";
                header.BrandingInstitutionName = authenticatedInstitution.BrandingInstitutionName;
            }

            baseModel.Header = header;
        }
    }
}