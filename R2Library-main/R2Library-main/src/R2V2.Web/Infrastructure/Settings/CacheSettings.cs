#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class CacheSettings : AutoSettings, ICacheSettings
    {
        public int DefaultExpirationInHours { get; set; }
    }
}