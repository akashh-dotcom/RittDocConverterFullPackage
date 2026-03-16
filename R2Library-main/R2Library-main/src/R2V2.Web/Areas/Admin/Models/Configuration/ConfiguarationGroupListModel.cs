#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Configuration
{
    public class ConfiguarationGroupListModel : AdminBaseModel
    {
        public List<ConfigurationGroupSettingsList> ConfiguarationGroupList { get; set; }

        public string BackUrl { get; set; }
    }
}