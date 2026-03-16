#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class WebImageSettings : AutoSettings, IWebImageSettings
    {
        public string BookCoverDirectory { get; set; }
        public string PublisherImageUrl { get; set; }
        public string PublisherImageDirectory { get; set; }
        public int BookCoverMaxWidth { get; set; }
        public int BookCoverMaxHeight { get; set; }
        public int BookCoverMaxSizeInKb { get; set; }
        public string SpecialIconDirectory { get; set; }
        public string SpecialIconBaseUrl { get; set; }
    }
}