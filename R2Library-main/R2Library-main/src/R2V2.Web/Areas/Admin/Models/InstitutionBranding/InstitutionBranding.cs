#region

using System.ComponentModel.DataAnnotations;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionBranding
{
    public class InstitutionBranding : AdminBaseModel
    {
        public int InstitutionBrandingId;

        public InstitutionBranding()
        {
        }

        public InstitutionBranding(IAdminInstitution institution) : base(institution)
        {
        }

        [Display(Name = "Message:")] public string Message { get; set; }

        [Required]
        [Display(Name = "Display Name:")]
        public string InstitutionDisplayName { get; set; }

        [Display(Name = "Logo:")] public string LogoDisplayUrl { get; set; }

        public string LogoFileName { get; set; }

        public void SetLogoDisplayUrl(string defaultLocation)
        {
            LogoDisplayUrl = string.IsNullOrWhiteSpace(LogoFileName) ? "" : $"{defaultLocation}{LogoFileName}";
        }
    }
}