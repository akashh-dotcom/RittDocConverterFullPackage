#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class AuthenticationInfoBuildFilter : R2V2ResultFilter
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IInstitutionSettings _institutionSettings;

        public AuthenticationInfoBuildFilter(IAuthenticationContext authenticationContext,
            IInstitutionSettings institutionSettings)
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

            var isAuthenticated = _authenticationContext.IsAuthenticated;
            var userId = 0;
            string displayName = null;
            var isNotGuest = true;

            var urlHelper = new UrlHelper(filterContext.RequestContext);

            AuthenticatedInstitution authenticatedInstitution = null;
            if (isAuthenticated)
            {
                authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
                if (authenticatedInstitution != null)
                {
                    userId = authenticatedInstitution.User != null ? authenticatedInstitution.User.Id : 0;
                    displayName = authenticatedInstitution.IsPublisherUser()
                        ? string.IsNullOrWhiteSpace(authenticatedInstitution.Publisher.DisplayName)
                            ? authenticatedInstitution.Publisher.Name
                            : authenticatedInstitution.Publisher.DisplayName
                        : authenticatedInstitution.DisplayName;
                    isNotGuest = authenticatedInstitution.AccountNumber != _institutionSettings.GuestAccountNumber &&
                                 !string.IsNullOrWhiteSpace(authenticatedInstitution.AccountNumber);

                    //baseModel.SearchUrl = string.Format("{0}#include={1}", urlHelper.Action("Index", "Search"),
                    //    (authenticatedInstitution.IncludeArchivedTitlesByDefault) ? "3" : "1");
                }
                else
                {
                    //baseModel.SearchUrl = string.Format("{0}#include=1", urlHelper.Action("Index", "Search"));
                }
            }
            else
            {
                //baseModel.SearchUrl = string.Format("{0}#include=1", urlHelper.Action("Index", "Search"));
            }

            baseModel.AuthenticationInfo = new AuthenticationInfo(displayName,
                _authenticationContext.IsInstitutionNoUser(), userId, isNotGuest,
                isAuthenticated, authenticatedInstitution);
        }
    }
}