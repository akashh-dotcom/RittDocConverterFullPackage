#region

using System;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CheckoutRequest
    {
        private static readonly Random Random = new Random();

        public CheckoutRequest(IOrder order, IAdminInstitution institution)
        {
            CartId = order.OrderId;
            PurchaseOrderNumber = order.PurchaseOrderNumber;
            PurchaseOrderComment = order.PurchaseOrderComment;
            BillingMethod = order.BillingMethod;
            Discount = institution.Discount;
            ForthcomingTitlesInvoicingMethod = order.ForthcomingTitlesInvoicingMethod;
            PurchaseDate = DateTime.Now;
            Processed = true;
            OrderNumber = $"R2{CartId:0000#}{Random.Next(999):000}";
        }

        public int CartId { get; }
        public string PurchaseOrderNumber { get; private set; }
        public string PurchaseOrderComment { get; private set; }
        public BillingMethodEnum BillingMethod { get; private set; }
        public decimal Discount { get; private set; }
        public ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; private set; }

        public DateTime PurchaseDate { get; private set; }
        public bool Processed { get; private set; }
        public string OrderNumber { get; private set; }
    }
}