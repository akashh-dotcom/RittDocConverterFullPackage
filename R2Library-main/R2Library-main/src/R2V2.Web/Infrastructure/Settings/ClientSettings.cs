#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class ClientSettings : AutoSettings, IClientSettings
    {
        public string GoogleAnalyticsAccount { get; set; }
        public string GoogleAnalyticsUrl { get; set; }
        public bool DisableRightClick { get; set; }
        public int BrowsePageSize { get; set; }
        public int ResourceTimeoutInMinutes { get; set; }
        public int ResourceTimeoutModalInSeconds { get; set; }
        public string DoodyReviewLink { get; set; }
        public string R2CmsContentLink { get; set; }
        public int RecaptchaRequestTimeInSeconds { get; set; }
        public int RecaptchaNumberOfRequests { get; set; }
        public string HomePageCarouselUrl { get; set; }
        public string CmsHtmlContentUrl { get; set; }
        public int CarouselAutoplaySpeedInSeconds { get; set; }
        public int CmsTimeout { get; set; }
        public int AutoLockUser { get; set; }
        public int AutoLockExpertReviewer { get; set; }
        public int AutoLockInstitutionAdmin { get; set; }
        public int AutoLockSalesAdmin { get; set; }
        public int AutoLockRittenhouseAdmin { get; set; }
        public string RittenhouseBaseUrl { get; set; }
        public string OoyalaPCode { get; set; }
        public string OoyalaPlayerBrandingId { get; set; }
        public string FlowplayerKey { get; set; }
        public string MediaBaseUrl { get; set; }
        public string TrialServiceLoginUrl { get; set; }
        public string TrialServiceAccountNumberUrl { get; set; }
    }
}