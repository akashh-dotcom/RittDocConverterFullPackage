#region

using System;

#endregion

namespace R2V2.Core.Resource.Content.Navigation
{
    [Serializable]
    public class Navigation
    {
        public NavigationItem Previous { get; set; }
        public NavigationItem Current { get; set; }
        public NavigationItem Next { get; set; }

        public NavigationItem Book { get; set; }
        public NavigationItem Part { get; set; }
        public NavigationItem Chapter { get; set; }
        public NavigationItem Section { get; set; }
    }
}