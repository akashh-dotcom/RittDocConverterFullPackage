#region

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Configuration;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.BackEnd;
using R2V2.Web.Areas.Admin.Models.Configuration;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(IsAdminAuthorizedArea = true)]
    public class ConfigurationController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IAdminSettings _adminSettings;
        private readonly ApplicationWideStorageService _applicationWideStorageService;
        private readonly ConfigurationService _configurationService;
        private readonly ILog<ConfigurationController> _log;
        private readonly IResourceService _resourceService;

        public ConfigurationController(
            IAuthenticationContext authenticationContext
            , IAdminSettings adminSettings
            , IAdminContext adminContext
            , ApplicationWideStorageService applicationWideStorageService
            , IResourceService resourceService
            , ConfigurationService configurationService
            , ILog<ConfigurationController> log
        ) : base(authenticationContext)
        {
            _adminSettings = adminSettings;
            _adminContext = adminContext;
            _applicationWideStorageService = applicationWideStorageService;
            _resourceService = resourceService;
            _configurationService = configurationService;
            _log = log;
        }

        public ActionResult Index()
        {
            var adminInstitution = _adminContext.GetAdminInstitution(CurrentUser.InstitutionId.GetValueOrDefault());
            return View(new AdminIndexModel(adminInstitution, _adminSettings.AdminControllAccess));
        }

        public ActionResult Cache()
        {
            var model = new Cache();
            var cache = _applicationWideStorageService.GetEnumerator();
            var sortedCache = new SortedList<string, ApplicationStorageItem>();
            while (cache.MoveNext())
            {
                var applicationStorageItem = cache.Value as ApplicationStorageItem;
                if (applicationStorageItem != null)
                {
                    if (!applicationStorageItem.Key.ToLower().Contains("cms."))
                    {
                        sortedCache.Add(applicationStorageItem.Key, applicationStorageItem);
                    }
                }
            }

            model.Items = sortedCache.Values;

            return View(model);
        }

        public ActionResult CacheUpdate()
        {
            var cache = _applicationWideStorageService.GetEnumerator();
            while (cache.MoveNext())
            {
                var applicationStorageItem = cache.Value as ApplicationStorageItem;
                if (applicationStorageItem != null)
                {
                    if (!applicationStorageItem.Key.ToLower().Contains("cms."))
                    {
                        _applicationWideStorageService.Remove(applicationStorageItem.Key);
                    }
                }
            }

            _resourceService.GetAllResources();
            return RedirectToAction("Cache");
        }

        public ActionResult CacheUpdateItem(string key)
        {
            var cachedItem = _applicationWideStorageService.Get(key);
            if (cachedItem != null && !key.ToLower().Contains("cms."))
            {
                _applicationWideStorageService.Remove(key);
            }

            return RedirectToAction("Cache");
        }

        public ActionResult ConfigurationGroupList()
        {
            var configuarationGroupList = _configurationService.GetConfigurationGroupSettingsList();

            var model =
                new ConfiguarationGroupListModel
                {
                    ConfiguarationGroupList = configuarationGroupList,
                    BackUrl = Url.Action("ConfigurationGroupList")
                };
            return View(model);
        }

        public ActionResult ConfigurationGroupSettings(string configurationName)
        {
            var configurationSettings = _configurationService.GetConfigurationGroupSettings(configurationName);
            var model = new ConfigurationGroupSettingsModel { Settings = configurationSettings };
            model.SetDropdowns();
            model.BackUrl = string.IsNullOrWhiteSpace(configurationName)
                ? Url.Action("ConfigurationGroupSettings")
                : Url.Action("ConfigurationGroupSettings", new { configurationName });
            return View(model);
        }

        public ActionResult AddConfigurationSetting(string configurationName, string backUrl)
        {
            var settingModel = new ConfigurationSettingModel
            {
                Setting = new ConfigurationSetting(),
                BackLinkUrl = backUrl,
                ConfigurationName = configurationName
            };

            return View("EditConfigurationSetting", settingModel);
        }

        [HttpPost]
        public ActionResult AddConfigurationSetting(ConfigurationSettingModel settingModel)
        {
            _configurationService.SaveConfigurationSetting(settingModel.Setting);
            return Redirect(settingModel.BackLinkUrl);
        }

        public ActionResult EditConfigurationSetting(string configurationName, int configurationSettingId,
            string backUrl)
        {
            var webConfigurationSetting = _configurationService.GetConfigurationSetting(configurationSettingId);

            var settingModel = new ConfigurationSettingModel
            {
                Setting = webConfigurationSetting,
                BackLinkUrl = backUrl,
                ConfigurationName = configurationName
            };
            return View(settingModel);
        }

        [HttpPost]
        public ActionResult EditConfigurationSetting(ConfigurationSettingModel settingModel)
        {
            _configurationService.SaveConfigurationSetting(settingModel.Setting);
            return Redirect(settingModel.BackLinkUrl);
        }


        public ActionResult Delete(string configurationName, int configurationSettingId, string backUrl)
        {
            var webConfigurationSetting = _configurationService.GetConfigurationSetting(configurationSettingId);

            var settingModel = new ConfigurationSettingModel
            {
                Setting = webConfigurationSetting,
                BackLinkUrl = backUrl,
                ConfigurationName = configurationName
            };
            return View(settingModel);
        }

        [HttpPost]
        public ActionResult Delete(ConfigurationSettingModel settingModel)
        {
            _configurationService.DeleteConfigurationSetting(settingModel.Setting.Id);
            return View(new ConfigurationSettingModel
            {
                BackLinkUrl = settingModel.BackLinkUrl
            });
        }


        public ActionResult ReloadSettings()
        {
            SettingsInitializer.Initialize(
                AutoDatabaseSettings.BuildAutoSettings(ConfigurationManager.AppSettings["SettingsConfigurationKey"]));
            return RedirectToAction("LiveConfigurationSettings");
        }

        public ActionResult LiveConfigurationSettings(string configurationName)
        {
            _log.Info(ConfigurationManager.AppSettings["SettingsConfigurationKey"]);
            var model = new ConfigurationGroupSettingsModel();

            // Check User.config status and local development mode
            var isLocalDev = ConfigurationManager.AppSettings["Environment.IsLocalDevelopment"];
            var messageQueueEnabled = ConfigurationManager.AppSettings["MessageQueue.Enabled"];
            
            model.IsLocalDevelopment = isLocalDev != null && isLocalDev.Equals("true", System.StringComparison.OrdinalIgnoreCase);
            model.UserConfigLoaded = isLocalDev != null || messageQueueEnabled != null;
            
            if (model.UserConfigLoaded)
            {
                model.EnvironmentInfo = $"User.config is LOADED | Local Dev: {(model.IsLocalDevelopment ? "YES" : "NO")} | MessageQueue: {messageQueueEnabled ?? "N/A"}";
            }
            else
            {
                model.EnvironmentInfo = "User.config is NOT LOADED - using Web.config only";
            }

            var settingGroupNames = _configurationService.GetConfigurationKeyNames();

            var configNameToFind = configurationName;

            if (string.IsNullOrWhiteSpace(configurationName))
            {
                var key = ConfigurationManager.AppSettings["SettingsConfigurationKey"];
                configNameToFind = settingGroupNames.FirstOrDefault(x => x.Contains(key));
            }

            var settingGroups = _configurationService.GetConfigurationSettingGroupItems(configNameToFind, true);

            if (configNameToFind != null &&
                configNameToFind.Contains(ConfigurationManager.AppSettings["SettingsConfigurationKey"]))
            {
                model.DisplayReloadButton = true;
            }

            model.Settings = _configurationService.GetLiveConfigurationSettings(configNameToFind, settingGroups);
            model.SetDropdowns(settingGroupNames, Url, configNameToFind);
            model.BackUrl = string.IsNullOrWhiteSpace(configurationName)
                ? Url.Action("LiveConfigurationSettings")
                : Url.Action("LiveConfigurationSettings", new { configurationName });

            return View(model);
        }

        public ActionResult SettingGroupList(string settingName, string backUrl)
        {
            var model = new ConfigurationGroupSettingsModel
            {
                Settings = _configurationService.GetSettingGroupConfigurationList(settingName),
                BackUrl = backUrl
            };
            return View(model);
        }
    }
}