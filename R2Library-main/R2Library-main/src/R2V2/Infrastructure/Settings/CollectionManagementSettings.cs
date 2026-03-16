namespace R2V2.Infrastructure.Settings
{
    public class CollectionManagementSettings : AutoSettings, ICollectionManagementSettings
    {
        public int AnnualMaintenanceFeeProductId { get; set; }
        public int PatronDriveAcquisitionMaxViews { get; set; }
        public int SubscriptionTrialDays { get; set; }
        public string YbpOrderFileLocation { get; set; }
        public string YbpSecretKey { get; set; }
        public string YbpRapidOrderRecipients { get; set; }
        public string OasisOrderFileLocation { get; set; }
        public string OasisSecretKey { get; set; }
        public string OasisRapidOrderRecipients { get; set; }
    }
}