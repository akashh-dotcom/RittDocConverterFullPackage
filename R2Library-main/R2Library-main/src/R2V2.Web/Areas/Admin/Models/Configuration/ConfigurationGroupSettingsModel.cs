#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Configuration
{
    public class ConfigurationGroupSettingsModel : AdminBaseModel
    {
        public ConfigurationGroupSettingsList Settings { get; set; }

        public List<PageLink> PageLinks { get; set; }

        public bool DisplayReloadButton { get; set; }

        public string NavigationValue => "Top of Page";

        public string ConfigurationValue { get; set; }

        public SelectList NavigationList { get; set; }

        public SelectList ConfigurationList { get; set; }


        public string BackUrl { get; set; }

        public bool IsLocalDevelopment { get; set; }

        public bool UserConfigLoaded { get; set; }

        public string EnvironmentInfo { get; set; }

        public void SetDropdowns()
        {
            var settingGroupNames = new List<string> { "Top of Page" };
            settingGroupNames.AddRange(Settings.ConfigurationSettings.Keys);

            var settingGroupList = settingGroupNames.Select(item => new SelectListItem { Text = item, Value = item })
                .ToList();

            NavigationList = new SelectList(settingGroupList, "Value", "Text");
        }

        public void SetDropdowns(List<string> groupKeys, UrlHelper url, string configToFind)
        {
            var settingGroupNames = new List<string> { "Top of Page" };
            settingGroupNames.AddRange(Settings.ConfigurationSettings.Keys);

            var settingGroupList = settingGroupNames.Select(item => new SelectListItem { Text = item, Value = item })
                .ToList();

            NavigationList = new SelectList(settingGroupList, "Value", "Text");
            var pageLinkList = groupKeys.Select(key => new SelectListItem
                {
                    Text = key,
                    Value = url.Action("LiveConfigurationSettings", "Configuration", new { configurationName = key }),
                    Selected = key == configToFind
                })
                .ToList();
            ConfigurationList = new SelectList(pageLinkList, "Value", "Text");
            ConfigurationValue = url.Action("LiveConfigurationSettings", "Configuration",
                new { configurationName = configToFind });
        }
    }
}