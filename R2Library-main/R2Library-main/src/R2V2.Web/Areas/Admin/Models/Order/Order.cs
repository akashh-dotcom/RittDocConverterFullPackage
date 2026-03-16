#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using R2V2.Core;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Order
{
    public class Order : AdminBaseModel, IOrder, IDebugInfo
    {
        private readonly IList<IOrderItem> _items = new List<IOrderItem>();

        private decimal _discountTotal;

        private int _licenseTotal;

        private decimal _orderTotal;

        private decimal _subTotal;

        public Order()
        {
        }

        public Order(IAdminInstitution institution) : base(institution)
        {
        }

        public bool Editable { get; set; }

        [Display(Name = @"Cart Name:")] public string CartName { get; set; }

        //public bool IsSavedCart { get; set; }

        public CartTypeEnum CartType { get; set; }

        public bool IsCartRename { get; set; }

        public string SaveCopyCartText { get; set; }

        public bool HasForthcomingTitles
        {
            get
            {
                return Items.OfType<ResourceOrderItem>()
                    .Any(x => x.Resource.StatusId == (int)ResourceStatus.Forthcoming);
            }
        }

        public IEnumerable<IOrderItem> ProcessedItems
        {
            get
            {
                foreach (var item in _items)
                {
                    var resourceOrderItem = item as ResourceOrderItem;

                    if (resourceOrderItem != null)
                    {
                        if (resourceOrderItem.NumberOfLicenses > 0 && resourceOrderItem.PurchaseDate != null)
                        {
                            yield return resourceOrderItem;
                        }
                    }

                    var productItem = item as ProductOrderItem;
                    if (productItem != null && productItem.PurchaseDate != null)
                    {
                        yield return productItem;
                    }
                }
            }
        }

        public IEnumerable<IOrderItem> PurchasableItems
        {
            get
            {
                foreach (var item in _items)
                {
                    var resourceOrderItem = item as ResourceOrderItem;
                    if (resourceOrderItem != null && resourceOrderItem.IsAvailableForSale &&
                        resourceOrderItem.Resource.IsForSale && !resourceOrderItem.Resource.NotSaleableDate.HasValue)
                    {
                        yield return resourceOrderItem;
                    }

                    var productItem = item as ProductOrderItem;
                    if (productItem != null)
                    {
                        yield return productItem;
                    }
                }
            }
        }

        public IEnumerable<ResourceOrderItem> ArchivedItems
        {
            get
            {
                return _items.OfType<ResourceOrderItem>().Where(x =>
                    x.Resource.StatusId == (int)ResourceStatus.Archived ||
                    x.Resource.StatusId == (int)ResourceStatus.Inactive);
            }
        }

        public IEnumerable<ResourceOrderItem> NotForSaleItems
        {
            get
            {
                return
                    _items.OfType<ResourceOrderItem>()
                        .Where(x =>
                            x.Resource != null && !x.Resource.IsForSale &&
                            x.Resource.StatusId != (int)ResourceStatus.Archived &&
                            x.Resource.StatusId != (int)ResourceStatus.Inactive
                        );
            }
        }

        public int ItemCount
        {
            get
            {
                var orderItems = IsOrderHistory() ? ProcessedItems : PurchasableItems;
                return orderItems.OfType<ResourceOrderItem>().Count();
            }
        }

        public bool ContainsPurchasableFreeResource
        {
            get
            {
                return
                    PurchasableItems.OfType<ResourceOrderItem>()
                        .Any(resourceOrderItem =>
                            resourceOrderItem != null && resourceOrderItem.Resource.IsFreeResource);
            }
        }

        public bool AllowCheckout
        {
            get
            {
                foreach (var orderItem in PurchasableItems)
                {
                    var resourceOrderItem = orderItem as ResourceOrderItem;
                    if (resourceOrderItem != null)
                    {
                        return true;
                    }

                    var productOrderItem = orderItem as ProductOrderItem;
                    if (productOrderItem != null && productOrderItem.Include)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsPromotionAvailable { get; set; }
        public bool IsPromotionApplied => !string.IsNullOrWhiteSpace(PromotionCode);
        public string PromotionDescription { get; set; }

        public string PromotionStatusMessage { get; set; }
        public string PromotionErrorMessage { get; set; }
        public string MergeCartsErrorMessage { get; set; }

        public string OrderNumber { get; set; }

        public bool OrderError { get; set; }

        public bool DisplayPurchaseButton => OrderTotal > 0.0m || ContainsPurchasableFreeResource;

        public string BillingMethodDescription { get; set; }

        public int[] PromotionProductIds { get; set; }

        public string SpecialIconBaseUrl { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("Order = [");
            sb.AppendFormat("OrderId: {0}", OrderId);
            sb.AppendFormat(", Editable: {0}", Editable);
            sb.AppendFormat(", PurchaseOrderNumber: {0}", PurchaseOrderNumber);
            sb.AppendFormat(", PurchaseOrderComment: {0}", PurchaseOrderComment);
            sb.AppendFormat(", PurchaseDate: {0}", PurchaseDate == null ? "null" : $"{PurchaseDate.Value:u}");
            sb.AppendFormat(", BillingMethod: {0}", BillingMethod);
            sb.AppendFormat(", ForthcomingTitlesInvoicingMethod: {0}", ForthcomingTitlesInvoicingMethod);
            sb.AppendFormat(", _items.Count: {0}", _items.Count);
            sb.AppendFormat(", PromotionCode: {0}", PromotionCode);
            sb.AppendFormat(", PromotionName: {0}", PromotionName);
            sb.AppendFormat(", PromotionDescription: {0}", PromotionDescription);
            sb.AppendFormat(", PromotionDiscount: {0}", PromotionDiscount);
            sb.AppendFormat(", PromotionStatusMessage: {0}", PromotionStatusMessage);
            sb.AppendFormat(", PromotionErrorMessage: {0}", PromotionErrorMessage);
            sb.AppendFormat(", OrderNumber: {0}", OrderNumber);
            sb.Append("]");
            return sb.ToString();
        }

        public int OrderId { get; set; }

        public int OrderHistoryId { get; set; }

        [StringLength(20, ErrorMessage = @"PO Number must be 20 characters or less")]
        public string PurchaseOrderNumber { get; set; }

        [StringLength(250, ErrorMessage = @"PO Comment must be 250 characters or less")]
        [RegularExpression(@"^(?!(.|\n)*<[a-z!\/?])(?!(.|\n)*&#)(.|\n)*$",
            ErrorMessage = @"This has been detected as scripting. Please change your comment to proceed.")]
        public string PurchaseOrderComment { get; set; }


        public DateTime? PurchaseDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime? SavedDate { get; set; }

        [Required(ErrorMessage = @"Billing Method is required")]
        public BillingMethodEnum BillingMethod { get; set; }

        [Required(ErrorMessage = @"Invoicing Method is required")]
        public ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }

        public IEnumerable<IOrderItem> Items => _items;

        public int LicenseTotal
        {
            get
            {
                if (_licenseTotal <= 0)
                {
                    var orderItems = IsOrderHistory() ? ProcessedItems : PurchasableItems;
                    _licenseTotal = orderItems.OfType<ResourceOrderItem>().Sum(x => x.GetLicenseCount());
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
                    var orderItems = IsOrderHistory() ? ProcessedItems : PurchasableItems;

                    foreach (var orderItem in orderItems.Where(x => x.Include))
                    {
                        _subTotal += orderItem.TotalListPrice;
                    }
                }

                return _subTotal;
            }
        }

        [DisplayFormat(DataFormatString = "{0}%")]
        public decimal Discount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountTotal
        {
            get
            {
                if (_discountTotal == 0)
                {
                    var orderItems = IsOrderHistory() ? ProcessedItems : PurchasableItems;
                    foreach (var orderItem in orderItems)
                    {
                        _discountTotal += orderItem.TotalDiscountPrice - orderItem.TotalListPrice;
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
                    var orderItems = IsOrderHistory() ? ProcessedItems : PurchasableItems;
                    foreach (var orderItem in orderItems.Where(x => x.Include))
                    {
                        _orderTotal += orderItem.TotalDiscountPrice;
                    }
                }

                return _orderTotal;
            }
        }

        public ICollectionManagementQuery CollectionManagementQuery { get; set; }

        [Display(Name = @"Promotion Code:")]
        [StringLength(20, ErrorMessage = @"Promotion code cannot be longer than 20 characters.")]
        public string PromotionCode { get; set; }

        public string PromotionName { get; set; }
        public decimal PromotionDiscount { get; set; }

        public void AddItem(ICartItem cartItem, IProduct product, IResource resource,
            IList<Recommendation> recommendations)
        {
            if (resource != null)
            {
                _items.Add(new ResourceOrderItem(cartItem, resource, recommendations,
                    SavedDate.HasValue && SavedDate.GetValueOrDefault() != DateTime.MinValue));
            }

            if (cartItem.Product != null)
            {
                _items.Add(new ProductOrderItem(cartItem, product));
            }
        }

        public bool IsOrderHistory()
        {
            return ProcessedItems.Any();
        }

        public string GetDisplayDiscountLabel(bool doNotFloat = false)
        {
            if (CartType == CartTypeEnum.AutomatedCart &&
                _items.OfType<ResourceOrderItem>().All(x => string.IsNullOrWhiteSpace(x.SpecialText)))
            {
                if (doNotFloat)
                {
                    return
                        $"<span style=\"font-weight:normal\">(Automated cart preferred discount applied)</span> Discount";
                }

                return
                    $"<div><span style=\"font-weight:normal\">(Automated cart preferred discount applied)</span> Discount</div>";
            }

            return "Discount";
        }

        public string GetDisplayDiscount(bool doNotFloat = false)
        {
            if (_items.OfType<ResourceOrderItem>().Any(x => !string.IsNullOrWhiteSpace(x.SpecialText)))
            {
                if (doNotFloat)
                {
                    return "Variable (specials applied)";
                }

                return "<div style=\"float:left\">Variable (specials applied)</div>";
            }

            return $"{Discount}%";
        }

        public bool IsSingleDiscount()
        {
            return _items.OfType<ResourceOrderItem>().All(x => string.IsNullOrWhiteSpace(x.SpecialText));
        }
    }
}