#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class ToolLinks
    {
        public EmailPage EmailPage { get; set; }

        public PageLink ExcelLink { get; set; }
        public PageLink MarcLink { get; set; }

        public bool HidePrint { get; set; }

        public MarcLinks MarcLinks { get; set; }
    }

    [Serializable]
    public class MarcLinks
    {
        public List<MarcLink> Links { get; set; }
    }

    [Serializable]
    public class MarcLink : PageLink
    {
        public bool IsDeleteLink { get; set; }
    }
}