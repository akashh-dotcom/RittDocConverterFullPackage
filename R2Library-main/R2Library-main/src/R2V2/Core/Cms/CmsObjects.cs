#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.Cms
{
    public class CmsObjects
    {
    }

    [Serializable]
    public class R2LibraryCarousel
    {
        private int _autoplaySpeedMilliseconds = 12000;

        public List<R2LibraryCarouselItem> Items { get; set; } = new List<R2LibraryCarouselItem>();

        public int AutoplaySpeedMilliseconds
        {
            get => _autoplaySpeedMilliseconds;
            set => _autoplaySpeedMilliseconds = value == 0 ? _autoplaySpeedMilliseconds : value;
        }
    }

    [Serializable]
    public class R2LibraryCarouselItem
    {
        public string ImageUrl { get; set; }
        public string DestinationUrl { get; set; }
        public int SortOrder { get; set; }
        public string NavigationText { get; set; }
        public string NavigationHoverText { get; set; }
    }

    [Serializable]
    public class R2LibraryCmsItem
    {
        public string Html { get; set; }
    }

    [Serializable]
    public class R2LibraryCmsContentPage
    {
        public string Html { get; set; }
        public string Title { get; set; }
    }
}