#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NHibernate.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.OrderHistory
{
    public class OrderHistoryService
    {
        private readonly ILog<OrderHistoryService> _log;
        private readonly IQueryable<DbOrderHistory> _orderHistories;
        private readonly IQueryable<IProduct> _products;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public OrderHistoryService(
            ILog<OrderHistoryService> log
            , IQueryable<DbOrderHistory> orderHistories
            , IQueryable<IProduct> products
            , IUnitOfWorkProvider unitOfWorkProvider
            , IResourceService resourceService
        )
        {
            _log = log;
            _orderHistories = orderHistories;
            _products = products;
            _unitOfWorkProvider = unitOfWorkProvider;
            _resourceService = resourceService;
        }

        public OrderHistory GetOrderHistory(int institutionId, int orderHistoryId, List<IResource> resources,
            IList<Recommendation> recommendations)
        {
            _orderHistories.FetchMany(y => y.OrderHistoryItems).ToFuture();
            var dbOrderHistory =
                _orderHistories.FirstOrDefault(x => x.InstitutionId == institutionId && x.Id == orderHistoryId);

            if (dbOrderHistory != null)
            {
                var products = _products.ToList();
                var orderHistory = new OrderHistory(dbOrderHistory);
                orderHistory.SetOrderHistoryItems(dbOrderHistory.OrderHistoryItems, resources, products,
                    recommendations);
                return orderHistory;
            }

            return null;
        }

        public OrderHistory GetOrderHistorySummary(int cartId, List<IResource> resources,
            IList<Recommendation> recommendations)
        {
            _orderHistories.FetchMany(y => y.OrderHistoryItems).ToFuture();
            var dbOrderHistory = _orderHistories.FirstOrDefault(x => x.CartId == cartId);
            if (dbOrderHistory != null)
            {
                var products = _products.ToList();
                var orderHistory = new OrderHistory(dbOrderHistory);
                orderHistory.SetOrderHistoryItems(dbOrderHistory.OrderHistoryItems, resources, products,
                    recommendations);
                return orderHistory;
            }

            return null;
        }

        public int GetOrderHistoryId(int cartId, int institutionId)
        {
            var orderHistory =
                _orderHistories.FirstOrDefault(x => x.CartId == cartId && x.InstitutionId == institutionId);
            if (orderHistory != null)
            {
                return orderHistory.Id;
            }

            return 0;
        }

        public IEnumerable<OrderHistory> GetAllInstitutionOrderHistories(int institutionId)
        {
            _orderHistories.FetchMany(y => y.OrderHistoryItems).ToFuture();
            var dbOrderHistories = _orderHistories.Where(x => x.InstitutionId == institutionId).ToList();

            //var dbOrderHistories = _orderHistories.FetchMany(y=> y.OrderHistoryItems).Where(x => x.InstitutionId == institutionId).ToList();
            var orderHistories = dbOrderHistories.Select(x => new OrderHistory(x));
            return orderHistories;
        }

        public void SavePreludeMessageToOrderHistory(string orderNumber, string orderFile)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var orderHistory = _orderHistories.FirstOrDefault(x => x.OrderNumber == orderNumber);
                        if (orderHistory != null)
                        {
                            _log.InfoFormat("OrderHistory Found  Id: {0}", orderHistory.Id);

                            orderHistory.OrderFile = orderFile;

                            uow.Update(orderHistory);
                            uow.Commit();
                            transaction.Commit();
                        }
                        else
                        {
                            _log.InfoFormat("OrderHistory Not Found  orderNumber: {0}", orderNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Debug(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }
        }

        public int SaveOrderHistory(Cart cart, CheckoutRequest checkoutRequest, CachedPromotion promotion,
            IUnitOfWork uow)
        {
            _log.DebugFormat("SaveOrderHistory cart: {0}", cart.ToDebugString());

            var dbOrderHistory = ConvertCartToOrderHistory(checkoutRequest, cart, promotion);

            _log.Debug(dbOrderHistory.ToDebugString());
            uow.Save(dbOrderHistory);

            return dbOrderHistory.Id;
        }

        private DbOrderHistory ConvertCartToOrderHistory(CheckoutRequest checkoutResult, Cart cart,
            CachedPromotion promotion)
        {
            var dbOrderHistory = new DbOrderHistory
            {
                CartId = checkoutResult.CartId,
                InstitutionId = cart.InstitutionId,
                OrderNumber = checkoutResult.OrderNumber,
                PurchaseOrderNumber = checkoutResult.PurchaseOrderNumber,
                PurchaseOrderComment = checkoutResult.PurchaseOrderComment,
                PurchaseDate = cart.PurchaseDate.GetValueOrDefault(DateTime.Now),
                BillingMethod = checkoutResult.BillingMethod,
                ForthcomingTitlesInvoicingMethod = checkoutResult.ForthcomingTitlesInvoicingMethod,
                CartName = cart.CartName
            };

            if (promotion != null)
            {
                dbOrderHistory.PromotionCode = cart.PromotionCode;
                dbOrderHistory.PromotionDescription = promotion.Description;
                dbOrderHistory.PromotionId = promotion.Id;
                dbOrderHistory.Discount = cart.PromotionDiscount;
                dbOrderHistory.DiscountTypeId = 2;
            }


            else if (cart.CartType == CartTypeEnum.AutomatedCart)
            {
                dbOrderHistory.Discount = cart.Discount;
                dbOrderHistory.DiscountTypeId = 3;
            }


            else if (cart.Reseller != null)
            {
                dbOrderHistory.Reseller = cart.Reseller;
                dbOrderHistory.ResellerName = cart.Reseller.Name;
                dbOrderHistory.Discount = cart.Reseller.Discount;
                dbOrderHistory.DiscountTypeId = 5;
            }
            else
            {
                dbOrderHistory.Discount = cart.Discount;
                dbOrderHistory.DiscountTypeId = 1;
            }

            dbOrderHistory.OrderHistoryItems = new Collection<DbOrderHistoryItem>();
            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.ResourceId.HasValue)
                {
                    var resource = _resourceService.GetResource(cartItem.ResourceId.Value);
                    if (resource != null && !resource.NotSaleableDate.HasValue && resource.IsForSale)
                    {
                        dbOrderHistory.OrderHistoryItems.Add(ConvertOrderHistoryItem(cartItem,
                            dbOrderHistory.DiscountTypeId, resource));
                    }
                }
                else if (cartItem.ProductId.HasValue)
                {
                    dbOrderHistory.OrderHistoryItems.Add(ConvertOrderHistoryItem(cartItem,
                        dbOrderHistory.DiscountTypeId));
                }
            }

            return dbOrderHistory;
        }

        private DbOrderHistoryItem ConvertOrderHistoryItem(CartItem cartItem, int cartDiscountTypeId,
            IResource resource = null)
        {
            return new DbOrderHistoryItem
            {
                ResourceId = cartItem.ResourceId,
                ProductId = cartItem.ProductId,
                InstitutionResourceLicenseId = cartItem.InstitutionResourceLicenseId,
                NumberOfLicenses = resource != null && resource.IsFreeResource ? 1 : cartItem.NumberOfLicenses,
                ListPrice = cartItem.ListPrice,
                DiscountPrice = cartItem.DiscountPrice,
                Discount = cartItem.Discount,
                SpecialText = cartItem.SpecialText,
                SpecialIconName = cartItem.SpecialIconName,
                SpecialDiscountId = cartItem.SpecialDiscountId > 0 ? cartItem.SpecialDiscountId : null,
                PdaPromotionId = cartItem.PdaPromotionId > 0 ? cartItem.PdaPromotionId : null,
                DiscountTypeId = cartItem.SpecialDiscountId > 0 ? 4 :
                    cartItem.PdaPromotionId > 0 ? 3 : cartDiscountTypeId,
                IsBundle = cartItem.IsBundle,
                BundlePrice = cartItem.BundlePrice
            };
        }
    }
}