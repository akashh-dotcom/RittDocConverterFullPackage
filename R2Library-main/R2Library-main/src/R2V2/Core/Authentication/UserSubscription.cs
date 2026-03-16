#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    public enum SubscriptionType
    {
        Monthly = 1,
        Annual = 2
    }

    [Serializable]
    public class UserSubscription : AuditableEntity, ISoftDeletable
    {
        public virtual int SubscriptionId { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime? ExpirationDate { get; set; }
        public virtual DateTime? TrialExpirationDate { get; set; }
        public virtual DateTime? ActivationDate { get; set; }
        public virtual SubscriptionType Type { get; set; }
        public virtual Subscription Subscription { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("UserSubscription = [")
                .Append($"Id: {Id}")
                .Append($", SubscriptionId: {SubscriptionId}")
                .Append($", UserId: {UserId}")
                .Append($", ExpirationDate: {ExpirationDate}")
                .Append($", TrialExpirationDate: {TrialExpirationDate}")
                .Append($", ActivationDate: {ActivationDate}")
                .Append($", RecordStatus: {RecordStatus}")
                .Append($", Type: {Type}")
                .Append("]");
            return sb.ToString();
        }

        public virtual bool CanView()
        {
            if (ActivationDate.HasValue && ActivationDate.Value.Date <= DateTime.Now.Date)
            {
                if (!ExpirationDate.HasValue || ExpirationDate.Value.Date <= DateTime.Now.Date)
                {
                    return true;
                }
            }
            else if (TrialExpirationDate.HasValue && TrialExpirationDate.Value.Date >= DateTime.Now.Date)
            {
                if (!ExpirationDate.HasValue || ExpirationDate.Value.Date <= DateTime.Now.Date)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool CanPurchase()
        {
            if (CanView())
            {
                return false;
            }

            if (TrialExpirationDate.HasValue && TrialExpirationDate.Value < DateTime.Now && !ActivationDate.HasValue)
            {
                return false;
            }

            return true;
        }

        public virtual bool CanTrial()
        {
            if (CanView())
            {
                return false;
            }

            //TODO: Should This just be TrialExpirationDate?
            return !ExpirationDate.HasValue && !TrialExpirationDate.HasValue;
        }
    }

    [Serializable]
    public class Subscription : AuditableEntity, ISoftDeletable
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string CmsName { get; set; }
        public virtual decimal MonthlyPrice { get; set; }
        public virtual decimal AnnualPrice { get; set; }
        public virtual bool AllowSubscription { get; set; }
        public virtual IList<SubscriptionResource> SubResources { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Subscription = [")
                .Append($"Id: {Id}")
                .Append($", Name: {Name}")
                .Append($", Description: {Description}")
                .Append($", CmsName: {CmsName}")
                .Append($", MonthlyPrice: {MonthlyPrice}")
                .Append($", AnnualPrice: {AnnualPrice}")
                .Append($", AllowSubscription: {AllowSubscription}")
                .Append($", RecordStatus: {RecordStatus}");

            foreach (var subscriptionResource in SubResources)
            {
                sb.Append(subscriptionResource.ToDebugString());
                sb.Append(", ");
            }

            sb.Append("]");
            return sb.ToString();
        }
    }

    [Serializable]
    public class SubscriptionResource : AuditableEntity, ISoftDeletable
    {
        public virtual int SubscriptionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("SubscriptionResource = [")
                .Append($"Id: {Id}")
                .Append($", SubscriptionId: {SubscriptionId}")
                .Append($", ResourceId: {ResourceId}")
                .Append($", RecordStatus: {RecordStatus}")
                .Append("]")
                .ToString();
        }
    }

    [Serializable]
    public class SubscriptionOrderHistory : AuditableEntity, ISoftDeletable
    {
        public virtual int SubscriptionId { get; set; }
        public virtual int UserId { get; set; }
        public virtual int SubscriptionUserId { get; set; }
        public virtual string OrderNumber { get; set; }
        public virtual string PurchaseOrderNumber { get; set; }
        public virtual string PurchaseOrderComment { get; set; }
        public virtual string MembershipNumber { get; set; }
        public virtual SubscriptionType Type { get; set; }
        public virtual decimal Price { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("SubscriptionOrderHistory = [")
                .Append($"Id: {Id}")
                .Append($", SubscriptionId: {SubscriptionId}")
                .Append($", UserId: {UserId}")
                .Append($", SubscriptionUserId: {SubscriptionUserId}")
                .Append($", OrderNumber: {OrderNumber}")
                .Append($", PurchaseOrderNumber: {PurchaseOrderNumber}")
                .Append($", PurchaseOrderComment: {PurchaseOrderComment}")
                .Append($", MembershipNumber: {MembershipNumber}")
                .Append($", Type: {Type}")
                .Append($", Price: {Price}")
                .Append($", RecordStatus: {RecordStatus}")
                .Append("]")
                .ToString();
        }
    }
}