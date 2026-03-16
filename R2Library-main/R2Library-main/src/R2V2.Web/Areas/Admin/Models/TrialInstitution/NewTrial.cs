#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.TrialInstitution
{
    public class NewTrial : AdminBaseModel
    {
        [Required]
        [Display(Name = "Account Number:")]
        public string AccountNumber { get; set; }

        public bool DisplayTestButton { get; set; }
    }
}