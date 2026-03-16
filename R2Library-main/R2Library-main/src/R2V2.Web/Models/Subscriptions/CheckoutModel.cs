#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Models.Subscriptions
{
    public class CheckoutModel : BaseModel
    {
        public SubscriptionOrderHistory OrderHistory { get; set; }
        public Subscription Subscription { get; set; }
        public SubscriptionType Type { get; set; }
    }

    public class SubscriptionOrderHistoryModel : BaseModel
    {
        public SubscriptionOrderHistory OrderHistory { get; set; }
        public Subscription Subscription { get; set; }
        public IUser CurrentUser { get; set; }
        public bool IsWebVersion { get; set; }
    }
}