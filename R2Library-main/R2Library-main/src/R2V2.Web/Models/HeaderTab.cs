#region

using System;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class HeaderTab
    {
        public string DisplayText { get; set; }
        public string Url { get; set; }
        public bool IsSelected { get; set; }
    }
}