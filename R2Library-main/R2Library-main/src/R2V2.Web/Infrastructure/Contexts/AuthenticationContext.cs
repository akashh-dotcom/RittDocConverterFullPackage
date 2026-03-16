#region

using System;
using System.Text.RegularExpressions;
using System.Web;
using R2V2.Contexts;
using R2V2.Extensions;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Infrastructure.Contexts
{
    public class AuthenticationContext : IAuthenticationContext
    {
        public const string AuthenticatedInstitutionKey = "Current.AuthenticatedInstitution";
        private static readonly Regex R2LibraryRegex = new Regex(@"^(http(s)?(:\/\/))(([\w\d\-]+\.)?r2library\.com)");

        private readonly Func<IUserSessionStorageService> _userSessionStorageServiceFactory;

        public AuthenticationContext(Func<IUserSessionStorageService> userSessionStorageServiceFactory)
        {
            _userSessionStorageServiceFactory = userSessionStorageServiceFactory;
        }

        private IUserSessionStorageService UserSessionStorageService => _userSessionStorageServiceFactory();

        public bool IsAuthenticated
        {
            get
            {
                var authenticatedInstitution = AuthenticatedInstitution;
                return authenticatedInstitution != null && authenticatedInstitution.Id > 0;
            }
        }

        public void SetAuthenticationReferrer(string referrer)
        {
            UserSessionStorageService.SetAuthenticationReferrer(referrer);
        }

        public string AuthenticationReferrer => UserSessionStorageService.GetAuthenticationReferrer();

        /// <summary>
        ///     Get the authenticated institution form the sesssion
        /// </summary>
        public AuthenticatedInstitution AuthenticatedInstitution =>
            UserSessionStorageService.Get<AuthenticatedInstitution>(AuthenticatedInstitutionKey);

        public void Set(AuthenticatedInstitution authenticatedInstitution)
        {
            UserSessionStorageService.Put(AuthenticatedInstitutionKey, authenticatedInstitution);
        }

        public string AuditId => AuthenticatedInstitution != null ? AuthenticatedInstitution.AuditId : "error";

        public bool IsRittenhouseAdmin()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.IsRittenhouseAdmin();
        }

        public bool IsInstitutionAdmin()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.IsInstitutionAdmin();
        }

        public bool IsPublisherUser()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.IsPublisherUser();
        }

        public bool IsSalesAssociate()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.IsSalesAssociate();
        }

        public bool IsExpertReviewer()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.IsExpertReviewer();
        }

        public bool IsSubscriptionUser()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.IsSubscriptionUser();
        }


        public bool IsInstitutionNoUser()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.User == null;
        }

        public bool IsInstitutionUser()
        {
            return AuthenticatedInstitution != null && AuthenticatedInstitution.User != null
                                                    && !IsSalesAssociate() && !IsRittenhouseAdmin() &&
                                                    !IsInstitutionAdmin() && !IsPublisherUser();
        }

        public bool IsIpAuthRequired()
        {
            var strIpAddress = HttpContext.Current.Request.GetHostIpAddress();
            if (string.IsNullOrWhiteSpace(strIpAddress))
            {
                return false;
            }

            var passiveAuthenticationStatus = UserSessionStorageService.GetPassiveAuthenticationStatus();
            return !passiveAuthenticationStatus.WasIpAddressAuthPreviouslyAttempted(strIpAddress) ||
                   !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["accountNumber"]);
        }

        public bool IsReferrerAuthRequired()
        {
            var httpReferrer = HttpContext.Current.Request.HttpReferrer();
            if (string.IsNullOrWhiteSpace(httpReferrer))
            {
                return false;
            }

            if (R2LibraryRegex.IsMatch(httpReferrer))
            {
                return false;
            }

            var passiveAuthenticationStatus = UserSessionStorageService.GetPassiveAuthenticationStatus();
            return !passiveAuthenticationStatus.WasReferrerAuthPreviouslyAttempted(httpReferrer);
        }

        public bool IsTrustedAuthRequired()
        {
            var hash = HttpContext.Current.Request.QueryString["hash"];
            if (string.IsNullOrWhiteSpace(hash))
            {
                return false;
            }

            var passiveAuthenticationStatus = UserSessionStorageService.GetPassiveAuthenticationStatus();
            return !passiveAuthenticationStatus.WasTrustedAuthPreviouslyAttempted(hash);
        }

        public bool IsAthensAuthRequired()
        {
            var authenticate = HttpContext.Current.Request.QueryString["Authenticate"] ?? "";
            if (authenticate.Contains("athens", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            var cookie = new AthensAuthenticationCookie();
            if (!cookie.Exists)
            {
                return false;
            }

            var passiveAuthenticationStatus = UserSessionStorageService.GetPassiveAuthenticationStatus();
            return !passiveAuthenticationStatus.WasAthensAuthPreviouslyAttempted(cookie.ScopedAffiliation,
                cookie.TargetedId);
        }
    }
}