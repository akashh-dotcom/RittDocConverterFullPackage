#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.InstitutionReferrer
{
    public class InstitutionReferrer
    {
        public int InstitutionId { get; set; }

        [Required] public string ValidReferer { get; set; }
        public int ValidReferrerId { get; set; }

        public bool WasVerified { get; set; }
        public string VerifiedReferrer { get; set; }
    }
}