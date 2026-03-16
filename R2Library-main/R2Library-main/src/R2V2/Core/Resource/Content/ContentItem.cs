#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource.Content
{
    public class ContentItem
    {
        public string Html { get; set; }
        public Navigation.Navigation Navigation { get; set; }
        public IEnumerable<string> Topics { get; set; }
    }
}