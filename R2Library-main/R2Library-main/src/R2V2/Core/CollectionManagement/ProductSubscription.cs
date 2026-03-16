#region

using System;
using System.Text;
using R2V2.Core.Institution;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    [Serializable]
    public class ProductSubscription : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual Product Product { get; set; }
        public virtual int InstitutionId { get; set; }

        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        public virtual int ProductSubscriptionStatusId { get; set; }

        public virtual ProductSubscriptionStatus ProductSubscriptionStatus =>
            (ProductSubscriptionStatus)ProductSubscriptionStatusId;

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("ProductSubscription = [");
            sb.AppendFormat("InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", ProductSubscriptionStatusId: {0}", ProductSubscriptionStatusId);
            sb.AppendFormat(", Product: {0}", Product.ToDebugString());
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}