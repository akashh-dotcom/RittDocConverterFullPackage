#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Order
{
    public class ProductOrderItem : IProductOrderItem
    {
        public ProductOrderItem()
        {
        }

        public ProductOrderItem(CartItem cartItem, IProduct product)
        {
            Product = product;

            ItemId = cartItem.Id;
            ListPrice = cartItem.ListPrice;
            DiscountPrice = cartItem.DiscountPrice;
            PurchaseDate = cartItem.PurchaseDate;
            Include = cartItem.Include;
            Agree = cartItem.Agree;
        }

        public ProductOrderItem(ICartItem cartItem, IProduct product)
        {
            Product = product;

            ItemId = cartItem.Id;
            ListPrice = cartItem.ListPrice;
            DiscountPrice = cartItem.DiscountPrice;
            PurchaseDate = cartItem.PurchaseDate;
            Include = cartItem.Include;
            Agree = cartItem.Agree;
        }

        public IProduct Product { get; set; }

        public bool Agree { get; set; }

        public int Id { get; set; }
        public int ItemId { get; set; }

        public int NumberOfLicenses { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ListPrice { get; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalListPrice => ListPrice;

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalDiscountPrice => DiscountPrice;

        public DateTime? PurchaseDate { get; set; }

        public bool Include { get; set; }
    }
}