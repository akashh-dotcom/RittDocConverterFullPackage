#region

using System;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface IOrderItem
    {
        int Id { get; set; }
        int ItemId { get; set; }

        int NumberOfLicenses { get; set; }

        decimal ListPrice { get; }
        decimal DiscountPrice { get; set; }

        decimal TotalListPrice { get; }
        decimal TotalDiscountPrice { get; }

        DateTime? PurchaseDate { get; set; }
        bool Include { get; }
    }
}