#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class PageLinkSection
    {
        public string Title { get; set; }
        public IEnumerable<PageLink> PageLinks { get; set; }
        public string Href { get; set; }
        public bool Active { get; set; }
        public bool Selected { get; set; }
        public int? NumberOfVisibleLinks { get; set; }
    }
}