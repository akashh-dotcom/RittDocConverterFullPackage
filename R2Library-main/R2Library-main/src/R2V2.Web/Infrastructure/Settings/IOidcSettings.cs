namespace R2V2.Web.Infrastructure.Settings
{
    public interface IOidcSettings
    {
        string Authority { get; set; }
        string AuthorityDomain { get; set; }
        string RedirectUrl { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
    }
}