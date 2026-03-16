#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Settings
{
    public class InstitutionSettings : AutoSettings, IInstitutionSettings
    {
        public string GuestAccountNumber { get; set; }
        public string LogoLocation { get; set; }

        public string LocalLogoLocation { get; set; }

        public int MinimumResourceCountForPaging { get; set; }
        public bool HideSubscriptionsTab { get; set; }
    }
}