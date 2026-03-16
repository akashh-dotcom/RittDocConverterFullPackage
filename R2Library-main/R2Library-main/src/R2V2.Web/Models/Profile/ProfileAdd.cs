#region

using System.ComponentModel.DataAnnotations;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Models.Profile
{
    public class ProfileAdd : BaseModel
    {
        public UserEdit User { get; set; }

        [Display(Name = "New Password")]
        [PasswordValidation]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = @"Username")]
        [Required]
        [StringLength(50, ErrorMessage = @"User Name Max Length is 50 and Minimum Length is 4 characters.",
            MinimumLength = 4)]
        public string UserName { get; set; }

        public bool IsExpertReviewerEnabled { get; set; }

        public string AthensTargetedId { get; set; }
    }
}