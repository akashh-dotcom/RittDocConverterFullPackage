#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.OrderRelay
{
    [Serializable]
    public class OrderMessage //: IDebugInfo
    {
        public string WebOrderNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ContactName { get; set; }
        public string ConfirmationEmailAddress { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PaymentTerm { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public char OrderSource { get; set; }
        public DateTime AutoCancelDate { get; set; }
        public decimal AdditionalDiscountPercentage { get; set; }
        public string Instruction1 { get; set; }
        public string Instruction2 { get; set; }
        public string Instruction3 { get; set; }
        public string HeaderNotes { get; set; }
        public string QuotationNumber { get; set; }
        public string PromotionCode { get; set; }

        public int SendAttemptCount { get; set; }
        public string PreviousSendErrorMessage { get; set; }
        public string ShipToNumber { get; set; }

        public OrderMessageItem[] OrderItems { get; set; }


        public string ToDebugString()
        {
            var sb = new StringBuilder("OrderMessage = [");
            sb.AppendFormat("WebOrderNumber: {0}", WebOrderNumber);
            sb.AppendFormat(", AccountNumber: {0}", AccountNumber);
            sb.AppendFormat(", ContactName: {0}", ContactName);
            sb.AppendFormat(", ConfirmationEmailAddress: {0}", ConfirmationEmailAddress);
            sb.AppendFormat(", PurchaseOrderNumber: {0}", PurchaseOrderNumber);
            sb.AppendFormat(", PaymentTerm: {0}", PaymentTerm);
            sb.AppendFormat(", OrderDate: {0:u}", OrderDate);
            sb.AppendFormat(", RequiredDate: {0:u}", RequiredDate);
            sb.AppendFormat(", AutoCancelDate: {0:u}", AutoCancelDate);
            sb.AppendFormat(", OrderSource: {0}", OrderSource);
            sb.AppendFormat(", AdditionalDiscountPercentage: {0}", AdditionalDiscountPercentage);
            sb.AppendFormat(", Instruction1: {0}", Instruction1);
            sb.AppendFormat(", Instruction2: {0}", Instruction2);
            sb.AppendFormat(", Instruction3: {0}", Instruction3);
            sb.AppendFormat(", HeaderNotes: {0}", HeaderNotes);
            sb.AppendFormat(", QuotationNumber: {0}", QuotationNumber);
            sb.AppendFormat(", PromotionCode: {0}", PromotionCode);
            sb.AppendFormat(", SendAttemptCount: {0}", SendAttemptCount);

            sb.AppendLine(", Items = [");

            foreach (var item in OrderItems)
            {
                sb.AppendFormat("\t{0}", item.ToDebugString()).AppendLine();
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}