#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Configuration
{
    public class SettingGroup
    {
        public string SettingName { get; set; }
        public List<ConfigurationSetting> ConfigurationSettings { get; set; }
    }
}