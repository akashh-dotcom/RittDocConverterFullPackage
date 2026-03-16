#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PublisherConsolidation
{
    public class PublisherUser
    {
        public int Id { get; set; }

        public int PublisherId { get; set; }

        [Display(Name = "User Name")]
        [Required]
        [StringLength(50, ErrorMessage = "User Name Max Length is 50 and Minimum Length is 4 characters.",
            MinimumLength = 4)]
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool RecordStatus { get; set; }

        public string RecordStatusValue { get; set; }

        [Display(Name = "Password")]
        //[Required]
        //[StringLength(20, ErrorMessage = "Password Max Length is 20 and Minimum Length is 4 characters.", MinimumLength = 4)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        //[Required]
        //[StringLength(20, ErrorMessage = "Confirm Password Max Length is 20 and Minimum Length is 4 characters.", MinimumLength = 4)]
        public string ConfirmPassword { get; set; }
    }
}