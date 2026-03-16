#region

using System;
using System.Collections.Generic;
using System.Configuration;
using R2V2.Extensions;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public interface ISettingsBuilderService
    {
        void Load(object settingContainer, IList<string> missingSettings);
    }

    public class SettingsBuilderService : ISettingsBuilderService
    {
        public void Load(object settingContainer, IList<string> missingSettings)
        {
            var containerType = settingContainer.GetType();
            var containerPreFix = containerType.Name.Replace("Settings", "");
            var properties = containerType.GetProperties();

            foreach (var p in properties)
            {
                if (p.DeclaringType != containerType)
                {
                    continue;
                }

                if (!p.CanWrite)
                {
                    continue;
                }

                var settingName = "{0}.{1}".Args(containerPreFix, p.Name);
                var valStr = ConfigurationManager.AppSettings[settingName];
                if (valStr == null)
                {
                    missingSettings.Add(
                        "Please set {0} setting in the database table tConfigurationSetting".Args(settingName));
                    continue;
                }

                var propertyType = p.PropertyType;

                object val = null;

                if (propertyType == typeof(int))
                {
                    val = int.Parse(valStr);
                }

                if (propertyType == typeof(decimal))
                {
                    val = decimal.Parse(valStr);
                }

                if (propertyType == typeof(double))
                {
                    val = double.Parse(valStr);
                }

                if (propertyType == typeof(bool))
                {
                    val = bool.Parse(valStr);
                }

                if (propertyType == typeof(string))
                {
                    val = valStr;
                }

                if (propertyType == typeof(string[]))
                {
                    val = valStr.Split(',');
                }

                if (propertyType == typeof(DateTime))
                {
                    val = DateTime.Parse(valStr);
                }

                try
                {
                    p.SetValue(settingContainer, val, null);
                }
                catch (Exception ex)
                {
                    missingSettings.Add(ex.Message);
                }
            }
        }
    }
}