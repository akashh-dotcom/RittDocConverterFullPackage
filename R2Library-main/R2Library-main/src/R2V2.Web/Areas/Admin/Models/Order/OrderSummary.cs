#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Order
{
    public class OrderSummary : AdminBaseModel, IOrder
    {
        private readonly IList<IOrderItem> _items = new List<IOrderItem>();

        private decimal _discountTotal;

        private int _licenseTotal;

        private decimal _orderTotal;

        private decimal _subTotal;

        public OrderSummary()
        {
        }

        public OrderSummary(Core.CollectionManagement.Cart cart, IAdminInstitution adminInstitution,
            List<IResource> resources, List<IProduct> products)
            : base(adminInstitution)
        {
            OrderId = cart.Id;

            PurchaseOrderNumber = cart.PurchaseOrderNumber;
            PurchaseOrderComment = cart.PurchaseOrderComment;
            PurchaseDate = cart.PurchaseDate;

            BillingMethod = cart.BillingMethod;
            ForthcomingTitlesInvoicingMethod = cart.ForthcomingTitlesInvoicingMethod;
            SavedDate = cart.ConvertDate;
            foreach (var cartItem in cart.CartItems.Where(x => x.Include))
            {
                var item = cartItem;
                if (cartItem.ResourceId != null)
                {
                    var resource = resources.FirstOrDefault(x => x.Id == item.ResourceId);
                    _items.Add(new OrderItemSummary(cartItem, resource));
                }
                else if (cartItem.ProductId != null)
                {
                    var product = products.FirstOrDefault(x => x.Id == cartItem.ProductId);
                    if (product != null)
                    {
                        _items.Add(new ProductOrderItem(cartItem, product));
                    }
                }
            }

            Discount = cart.Discount;
            PromotionDiscount = cart.PromotionDiscount;
            PromotionCode = cart.PromotionCode;
        }

        public int ItemCount
        {
            get { return _items.Count(x => x is ResourceOrderItem); }
        }

        public int OrderId { get; set; }
        public int OrderHistoryId { get; set; }

        public string PurchaseOrderNumber { get; set; }
        public string PurchaseOrderComment { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? SavedDate { get; set; }

        public BillingMethodEnum BillingMethod { get; set; }
        public ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }

        public IEnumerable<IOrderItem> Items => _items;

        public int LicenseTotal
        {
            get
            {
                if (_licenseTotal <= 0)
                {
                    foreach (var orderItem in Items)
                    {
                        _licenseTotal += orderItem.NumberOfLicenses;
                    }
                }

                return _licenseTotal;
            }
        }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SubTotal
        {
            get
            {
                if (_subTotal == 0)
                {
                    foreach (var orderItem in Items)
                    {
                        _subTotal += orderItem.TotalListPrice;
                    }
                }

                return _subTotal;
            }
        }

        [DisplayFormat(DataFormatString = "{0}%")]
        public decimal Discount { get; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountTotal
        {
            get
            {
                if (_discountTotal == 0)
                {
                    foreach (var item in Items)
                    {
                        _discountTotal += item.TotalDiscountPrice - item.TotalListPrice;
                    }
                }

                return _discountTotal;
            }
        }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal OrderTotal
        {
            get
            {
                if (_orderTotal == 0)
                {
                    foreach (var item in Items)
                    {
                        _orderTotal += item.TotalDiscountPrice;
                    }
                }

                return _orderTotal;
            }
        }

        public ICollectionManagementQuery CollectionManagementQuery { get; set; }

        public decimal PromotionDiscount { get; }
        public string PromotionCode { get; }
        public string PromotionName { get; set; }
    }
}