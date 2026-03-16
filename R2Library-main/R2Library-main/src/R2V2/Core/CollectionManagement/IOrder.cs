#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface IOrder
    {
        int OrderId { get; set; }
        int OrderHistoryId { get; set; }
        int InstitutionId { get; set; }

        string PurchaseOrderNumber { get; set; }
        string PurchaseOrderComment { get; set; }

        DateTime? PurchaseDate { get; set; }
        DateTime? SavedDate { get; set; }

        BillingMethodEnum BillingMethod { get; set; }
        ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }

        IEnumerable<IOrderItem> Items { get; }

        int LicenseTotal { get; }

        decimal SubTotal { get; }
        decimal Discount { get; }
        decimal DiscountTotal { get; }
        decimal OrderTotal { get; }

        ICollectionManagementQuery CollectionManagementQuery { get; set; }

        decimal PromotionDiscount { get; }
        string PromotionCode { get; }
        string PromotionName { get; }
    }
}