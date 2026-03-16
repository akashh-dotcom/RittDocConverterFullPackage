#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Models.Subscriptions
{
    public class SubscriptionListModel : BaseModel
    {
        public SubscriptionListModel()
        {
        }

        public SubscriptionListModel(IEnumerable<Subscription> subscriptions, IUser user)
        {
            SubscriptionDetailsList = new List<SubscriptionDetail>();
            foreach (var subscription in subscriptions)
            {
                var sub = new SubscriptionDetail { Subscription = subscription };
                if (user == null || user.Role.Code == RoleCode.SUBUSER)
                {
                    DisplayPricing = true;
                    var userSub = user?.Subscriptions.FirstOrDefault(x => x.Subscription.Id == subscription.Id);
                    if (userSub != null)
                    {
                        sub.IsSubscribed = userSub.CanView();
                        sub.CanPurchase = userSub.CanPurchase();
                        sub.CanTrial = userSub.CanTrial();
                        if (sub.IsSubscribed)
                        {
                            DateTime renewalDate;
                            if (userSub.ActivationDate.HasValue)
                            {
                                sub.IsActive = true;
                                renewalDate = userSub.ActivationDate.GetValueOrDefault();
                            }
                            else
                            {
                                renewalDate = userSub.TrialExpirationDate.GetValueOrDefault();
                            }

                            if (userSub.Type == SubscriptionType.Monthly)
                            {
                                var m = GetMonthsDiff(renewalDate, DateTime.Now);
                                m++;
                                sub.RenewalDate = renewalDate.AddMonths(m);
                            }
                            else
                            {
                                var y = GetYearsDiff(renewalDate, DateTime.Now);
                                y++;
                                sub.RenewalDate = renewalDate.AddYears(y);
                            }
                        }
                    }
                    else
                    {
                        sub.CanPurchase = true;
                        sub.CanTrial = true;
                    }
                }


                SubscriptionDetailsList.Add(sub);
            }
        }

        public List<SubscriptionDetail> SubscriptionDetailsList { get; set; }
        public SubscriptionDetail SelectedSubscriptionDetail { get; set; }

        public bool DisplayPricing { get; set; }

        private int GetMonthsDiff(DateTime fromDate, DateTime toDate)
        {
            var yearDifference = toDate.Year - fromDate.Year;
            var monthDifference = toDate.Month - fromDate.Month;

            var totalMonths = yearDifference * 12 + monthDifference;

            // Adjust if the day of the month has not been reached yet
            if (toDate.Day < fromDate.Day)
            {
                totalMonths--;
            }

            return totalMonths;
        }

        private int GetYearsDiff(DateTime fromDate, DateTime toDate)
        {
            var yearDifference = toDate.Year - fromDate.Year;

            // Adjust if the current date has not reached the month/day of the start date in this year
            if (toDate < fromDate.AddYears(yearDifference))
            {
                yearDifference--;
            }

            return yearDifference;

            return yearDifference;
        }
    }

    public class SubscriptionDetail
    {
        public Subscription Subscription { get; set; }
        public bool IsSubscribed { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public bool IsActive { get; set; }

        public bool CanPurchase { get; set; }
        public bool CanTrial { get; set; }

        public IEnumerable<IResourceSummary> Resources { get; set; }
    }

    public class SubscriptionPurchaseModel : SubscriptionDetail
    {
        public SubscriptionPurchaseModel()
        {
        }

        public SubscriptionPurchaseModel(SubscriptionDetail detail, AuthenticationInfo authInfo)
        {
            Subscription = detail.Subscription;
            IsSubscribed = detail.IsSubscribed;
            ExpirationDate = detail.ExpirationDate;
            RenewalDate = detail.RenewalDate;
            Resources = detail.Resources;
            IsAuthenticated = authInfo.IsAuthenticated;
            if (!IsSubscribed)
            {
                Type = SubscriptionType.Annual;
            }
        }

        public bool IsAuthenticated { get; }
        public SubscriptionType Type { get; set; }
    }
}