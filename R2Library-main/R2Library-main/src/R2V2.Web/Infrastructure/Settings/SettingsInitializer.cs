#region

using System.Collections.Generic;
using R2V2.Core.Configuration;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public static class SettingsInitializer
    {
        public static void Initialize(List<SettingGroup> configurationSettings)
        {
            var sib = new SettingsInitializerBase(configurationSettings);

            sib.SetValues<AdminSettings>(configurationSettings);
            sib.SetValues<CacheSettings>(configurationSettings);
            sib.SetValues<ClientSettings>(configurationSettings);
            sib.SetValues<InstitutionSettings>(configurationSettings);
            sib.SetValues<WebImageSettings>(configurationSettings);
            sib.SetValues<WebSettings>(configurationSettings);
            sib.SetValues<OidcSettings>(configurationSettings);
        }
    }
}