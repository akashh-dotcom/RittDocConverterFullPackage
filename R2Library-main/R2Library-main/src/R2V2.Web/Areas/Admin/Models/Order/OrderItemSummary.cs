#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Order
{
    public class OrderItemSummary : IOrderItem
    {
        public OrderItemSummary()
        {
        }

        public OrderItemSummary(CartItem cartItem, IResource resource)
        {
            ItemId = cartItem.Id;
            NumberOfLicenses = cartItem.GetLicenseCount(resource);
            ListPrice = cartItem.ListPrice;
            DiscountPrice = cartItem.DiscountPrice;
            IsBundle = cartItem.IsBundle;
        }

        public bool IsBundle { get; set; }

        public int Id { get; set; }

        public int ItemId { get; set; }
        public int NumberOfLicenses { get; set; }

        public decimal ListPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalListPrice => NumberOfLicenses * ListPrice;

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalDiscountPrice => NumberOfLicenses * DiscountPrice;

        public DateTime? PurchaseDate { get; set; }

        public bool Include { get; set; }
    }
}