#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public interface IWebImageSettings : IAutoSettings
    {
        string BookCoverDirectory { get; set; }

        string PublisherImageUrl { get; set; }
        string PublisherImageDirectory { get; set; }
        int BookCoverMaxWidth { get; set; }
        int BookCoverMaxHeight { get; set; }
        int BookCoverMaxSizeInKb { get; set; }
        string SpecialIconDirectory { get; set; }
        string SpecialIconBaseUrl { get; set; }
    }
}