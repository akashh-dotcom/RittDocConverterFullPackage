namespace R2V2.Web.Infrastructure.Settings
{
    public interface IClientSettings
    {
        string GoogleAnalyticsUrl { get; set; }
        string GoogleAnalyticsAccount { get; set; }
        bool DisableRightClick { get; set; }
        int BrowsePageSize { get; set; }
        int ResourceTimeoutInMinutes { get; set; }
        int ResourceTimeoutModalInSeconds { get; set; }
        string DoodyReviewLink { get; set; }
        string R2CmsContentLink { get; set; }
        int RecaptchaRequestTimeInSeconds { get; set; }
        int RecaptchaNumberOfRequests { get; set; }
        string HomePageCarouselUrl { get; set; }
        string CmsHtmlContentUrl { get; set; }
        int CarouselAutoplaySpeedInSeconds { get; set; }
        int CmsTimeout { get; set; }
        int AutoLockUser { get; set; }
        int AutoLockExpertReviewer { get; set; }
        int AutoLockInstitutionAdmin { get; set; }
        int AutoLockSalesAdmin { get; set; }
        int AutoLockRittenhouseAdmin { get; set; }
        string RittenhouseBaseUrl { get; set; }
        string OoyalaPCode { get; set; }
        string OoyalaPlayerBrandingId { get; set; }
        string FlowplayerKey { get; set; }
        string MediaBaseUrl { get; set; }
        string TrialServiceLoginUrl { get; set; }
        string TrialServiceAccountNumberUrl { get; set; }
    }
}