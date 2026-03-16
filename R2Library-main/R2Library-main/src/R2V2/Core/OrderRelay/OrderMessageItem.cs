#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.OrderRelay
{
    [Serializable]
    public class OrderMessageItem //: IDebugInfo
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string LineNotes { get; set; }
        public bool IsSpecialDiscount { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("OrderMessageItem = [");
            sb.AppendFormat("Sku: {0}", Sku);
            sb.AppendFormat(", Quantity: {0}", Quantity);
            sb.AppendFormat(", DiscountPercentage: {0}", DiscountPercentage);
            sb.AppendFormat(", LineNotes: {0}", LineNotes);
            sb.AppendFormat(", IsSpecialDiscount: {0}", IsSpecialDiscount);
            sb.Append("]");
            return sb.ToString();
        }
    }
}