namespace R2V2.Infrastructure.Settings
{
    public interface ICollectionManagementSettings
    {
        int AnnualMaintenanceFeeProductId { get; set; }
        int PatronDriveAcquisitionMaxViews { get; set; }
        int SubscriptionTrialDays { get; set; }
        string YbpOrderFileLocation { get; set; }
        string YbpSecretKey { get; set; }
        string YbpRapidOrderRecipients { get; set; }
        string OasisOrderFileLocation { get; set; }
        string OasisSecretKey { get; set; }
        string OasisRapidOrderRecipients { get; set; }
    }
}