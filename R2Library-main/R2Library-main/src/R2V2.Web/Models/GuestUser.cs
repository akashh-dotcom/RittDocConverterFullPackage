#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Models
{
    public class GuestUser : BaseModel
    {
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "User Name")] public string UserName { get; set; }

        [Display(Name = "New Password")]
        [Required]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}