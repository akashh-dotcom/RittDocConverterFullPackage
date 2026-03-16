#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.OrderHistory;
using R2V2.Web.Areas.Admin.Models.Checkout;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Recommendations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.OrderHistory
{
    public class WebOrderHistory
    {
        /// <summary>
        ///     Sets the Base Order History and also the Items for display
        /// </summary>
        public WebOrderHistory(Core.OrderHistory.OrderHistory orderHistory, IAdminInstitution institution)
        {
            Institution = institution;
            if (orderHistory != null)
            {
                SetOrderHistory(orderHistory);

                OrderHistoryResources = new List<WebOrderHistoryItem>();
                OrderHistoryProducts = new List<WebOrderHistoryItem>();

                foreach (var orderHistoryResource in orderHistory.OrderHistoryResources)
                {
                    OrderHistoryResources.Add(new WebOrderHistoryItem(orderHistoryResource));
                }

                foreach (var orderHistoryProduct in orderHistory.OrderHistoryProducts)
                {
                    OrderHistoryProducts.Add(new WebOrderHistoryItem(orderHistoryProduct));
                }
            }
        }

        public int OrderHistoryId { get; set; }
        public string OrderNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PurchaseOrderComment { get; set; }
        public decimal Discount { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionDescription { get; set; }

        public DateTime PurchaseDate { get; set; }

        //public BillingMethodEnum BillingMethod { get; set; }
        public string BillingMethod { get; set; }
        public string ForthcomingTitlesInvoicingMethod { get; set; }
        public string CartName { get; set; }
        public string ResellerName { get; set; }

        public int NumberofTitles { get; set; }
        public int NumberofLicenses { get; set; }


        public List<WebOrderHistoryItem> OrderHistoryResources { get; set; }

        public List<WebOrderHistoryItem> OrderHistoryProducts { get; set; }
        public IAdminInstitution Institution { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal OrderTotal { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SubTotal { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountTotal { get; set; }

        public int DiscountType { get; set; }

        public bool HasForthcomingTitles { get; set; }

        public bool IsPromotionApplied { get; set; }

        public string GetDisplayDiscount(bool doNotFloat = false)
        {
            if (OrderHistoryResources.Any(x => !string.IsNullOrWhiteSpace(x.SpecialText)))
            {
                if (doNotFloat)
                {
                    return "Variable (specials applied)";
                }

                return "<div style=\"float:left\">Variable (specials applied)</div>";
            }

            return $"{Discount}%";
        }

        private void SetOrderHistory(Core.OrderHistory.OrderHistory orderHistory)
        {
            if (orderHistory != null)
            {
                OrderHistoryId = orderHistory.OrderHistoryId;
                OrderNumber = orderHistory.OrderNumber;
                PurchaseOrderNumber = orderHistory.PurchaseOrderNumber;
                PurchaseOrderComment = orderHistory.PurchaseOrderComment;
                Discount = orderHistory.Discount;
                PromotionCode = orderHistory.PromotionCode;
                PromotionDescription = orderHistory.PromotionDescription;
                PurchaseDate = orderHistory.PurchaseDate;
                BillingMethod = orderHistory.Reseller == null
                    ? orderHistory.BillingMethod.ToBillingMethod().Description
                    : orderHistory.Reseller.DisplayName;

                ForthcomingTitlesInvoicingMethod =
                    orderHistory.ForthcomingTitlesInvoicingMethod.ToForthcomingTitlesInvoicingMethod().Description;
                CartName = orderHistory.CartName;

                NumberofTitles = orderHistory.NumberofTitles;
                NumberofLicenses = orderHistory.NumberofLicenses;

                DiscountTotal = orderHistory.DiscountTotal;
                OrderTotal = orderHistory.OrderTotal;
                SubTotal = orderHistory.SubTotal;
                DiscountType = orderHistory.DiscountType;
            }
        }
    }

    public class WebOrderHistoryItem
    {
        public WebOrderHistoryItem(OrderHistoryItem orderHistoryItem)
        {
            IsBundle = orderHistoryItem.IsBundle;
            Resource = new Resource.Resource(orderHistoryItem.Resource);
            Product = orderHistoryItem.Product;

            NumberOfLicenses = orderHistoryItem.NumberOfLicenses;
            ListPrice = orderHistoryItem.ListPrice;
            DiscountPrice = orderHistoryItem.DiscountPrice;
            SpecialText = orderHistoryItem.SpecialText;
            SpecialIconName = orderHistoryItem.SpecialIconName;

            if (orderHistoryItem.Recommendations != null && orderHistoryItem.Recommendations.Any())
            {
                Recommendations = orderHistoryItem.Recommendations.Select(x => new Recommended(x));
            }
        }

        public Resource.Resource Resource { get; set; }
        public IProduct Product { get; set; }
        public int NumberOfLicenses { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public string SpecialText { get; set; }
        public string SpecialIconName { get; set; }
        public IEnumerable<Recommended> Recommendations { get; set; }
        public bool IsBundle { get; set; }

        public decimal TotalDiscountPrice()
        {
            return IsBundle ? DiscountPrice : NumberOfLicenses * DiscountPrice;
        }

        public decimal TotalListPrice()
        {
            return IsBundle ? ListPrice : NumberOfLicenses * ListPrice;
        }
    }
}