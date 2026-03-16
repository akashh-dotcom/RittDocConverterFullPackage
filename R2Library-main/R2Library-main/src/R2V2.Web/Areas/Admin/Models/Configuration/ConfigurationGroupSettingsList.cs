#region

using System.Collections.Generic;
using R2V2.Core.Configuration;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Configuration
{
    public class ConfigurationGroupSettingsList
    {
        public IDictionary<string, List<ConfigurationSetting>> ConfigurationSettings { get; set; }
        public string ConfigurationName { get; set; }
        public int NumberOfSettings { get; set; }
    }
}