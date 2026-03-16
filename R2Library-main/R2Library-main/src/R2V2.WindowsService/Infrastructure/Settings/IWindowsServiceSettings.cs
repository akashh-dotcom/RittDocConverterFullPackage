namespace R2V2.WindowsService.Infrastructure.Settings
{
    public interface IWindowsServiceSettings
    {
        string OrderRelayAcknowledgeFlag { get; set; }
        string OrderRelayAdminHoldFlag { get; set; }
        string OrderRelayAdminHoldManager { get; set; }
        string OrderRelayBlindShipFlag { get; set; }
        string OrderRelayFollettCorpFlag { get; set; }
        string OrderRelayInternalOrderEmailFromAddress { get; set; }
        string OrderRelayInternalOrderEmailToAddresses { get; set; }
        string OrderRelayLogDirectory { get; set; }
        string OrderRelayOrderFileNamePrefix { get; set; }
        string OrderRelayOrderType { get; set; }
        string OrderRelayResidentFlag { get; set; }
        bool OrderRelaySendOrderFileToPrelude { get; set; }
        string OrderRelaySFtpHost { get; set; }
        string OrderRelaySFtpPassword { get; set; }
        string OrderRelaySFtpUsername { get; set; }
        string OrderRelayShipToNumber { get; set; }
        string OrderRelayShipVia { get; set; }
        string OrderRelayWrittenBy { get; set; }
        int PromoteAutoLicenseCount { get; set; }
        string PromoteCoverImageDestinationDirectory { get; set; }
        string PromoteCoverImageSourceDirectory { get; set; }
        string PromoteFromDisplayName { get; set; }
        string PromoteFromEmailAddress { get; set; }
        string PromoteImagesDestinationDirectory { get; set; }
        string PromoteImagesSourceDirectory { get; set; }
        string PromoteMediaFilesDestinationDirectory { get; set; }
        string PromoteMediaFilesSourceDirectory { get; set; }
        string PromoteProductionDomain { get; set; }
        string PromoteSqlScriptFile { get; set; }
        string PromoteStagingDomain { get; set; }
        string PromoteStatusEmailToAddresses { get; set; }
        string PromoteXmlDestinationDirectory { get; set; }
        string PromoteXmlSourceDirectory { get; set; }
        string R2ReportsConnectionString { get; set; }
        string R2UtilitiesProductionConnectionString { get; set; }
        string RIT001ProductionConnectionString { get; set; }
        string RIT001StagingConnectionString { get; set; }

        int GoogleAnalyticsTimeoutInMilliseconds { get; set; }
        int GoogleAnalyticsSecondsToPauseAfterException { get; set; }

        string MessageFailureDirectory { get; set; }
    }
}