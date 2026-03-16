#region

using System;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class ResourceSummaryBase : BaseModel, IResourceSummary
    {
        private bool _showLicenseCount { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string Gist { get; set; }
        public int LicenseCount { get; set; }
        public bool ShowLicenseCount { get; protected set; }

        public void SetShowLicenseCount(AuthenticatedInstitution authenticatedInstitution)
        {
            if (authenticatedInstitution == null)
            {
                _showLicenseCount = false;
                return;
            }

            IUser user = authenticatedInstitution.User;
            if (user != null && user.Role != null && authenticatedInstitution.AccountStatus.Id == AccountStatus.Active)
            {
                if (user.Role.Code == RoleCode.INSTADMIN || user.Role.Code == RoleCode.RITADMIN)
                {
                    _showLicenseCount = true;
                    return;
                }
            }

            _showLicenseCount = false;
        }
    }
}