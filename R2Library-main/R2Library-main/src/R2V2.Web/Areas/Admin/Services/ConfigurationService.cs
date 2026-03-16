#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.Configuration;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Models.Configuration;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class ConfigurationService
    {
        private readonly IAdminSettings _adminSettings;
        private readonly IQueryable<DbConfigurationSetting> _configurationSettings;
        private readonly ILog<ConfigurationService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ConfigurationService(
            IQueryable<DbConfigurationSetting> configurationSettings
            , ILog<ConfigurationService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IAdminSettings adminSettings)
        {
            _configurationSettings = configurationSettings;
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _adminSettings = adminSettings;
        }

        public List<ConfigurationGroupSettingsList> GetConfigurationGroupSettingsList()
        {
            var configuarationGroupList = _configurationSettings.GroupBy(x => x.Configuration).Select(y =>
                new ConfigurationGroupSettingsList
                {
                    ConfigurationName = y.Key,
                    NumberOfSettings = y.Select(x => x.Configuration).Count()
                }).ToList();

            return configuarationGroupList;
        }

        public ConfigurationSetting GetConfigurationSetting(int configurationSettingId)
        {
            var configurationSetting = _configurationSettings.FirstOrDefault(x => x.Id == configurationSettingId);
            if (configurationSetting == null)
            {
                return null;
            }

            var webConfigurationSetting = new ConfigurationSetting
            {
                Id = configurationSetting.Id,
                Configuration = configurationSetting.Configuration,
                Setting = configurationSetting.Setting,
                Description = configurationSetting.Description,
                Key = configurationSetting.Key,
                Value = configurationSetting.Value
            };
            return webConfigurationSetting;
        }

        public bool DeleteConfigurationSetting(int configurationSettingId)
        {
            var configurationSetting = _configurationSettings.FirstOrDefault(x => x.Id == configurationSettingId);
            if (configurationSetting == null)
            {
                return false;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        uow.Delete(configurationSetting);

                        uow.Commit();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        return false;
                    }

                    return true;
                }
            }
        }

        public void SaveConfigurationSetting(ConfigurationSetting configurationSetting)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var dbConfigurationSetting =
                        _configurationSettings.FirstOrDefault(x => x.Id == configurationSetting.Id) ??
                        new DbConfigurationSetting();

                    dbConfigurationSetting.Configuration = configurationSetting.Configuration;
                    dbConfigurationSetting.Description = configurationSetting.Description;
                    dbConfigurationSetting.Key = configurationSetting.Key;
                    dbConfigurationSetting.Setting = configurationSetting.Setting;
                    dbConfigurationSetting.Value = configurationSetting.Value;

                    uow.SaveOrUpdate(dbConfigurationSetting);

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }


        public ConfigurationGroupSettingsList GetConfigurationGroupSettings(string configurationName)
        {
            var dbsettings =
                _configurationSettings.Where(x => x.Configuration == configurationName)
                    .OrderBy(x => x.Setting)
                    .ThenBy(x => x.Key)
                    .ToList();

            var list = new Dictionary<string, List<ConfigurationSetting>>();
            foreach (var setting in dbsettings)
            {
                var configSetting = new ConfigurationSetting(setting);

                if (list.ContainsKey(setting.Setting))
                {
                    list[setting.Setting].Add(configSetting);
                }
                else
                {
                    list.Add(setting.Setting, new List<ConfigurationSetting> { configSetting });
                }
            }

            return PopulateConfigurationGroupSettingsList(configurationName, list, dbsettings.Count);
        }

        public ConfigurationGroupSettingsList GetLiveConfigurationSettings(string configurationName,
            List<SettingGroup> settingGroups)
        {
            IDictionary<string, List<ConfigurationSetting>> list = new Dictionary<string, List<ConfigurationSetting>>();

            var settingCount = 0;
            foreach (var settingGroup in settingGroups)
            {
                var websettings = new List<ConfigurationSetting>();
                string setting = null;
                foreach (var configurationSetting in settingGroup.ConfigurationSettings)
                {
                    if (string.IsNullOrWhiteSpace(setting))
                    {
                        setting = configurationSetting.Setting;
                    }

                    websettings.Add(configurationSetting);
                    settingCount++;
                }

                websettings = websettings.OrderBy(x => x.Key).ToList();

                list.Add(setting, websettings);
            }

            return PopulateConfigurationGroupSettingsList(configurationName, list, settingCount);
        }

        public ConfigurationGroupSettingsList GetSettingGroupConfigurationList(string settingGroupName)
        {
            var settingGroup = GetSettingGroupItems(settingGroupName);
            var list = new Dictionary<string, List<ConfigurationSetting>>
            {
                { settingGroupName, settingGroup.ConfigurationSettings }
            };
            return PopulateConfigurationGroupSettingsList("", list, settingGroup.ConfigurationSettings.Count);
        }

        private ConfigurationGroupSettingsList PopulateConfigurationGroupSettingsList(string configurationName,
            IDictionary<string, List<ConfigurationSetting>> settingGroups, int settingCount)
        {
            var settings = new ConfigurationGroupSettingsList
            {
                ConfigurationName = configurationName,
                ConfigurationSettings = settingGroups.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value),
                NumberOfSettings = settingCount
            };

            return settings;
        }

        public List<SettingGroup> GetConfigurationSettingGroupItems(string configurationKey, bool isLiveSetting)
        {
            var keys = GetConfigurationKeyNames();
            var key = keys.FirstOrDefault(x => x == configurationKey);

            var databaseSettings = AutoDatabaseSettings.BuildAutoSettings(key);

            if (configurationKey.ToLower().Contains("web"))
            {
                return isLiveSetting
                    ? GetLiveWebSettings(databaseSettings
                        .Where(x => GetWebSettingNames().Contains(x.SettingName.ToLower())).ToList())
                    : databaseSettings.Where(x => GetWebSettingNames().Contains(x.SettingName.ToLower())).ToList();
            }

            if (configurationKey.ToLower().Contains("utilities"))
            {
                return databaseSettings.Where(x => GetUtilitiesSettingNames().Contains(x.SettingName.ToLower()))
                    .ToList();
            }

            return configurationKey.ToLower().Contains("service")
                ? databaseSettings.Where(x => GetServiceSettingNames().Contains(x.SettingName.ToLower())).ToList()
                : null;
        }

        private SettingGroup GetSettingGroupItems(string settingGroupName)
        {
            var configurationSettings = _configurationSettings.Where(x => x.Setting == settingGroupName);

            var settingGroup = new SettingGroup
            {
                ConfigurationSettings = configurationSettings.Select(x => new ConfigurationSetting(x)).ToList(),
                SettingName = settingGroupName
            };
            return settingGroup;
        }

        private List<SettingGroup> GetLiveWebSettings(List<SettingGroup> databaseSettings)
        {
            var settings = ServiceLocator.Current.GetAllInstances<IAutoSettings>().ToList();

            foreach (var autoSetting in settings)
            {
                var objectsInAutoSetting = autoSetting.GetType().GetProperties();
                if (objectsInAutoSetting.Any())
                {
                    var settingName = objectsInAutoSetting.First().DeclaringType?.Name.Replace("Settings", "");

                    var settingGroup = databaseSettings.FirstOrDefault(x => x.SettingName == settingName);
                    if (settingGroup == null)
                    {
                        continue;
                    }

                    foreach (var propertyInfo in objectsInAutoSetting.Where(x => x.Name != "MissingSettings"))
                    {
                        var objectValue = propertyInfo.GetValue(autoSetting, null);

                        var dbSetting =
                            settingGroup.ConfigurationSettings.FirstOrDefault(x => x.Key == propertyInfo.Name);

                        if (dbSetting == null)
                        {
                            continue;
                        }

                        var value = objectValue.ToString();

                        if (objectValue.GetType() == typeof(string[]))
                        {
                            var seperator = GetArraySeperator(dbSetting.Value);

                            var objectArray = objectValue as string[];
                            value = objectArray?.Aggregate((a, b) => $"{a}{seperator}{b}");
                        }
                        else if (objectValue.GetType() == typeof(int[]))
                        {
                            var seperator = GetArraySeperator(dbSetting.Value);

                            var objectArray = objectValue as int[];
                            // ReSharper disable once AssignNullToNotNullAttribute
                            value = string.Join(seperator, objectArray.Select(x => x.ToString()).ToArray());
                        }


                        if (value != null && !value.Equals(dbSetting.Value, StringComparison.CurrentCultureIgnoreCase))
                        {
                            dbSetting.NewValue = $"Old Value [{value}] will not change until reload is forced.";
                        }
                    }
                }
            }

            return databaseSettings;
        }

        private static string GetArraySeperator(string value)
        {
            string seperator;
            if (value.Contains(";"))
            {
                seperator = ";";
            }
            else if (value.Contains(","))
            {
                seperator = ",";
            }
            else
            {
                seperator = "";
            }

            return seperator;
        }

        public List<string> GetConfigurationKeyNames()
        {
            var utilitieFiles = _adminSettings.UtilitiesConfigurationFile;
            var serviceFiles = _adminSettings.WindowsServiceConfigurationFile;


            var utilitieFilesKeys = utilitieFiles.Select(ParseKeyFromFile).ToList();
            var serviceFilesKeys = serviceFiles.Select(ParseKeyFromFile).ToList();
            _log.Info($"UtilitieFile Keys: {string.Join(" || ", utilitieFilesKeys)}");
            _log.Info($"ServiceFile Keys: {string.Join(" || ", serviceFilesKeys)}");
            _log.Info($"Settings Configuration Key: {ConfigurationManager.AppSettings["SettingsConfigurationKey"]}");

            utilitieFilesKeys.RemoveAll(x => x == null);
            serviceFilesKeys.RemoveAll(x => x == null);

            var keys = new List<string>();

            keys.AddRange(utilitieFilesKeys);
            keys.AddRange(serviceFilesKeys);
            keys.Add(ConfigurationManager.AppSettings["SettingsConfigurationKey"]);

            return keys;
        }

        private string ParseKeyFromFile(string fileName)
        {
            _log.Info($"ParseKeyFromFile(fileName: {fileName})");

            var file = new FileInfo(fileName);
            if (file.Exists)
            {
                _log.InfoFormat("Analyzing File:{0}", file.Name);

                var doc = new XmlDocument();
                doc.Load(file.FullName);

                var xmlNodeList = doc.SelectNodes("//appSettings//add");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode node in xmlNodeList)
                    {
                        if (node.Attributes != null)
                        {
                            var key = node.Attributes["key"].Value;
                            var value = node.Attributes["value"].Value;
                            if (key == "SettingsConfigurationKey")
                            {
                                return value;
                            }
                        }
                    }
                }

                _log.Error(
                    $"ParseKeyFromFile(fileName: {fileName}) - Setting 'SettingsConfigurationKey' was NOT found!!!");
            }
            else
            {
                _log.Error($"ParseKeyFromFile(fileName: {fileName}) - File does NOT exist!!!");
            }

            return null;
        }

        private List<string> GetWebSettingNames()
        {
            return new List<string>(GetCoreSettingNames())
            {
                "admin",
                "cache",
                "client",
                "institution",
                "webimage",
                "web"
            };
        }

        private List<string> GetUtilitiesSettingNames()
        {
            return new List<string>(GetCoreSettingNames())
            {
                "r2utilities",
                "indextermhighlight",
                "taberstermhighlight"
            };
        }

        private List<string> GetServiceSettingNames()
        {
            return new List<string>(GetCoreSettingNames())
            {
                "windowsservice"
            };
        }

        private List<string> GetCoreSettingNames()
        {
            return new List<string>
            {
                "content",
                "collectionmanagement",
                "email",
                "messagequeue"
            };
        }
    }
}