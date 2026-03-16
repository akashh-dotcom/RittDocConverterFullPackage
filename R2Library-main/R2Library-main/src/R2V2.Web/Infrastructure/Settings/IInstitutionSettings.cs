namespace R2V2.Web.Infrastructure.Settings
{
    public interface IInstitutionSettings
    {
        string GuestAccountNumber { get; set; }
        string LogoLocation { get; set; }
        string LocalLogoLocation { get; set; }
        int MinimumResourceCountForPaging { get; set; }
        bool HideSubscriptionsTab { get; set; }
    }
}