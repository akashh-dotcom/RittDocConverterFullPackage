#region

using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Contexts
{
    public interface IAuthenticationContext
    {
        bool IsAuthenticated { get; }
        string AuthenticationReferrer { get; }
        AuthenticatedInstitution AuthenticatedInstitution { get; }

        string AuditId { get; }
        void SetAuthenticationReferrer(string referrer);
        void Set(AuthenticatedInstitution authenticatedInstitution);

        bool IsRittenhouseAdmin();
        bool IsInstitutionAdmin();
        bool IsPublisherUser();
        bool IsSalesAssociate();
        bool IsInstitutionNoUser();
        bool IsInstitutionUser();
        bool IsExpertReviewer();
        bool IsSubscriptionUser();

        bool IsIpAuthRequired();
        bool IsReferrerAuthRequired();
        bool IsTrustedAuthRequired();
        bool IsAthensAuthRequired();
    }

    public static class AuthenticationContextExtensions
    {
        public static bool DisplayTocAvailable(this IAuthenticationContext c)
        {
            return c.IsAuthenticated && c.AuthenticatedInstitution != null &&
                   c.AuthenticatedInstitution.DisplayAllProducts;
        }
    }
}