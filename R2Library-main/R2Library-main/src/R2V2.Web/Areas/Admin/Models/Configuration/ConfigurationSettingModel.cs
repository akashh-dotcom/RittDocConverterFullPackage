#region

using R2V2.Core.Configuration;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Configuration
{
    public class ConfigurationSettingModel : AdminBaseModel
    {
        public ConfigurationSetting Setting { get; set; }
        public string ConfigurationName { get; set; }
        public string BackLinkUrl { get; set; }
        public bool LiveSettings { get; set; }
    }
}