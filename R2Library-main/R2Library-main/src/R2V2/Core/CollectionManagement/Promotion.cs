#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class Promotion : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int Discount { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual string OrderSource { get; set; }
        public virtual bool EnableCartAlert { get; set; }
        public virtual int MaximumUses { get; set; }
        public virtual IList<PromotionProduct> PromotionProducts { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Promotion = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Code: {0}", Code);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", OrderSource: {0}", OrderSource);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", Description: {0}", Description);
            sb.AppendFormat(", EnableCartAlert: {0}", EnableCartAlert);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}