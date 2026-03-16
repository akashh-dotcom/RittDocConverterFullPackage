namespace R2V2.Core.Configuration
{
    public class ConfigurationSetting
    {
        public ConfigurationSetting()
        {
        }

        public ConfigurationSetting(DbConfigurationSetting configurationSetting)
        {
            Id = configurationSetting.Id;
            Configuration = configurationSetting.Configuration;
            Setting = configurationSetting.Setting;
            Key = configurationSetting.Key;
            Value = configurationSetting.Value;
            Description = configurationSetting.Description;
        }

        public int Id { get; set; }
        public string Configuration { get; set; }
        public string Setting { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string NewValue { get; set; }
    }
}