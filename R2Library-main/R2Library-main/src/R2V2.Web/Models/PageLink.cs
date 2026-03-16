#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class PageLink
    {
        public string Text { get; set; }
        public string Href { get; set; }
        public bool Active { get; set; } = true;
        public bool Selected { get; set; } = false;

        public string HoverText { get; set; }
        public string Target { get; set; }

        public IEnumerable<PageLinkSection> ChildLinks { get; set; }
    }

    [Serializable]
    public class SortLink : PageLink
    {
        public string HrefAscending { get; set; }
        public string HrefDescending { get; set; }
    }
}