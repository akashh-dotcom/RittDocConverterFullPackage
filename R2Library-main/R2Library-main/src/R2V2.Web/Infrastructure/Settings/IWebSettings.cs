namespace R2V2.Web.Infrastructure.Settings
{
    public interface IWebSettings
    {
        bool EnablePromotionToProduction { get; set; }
        bool DisplayPromotionFields { get; set; }
        bool DisplayOngoingPdaEventLinks { get; set; }
        string EnvironmentName { get; set; }
        string ExternalSearchPubMedUrl { get; set; }
        string ExternalSearchMeshUrl { get; set; }
        int ExternalSearchTimeoutInMilliseconds { get; set; }
        bool EnablePatronDrivenAcquisitions { get; set; }
        int IpSecurityCacheTimeToLiveInMinutes { get; set; }
        decimal ResourceMinimumPromotionPrice { get; set; }
        int ResourcePrintLimitMax { get; set; }
        int ResourcePrintLimitMin { get; set; }
        int ResourcePrintLimitPercentage { get; set; }
        int ResourcePrintLockDurationInHours { get; set; }
        int ResourcePrintCheckPeriodInHours { get; set; }
        int ResourcePrintWarningPercentage { get; set; }
        string ResourcePrintAlertBcc { get; set; }
        int ServerNumber { get; set; }
        bool RequireSsl { get; set; }
        bool MinifyJavascript { get; set; }
    }
}