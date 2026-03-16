#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class AdminSettings : AutoSettings, IAdminSettings
    {
        public bool DisplayAdminTab { get; set; }
        public string PurchaseConfirmationEmail { get; set; }
        public string ContactUsEmail { get; set; }
        public string TrialInitializeEmail { get; set; }
        public string MarcRecordWebsite { get; set; }
        public string NewAccountNotificationEmail { get; set; }
        public string[] AdminControllAccess { get; set; }
        public string[] WindowsServiceConfigurationFile { get; set; }
        public string[] UtilitiesConfigurationFile { get; set; }
        public string QaApprovalEmailTo { get; set; }
        public string QaApprovalEmailCc { get; set; }
        public string AlertImageLocation { get; set; }
        public string AlertImagePhysicalLocation { get; set; }
        public string PublicCollectionTabName { get; set; }
    }
}