#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.Configuration;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public class SettingsInitializerBase
    {
        public SettingsInitializerBase(List<SettingGroup> configurationSettings)
        {
            SetValues<ContentSettings>(configurationSettings);
            SetValues<CollectionManagementSettings>(configurationSettings);
            SetValues<EmailSettings>(configurationSettings);
            SetValues<MessageQueueSettings>(configurationSettings);
        }

        public void SetValues<T>(List<SettingGroup> configurationSettings)
        {
            var missingSettings = new List<string>();
            var dynamicType = typeof(T);
            var properties = dynamicType.GetProperties();
            var isLocalDevelopment = IsLocalDevelopment();

            var settings = ServiceLocator.Current.GetAllInstances<IAutoSettings>();

            var dynamicSettingsClass = settings.Select(x => x).FirstOrDefault(y => y.GetType() == dynamicType);

            var dynamicSettingsName = dynamicType.Name.Split(new[] { "Setting" }, StringSplitOptions.None).First();

            var dynamicConfigurationSettings =
                configurationSettings.FirstOrDefault(x =>
                    string.Equals(x.SettingName, dynamicSettingsName,
                        StringComparison.CurrentCultureIgnoreCase));

            if (dynamicConfigurationSettings != null)
            {
                foreach (var propertyInfo in properties.Where(x => x.Name != "MissingSettings"))
                {
                    object objectValue = null;
                    var configurationSetting =
                        dynamicConfigurationSettings.ConfigurationSettings.FirstOrDefault(x =>
                            x.Key == propertyInfo.Name);
                    var propertyType = propertyInfo.PropertyType;
                    if (isLocalDevelopment
                        && TryGetAppSettingOverride(dynamicSettingsName, propertyInfo.Name, out var appSettingValue))
                    {
                        objectValue = ConvertSettingValue(propertyType, appSettingValue);
                    }
                    else if (configurationSetting != null)
                    {
                        objectValue = ConvertSettingValue(propertyType, configurationSetting.Value);
                    }

                    if (objectValue != null)
                    {
                        propertyInfo.SetValue(dynamicSettingsClass, objectValue, null);
                    }

                    if (objectValue == null)
                    {
                        if (propertyInfo.GetSetMethod() != null)
                        {
                            missingSettings.Add(
                                $"Please add [vchSetting: {dynamicSettingsName}]-[vchKey: {propertyInfo.Name}] in the database table tConfigurationSetting");
                        }
                    }
                }

                if (missingSettings.Any())
                {
                    var missingSettingObject = properties.First(x => x.Name == "MissingSettings");

                    var objectValue = Convert.ChangeType(missingSettings, missingSettingObject.PropertyType);

                    missingSettingObject.SetValue(dynamicSettingsClass, objectValue, null);
                }
            }
        }

        private static bool TryGetAppSettingOverride(string settingName, string propertyName, out string value)
        {
            var prefixedKey = $"{settingName}.{propertyName}";
            var prefixedValue = ConfigurationManager.AppSettings[prefixedKey];
            if (prefixedValue != null)
            {
                value = prefixedValue;
                return true;
            }

            var unprefixedValue = ConfigurationManager.AppSettings[propertyName];
            if (unprefixedValue != null)
            {
                value = unprefixedValue;
                return true;
            }

            value = null;
            return false;
        }

        private static bool IsLocalDevelopment()
        {
            var isLocalDevConfig = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            return !string.IsNullOrEmpty(isLocalDevConfig)
                   && isLocalDevConfig.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static object ConvertSettingValue(Type propertyType, string rawValue)
        {
            if (propertyType == typeof(string[]))
            {
                if (rawValue.Contains(";"))
                {
                    return rawValue.Split(';');
                }

                if (rawValue.Contains(","))
                {
                    return rawValue.Split(',');
                }

                return new[] { rawValue };
            }

            if (propertyType == typeof(int[]))
            {
                if (rawValue.Contains(";"))
                {
                    return rawValue.Split(';').Select(x => Convert.ToInt32(x)).ToArray();
                }

                if (rawValue.Contains(","))
                {
                    return rawValue.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                }

                return rawValue.Select(Convert.ToInt32).ToArray();
            }

            return Convert.ChangeType(rawValue, propertyType);
        }
    }
}
