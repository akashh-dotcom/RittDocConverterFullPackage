#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Cryptography;

#endregion

namespace R2V2.Web.Models.Contact
{
    public class ContactUs : BaseModel
    {
        public ContactUs()
        {
            SetTokens();
        }

        public ContactUs(AuthenticatedInstitution authenticatedInstitution, IUser user, string title,
            bool isAskYourLibrarian = false)
        {
            if (authenticatedInstitution != null)
            {
                InstitutionName = authenticatedInstitution.Name;
                AccountNumber = authenticatedInstitution.AccountNumber;
                if (authenticatedInstitution.Address != null)
                {
                    Address1 = authenticatedInstitution.Address.Address1;
                    Address2 = authenticatedInstitution.Address.Address2;
                    City = authenticatedInstitution.Address.City;
                    State = authenticatedInstitution.Address.State;
                    Zip = authenticatedInstitution.Address.Zip;
                }


                if (user != null)
                {
                    Name = $"{user.FirstName} {user.LastName}";
                    Email = user.Email;
                }

                IsAskYourLibrarian = isAskYourLibrarian;
                PageTitle = title; //isAskYourLibrarian ? "Ask Your Librarian" : "Contact Us";
            }

            SetTokens();
        }

        public ContactUs(User user)
        {
            InstitutionName = user.Institution.Name;
            AccountNumber = user.Institution.AccountNumber;
            Address1 = user.Institution.Address.Address1;
            Address2 = user.Institution.Address.Address2;
            City = user.Institution.Address.City;
            State = user.Institution.Address.State;
            Zip = user.Institution.Address.Zip;

            Name = $"{user.FirstName} {user.LastName}";
            Email = user.Email;

            IsAskYourLibrarian = false;
            PageTitle = "Contact Administrator"; //isAskYourLibrarian ? "Ask Your Librarian" : "Contact Us";

            InstitutionId = user.InstitutionId.GetValueOrDefault();
            UserId = user.Id;

            IsAdmin = user.IsInstitutionAdmin() || user.IsRittenhouseAdmin() || user.IsSalesAssociate();

            SetTokens();
        }

        public int InstitutionId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }

        [Display(Name = @"Your Full Name: ")]
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Display(Name = @"Your Position/Title: ")]
        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; }

        [Display(Name = @"Institution Name: ")]
        [StringLength(100, ErrorMessage = "Institution Name cannot be longer than 100 characters.")]
        public string InstitutionName { get; set; }

        [Display(Name = @"Account Number: ")]
        [StringLength(12, ErrorMessage = "Account Number cannot be longer than 12 characters.")]
        public string AccountNumber { get; set; }

        [Display(Name = @"Address 1: ")]
        [StringLength(50, ErrorMessage = "Address 1 cannot be longer than 50 characters.")]
        public string Address1 { get; set; }

        [Display(Name = @"Address 2: ")]
        [StringLength(50, ErrorMessage = "Address 2 cannot be longer than 50 characters.")]
        public string Address2 { get; set; }

        [Display(Name = @"City: ")]
        [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters.")]
        public string City { get; set; }

        [Display(Name = @"State: ")]
        [StringLength(50, ErrorMessage = "State cannot be longer than 50 characters.")]
        public string State { get; set; }

        [Display(Name = @"Zip: ")]
        [StringLength(20, ErrorMessage = "Zip cannot be longer than 20 characters.")]
        public string Zip { get; set; }

        [Display(Name = @"Phone Number: ")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters.")]
        public string PhoneNumber { get; set; }

        [Display(Name = @"Email: ")]
        [Required]
        [EmailAddress(ErrorMessage = @"Invalid Email Address")]
        [StringLength(50, ErrorMessage = "Email cannot be longer than 50 characters.")]
        public string Email { get; set; }

        [RegularExpression(@"^(?!(.|\n)*<[a-z!\/?])(?!(.|\n)*&#)(.|\n)*$",
            ErrorMessage = @"This has been detected as scripting. Please change your comment to proceed.")]
        [Display(Name = @"Comment: ")]
        [AllowHtml]
        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot be longer than 1000 characters.")]
        public string Comment { get; set; }

        public bool IsEmailView { get; set; }

        public string PageTitle { get; set; }

        public bool IsAskYourLibrarian { get; set; }

        public string RecaptchaMessage { get; set; }

        [Display(Name = @"Isbn: ")]
        [StringLength(20, ErrorMessage = "Isbn cannot be longer than 20 characters.")]
        public string ResourceIsbn { get; set; }

        [Display(Name = @"Isbn 13: ")]
        [StringLength(20, ErrorMessage = "Isbn 13 cannot be longer than 20 characters.")]
        public string ResourceIsbn13 { get; set; }

        [Display(Name = @"Title: ")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string ResourceTitle { get; set; }

        // tokens to stop bots
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
            Token3 = token3; // token 3 is the timestamp encrypted, this make it very difficult to spoof. If toekns 1 & 2 pass the test, token 3 is decrypted to get the value in token 2
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