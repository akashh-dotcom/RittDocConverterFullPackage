#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.Configuration;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public static class AutoDatabaseSettings
    {
        public static List<SettingGroup> BuildAutoSettings(string configurationKey)
        {
            try
            {
                var configurationSettings = ServiceLocator.Current.GetInstance<IQueryable<DbConfigurationSetting>>();

                var priorityConfigurationSettings = new Dictionary<int, List<DbConfigurationSetting>>();

                var counter = 1;
                while (!string.IsNullOrWhiteSpace(configurationKey))
                {
                    var configurationSettingsFound = configurationSettings
                        .Where(x => x.Configuration.ToLower() == configurationKey.ToLower()).ToList();

                if (configurationKey.Contains("."))
                {
                    var removeThis = configurationKey.Split('.').Last();
                    configurationKey = configurationKey.Replace(
                        string.Format("{1}{0}", removeThis, configurationKey.Length > removeThis.Length ? "." : ""),
                        "");
                }
                else
                {
                    configurationKey = null;
                }

                priorityConfigurationSettings.Add(counter, configurationSettingsFound);
                counter++;
            }

            var settingGroups = new List<SettingGroup>();
            foreach (var priorityConfigurationSetting in priorityConfigurationSettings.OrderBy(x => x.Key))
            {
                foreach (var dbConfigurationSetting in priorityConfigurationSetting.Value)
                {
                    var settingGroup =
                        settingGroups.FirstOrDefault(x => x.SettingName == dbConfigurationSetting.Setting);
                    if (settingGroup != null)
                    {
                        var configurationSetting =
                            settingGroup.ConfigurationSettings.FirstOrDefault(x => x.Key == dbConfigurationSetting.Key);
                        if (configurationSetting == null)
                        {
                            settingGroup.ConfigurationSettings.Add(new ConfigurationSetting(dbConfigurationSetting));
                        }
                        //Setting is already there
                    }
                    else
                    {
                        //Add settings group
                        settingGroup = new SettingGroup
                        {
                            //ConfigurationName =  dbConfigurationSetting.Configuration,
                            SettingName = dbConfigurationSetting.Setting,
                            ConfigurationSettings = new List<ConfigurationSetting>
                            {
                                new ConfigurationSetting(dbConfigurationSetting)
                            }
                        };
                        settingGroups.Add(settingGroup);
                    }
                }
            }

            return settingGroups;
            }
            catch (Exception)
            {
                // If database settings table doesn't exist or can't be queried,
                // return empty settings list and let the application use defaults
                return new List<SettingGroup>();
            }
        }
    }
}