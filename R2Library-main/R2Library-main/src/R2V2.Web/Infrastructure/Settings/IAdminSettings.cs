namespace R2V2.Web.Infrastructure.Settings
{
    public interface IAdminSettings
    {
        bool DisplayAdminTab { get; set; }
        string PurchaseConfirmationEmail { get; set; }
        string ContactUsEmail { get; set; }
        string TrialInitializeEmail { get; set; }
        string MarcRecordWebsite { get; set; }
        string NewAccountNotificationEmail { get; set; }
        string QaApprovalEmailTo { get; set; }
        string QaApprovalEmailCc { get; set; }
        string AlertImageLocation { get; set; }
        string AlertImagePhysicalLocation { get; set; }
        string[] AdminControllAccess { get; set; }
        string[] WindowsServiceConfigurationFile { get; set; }
        string[] UtilitiesConfigurationFile { get; set; }
        string PublicCollectionTabName { get; set; }
    }
}