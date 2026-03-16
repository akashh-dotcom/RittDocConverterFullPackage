#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Web.Areas.Admin.Models.User;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.SubscriptionManagement
{
    public class SubscriptionUser : AdminBaseModel
    {
        public SubscriptionUser()
        {
        }

        public SubscriptionUser(IUser user)
        {
            UserId = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            UserName = user.UserName;
            RecordStatus = user.RecordStatus;
            ExpirationDate = user.ExpirationDate;
            LastPasswordChange = user.LastPasswordChange;
            //UserSubscriptions = user.Subscriptions;
            PopulateUserStatusSelectList();
            UserSubscriptions = new List<AdminUserSubscription>();
            foreach (var userSubscription in user.Subscriptions)
            {
                var item = new AdminUserSubscription
                {
                    Id = userSubscription.Id,
                    UserId = userSubscription.UserId,
                    Name = userSubscription.Subscription.Name,
                    ActivationDate = userSubscription.ActivationDate,
                    ExpirationDate = userSubscription.ExpirationDate,
                    TrialExpirationDate = userSubscription.TrialExpirationDate
                };
                UserSubscriptions.Add(item);
            }
            //UserSubscriptions
        }

        public int UserId { get; set; }

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
        [PasswordValidation]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [PasswordCompare("NewPassword", "ConfirmPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = @"Status")] public bool RecordStatus { get; set; }

        [Display(Name = @"Status")] public SelectList StatusList { get; set; }

        [Display(Name = @"Expires:")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [DateTenYears("User.ExpirationDate")]
        public DateTime? ExpirationDate { get; set; }

        public DateTime? LastPasswordChange { get; set; }

        [Display(Name = @"Subscriptions")] public IList<AdminUserSubscription> UserSubscriptions { get; set; }

        private void PopulateUserStatusSelectList()
        {
            var userStatuses = new List<UserStatus>
            {
                new UserStatus { Description = "Active", Value = true },
                new UserStatus { Description = "InActive", Value = false }
            };

            StatusList = new SelectList(userStatuses, "Value", "Description");
        }
    }

    public class AdminUserSubscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }

        [Display(Name = @"Trial Expiration Date:")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? TrialExpirationDate { get; set; }

        [Display(Name = @"Activation Date:")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ActivationDate { get; set; }

        [Display(Name = @"Expiration Date:")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ExpirationDate { get; set; }
    }
}