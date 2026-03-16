#region

using System;
using System.Web;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Infrastructure.Authentication
{
    public interface IAuthenticationService
    {
        AuthenticationResult Login(string userName, string password, HttpRequestBase request, int institutionId,
            string countryCode);

        AuthenticationResult AttemptPassiveAuthentication(HttpRequestBase request, HttpResponseBase response);
        AuthenticationResult ReloadUser(int userId);

        bool IsIpAddressBlocked(HttpRequestBase request);
        PublisherUser GetPublisherUser(string userName, string password);
        bool UpdateUserLastLoginDate(int userId, DateTime lastLoginDate);
        bool AutoLockUser(User user);
    }
}