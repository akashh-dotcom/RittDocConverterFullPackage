#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.OrderHistory
{
    public class OrderHistoryItem
    {
        public OrderHistoryItem(IResource resource, DbOrderHistoryItem orderHistoryItem,
            IEnumerable<Recommendation> recommendationsFound)
        {
            IsBundle = orderHistoryItem.IsBundle;
            Resource = resource;
            SetOrderHistoryDetails(orderHistoryItem);
            Recommendations = recommendationsFound.Select(x => x);
        }

        public OrderHistoryItem(IResource resource, DbOrderHistoryItem orderHistoryItem)
        {
            IsBundle = orderHistoryItem.IsBundle;
            Resource = resource;
            SetOrderHistoryDetails(orderHistoryItem);
        }

        public OrderHistoryItem(IProduct product, DbOrderHistoryItem orderHistoryItem)
        {
            Product = product;
            SetOrderHistoryDetails(orderHistoryItem);
        }

        public int Id { get; set; }
        public IResource Resource { get; set; }
        public IProduct Product { get; set; }
        public int NumberOfLicenses { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public string SpecialText { get; set; }
        public string SpecialIconName { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
        public bool IsBundle { get; set; }

        public decimal TotalDiscountPrice()
        {
            return IsBundle ? DiscountPrice : NumberOfLicenses * DiscountPrice;
        }

        public decimal TotalListPrice()
        {
            return IsBundle ? ListPrice : NumberOfLicenses * ListPrice;
        }

        private void SetOrderHistoryDetails(DbOrderHistoryItem orderHistoryItem)
        {
            Id = orderHistoryItem.Id;
            NumberOfLicenses = orderHistoryItem.NumberOfLicenses;
            ListPrice = orderHistoryItem.ListPrice;
            DiscountPrice = orderHistoryItem.DiscountPrice;
            SpecialText = orderHistoryItem.SpecialText;
            SpecialIconName = orderHistoryItem.SpecialIconName;
        }
    }
}