#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Infrastructure.Cryptography;

#endregion

namespace R2V2.Web.Models.Subscriptions
{
    public class SubscriptionUserModel : BaseModel
    {
        public SubscriptionUserModel()
        {
            SetTokens();
        }

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

        [Display(Name = "Password")]
        [Required]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }

        public string RecaptchaMessage { get; set; }
        public string Token1 { get; set; }
        public string Token2 { get; set; }
        public string Token3 { get; set; }

        [Display(Name = @"Please leave this field blank: ")]
        public string Token4 { get; set; }

        public string SecurityMessage { get; set; }

        public void SetTokens()
        {
            var rijndaelCipher = new RijndaelCipher();
            var now = DateTime.Now;
            var random = new Random();

            var textToEncrypt = $"{random.Next(1, 99999999):00000000}~~~{now:o}";
            var token3 = rijndaelCipher.Encrypt(textToEncrypt);

            Token1 = Guid.NewGuid().ToString(); // token 1 is a guid that is stored in the session and must match
            Token2 = $"{now.Ticks}"; // token 2 is the timestamp in ticks. this value should only be valid for 1 hour
            Token3 = token3; // token 3 is the timestamp encrypted, this make it very difficult to spoof. If tokens 1 & 2 pass the test, token 3 is decrypted to get the value in token 2
            Token4 = ""; // Token4 should always be empty, if it is set we know that it is a bot
        }

        public DateTime GetToken3DateTime()
        {
            if (string.IsNullOrWhiteSpace(Token3) || Token3.Length < 20)
            {
                return DateTime.MinValue;
            }

            var rijndaelCipher = new RijndaelCipher();
            var decryptedText = rijndaelCipher.Decrypt(Token3);
            return DateTime.Parse(decryptedText.Substring(11));
        }
    }
}