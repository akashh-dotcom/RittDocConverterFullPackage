#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface ICart : IDebugInfo
    {
        int Id { get; set; }
        IEnumerable<ICartItem> CartItems { get; }
        int InstitutionId { get; set; }
        string PurchaseOrderNumber { get; set; }
        string PurchaseOrderComment { get; set; }
        DateTime? PurchaseDate { get; set; }
        BillingMethodEnum BillingMethod { get; set; }
        ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }
        decimal Discount { get; set; }
        bool Processed { get; set; }
        void AddProduct(Product product);
    }
}