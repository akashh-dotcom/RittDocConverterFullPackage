#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class WebSettings : AutoSettings, IWebSettings
    {
        public bool EnablePromotionToProduction { get; set; }
        public bool DisplayPromotionFields { get; set; }
        public bool DisplayOngoingPdaEventLinks { get; set; }
        public string EnvironmentName { get; set; }
        public string ExternalSearchPubMedUrl { get; set; }
        public string ExternalSearchMeshUrl { get; set; }
        public int ExternalSearchTimeoutInMilliseconds { get; set; }
        public bool EnablePatronDrivenAcquisitions { get; set; }
        public int IpSecurityCacheTimeToLiveInMinutes { get; set; }
        public decimal ResourceMinimumPromotionPrice { get; set; }
        public int ResourcePrintLimitMax { get; set; }
        public int ResourcePrintLimitMin { get; set; }
        public int ResourcePrintLimitPercentage { get; set; }
        public int ResourcePrintLockDurationInHours { get; set; }
        public int ResourcePrintCheckPeriodInHours { get; set; }
        public int ResourcePrintWarningPercentage { get; set; }
        public string ResourcePrintAlertBcc { get; set; }
        public int ServerNumber { get; set; }
        public bool RequireSsl { get; set; }
        public bool MinifyJavascript { get; set; }
    }
}