#region

using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Models
{
    public interface IResourceSummary
    {
        string Isbn { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        string Url { get; set; }
        string ImageUrl { get; }
        string Description { get; }
        string Gist { get; }


        int LicenseCount { get; set; }
        bool ShowLicenseCount { get; }
        void SetShowLicenseCount(AuthenticatedInstitution authenticatedInstitution);
    }
}