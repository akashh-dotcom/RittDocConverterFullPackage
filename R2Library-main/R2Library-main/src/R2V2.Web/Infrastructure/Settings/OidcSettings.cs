#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class OidcSettings : AutoSettings, IOidcSettings
    {
        public string Authority { get; set; }
        public string AuthorityDomain { get; set; }
        public string RedirectUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}