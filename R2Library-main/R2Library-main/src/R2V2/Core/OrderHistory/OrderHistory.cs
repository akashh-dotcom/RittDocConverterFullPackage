#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.OrderHistory
{
    public class OrderHistory
    {
        private decimal _discountTotal;

        private decimal _orderTotal;


        private decimal _subTotal;

        public OrderHistory(DbOrderHistory orderHistory)
        {
            SetOrderHistory(orderHistory);
        }

        public int OrderHistoryId { get; set; }
        public string OrderNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PurchaseOrderComment { get; set; }
        public decimal Discount { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionDescription { get; set; }
        public DateTime PurchaseDate { get; set; }
        public BillingMethodEnum BillingMethod { get; set; }
        public ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }

        public string CartName { get; set; }

        //public string ResellerName { get; set; }
        public Reseller Reseller { get; set; }
        public int NumberofTitles { get; set; }
        public int NumberofLicenses { get; set; }
        public int DiscountType { get; set; }

        public List<OrderHistoryItem> OrderHistoryResources { get; set; }

        public List<OrderHistoryItem> OrderHistoryProducts { get; set; }

        public int InstitutionId { get; set; }

        public decimal OrderTotal
        {
            get
            {
                if (_orderTotal == 0)
                {
                    foreach (var orderItem in OrderHistoryResources)
                    {
                        _orderTotal += orderItem.TotalDiscountPrice();
                    }

                    foreach (var orderItem in OrderHistoryProducts)
                    {
                        _orderTotal += orderItem.TotalDiscountPrice();
                    }
                }

                return _orderTotal;
            }
        }

        public decimal SubTotal
        {
            get
            {
                if (_subTotal == 0)
                {
                    foreach (var orderItem in OrderHistoryResources)
                    {
                        _subTotal += orderItem.TotalListPrice();
                    }

                    foreach (var orderItem in OrderHistoryProducts)
                    {
                        _subTotal += orderItem.TotalListPrice();
                    }
                }

                return _subTotal;
            }
        }

        public decimal DiscountTotal
        {
            get
            {
                if (_discountTotal == 0)
                {
                    foreach (var orderItem in OrderHistoryResources)
                    {
                        _discountTotal += orderItem.TotalDiscountPrice() - orderItem.TotalListPrice();
                    }

                    foreach (var orderItem in OrderHistoryProducts)
                    {
                        _discountTotal += orderItem.TotalDiscountPrice() - orderItem.TotalListPrice();
                    }
                }

                return _discountTotal;
            }
        }


        public bool HasForthcomingTitles
        {
            get { return OrderHistoryResources.Any(x => x.Resource.StatusId == (int)ResourceStatus.Forthcoming); }
        }

        public bool IsPromotionApplied => !string.IsNullOrWhiteSpace(PromotionCode);

        public void SetOrderHistoryItems(ICollection<DbOrderHistoryItem> orderHistoryItems, List<IResource> resources,
            List<IProduct> products, IList<Recommendation> recommendations)
        {
            if (orderHistoryItems != null && orderHistoryItems.Any())
            {
                OrderHistoryResources = new List<OrderHistoryItem>();
                OrderHistoryProducts = new List<OrderHistoryItem>();
                foreach (var orderHistoryItem in orderHistoryItems)
                {
                    if (orderHistoryItem.ResourceId > 0)
                    {
                        var recommendationsFound =
                            recommendations.Where(x => x.ResourceId == orderHistoryItem.ResourceId);
                        var resource = resources.FirstOrDefault(x => x.Id == orderHistoryItem.ResourceId);
                        var item = new OrderHistoryItem(resource, orderHistoryItem, recommendationsFound);
                        if (Reseller != null)
                        {
                            item.DiscountPrice = item.ListPrice;
                        }

                        OrderHistoryResources.Add(item);
                    }
                    else
                    {
                        var item = new OrderHistoryItem(
                            products.FirstOrDefault(x => x.Id == orderHistoryItem.ProductId), orderHistoryItem);
                        if (Reseller != null)
                        {
                            item.DiscountPrice = item.ListPrice;
                        }

                        OrderHistoryProducts.Add(item);
                    }
                }
            }
        }

        public void SetOrderHistoryItems(ICollection<DbOrderHistoryItem> orderHistoryItems, List<IResource> resources,
            List<IProduct> products)
        {
            if (orderHistoryItems != null && orderHistoryItems.Any())
            {
                OrderHistoryResources = new List<OrderHistoryItem>();
                OrderHistoryProducts = new List<OrderHistoryItem>();
                foreach (var orderHistoryItem in orderHistoryItems)
                {
                    if (orderHistoryItem.ResourceId > 0)
                    {
                        var resource = resources.FirstOrDefault(x => x.Id == orderHistoryItem.ResourceId);
                        var item = new OrderHistoryItem(resource, orderHistoryItem);
                        if (Reseller != null)
                        {
                            item.DiscountPrice = item.ListPrice;
                        }

                        OrderHistoryResources.Add(item);
                    }
                    else
                    {
                        var item = new OrderHistoryItem(
                            products.FirstOrDefault(x => x.Id == orderHistoryItem.ProductId), orderHistoryItem);
                        if (Reseller != null)
                        {
                            item.DiscountPrice = item.ListPrice;
                        }

                        OrderHistoryProducts.Add(item);
                    }
                }
            }
        }

        private void SetOrderHistory(DbOrderHistory orderHistory)
        {
            if (orderHistory != null)
            {
                OrderHistoryId = orderHistory.Id;
                OrderNumber = orderHistory.OrderNumber;
                PurchaseOrderNumber = orderHistory.PurchaseOrderNumber;
                PurchaseOrderComment = orderHistory.PurchaseOrderComment;
                Discount = orderHistory.Discount;
                PromotionCode = orderHistory.PromotionCode;
                PromotionDescription = orderHistory.PromotionDescription;
                PurchaseDate = orderHistory.PurchaseDate;
                BillingMethod = orderHistory.BillingMethod;
                ForthcomingTitlesInvoicingMethod = orderHistory.ForthcomingTitlesInvoicingMethod;
                CartName = orderHistory.CartName;
                Reseller = orderHistory.Reseller;
                InstitutionId = orderHistory.InstitutionId;
                if (orderHistory.OrderHistoryItems != null && orderHistory.OrderHistoryItems.Any())
                {
                    var z = orderHistory.OrderHistoryItems;
                    orderHistory.OrderHistoryItems = new List<DbOrderHistoryItem>();
                    foreach (var dbOrderHistoryItem in z.Where(x => x.RecordStatus))
                    {
                        orderHistory.OrderHistoryItems.Add(dbOrderHistoryItem);
                    }

                    NumberofTitles = orderHistory.OrderHistoryItems.Count(x => x.ResourceId > 0);
                    NumberofLicenses = orderHistory.OrderHistoryItems.Where(x => x.ResourceId > 0)
                        .Sum(y => y.NumberOfLicenses);

                    _discountTotal = orderHistory.OrderHistoryItems.Sum(x =>
                        x.IsBundle
                            ? x.ListPrice - x.DiscountPrice
                            : (x.ListPrice - x.DiscountPrice) * (x.NumberOfLicenses == 0 ? 1 : x.NumberOfLicenses));
                    _subTotal = orderHistory.OrderHistoryItems.Sum(x =>
                        x.IsBundle ? x.ListPrice : x.ListPrice * (x.NumberOfLicenses == 0 ? 1 : x.NumberOfLicenses));
                    _orderTotal = orderHistory.OrderHistoryItems.Sum(x =>
                        x.IsBundle
                            ? x.DiscountPrice
                            : x.DiscountPrice * (x.NumberOfLicenses == 0 ? 1 : x.NumberOfLicenses));
                }


                if (orderHistory.Reseller != null)
                {
                    _orderTotal = _subTotal;
                    _discountTotal = 0;
                    Discount = 0;
                }

                DiscountType = orderHistory.DiscountTypeId;
            }
        }
    }
}