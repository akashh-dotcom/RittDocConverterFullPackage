#region

using System;

#endregion

namespace R2V2.Core.Resource.Content
{
    [Serializable]
    public class ActionMenu
    {
        public bool ShowToc { get; set; }
        public string BrowseSearchText { get; set; }
    }
}