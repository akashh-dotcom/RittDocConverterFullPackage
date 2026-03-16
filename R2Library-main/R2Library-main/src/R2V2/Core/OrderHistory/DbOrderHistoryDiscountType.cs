#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.OrderHistory
{
    public class DbOrderHistoryDiscountType : AuditableEntity, IDebugInfo
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("OrderHistoryDiscountType = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", Name: {0}", Name)
                .AppendFormat(", Description: {0}", Description)
                .ToString();
        }
    }
}