#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Infrastructure.Settings
{
    public class WindowsServiceSettings : AutoSettings, IWindowsServiceSettings
    {
        // database connection string settings
        public string R2ReportsConnectionString { get; set; }
        public string RIT001ProductionConnectionString { get; set; }
        public string R2UtilitiesProductionConnectionString { get; set; }
        public string RIT001StagingConnectionString { get; set; }

        public int GoogleAnalyticsTimeoutInMilliseconds { get; set; }
        public int GoogleAnalyticsSecondsToPauseAfterException { get; set; }
        public string MessageFailureDirectory { get; set; }

        // promotion settings
        public string PromoteXmlSourceDirectory { get; set; }
        public string PromoteXmlDestinationDirectory { get; set; }
        public string PromoteImagesSourceDirectory { get; set; }
        public string PromoteImagesDestinationDirectory { get; set; }
        public string PromoteCoverImageSourceDirectory { get; set; }
        public string PromoteCoverImageDestinationDirectory { get; set; }
        public string PromoteStatusEmailToAddresses { get; set; }
        public string PromoteFromEmailAddress { get; set; }
        public string PromoteFromDisplayName { get; set; }
        public string PromoteSqlScriptFile { get; set; }
        public string PromoteStagingDomain { get; set; }
        public string PromoteProductionDomain { get; set; }
        public int PromoteAutoLicenseCount { get; set; }
        public string PromoteMediaFilesSourceDirectory { get; set; }

        public string PromoteMediaFilesDestinationDirectory { get; set; }

        // order relay settings
        public string OrderRelayShipToNumber { get; set; }
        public string OrderRelayBlindShipFlag { get; set; }
        public string OrderRelayFollettCorpFlag { get; set; }
        public string OrderRelayResidentFlag { get; set; }
        public string OrderRelayOrderType { get; set; }
        public string OrderRelayShipVia { get; set; }
        public string OrderRelayWrittenBy { get; set; }
        public string OrderRelayAcknowledgeFlag { get; set; }
        public string OrderRelayAdminHoldFlag { get; set; }
        public string OrderRelayAdminHoldManager { get; set; }

        public string OrderRelayLogDirectory { get; set; }

        // location to write order file logs
        public string OrderRelayOrderFileNamePrefix { get; set; }

        // should not be set in production, used as a way to distinguish dev & stage orders
        public bool OrderRelaySendOrderFileToPrelude { get; set; }

        // true = send file to prelude, false = no not sent file to Prelude, used for testing
        public string OrderRelaySFtpHost { get; set; }
        public string OrderRelaySFtpUsername { get; set; }
        public string OrderRelaySFtpPassword { get; set; }
        public string OrderRelayInternalOrderEmailToAddresses { get; set; }
        public string OrderRelayInternalOrderEmailFromAddress { get; set; }
    }
}