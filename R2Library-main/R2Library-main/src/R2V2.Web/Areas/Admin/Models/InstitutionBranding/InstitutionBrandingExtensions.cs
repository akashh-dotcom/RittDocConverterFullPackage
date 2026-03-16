#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionBranding
{
    public static class InstitutionBrandingExtensions
    {
        public static InstitutionBranding ToInstitutionBranding(
            this Core.Institution.InstitutionBranding institutionBranding, IAdminInstitution institution)
        {
            return new InstitutionBranding(institution)
            {
                InstitutionId = institutionBranding.Institution.Id,
                InstitutionBrandingId = institutionBranding.Id,
                Message = institutionBranding.Message,
                InstitutionDisplayName = institutionBranding.InstitutionDisplayName,
                LogoFileName = institutionBranding.LogoFileName
            };
        }
    }
}