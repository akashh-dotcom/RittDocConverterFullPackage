#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class PromotionProduct : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int PromotionId { get; set; }
        public virtual int ProductId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Promotion = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", PromotionId: {0}", PromotionId);
            sb.AppendFormat(", ProductId: {0}", ProductId);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}