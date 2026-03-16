#region

using System;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class Layout
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string GoogleAnalyticsAccount { get; set; }
        public bool DisableRightClick { get; set; }
        public bool IsMarketingHome { get; set; }
    }
}