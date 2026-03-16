namespace R2V2.Web.Infrastructure.Settings
{
    public interface ICacheSettings
    {
        int DefaultExpirationInHours { get; set; }
    }
}