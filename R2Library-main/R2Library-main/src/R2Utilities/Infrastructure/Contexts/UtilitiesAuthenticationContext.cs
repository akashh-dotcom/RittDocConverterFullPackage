#region

using System;
using R2V2.Contexts;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2Utilities.Infrastructure.Contexts
{
    public class UtilitiesAuthenticationContext : IAuthenticationContext
    {
        private readonly Func<IApplicationWideStorageService> _applicationWideStorageServiceFactory;

        public UtilitiesAuthenticationContext(Func<IApplicationWideStorageService> applicationWideStorageServiceFactory)
        {
            _applicationWideStorageServiceFactory = applicationWideStorageServiceFactory;
            IsAuthenticated = true;
            AuthenticatedInstitution = null;
            AuthenticationReferrer = null;
        }

        private IApplicationWideStorageService ApplicationWideStorageService => _applicationWideStorageServiceFactory();


        public bool IsAuthenticated { get; }

        public void SetAuthenticationReferrer(string referrer)
        {
            throw new NotImplementedException();
        }

        public string AuthenticationReferrer { get; }

        public AuthenticatedInstitution AuthenticatedInstitution { get; }

        public void Set(AuthenticatedInstitution authenticatedInstitution)
        {
            throw new NotImplementedException();
        }

        public bool IsRittenhouseAdmin()
        {
            return false;
        }

        public bool IsInstitutionAdmin()
        {
            return false;
        }

        public bool IsPublisherUser()
        {
            return false;
        }

        public bool IsSubscriptionUser()
        {
            return false;
        }

        public bool IsSalesAssociate()
        {
            return false;
        }

        public bool IsInstitutionNoUser()
        {
            return false;
        }

        public bool IsInstitutionUser()
        {
            return false;
        }

        public bool IsExpertReviewer()
        {
            return false;
        }

        public bool IsIpAuthRequired()
        {
            return false;
        }

        public bool IsReferrerAuthRequired()
        {
            return false;
        }

        public bool IsTrustedAuthRequired()
        {
            return false;
        }

        public bool IsAthensAuthRequired()
        {
            return false;
        }

        public string AuditId => ApplicationWideStorageService.Get<string>("AuthenticationContext.AuditId");
    }
}