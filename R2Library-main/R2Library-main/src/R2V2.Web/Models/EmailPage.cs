#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Email;

#endregion

namespace R2V2.Web.Models
{
    [Serializable]
    public class EmailPage : IEmailData
    {
        public EmailPage()
        {
        }

        public EmailPage(string from)
        {
            From = from;
        }

        public string WarningMessage { get; set; }
        public string ErrorMessage { get; set; }

        [Display(Name = "Your Email Address:")]
        [StringLength(100, ErrorMessage = @"Email address can't exceed 100 characters.")]
        public string From { get; set; }

        [Display(Name = "To:")]
        [StringLength(250, ErrorMessage = @"To addresses can't exceed 250 characters.")]
        public string To { get; set; }

        [Display(Name = "CC:")]
        [StringLength(250, ErrorMessage = @"CC addresses can't exceed 250 characters.")]
        public string Cc { get; set; }

        public string Bcc { get; set; }

        [Display(Name = "Subject:")]
        [StringLength(250, ErrorMessage = @"Subject can't exceed 250 characters.")]
        public string Subject { get; set; }

        [RegularExpression(@"^(?!(.|\n)*<[a-z!\/?])(?!(.|\n)*&#)(.|\n)*$",
            ErrorMessage = "This has been detected as scripting. Please change your comment to proceed.")]
        [StringLength(2000, ErrorMessage = @"Comments can't exceed 2000 characters.")]
        [Display(Name = "Comments:")]
        public string Comments { get; set; }
    }
}