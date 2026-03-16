#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CartService : ICartService
    {
        public const string ActiveCartKey = "Active.Cart";
        public const string AllCartsKey = "All.Carts";

        public const string ActiveCartDateCachedKey = "Active.Cart.DateTime";
        public const string SavedCartsDateCachedKey = "Saved.Carts.DateTime";
        private readonly IAdminContext _adminContext;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly AutomatedCartFactory _automatedCartFactory;
        private readonly CachedDiscountFactory _cachedDiscountFactory;
        private readonly CartFactory _cartFactory;
        private readonly CartItemFactory _cartItemFactory;
        private readonly IQueryable<Cart> _carts;
        private readonly InstitutionResourceAuditFactory _institutionResourceAuditFactory;

        private readonly ILog<CartService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public CartService(ILog<CartService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IUserSessionStorageService userSessionStorageService
            , IAdminContext adminContext
            , IAuthenticationContext authenticationContext
            , CartFactory cartFactory
            , CartItemFactory cartItemFactory
            , CachedDiscountFactory cachedDiscountFactory
            , AutomatedCartFactory automatedCartFactory
            , IQueryable<Cart> carts
            , InstitutionResourceAuditFactory institutionResourceAuditFactory
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _userSessionStorageService = userSessionStorageService;
            _adminContext = adminContext;
            _authenticationContext = authenticationContext;
            _cartFactory = cartFactory;
            _cartItemFactory = cartItemFactory;
            _cachedDiscountFactory = cachedDiscountFactory;
            _automatedCartFactory = automatedCartFactory;
            _carts = carts;
            _institutionResourceAuditFactory = institutionResourceAuditFactory;
        }

        public CachedCart GetInstitutionCartFromCache(int institutionId, int cartId = 0)
        {
            if (institutionId >= int.MaxValue - 100)
            {
                return null;
            }

            var cachedCart = _userSessionStorageService.Get<CachedCart>(ActiveCartKey);
            if (cachedCart == null || cachedCart.InstitutionId != institutionId || ForceCartReload() ||
                (cartId > 0 && cartId != cachedCart.Id))
            {
                cachedCart = GetInstitutionCartFromDatabase(institutionId, cartId);
            }

            return cachedCart;
        }

        public CachedCart GetInstitutionCartFromDatabase(int institutionId, int cartId = 0)
        {
            var updatedCart = GetUpdatedInstitutionCart(institutionId, cartId);
            if (updatedCart != null)
            {
                var cachedCart = _cartFactory.ConvertToCachedCart(institutionId, updatedCart);

                if (_userSessionStorageService.Has(ActiveCartKey))
                {
                    _userSessionStorageService.Remove(ActiveCartKey);
                }

                if (_userSessionStorageService.Has(ActiveCartDateCachedKey))
                {
                    _userSessionStorageService.Remove(ActiveCartDateCachedKey);
                }

                _userSessionStorageService.Put(ActiveCartKey, cachedCart);
                _userSessionStorageService.Put(ActiveCartDateCachedKey, DateTime.Now);

                return cachedCart;
            }

            return null;
        }

        public List<CachedCart> GetAllInstitutionCartsFromCache(int institutionId)
        {
            var carts = _userSessionStorageService.Get<List<CachedCart>>(AllCartsKey);
            if (carts == null || carts.Any(x => x.InstitutionId != institutionId) || ForceCartReload())
            {
                var currentCart = GetInstitutionCartFromCache(institutionId);
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var dbCarts = _cartFactory.GetAllInstitutionCarts(institutionId).ToList();

                        var availablePromotions = _cartFactory.GetAvailablePromotions();
                        var appliedPromotionCodes = _cartFactory.GetAppliedPromotionCodes(institutionId);

                        var promotions = availablePromotions as IList<Promotion> ?? availablePromotions.ToList();
                        var institution = _adminContext.GetAdminInstitution(institutionId);

                        foreach (var cart in dbCarts)
                        {
                            _cartItemFactory.UpdateCartItems(cart, institution);
                            foreach (var cartItem in cart.CartItems)
                            {
                                uow.SaveOrUpdate(cartItem);
                            }

                            uow.Update(cart);
                        }

                        uow.Commit();
                        transaction.Commit();

                        uow.Evict(dbCarts);

                        carts = dbCarts.Select(x => new CachedCart(x, promotions, appliedPromotionCodes)).ToList();

                        _userSessionStorageService.Put(AllCartsKey, carts);
                        _userSessionStorageService.Put(SavedCartsDateCachedKey, DateTime.Now);

                        if (_userSessionStorageService.Has(ActiveCartKey))
                        {
                            _userSessionStorageService.Remove(ActiveCartKey);
                        }

                        if (_userSessionStorageService.Has(ActiveCartDateCachedKey))
                        {
                            _userSessionStorageService.Remove(ActiveCartDateCachedKey);
                        }

                        _userSessionStorageService.Put(ActiveCartKey, currentCart);
                        _userSessionStorageService.Put(ActiveCartDateCachedKey, DateTime.Now);
                    }
                }
            }

            return carts;
        }

        public bool SaveCart(int cartId, int institutionId, string cartName)
        {
            var saved = _cartFactory.SaveCart(cartId, institutionId, cartName);

            _userSessionStorageService.Remove(ActiveCartKey);
            _userSessionStorageService.Remove(AllCartsKey);
            return saved;
        }

        public int CopyCart(int cartId, int institutionId, string cartName)
        {
            //Get Cart that will be copied
            var originalCart = GetUpdatedInstitutionCart(institutionId, cartId);
            if (originalCart == null)
            {
                return 0;
            }

            //Save new cart
            var newCart = new Cart
            {
                InstitutionId = institutionId,
                CartType = CartTypeEnum.Saved,
                CartName = cartName,
                ConvertDate = DateTime.Now
            };

            _cartFactory.SaveCart(newCart);

            //Copy Cart Items into new cart
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var institution = _adminContext.GetAdminInstitution(institutionId);
                    _cartItemFactory.CopyCartItemsIntoNewCart(newCart, originalCart, institution, uow);

                    uow.Commit();
                    transaction.Commit();
                }
            }

            _userSessionStorageService.Remove(ActiveCartKey);
            _userSessionStorageService.Remove(AllCartsKey);
            return newCart.Id;
        }

        public int MergeCarts(int currentCartId, int cartToMergeIntoId, int institutionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var sourceCart = _carts.FirstOrDefault(x => x.Id == currentCartId);
                    var destinationCart = _carts.FirstOrDefault(x => x.Id == cartToMergeIntoId);

                    if (sourceCart == null || destinationCart == null)
                    {
                        return 0;
                    }

                    //Should never happen, but just a check
                    if (sourceCart.InstitutionId != destinationCart.InstitutionId ||
                        institutionId != sourceCart.InstitutionId)
                    {
                        _log.Error("Attempted to Merge 2 carts that have different institutionIds");
                        return 0;
                    }

                    var isSourceCartAutomatedCart = sourceCart.CartType == CartTypeEnum.AutomatedCart;
                    var isDestinationCartAutomatedCart = destinationCart.CartType == CartTypeEnum.AutomatedCart;

                    CartItem lastSourceCartItem = null;
                    CartItem lastDestinationCartItem = null;
                    var switchCarts = false;
                    if (isSourceCartAutomatedCart || isDestinationCartAutomatedCart)
                    {
                        switchCarts = _automatedCartFactory.IsSourceCartHigherDiscount(sourceCart, destinationCart);
                    }

                    var newSourceCart = switchCarts ? destinationCart : sourceCart;
                    var newDestinationCart = switchCarts ? sourceCart : destinationCart;
                    try
                    {
                        foreach (var item in newSourceCart.CartItems.Where(x => x.ResourceId.HasValue))
                        {
                            var destinationCartItem =
                                newDestinationCart.CartItems.FirstOrDefault(x => x.ResourceId == item.ResourceId);
                            if (destinationCartItem != null)
                            {
                                destinationCartItem.NumberOfLicenses += item.NumberOfLicenses;
                            }
                            else
                            {
                                destinationCartItem = new CartItem
                                {
                                    Cart = newDestinationCart,
                                    NumberOfLicenses = item.NumberOfLicenses,
                                    ListPrice = item.ListPrice,
                                    ResourceId = item.ResourceId.GetValueOrDefault(),
                                    Include = item.Include,
                                    OriginalSourceId = item.OriginalSourceId
                                };
                            }

                            var audit =
                                _institutionResourceAuditFactory.BuildAuditRecord(
                                    InstitutionResourceAuditType.ResourceAddedToCart,
                                    destinationCart.InstitutionId,
                                    item.ResourceId.GetValueOrDefault(), item.NumberOfLicenses,
                                    null, destinationCartItem.DiscountPrice, -1);

                            lastSourceCartItem = item;
                            lastDestinationCartItem = destinationCartItem;

                            uow.SaveOrUpdate(destinationCartItem);
                            uow.Save(audit);
                        }

                        if (switchCarts && newSourceCart.CartType != CartTypeEnum.Active)
                        {
                            newDestinationCart.CartName = newSourceCart.CartName;
                            uow.Update(newDestinationCart);
                        }

                        uow.Commit();
                        transaction.Commit();

                        uow.Evict(sourceCart);
                        uow.Evict(destinationCart);
                        uow.Evict(newSourceCart);
                        uow.Evict(newDestinationCart);

                        return newDestinationCart.Id;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"{ex.Message}\r\nSource: {lastSourceCartItem?.ToDebugString()} \r\n Destination: {lastDestinationCartItem?.ToDebugString()}";
                        _log.Error(errorMessage, ex);
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (!transaction.WasRolledBack)
                        {
                            _userSessionStorageService.Remove(ActiveCartKey);
                            _userSessionStorageService.Remove(AllCartsKey);

                            DeleteCart(newSourceCart.InstitutionId, newSourceCart.Id);
                        }
                    }
                }
            }

            return 0;
        }

        public bool AddItemToCart(int institutionId, int resourceId, int numberOfLicenses,
            LicenseOriginalSource originalSource, int cartId, bool is3BundlePurchase = false)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        if (numberOfLicenses <= 0 && !is3BundlePurchase)
                        {
                            return false;
                        }

                        var cart = _cartFactory.GetDatabaseCart(institutionId, cartId);
                        if (cart == null)
                        {
                            cart = new Cart
                            {
                                InstitutionId = institutionId
                            };

                            uow.Save(cart);
                        }

                        var institution = _adminContext.GetAdminInstitution(institutionId);
                        _cartItemFactory.AddItemToCart(cart, resourceId, numberOfLicenses, originalSource, institution,
                            uow, is3BundlePurchase);
                        uow.Commit();
                        transaction.Commit();

                        uow.Evict(cart.CartItems);
                        uow.Evict(cart);


                        // force order to be reloaded
                        _userSessionStorageService.Remove(ActiveCartKey);
                        _userSessionStorageService.Remove(AllCartsKey);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public bool AddBulkItemsToCart(int institutionId, IEnumerable<BulkAddResource> bulkAddResources)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var institution = _adminContext.GetAdminInstitution(institutionId);
                        var cachedCart = GetInstitutionCartFromCache(institution.Id);

                        var cart = _cartFactory.GetDatabaseCart(institutionId, 0);

                        if (cart == null)
                        {
                            cart = new Cart
                            {
                                InstitutionId = institution.Id
                            };

                            uow.Save(cart);
                        }

                        if (cart.Id != cachedCart.Id)
                        {
                            var dbCart = _cartFactory.GetDatabaseCart(institution.Id, cachedCart.Id);
                            if (dbCart != null)
                            {
                                cart = dbCart;
                            }
                        }

                        _cartItemFactory.AddBulkItemsToCart(cart, bulkAddResources, institution, uow);

                        uow.Commit();
                        transaction.Commit();

                        var cartId = cart.Id;

                        uow.Evict(cart);

                        // force order to be reloaded
                        _userSessionStorageService.Remove(ActiveCartKey);
                        _userSessionStorageService.Remove(AllCartsKey);

                        GetInstitutionCartFromCache(institutionId, cartId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public void RemoveCartsFromCache()
        {
            _userSessionStorageService.Remove(ActiveCartKey);
            _userSessionStorageService.Remove(AllCartsKey);
            _userSessionStorageService.Remove(ActiveCartDateCachedKey);
            _userSessionStorageService.Remove(SavedCartsDateCachedKey);
        }

        public bool UpdateItemInCart(int institutionId, int itemId, int numberOfLicenses, int cartId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var institution = _adminContext.GetAdminInstitution(institutionId);
                        var cart = _cartFactory.GetDatabaseCart(institution.Id, cartId);

                        if (cart != null)
                        {
                            _cartItemFactory.UpdateCartItemLicenseInCart(cart, numberOfLicenses, itemId, institution,
                                uow);
                            uow.Commit();
                            transaction.Commit();

                            _userSessionStorageService.Remove(ActiveCartKey);

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public bool UpdateLicenseCountInCart(int institutionId, int resourceId, int newLicenseCount, int cartId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var institution = _adminContext.GetAdminInstitution(institutionId);

                        var cart = _cartFactory.GetDatabaseCart(institution.Id, 0);
                        if (cart.Id != cartId)
                        {
                            var dbCart = _cartFactory.GetDatabaseCart(institution.Id, cartId);
                            //Only switch carts if the cached cart has not been processed.
                            if (dbCart != null)
                            {
                                cart = dbCart;
                            }
                        }

                        _cartItemFactory.UpdateResourceLicenseInCart(cart, newLicenseCount, resourceId, institution,
                            uow);

                        uow.Commit();
                        transaction.Commit();

                        cartId = cart.Id;

                        uow.Evict(cart);

                        // force order to be reloaded
                        _userSessionStorageService.Remove(ActiveCartKey);
                        _userSessionStorageService.Remove(AllCartsKey);

                        GetInstitutionCartFromCache(institutionId, cartId);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }


        public bool UpdateFreeResourceLicenseCountInCart(int resourceId, bool isFreeResource)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        _cartItemFactory.UpdateFreeResourceLicensesInAllCarts(resourceId, isFreeResource, uow);

                        uow.Commit();
                        transaction.Commit();

                        _userSessionStorageService.Remove(ActiveCartKey);
                        _userSessionStorageService.Remove(AllCartsKey);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public bool UpdateItemInCart(int institutionId, IProductOrderItem productOrderItem)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var institution = _adminContext.GetAdminInstitution(institutionId);

                        var cachedCart = GetInstitutionCartFromCache(institutionId);

                        var cart = _cartFactory.GetDatabaseCart(institution.Id, 0);
                        if (cart == null)
                        {
                            return false;
                        }

                        if (cart.Id != cachedCart.Id)
                        {
                            var dbCart = _cartFactory.GetDatabaseCart(institution.Id, cachedCart.Id);
                            //Only switch carts if the cached cart has not been processed.
                            if (dbCart != null)
                            {
                                cart = dbCart;
                            }
                        }

                        _cartItemFactory.UpdateProductInCart(cart, productOrderItem, uow);

                        uow.Commit();
                        transaction.Commit();

                        var cartId = cart.Id;

                        uow.Evict(cart);

                        // force order to be reloaded
                        _userSessionStorageService.Remove(ActiveCartKey);
                        _userSessionStorageService.Remove(AllCartsKey);

                        GetInstitutionCartFromCache(institutionId, cartId);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public bool RemoveItemFromCart(int institutionId, int itemId, int cartId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var cart = _cartFactory.GetDatabaseCart(institutionId, cartId);
                        if (cart == null)
                        {
                            return false;
                        }

                        if (cart.CartItems.Any(x => x.Id == itemId))
                        {
                            _cartItemFactory.RemoveItemFromCart(cart, itemId, institutionId, uow);

                            uow.Commit();
                            transaction.Commit();
                        }

                        // force order to be reloaded
                        _userSessionStorageService.Remove(ActiveCartKey);
                        _userSessionStorageService.Remove(AllCartsKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Improve performance by removed resources from a cart in one simple sql update statement
        /// </summary>
        public bool RemoveAllResourcesFromCart(int institutionId, int cartId, CartTypeEnum cartType)
        {
            if (cartType == CartTypeEnum.Saved || cartType == CartTypeEnum.AutomatedCart)
            {
                return DeleteCart(institutionId, cartId);
            }

            bool success;
            using (var uow = _unitOfWorkProvider.Start())
            {
                success = _cartItemFactory.RemoveAllResourcesFromCart(institutionId, cartId, cartType,
                    _authenticationContext.AuthenticatedInstitution.AuditId, uow);
            }

            _userSessionStorageService.Remove(ActiveCartKey);
            _userSessionStorageService.Remove(AllCartsKey);
            return success;
        }

        public bool RemoveResourcesFromCart(int institutionId, int cartId, int[] resourceIds)
        {
            bool success;
            using (var uow = _unitOfWorkProvider.Start())
            {
                success = _cartItemFactory.RemoveResourcesFromCart(institutionId, cartId, resourceIds,
                    _authenticationContext.AuthenticatedInstitution.AuditId, uow);
            }

            _userSessionStorageService.Remove(ActiveCartKey);
            _userSessionStorageService.Remove(AllCartsKey);
            return success;
        }

        public PromotionAction ApplyPromotion(int institutionId, CachedPromotion promotion)
        {
            var promotionAction = _cartFactory.ApplyPromotion(institutionId, promotion);
            if (promotionAction != PromotionAction.PromotionApplied)
            {
                return promotionAction;
            }

            var cachedCart = GetInstitutionCartFromCache(institutionId);
            if (cachedCart.CartType == CartTypeEnum.AutomatedCart)
            {
                if (cachedCart.Discount >= promotion.Discount)
                {
                    return PromotionAction.PromotionLowerThenCurrent;
                }
            }

            return UpdateCartPromotion(institutionId, promotion)
                ? PromotionAction.PromotionApplied
                : PromotionAction.PromotionError;
        }

        public PromotionAction RemovePromotion(int institutionId)
        {
            return UpdateCartPromotion(institutionId, null)
                ? PromotionAction.PromotionDeleted
                : PromotionAction.PromotionError;
        }

        public CheckoutResult Checkout(CheckoutRequest request, IUnitOfWork uow, IAdminInstitution adminInstitution)
        {
            //Gets cart and updates cart to with request data.
            var cart = _cartFactory.GetUpdatedCartForCheckout(request);

            //Updates cart items to make sure they are priced properly
            _cartItemFactory.UpdateCartItems(cart, adminInstitution);

            //Sets Annual Maintenace Fee
            var institution = _cartFactory.SetAnnualFeeIfNeeded(cart);

            //Saves Annual Maintenace Fee
            uow.SaveOrUpdate(institution);

            //Add/Update Liceses
            _cartItemFactory.CheckOutCartItems(cart, uow);

            //Saves cart and cart items
            uow.SaveOrUpdate(cart);

            return new CheckoutResult(true, cart);
        }


        public bool DeleteResourceFromAllCarts(int resourceId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var success = _cartItemFactory.DeleteResourceFromAllCarts(resourceId, uow);
                        uow.Commit();
                        transaction.Commit();
                        return success;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public List<Reseller> GetResellers()
        {
            return _cartFactory.GetAllResellers();
        }

        public CachedCart CreateResellerCart(int institutionId, int resellerId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var newCart = _cartFactory.CreateResellerCart(institutionId, resellerId);
                        if (newCart != null)
                        {
                            uow.Save(newCart);
                            uow.Commit();
                            transaction.Commit();

                            _userSessionStorageService.Remove(ActiveCartKey);
                            _userSessionStorageService.Remove(AllCartsKey);

                            return GetInstitutionCartFromCache(institutionId, newCart.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        private Cart GetUpdatedInstitutionCart(int institutionId, int cartId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);

            var cart = _cartFactory.GetInstitutionCart(institutionId, cartId);

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    if (cart != null)
                    {
                        _cartItemFactory.UpdateCartItems(cart, institution);

                        //Evicts Cart from Session
                        //Should address an issue when occasionaly two users are working the same cart.
                        var sessionCart = uow.Session.Load<Cart>(cart.Id);
                        if (sessionCart != null && sessionCart.Id == cart.Id)
                        {
                            uow.Session.Evict(sessionCart);
                        }

                        uow.SaveOrUpdate(cart);
                        uow.Commit();
                        transaction.Commit();
                        uow.Evict(cart);
                        _userSessionStorageService.Remove(ActiveCartKey);
                    }

                    return cart;
                }
            }
        }


        private bool DeleteCart(int institutionId, int cartId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    _cartFactory.DeleteCart(institutionId, cartId, uow);

                    uow.Commit();
                    transaction.Commit();

                    //Remove cached cart: If active cart blow away or blow away saved carts
                    var activeCart = GetInstitutionCartFromCache(institutionId);
                    if (activeCart.Id == cartId)
                    {
                        _userSessionStorageService.Remove(ActiveCartKey);
                    }

                    _userSessionStorageService.Remove(AllCartsKey);

                    return true;
                }
            }
        }


        public bool UpdateCartPromotion(int institutionId, CachedPromotion promotion)
        {
            try
            {
                var cachedCart = GetInstitutionCartFromCache(institutionId);
                Cart cart;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        cart = _cartFactory.UpdateCartPromotion(institutionId, cachedCart, promotion, uow);
                        uow.Commit();
                        transaction.Commit();
                    }
                }

                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var institution = _adminContext.GetAdminInstitution(institutionId);
                        _cartItemFactory.UpdateCartItems(cart, institution);

                        uow.Update(cart);
                        uow.Commit();
                        transaction.Commit();

                        uow.Evict(cart);
                    }
                }

                _userSessionStorageService.Remove(ActiveCartKey);
                _userSessionStorageService.Remove(AllCartsKey);

                GetInstitutionCartFromCache(institutionId, cart.Id);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public bool ForceCartReload()
        {
            if (_userSessionStorageService.Has(ActiveCartDateCachedKey))
            {
                var dateTimeCached = _userSessionStorageService.Get<DateTime>(ActiveCartDateCachedKey);
                return _cachedDiscountFactory.HaveDiscountsChanged(dateTimeCached);
            }

            return false;
        }
    }
}