#region

using System.ComponentModel.DataAnnotations;
using R2V2.Web.Areas.Admin.Models.Institution;
using R2V2.Web.Areas.Admin.Models.User;

#endregion

namespace R2V2.Web.Models.Trial
{
    public class Trial : BaseModel
    {
        public Institution Institution { get; set; }
        public UserEdit User { get; set; }

        public string AccountNumber { get; set; }
        public string Timestamp { get; set; }
        public string Hash { get; set; }

        public string ErrorMessage { get; set; }
        public bool Error { get; set; }

        [Display(Name = "New Password")]
        [Required]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public string FormPost { get; set; }
    }
}