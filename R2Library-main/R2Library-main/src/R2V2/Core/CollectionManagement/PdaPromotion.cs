#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class PdaPromotion : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int Discount { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }


        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Promotion = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", Description: {0}", Description);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }

        public virtual string PdaPromotionText()
        {
            return
                $"{Discount}% {Name} discount applies to all PDA orders placed through {EndDate.ToShortDateString()}";
        }
    }
}