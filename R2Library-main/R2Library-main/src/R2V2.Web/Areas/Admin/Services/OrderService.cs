#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.OrderRelay;
using R2V2.Core.Publisher;
using R2V2.Core.R2Utilities;
using R2V2.Core.Recommendations;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Subscriptions;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.Checkout;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Order;
using R2V2.Web.Areas.Admin.Models.Promotion;
using R2V2.Web.Areas.Admin.Models.ReserveShelfManagement;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Services;
using InstitutionResource = R2V2.Web.Areas.Admin.Models.CollectionManagement.InstitutionResource;
using ReserveShelfResource = R2V2.Web.Areas.Admin.Models.ReserveShelfManagement.ReserveShelfResource;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class OrderService : IOrderService
    {
        private static readonly Random Random = new Random();
        private readonly IAdminContext _adminContext;
        private readonly IQueryable<Cart> _carts;
        private readonly ICartService _cartService;
        private readonly IClientSettings _clientSettings;
        private readonly ICollectionManagementService _collectionManagementService;
        private readonly ICollectionService _collectionService;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<OrderService> _log;
        private readonly WebOrderHistoryService _orderHistoryService;
        private readonly OrderMessageService _orderMessageService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly IQueryable<IProduct> _products;
        private readonly PromotionsService _promotionsService;
        private readonly PublisherService _publisherService;
        private readonly RecommendationsService _recommendationsService;
        private readonly IQueryable<ReserveShelf> _reserveShelves;
        private readonly ResourceDiscountService _resourceDiscountService;
        private readonly IResourceService _resourceService;
        private readonly ISearchService _searchService;
        private readonly ISpecialtyService _specialtyService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IWebImageSettings _webImageSettings;

        public OrderService(ICollectionManagementService collectionManagementService
            , ICartService cartService
            , IAdminContext adminContext
            , IResourceService resourceService
            , IQueryable<Cart> carts
            , IInstitutionSettings institutionSettings
            , IQueryable<ReserveShelf> reserveShelves
            , ISpecialtyService specialtyService
            , IPracticeAreaService practiceAreaService
            , IClientSettings clientSettings
            , ILog<OrderService> log
            , RecommendationsService recommendationsService
            , OrderMessageService orderMessageService
            , PromotionsService promotionsService
            , IUnitOfWorkProvider unitOfWorkProvider
            , IWebImageSettings webImageSettings
            , PublisherService publisherService
            , ICollectionService collectionService
            , ResourceDiscountService resourceDiscountService
            , WebOrderHistoryService orderHistoryService
            , IQueryable<IProduct> products
            , ISearchService searchService
            , ISubscriptionService subscriptionService
        )
        {
            _collectionManagementService = collectionManagementService;
            _cartService = cartService;
            _adminContext = adminContext;
            _resourceService = resourceService;
            _carts = carts;
            _institutionSettings = institutionSettings;
            _reserveShelves = reserveShelves;
            _specialtyService = specialtyService;
            _practiceAreaService = practiceAreaService;
            _clientSettings = clientSettings;
            _log = log;
            _recommendationsService = recommendationsService;
            _orderMessageService = orderMessageService;
            _promotionsService = promotionsService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _webImageSettings = webImageSettings;
            _publisherService = publisherService;
            _collectionService = collectionService;
            _resourceDiscountService = resourceDiscountService;
            _orderHistoryService = orderHistoryService;
            _products = products;
            _searchService = searchService;
            _subscriptionService = subscriptionService;
        }

        public Order GetOrder(int orderId, IAdminInstitution adminInstitution)
        {
            // ReSharper disable once UnusedVariable
            var cartResources = _carts
                .Where(c => c.Id == orderId)
                .FetchMany(c => c.CartItems)
                .ToFuture();

            var cart = _carts
                .Where(c => c.Id == orderId)
                .ToFuture()
                .FirstOrDefault();

            return BuildOrder(cart, adminInstitution, false);
        }

        public Order GetOrderForInstitution(int institutionId, int cartId = 0)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            if (institution == null || institution.AccountNumber == _institutionSettings.GuestAccountNumber)
            {
                return null;
            }

            var cart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
            if (cart != null)
            {
                return BuildOrderFromCashedCart(cart, institution);
            }

            return null;
        }

        public Order GetOrderFromDatabaseForInstitution(int institutionId, int cartId = 0)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            if (institution == null || institution.AccountNumber == _institutionSettings.GuestAccountNumber)
            {
                return null;
            }

            if (cartId == 0)
            {
                var cachedCart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
                cartId = cachedCart.Id;
            }

            var cart = _cartService.GetInstitutionCartFromDatabase(institutionId, cartId);

            return BuildOrderFromCashedCart(cart, institution);
        }

        public IEnumerable<IOrder> GetOrdersForInstitution(IAdminInstitution adminInstitution)
        {
            var resources = _resourceService.GetAllResources().ToList();
            var products = _products.ToList();
            return _carts
                .FetchMany(c => c.CartItems)
                .Where(c => c.InstitutionId == adminInstitution.Id && c.Processed)
                .OrderByDescending(c => c.PurchaseDate)
                .ToList()
                .Select(cart => new OrderSummary(cart, adminInstitution, resources, products));
        }

        public bool AddItemToOrder(CollectionAdd collectionAdd)
        {
            var institutionId = collectionAdd.InstitutionId;
            var resourceId = collectionAdd.InstitutionResource.Id;
            var cartId = collectionAdd.CartId;
            _log.DebugFormat("AddItemToOrder() - institutionId: {0}, resourceId: {1}", institutionId, resourceId);

            var institutionResource = GetInstitutionResource(institutionId, resourceId, cartId);

            collectionAdd.InstitutionResource = institutionResource;

            if (institutionResource.LicenseType == LicenseType.Purchased)
            {
                collectionAdd.OriginalNumberOfLicenses = institutionResource.LicenseCount;
            }

            if (institutionResource.CartLicenseCount > 0)
            {
                if (collectionAdd.IsBundlePurchase)
                {
                    return false;
                }

                return _cartService.UpdateLicenseCountInCart(institutionId, resourceId, collectionAdd.NumberOfLicenses,
                    cartId);
            }

            var saveSuccess = _cartService.AddItemToCart(institutionId, resourceId, collectionAdd.NumberOfLicenses,
                LicenseOriginalSource.FirmOrder, cartId, collectionAdd.IsBundlePurchase);
            _cartService.GetInstitutionCartFromCache(institutionId, cartId);

            return saveSuccess;
        }

        public bool AddItemToOrder2(CollectionAdd collectionAdd)
        {
            var institutionId = collectionAdd.InstitutionId;
            var resourceId = collectionAdd.InstitutionResource.Id;
            var cartId = collectionAdd.CartId;
            _log.DebugFormat("AddItemToOrder() - institutionId: {0}, resourceId: {1}", institutionId, resourceId);

            var currentCart = _cartService.GetInstitutionCartFromCache(institutionId);

            var institutionResource = GetInstitutionResource(institutionId, resourceId, cartId);

            collectionAdd.InstitutionResource = institutionResource;

            if (institutionResource.LicenseType == LicenseType.Purchased)
            {
                collectionAdd.OriginalNumberOfLicenses = institutionResource.LicenseCount;
            }

            if (institutionResource.CartLicenseCount > 0)
            {
                return _cartService.UpdateLicenseCountInCart(institutionId, resourceId, collectionAdd.NumberOfLicenses,
                    cartId);
            }

            var saveSuccess = _cartService.AddItemToCart(institutionId, resourceId, collectionAdd.NumberOfLicenses,
                LicenseOriginalSource.FirmOrder, cartId);

            _cartService.GetInstitutionCartFromCache(institutionId, currentCart.Id);

            return saveSuccess;
        }

        public bool AddBulkItemsToOrder(int institutionId, IEnumerable<InstitutionResource> institutionResources)
        {
            var bulkAddResources = new List<BulkAddResource>();
            foreach (var institutionResource in institutionResources)
            {
                var bulkAddResource = new BulkAddResource
                {
                    InstitutionId = institutionId,
                    ResourceId = institutionResource.Id,
                    NumberOfLicenses = 1,
                    OriginalSource = LicenseOriginalSource.FirmOrder
                };

                if (institutionResource.IsFreeResource)
                {
                    bulkAddResource.NumberOfLicenses = 500;
                }

                bulkAddResources.Add(bulkAddResource);
            }

            return _cartService.AddBulkItemsToCart(institutionId, bulkAddResources);
        }

        public bool UpdateOrderItem(int institutionId, ResourceOrderItem resourceOrderItem, int cartId)
        {
            return resourceOrderItem.NumberOfLicenses <= 0
                ? _cartService.RemoveItemFromCart(institutionId, resourceOrderItem.ItemId, cartId)
                : _cartService.UpdateItemInCart(institutionId, resourceOrderItem.ItemId,
                    resourceOrderItem.NumberOfLicenses, cartId);
        }

        public bool UpdateOrderItem(int institutionId, IProductOrderItem productOrderItem)
        {
            return _cartService.UpdateItemInCart(institutionId, productOrderItem);
        }

        public bool RemoveItemFromOrder(int institutionId, int itemId, int cartId)
        {
            return _cartService.RemoveItemFromCart(institutionId, itemId, cartId);
        }

        public int MergeCarts(int currentCartId, int mergeIntoCartId, int institutionId)
        {
            return _cartService.MergeCarts(currentCartId, mergeIntoCartId, institutionId);
        }

        public bool RemoveAllResourcesFromOrder(int institutionId, int cartId)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
            return _cartService.RemoveAllResourcesFromCart(institutionId, cart.Id, cart.CartType);
        }

        public bool RemoveResourcesFromOrder(int institutionId, int cartId, int[] resourceIds)
        {
            return _cartService.RemoveResourcesFromCart(institutionId, cartId, resourceIds);
        }

        public IEnumerable<IProductOrderItem> GetProductsRequiringAgreements(Order order)
        {
            var o = GetOrder(order.OrderId, order.Institution); // get a fully hydrated order

            return o.Items.OfType<IProductOrderItem>()
                .Where(productOrderItem => !productOrderItem.Agree && productOrderItem.Include);
        }

        public PromotionAction ApplyPromotion(int institutionId, CachedPromotion promotion)
        {
            return _cartService.ApplyPromotion(institutionId, promotion);
        }

        public PromotionAction RemovePromotion(int institutionId)
        {
            return _cartService.RemovePromotion(institutionId);
        }

        public bool AgreeToProductLicense(Order order, int productId)
        {
            var o = GetOrder(order.OrderId,
                _adminContext.GetAdminInstitution(order.InstitutionId)); // get a fully hydrated order

            var productOrderItem = o.Items.OfType<IProductOrderItem>().FirstOrDefault(x => x.Product.Id == productId);
            if (productOrderItem != null)
            {
                productOrderItem.Agree = true;
                return _cartService.UpdateItemInCart(o.InstitutionId, productOrderItem);
            }

            return false;
        }

        public bool PlaceOrder(Order order, IUser currentUser, bool sendNewAccountEmail)
        {
            string lastStepCompleted = null;
            var preludeOrderFileSentSuccessfully = false;
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var institution = _adminContext.GetAdminInstitution(order.InstitutionId);
                        lastStepCompleted = "GetAdminInstitution() completed successfully.";

                        var request = new CheckoutRequest(order, institution);
                        lastStepCompleted = "new CheckoutRequest() created successfully.";

                        var result = _cartService.Checkout(request, uow, institution);
                        lastStepCompleted = "Checkout() completed successfully.";

                        if (result.Successful)
                        {
                            //Must save Order History before we send the prelude Order file to queue.
                            //The queue updates the orderhistory
                            order.OrderHistoryId = _orderHistoryService.SaveOrderHistory(request, result.Cart, uow);


                            // send prelude order file to the queue
                            var message = SendOrderFileToPrelude(result.Cart, institution, currentUser);
                            preludeOrderFileSentSuccessfully = message != null;
                            lastStepCompleted = "SendOrderFileToPrelude() completed successfully.";
                            if (preludeOrderFileSentSuccessfully)
                            {
                                // remove cart from the session so a new cart is created
                                _cartService.RemoveCartsFromCache();
                                lastStepCompleted = "RemoveCartFromCache() completed successfully.";

                                _recommendationsService.RecommendationsPurchased(order, currentUser, uow);


                                return true;
                            }
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine("R2 CHECKOUT PROCESS ERROR -- IMMEDIATE ATTENTION REQUIRED!!!");
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine(order.ToDebugString());
                        msg.AppendLine(currentUser.ToDebugString());
                        msg.AppendFormat("sendNewAccountEmail: {0}", sendNewAccountEmail);
                        msg.AppendFormat("lastStepCompleted: {0}", lastStepCompleted);
                        msg.AppendFormat("preludeOrderFileSentSuccessfully: {0}", preludeOrderFileSentSuccessfully);
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine();
                        msg.AppendFormat("Exception: {0}", ex.Message);

                        _log.Error(msg.ToString(), ex);

                        return false;
                    }
                    finally
                    {
                        if (preludeOrderFileSentSuccessfully)
                        {
                            uow.Commit();
                            transaction.Commit();

                            // release the admin context
                            _adminContext.ReloadAdminInstitution(order.InstitutionId, currentUser.Id);
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        public int PlaceOrder(SubscriptionOrderHistory order, IUser currentUser)
        {
            string lastStepCompleted = null;
            var orderHistoryId = 0;
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        //TODO: Create User Subscription with endDate
                        //TODO: Create Order History of Subscription
                        order.SubscriptionUserId = _subscriptionService.CreateUserSubscription(order, currentUser, uow);
                        lastStepCompleted = "CreateUserSubscription() completed successfully.";
                        order.OrderNumber = $"R2{order.SubscriptionUserId:0000#}{Random.Next(999):000}";
                        orderHistoryId = _subscriptionService.CreateOrderHistory(order, currentUser, uow);
                        lastStepCompleted = "CreateOrderHistory() completed successfully.";
                    }
                    catch (Exception ex)
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine("R2 CHECKOUT PROCESS ERROR -- IMMEDIATE ATTENTION REQUIRED!!!");
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine(order.ToDebugString());
                        msg.AppendLine(currentUser.ToDebugString());
                        msg.AppendFormat("lastStepCompleted: {0}", lastStepCompleted);
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine();
                        msg.AppendFormat("Exception: {0}", ex.Message);

                        _log.Error(msg.ToString(), ex);
                    }
                    finally
                    {
                        if (orderHistoryId > 0)
                        {
                            uow.Commit();
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }

            return orderHistoryId;
        }

        public bool SaveCart(int cartId, int institutionId, string cartName)
        {
            return _cartService.SaveCart(cartId, institutionId, cartName);
        }

        public int CopyCart(int cartId, int institutionId, string cartName)
        {
            return _cartService.CopyCart(cartId, institutionId, cartName);
        }

        public ShoppingCartExcelExport GetShoppingCartExcelExport(int institutionId, string bookPrefixUrl,
            string bookSuffixUrl, string bookUrl)
        {
            var order = GetOrderForInstitution(institutionId);

            return new ShoppingCartExcelExport(order, bookPrefixUrl, bookSuffixUrl, bookUrl);
        }

        private Order BuildOrder(Cart cart, IAdminInstitution adminInstitution, bool isOrderHistory)
        {
            Order order = null;

            if (cart != null)
            {
                var hideDiscount = cart.Reseller != null && cart.Reseller.Id > 0 && isOrderHistory;
                order = new Order(adminInstitution)
                {
                    OrderId = cart.Id,
                    PurchaseOrderNumber = cart.PurchaseOrderNumber,
                    PurchaseOrderComment = cart.PurchaseOrderComment,
                    PurchaseDate = cart.PurchaseDate,
                    BillingMethod = cart.BillingMethod,
                    ForthcomingTitlesInvoicingMethod = cart.ForthcomingTitlesInvoicingMethod,
                    OrderNumber = cart.OrderNumber,
                    BillingMethodDescription = cart.Reseller != null
                        ? cart.Reseller.DisplayName
                        : cart.BillingMethod.ToBillingMethod().Description,
                    SavedDate = cart.ConvertDate
                };

                if (!hideDiscount)
                {
                    if (string.IsNullOrWhiteSpace(cart.PromotionCode))
                    {
                        order.Discount = cart.Discount;
                    }
                    else
                    {
                        order.Discount = cart.PromotionDiscount;
                        order.PromotionCode = cart.PromotionCode;
                        order.PromotionName = "";
                        order.PromotionDiscount = cart.PromotionDiscount;
                        order.PromotionDescription = "";
                    }
                }
                else
                {
                    order.Discount = 0;
                    order.PromotionCode = null;
                    order.PromotionName = null;
                    order.PromotionDiscount = 0;
                    order.PromotionDescription = null;
                }

                foreach (var cartItem in cart.CartItems.ToList())
                {
                    IResource resource = null;
                    IProduct product = null;
                    CachedSpecialResource cachedSpecialResource = null;
                    IList<Recommendation> recommendations = null;
                    if (cartItem.ResourceId != null)
                    {
                        // if not in cache, query db
                        resource = _resourceService.GetResource(cartItem.ResourceId.Value) ??
                                   _resourceService.GetSoftDeletedResource(cartItem.ResourceId.Value);
                        //Don't count this as purchased if archived or deleted.
                        if (!isOrderHistory)
                        {
                            if (resource.StatusId == (int)ResourceStatus.Inactive ||
                                resource.StatusId == (int)ResourceStatus.Archived)
                            {
                                continue;
                            }

                            _resourceDiscountService.SetDiscount(cartItem, adminInstitution);
                        }

                        recommendations = resource != null
                            ? _recommendationsService.GetRecommendations(adminInstitution.Id, resource.Id)
                            : null;
                    }
                    else if (cartItem.ProductId != null)
                    {
                        product = _products.FirstOrDefault(x => x.Id == cartItem.ProductId);
                    }

                    if (cartItem.ProductId != null && !isOrderHistory)
                    {
                        _resourceDiscountService.SetDiscount(cartItem, adminInstitution);
                    }

                    //Hide Discount from Customer
                    if (cart.Reseller != null && cart.Reseller.Id > 0 && isOrderHistory)
                    {
                        cartItem.Discount = 0;
                        cartItem.DiscountPrice = cartItem.ListPrice;
                    }


                    order.AddItem(cartItem, product, resource, recommendations);
                }
            }

            //The order may not be the current active cart so we must blow away the cache.
            _cartService.RemoveCartsFromCache();


            return order;
        }

        private Order BuildOrderFromCashedCart(CachedCart cart, IAdminInstitution adminInstitution)
        {
            Order order = null;

            if (cart != null)
            {
                order = new Order(adminInstitution)
                {
                    OrderId = cart.Id,
                    PurchaseOrderNumber = cart.PurchaseOrderNumber,
                    PurchaseOrderComment = cart.PurchaseOrderComment,
                    PurchaseDate = cart.PurchaseDate,
                    BillingMethod = cart.BillingMethod,
                    ForthcomingTitlesInvoicingMethod = cart.ForthcomingTitlesInvoicingMethod,
                    IsPromotionAvailable = cart.IsPromotionAvailable,
                    OrderNumber = cart.OrderNumber,
                    SpecialIconBaseUrl = _webImageSettings.SpecialIconBaseUrl,
                    CartName = cart.CartName,
                    //IsSavedCart = cart.CartType == CartTypeEnum.Saved,
                    CartType = cart.CartType,
                    SaveCopyCartText = cart.CartType == CartTypeEnum.Active ? "Save" : "Copy",
                    SavedDate = cart.ConvertDate
                };

                if (cart.Promotion != null)
                {
                    order.Discount = cart.Promotion.Discount;
                    order.PromotionCode = cart.Promotion.Code;
                    order.PromotionName = cart.Promotion.Name;
                    order.PromotionDiscount = cart.Promotion.Discount;
                    order.PromotionDescription = cart.Promotion.Description;
                    order.PromotionProductIds = cart.Promotion.PromotionProductIds;
                }
                else
                {
                    order.Discount = cart.Discount;
                }

                var allRecommendations = _recommendationsService.GetRecommendations(adminInstitution.Id);

                foreach (var cartItem in cart.CartItems.ToList())
                {
                    IResource resource = null;
                    IProduct product = null;
                    CachedSpecialResource cachedSpecialResource = null;
                    IList<Recommendation> recommendations = null;
                    if (cartItem.ResourceId != null)
                    {
                        resource = _resourceService.GetResource(cartItem.ResourceId.Value);
                    }
                    else if (cartItem.ProductId != null)
                    {
                        product = _products.FirstOrDefault(x => x.Id == cartItem.ProductId);
                    }

                    if (resource != null)
                    {
                        recommendations = allRecommendations.Where(x => x.ResourceId == resource.Id).ToList();
                        _resourceDiscountService.SetDiscount(cartItem, adminInstitution);
                    }
                    else
                    {
                        if (product != null)
                        {
                            _resourceDiscountService.SetDiscount(cartItem, adminInstitution);
                        }
                    }

                    order.AddItem(cartItem, product, resource, recommendations);
                }

                var isPdaPromotionOnly = true;
                foreach (var item in order.Items.OfType<ResourceOrderItem>())
                {
                    isPdaPromotionOnly = isPdaPromotionOnly && item.PdaPromotionApplied;
                }

                //Need to hide Promotion if the cart is PDA promotion only.
                order.IsPromotionAvailable = order.IsPromotionAvailable && !isPdaPromotionOnly;

                if (cart.ResellerId > 0)
                {
                    order.IsPromotionAvailable = false;
                }

                if (order.PurchaseDate != null)
                {
                    order.OrderHistoryId = _orderHistoryService.GetOrderHistoryId(cart.Id, order.InstitutionId);
                }
            }

            return order;
        }

        private string SendOrderFileToPrelude(Cart cart, IAdminInstitution institution, IUser currentUser)
        {
            try
            {
                var promotion = string.IsNullOrWhiteSpace(cart.PromotionCode)
                    ? null
                    : _promotionsService.GetDbPromotion(cart.PromotionCode);
                // Send message to the queue so the windows service will sent the order to Prelude
                var orderMessage = _orderMessageService.BuildOrderMessage(cart, promotion, institution, currentUser);
                //OrderMessage orderMessage = _orderMessageService.BuildOrderMessage(cart, promotion, pdaPromotion, institution, currentUser, specialDiscountResources);
                var message = _orderMessageService.SendOrderMessageToQueue(orderMessage);

                return message;
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder();
                msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                msg.AppendLine("R2 CHECKOUT PROCESS ERROR -- IMMEDIATE ATTENTION REQUIRED!!!");
                msg.AppendLine("ERROR SENDING R2 ORDER FILE TO PRELUDE!");
                msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                msg.AppendLine(cart.ToDebugString());
                msg.AppendLine(currentUser.ToDebugString());
                msg.AppendLine(institution.ToDebugString());
                msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                msg.AppendLine();
                msg.AppendFormat("Exception: {0}", ex.Message);

                _log.Error(msg.ToString(), ex);
                return null;
            }
        }

        #region CollectionManagementService

        public InstitutionResource GetInstitutionResource(int institutionId, string isbn, int cartId)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
            var collectionManagementResource =
                _collectionManagementService.GetCollectionManagementResource(institutionId, isbn, cart);
            return collectionManagementResource != null
                ? GetInstitutionResource(institutionId, collectionManagementResource)
                : null;
        }

        public IEnumerable<InstitutionResource> GetInstitutionResources(int institutionId, List<string> isbns)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId);

            var collectionManagementResources =
                isbns.Select(x =>
                    _collectionManagementService.GetCollectionManagementResourceWithoutDatabase(institutionId, x,
                        cart)).Distinct(new CollectionManagementResourceEqualityComparer());

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var x in collectionManagementResources)
            {
                if (x != null)
                {
                    yield return GetInstitutionResource(institutionId, x);
                }
            }
            // ReSharper restore LoopCanBeConvertedToQuery
        }

        public InstitutionResource GetInstitutionResource(int institutionId, int resourceId, int cartId)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
            var collectionManagementResource = _collectionManagementService.GetCollectionManagementResource(
                institutionId, resourceId,
                cart);
            return GetInstitutionResource(institutionId, collectionManagementResource);
        }

        public InstitutionResource GetInstitutionResource(int institutionId,
            CollectionManagementResource collectionManagementResource)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            IEnumerable<Recommendation> recommendations = _recommendationsService.GetRecommendations(institutionId);
            return collectionManagementResource.ToInstitutionResource(institution, recommendations);
        }

        public InstitutionResources GetInstitutionResources(CollectionManagementQuery collectionManagementQuery,
            IUser user)
        {
            var institution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);
            if (institution == null || institution.Id == 0)
            {
                return new InstitutionResources { CollectionManagementQuery = new CollectionManagementQuery() };
            }

            var collectionManagementResources =
                GetBaseCollectionManagementResources(collectionManagementQuery, user.IsExpertReviewer());


            var resourceCount = collectionManagementResources.Count();
            var firstItemNumber = (collectionManagementQuery.Page - 1) * collectionManagementQuery.PageSize + 1;
            if (resourceCount > collectionManagementQuery.Page * collectionManagementQuery.PageSize)
            {
                collectionManagementResources =
                    collectionManagementResources
                        .Skip((collectionManagementQuery.Page - 1) * collectionManagementQuery.PageSize)
                        .Take(collectionManagementQuery.PageSize).ToList();
            }
            else if (resourceCount < collectionManagementQuery.Page * collectionManagementQuery.PageSize &&
                     resourceCount > collectionManagementQuery.PageSize)
            {
                var takeSize = resourceCount -
                               (collectionManagementQuery.Page - 1) * collectionManagementQuery.PageSize;
                collectionManagementResources = collectionManagementResources.Skip(resourceCount - takeSize).ToList();
            }
            else
            {
                collectionManagementResources = collectionManagementResources
                    .Skip((collectionManagementQuery.Page - 1) * collectionManagementQuery.PageSize).ToList();
            }

            IEnumerable<Recommendation> recommendations = user.IsExpertReviewer()
                ? _recommendationsService.GetRecommendationsIncludeDeleted(institution.Id)
                : _recommendationsService.GetRecommendations(institution.Id);

            var institutionResourcesList =
                collectionManagementResources.ToInstitutionResources(institution, recommendations, user);

            var institutionResources = new InstitutionResources(institution, collectionManagementQuery,
                institutionResourcesList,
                _practiceAreaService, _specialtyService, _collectionService, _clientSettings.DoodyReviewLink,
                _webImageSettings.SpecialIconBaseUrl)
            {
                TotalCount = resourceCount,
                ResultsFirstItem = firstItemNumber
            };

            if (collectionManagementQuery.PublisherId > 0)
            {
                var publisher = _publisherService.GetPublisher(collectionManagementQuery.PublisherId);
                institutionResources.Publisher = new ActivePublisher(publisher, _webImageSettings.PublisherImageUrl);
            }

            institutionResources.ResultsLastItem =
                firstItemNumber + institutionResources.InstitutionResourcesList.Count() - 1;

            return institutionResources;
        }

        /// <summary>
        ///     TODO: Use this as a BASE to get all Collection Management Resources
        /// </summary>
        private List<CollectionManagementResource> GetBaseCollectionManagementResources(
            CollectionManagementQuery collectionManagementQuery, bool isExpertReviewer)
        {
            var institution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);
            if (institution == null || institution.Id == 0)
            {
                return null;
            }

            if (collectionManagementQuery.InstitutionId == 0)
            {
                collectionManagementQuery.InstitutionId = institution.Id;
            }

            var order = GetOrderForInstitution(institution.Id);

            var filteredResources = _searchService.SearchAdmin(collectionManagementQuery.Query, institution.Licenses);

            return _collectionManagementService
                .GetCollectionManagementResources(
                    filteredResources
                    , collectionManagementQuery
                    , order
                    , isExpertReviewer)
                .ToList();
        }


        public IEnumerable<InstitutionResource> GetInstitutionResources(
            CollectionManagementQuery collectionManagementQuery, out List<string> isbnsNotFound)
        {
            IEnumerable<InstitutionResource> resources = new List<InstitutionResource>();
            isbnsNotFound = new List<string>();

            if (!string.IsNullOrEmpty(collectionManagementQuery.Resources))
            {
                resources = GetInstitutionResourcesById(collectionManagementQuery);
            }
            else if (!string.IsNullOrEmpty(collectionManagementQuery.Isbns))
            {
                resources = GetInstitutionResourcesByIsbn(collectionManagementQuery, out isbnsNotFound);
            }

            return resources;
        }

        public IEnumerable<InstitutionResource> GetInstitutionResourcesWithoutDatabase(
            CollectionManagementQuery collectionManagementQuery, out List<string> isbnsNotFound)
        {
            IEnumerable<InstitutionResource> resources = new List<InstitutionResource>();
            isbnsNotFound = new List<string>();

            if (!string.IsNullOrEmpty(collectionManagementQuery.Resources))
            {
                resources = GetInstitutionResourcesById(collectionManagementQuery);
            }
            else if (!string.IsNullOrEmpty(collectionManagementQuery.Isbns))
            {
                resources = GetInstitutionResourcesByIsbnWithoutDatabase(collectionManagementQuery, out isbnsNotFound);
            }

            return resources;
        }

        private IEnumerable<InstitutionResource> GetInstitutionResourcesById(
            CollectionManagementQuery collectionManagementQuery)
        {
            return collectionManagementQuery.GetResourceIds()
                .Select(resourceId => GetInstitutionResource(collectionManagementQuery.InstitutionId, resourceId,
                    collectionManagementQuery.CartId));
        }

        private IEnumerable<InstitutionResource> GetInstitutionResourcesByIsbn(
            CollectionManagementQuery collectionManagementQuery,
            out List<string> isbnsNotFound)
        {
            var resources = new List<InstitutionResource>();
            isbnsNotFound = new List<string>();

            foreach (var isbn in IsbnUtilities.GetDelimitedIsbns(collectionManagementQuery.Isbns))
            {
                var result = GetInstitutionResource(collectionManagementQuery.InstitutionId, isbn,
                    collectionManagementQuery.CartId);
                if (result != null)
                {
                    resources.Add(result);
                }
                else
                {
                    isbnsNotFound.Add(isbn);
                }
            }

            collectionManagementQuery.SerializeResources(resources.Select(resource => resource.Id));

            return resources;
        }

        private IEnumerable<InstitutionResource> GetInstitutionResourcesByIsbnWithoutDatabase(
            CollectionManagementQuery collectionManagementQuery,
            out List<string> isbnsNotFound)
        {
            var resources = new List<InstitutionResource>();

            var isbns = IsbnUtilities.GetDelimitedIsbns(collectionManagementQuery.Isbns)
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            resources.AddRange(GetInstitutionResources(collectionManagementQuery.InstitutionId, isbns));

            isbnsNotFound = (from isbn in isbns
                let resource =
                    resources.FirstOrDefault(x =>
                        x.Isbn == isbn || x.Isbn10 == isbn || x.Isbn13 == isbn || x.EIsbn == isbn)
                where resource == null
                select isbn).ToList();

            collectionManagementQuery.SerializeResources(resources.Select(resource => resource.Id));

            return resources;
        }

        public ReserveShelfManagement GetReserveShelfResources(ReserveShelfQuery reserveShelfQuery)
        {
            var institution = _adminContext.GetAdminInstitution(reserveShelfQuery.InstitutionId);
            if (institution == null)
            {
                return null;
            }

            var instituionId = reserveShelfQuery.InstitutionId;
            var reserveShelfId = reserveShelfQuery.ReserveShelfId;

            var reserveShelf =
                _reserveShelves.SingleOrDefault(x => x.Institution.Id == instituionId && x.Id == reserveShelfId);

            var filteredResources = _searchService.SearchAdmin(reserveShelfQuery.Query, institution.Licenses);
            //filteredResources = filteredResources.Where(x => x.StatusId != (int) ResourceStatus.Archived).ToList();
            var order = GetOrderForInstitution(institution.Id);
            var allCollectionManagementResources =
                _collectionManagementService.GetCollectionManagementResources(filteredResources, reserveShelfQuery,
                    order);
            var collectionManagementResources = allCollectionManagementResources.ToList();
            var resourceCount = collectionManagementResources.Count();
            if (resourceCount > reserveShelfQuery.Page * reserveShelfQuery.PageSize)
            {
                collectionManagementResources = collectionManagementResources
                    .Skip((reserveShelfQuery.Page - 1) * reserveShelfQuery.PageSize).Take(reserveShelfQuery.PageSize)
                    .ToList();
            }
            else if (resourceCount < reserveShelfQuery.Page * reserveShelfQuery.PageSize &&
                     resourceCount > reserveShelfQuery.PageSize)
            {
                collectionManagementResources = collectionManagementResources
                    .Skip((reserveShelfQuery.Page - 1) * reserveShelfQuery.PageSize)
                    .Take(resourceCount - reserveShelfQuery.PageSize).ToList();
            }
            else
            {
                collectionManagementResources = collectionManagementResources
                    .Skip((reserveShelfQuery.Page - 1) * reserveShelfQuery.PageSize).ToList();
            }

            var reserveShelfResources =
                GetReserveShelfResources(reserveShelf, collectionManagementResources, institution);

            var reserveShelfManagement = new ReserveShelfManagement(institution, reserveShelfQuery,
                reserveShelfResources, reserveShelf, _practiceAreaService,
                _specialtyService, _collectionService, _clientSettings.DoodyReviewLink,
                _webImageSettings.SpecialIconBaseUrl);

            var currentCount = (reserveShelfQuery.Page - 1) * reserveShelfQuery.PageSize;
            reserveShelfManagement.ResultsFirstItem = currentCount + 1;
            reserveShelfManagement.ResultsLastItem =
                currentCount + reserveShelfManagement.ReserveShelfResources.Count();

            reserveShelfManagement.TotalCount = resourceCount;

            return reserveShelfManagement;
        }

        public List<CollectionManagementResource> GetCollectionManagementResources(
            CollectionManagementQuery collectionManagementQuery, bool isExpertReviewer)
        {
            return GetBaseCollectionManagementResources(collectionManagementQuery, isExpertReviewer);
        }

        public bool UpdateLicenseCount(CollectionEdit collectionEdit)
        {
            var institutionId = collectionEdit.InstitutionId;
            var resourceId = collectionEdit.InstitutionResource.Id;

            return _collectionManagementService.UpdateInstitutionResourceLicenses(institutionId, resourceId,
                collectionEdit.NumberOfLicenses);
        }

        private IEnumerable<ReserveShelfResource> GetReserveShelfResources(ReserveShelf reserveShelf,
            IEnumerable<CollectionManagementResource> collectionManagementResources,
            IAdminInstitution adminInstitution)
        {
            return collectionManagementResources.Select(x =>
                CreateReserveShelfResource(x, reserveShelf, adminInstitution));
        }

        private ReserveShelfResource CreateReserveShelfResource(
            CollectionManagementResource collectionManagementResource, ReserveShelf reserveShelf,
            IAdminInstitution adminInstitution)
        {
            IEnumerable<Recommendation> recommendations =
                _recommendationsService.GetRecommendations(adminInstitution.Id);

            return new ReserveShelfResource(collectionManagementResource, adminInstitution, recommendations
                , reserveShelf.ReserveShelfResources.Select(x => x.ResourceId).Contains(collectionManagementResource.Resource.Id));
        }

        #endregion
    }
}

class CollectionManagementResourceEqualityComparer : IEqualityComparer<CollectionManagementResource>
{
    public bool Equals(CollectionManagementResource collectionManagementResource1,
        CollectionManagementResource collectionManagementResource2)
    {
        if (collectionManagementResource1 == null || collectionManagementResource2 == null)
        {
            return false;
        }

        if (collectionManagementResource1.Resource == null || collectionManagementResource2.Resource == null)
        {
            return false;
        }


        return collectionManagementResource1.Resource == collectionManagementResource2.Resource;
    }

    public int GetHashCode(CollectionManagementResource collectionManagementResource)
    {
        var hashCode = collectionManagementResource.Resource.Id;
        return hashCode.GetHashCode();
    }
}