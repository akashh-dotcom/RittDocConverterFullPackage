#region

using R2V2.Infrastructure.Storages;
using R2V2.Web.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Helpers
{
    public static class UserSessionStorageExtensions
    {
        private const string AuthenticationFlag = "Current.AuthenticationFlag";
        private const string AuthenticationReferrerKey = "Current.Authentication.Referrer";
        private const string PassiveAuthenticationStatusKey = "Current.PassiveAuthenticationStatus";

        public static void SetAuthenticationReferrer(this IUserSessionStorageService storageService, string referrer)
        {
            storageService.Put(AuthenticationReferrerKey, referrer);
        }

        public static string GetAuthenticationReferrer(this IUserSessionStorageService storageService)
        {
            return storageService.Has(AuthenticationReferrerKey)
                ? storageService.Get<string>(AuthenticationReferrerKey)
                : string.Empty;
        }

        public static void SetAuthenticationFlag(this IUserSessionStorageService storageService, bool? value)
        {
            storageService.Put(AuthenticationFlag, value);
        }

        public static bool? GetAuthenticationFlag(this IUserSessionStorageService storageService)
        {
            return !storageService.Has(AuthenticationFlag) ? null : storageService.Get<bool?>(AuthenticationFlag);
        }

        public static PassiveAuthenticationStatus GetPassiveAuthenticationStatus(
            this IUserSessionStorageService storageService)
        {
            if (!storageService.Has(PassiveAuthenticationStatusKey))
            {
                var passiveAuthenticationStatus = new PassiveAuthenticationStatus();
                storageService.Put(PassiveAuthenticationStatusKey, passiveAuthenticationStatus);
                return passiveAuthenticationStatus;
            }

            return storageService.Get<PassiveAuthenticationStatus>(PassiveAuthenticationStatusKey);
        }
    }
}