#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.SubscriptionManagement
{
    public class AdminUserSubscriptionModel : AdminBaseModel
    {
        public AdminUserSubscriptionModel()
        {
        }

        public AdminUserSubscriptionModel(UserSubscription userSubscription)
        {
            Id = userSubscription.Id;
            UserId = userSubscription.UserId;
            Name = userSubscription.Subscription.Name;
            Description = userSubscription.Subscription.Description;
            TrialExpirationDate = userSubscription.TrialExpirationDate;
            ActivationDate = userSubscription.ActivationDate;
            ExpirationDate = userSubscription.ExpirationDate;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? TrialExpirationDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ActivationDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ExpirationDate { get; set; }
    }
}