#region

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CartFactory
    {
        private readonly IAdminContext _adminContext;
        private readonly AutomatedCartFactory _automatedCartFactory;
        private readonly IQueryable<Cart> _cart;
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly InstitutionService _institutionService;
        private readonly ILog<CartService> _log;
        private readonly IQueryable<Promotion> _promotions;
        private readonly IQueryable<Reseller> _resellers;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;


        public CartFactory(
            ILog<CartService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<Cart> cart
            , IQueryable<Promotion> promotions
            , IAdminContext adminContext
            , ICollectionManagementSettings collectionManagementSettings
            , InstitutionService institutionService
            , IQueryable<Reseller> resellers
            , AutomatedCartFactory automatedCartFactory
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _cart = cart;
            _promotions = promotions;
            _adminContext = adminContext;
            _collectionManagementSettings = collectionManagementSettings;
            _institutionService = institutionService;
            _resellers = resellers;
            _automatedCartFactory = automatedCartFactory;
        }

        public Cart CreateResellerCart(int institutionId, int resellerId)
        {
            var reseller = _resellers.FirstOrDefault(x => x.Id == resellerId);
            if (reseller != null)
            {
                var newCart = new Cart
                {
                    CartType = CartTypeEnum.Saved,
                    InstitutionId = institutionId,
                    Reseller = reseller,
                    Discount = reseller.Discount,
                    CartName = $"{reseller.Name}_{DateTime.Now:yyyyMMddhhmmss} - {reseller.Discount}% Discount ",
                    BillingMethod = BillingMethodEnum.Reseller
                };
                return newCart;
            }

            return null;
        }

        public List<Reseller> GetAllResellers()
        {
            return _resellers.Where(x => x.RecordStatus).ToList();
        }

        /// <summary>
        ///     Only used by the ResourceDiscountService
        /// </summary>
        public Cart GetDatabaseCartOnlyNoItems(int institutionId, int cartId)
        {
            Cart cart;
            if (cartId == 0)
            {
                cart =
                    _cart.SingleOrDefault(x =>
                        x.InstitutionId == institutionId && !x.Processed && x.CartType == CartTypeEnum.Active);
            }
            else
            {
                cart = _cart.SingleOrDefault(x => x.InstitutionId == institutionId && x.Id == cartId);
            }

            return cart ?? new Cart { InstitutionId = institutionId, CartType = CartTypeEnum.Active };
            ;
        }

        /// <summary>
        ///     Returns Database cart if cartid > 0 or returns default cart
        /// </summary>
        public Cart GetDatabaseCart(int institutionId, int cartId)
        {
            return cartId == 0
                ? _cart.FetchMany(x => x.CartItems).SingleOrDefault(x =>
                    x.InstitutionId == institutionId && !x.Processed && x.CartType == CartTypeEnum.Active)
                //No need to make sure the order is not processed when you have a cart id.
                : _cart.FetchMany(x => x.CartItems)
                    .SingleOrDefault(x => x.InstitutionId == institutionId && x.Id == cartId);
        }

        /// <summary>
        ///     Need to Save Cart then update items
        /// </summary>
        public Cart GetInstitutionCart(int institutionId, int cartId)
        {
            if (institutionId > 0)
            {
                var cart = GetDatabaseCart(institutionId, cartId) ?? GetInstitutionsActiveCart(institutionId);
                UpdateCart(cart);
                return cart;
            }

            return null;
        }

        public Cart GetInstitutionsActiveCart(int institutionId)
        {
            var carts = _cart.FetchMany(x => x.CartItems)
                .Where(x => x.InstitutionId == institutionId && !x.Processed && x.CartType == CartTypeEnum.Active)
                .OrderBy(x => x.Id)
                .ToArray();
            if (carts.Length == 0)
            {
                return new Cart { InstitutionId = institutionId, CartType = CartTypeEnum.Active };
            }

            if (carts.Length == 1)
            {
                return carts[0];
            }

            // this is not good, but we don't want to prevent the user from getting a cart
            _log.ErrorFormat("Two or more active carts exist for institutionId: {0}", institutionId);
            return carts[0];
        }

        public IEnumerable<Cart> GetAllInstitutionCarts(int institutionId)
        {
            return _cart.FetchMany(x => x.CartItems).Where(x => x.InstitutionId == institutionId && !x.Processed);
        }

        public CachedCart ConvertToCachedCart(int institutionId, Cart dbCart)
        {
            var availablePromotions = GetAvailablePromotions();
            var appliedPromotionCodes = GetAppliedPromotionCodes(institutionId);

            // convert dbCart to cachedCart
            var cart = new CachedCart(dbCart, availablePromotions, appliedPromotionCodes);

            if (cart.CartType == CartTypeEnum.AutomatedCart)
            {
                _automatedCartFactory.PopulateAutomatedCartReasonCodes(cart);
            }

            return cart;
        }

        /// <summary>
        ///     Cart Items have not been updated yet
        /// </summary>
        private void UpdateCart(Cart cart)
        {
            var institution = _adminContext.GetAdminInstitution(cart.InstitutionId);
            if (cart.CartType == CartTypeEnum.AutomatedCart)
            {
                var discount = _automatedCartFactory.GetAutomatedCartDiscount(cart.Id, cart.InstitutionId);

                cart.Discount = discount > institution.Discount ? discount : institution.Discount;
            }
            else
            {
                cart.Discount = cart.Reseller?.Discount ?? institution.Discount;
            }

            // if a promotion is applied to the cart, validate that the promotion is still valid
            if (!string.IsNullOrWhiteSpace(cart.PromotionCode) && !cart.Processed)
            {
                var availablePromotons = GetAvailablePromotions();
                var promotion = availablePromotons.FirstOrDefault(x => x.Code == cart.PromotionCode);
                if (promotion == null)
                {
                    _log.WarnFormat(
                        "Promotion was removed from cart because it is no longer a valid promotion - promotion code: {0}, {1}",
                        cart.PromotionCode, cart.ToDebugString());
                    cart.PromotionCode = null;
                    cart.PromotionDiscount = 0.0m;
                }
                else
                {
                    // always update the promotion discount just in case the promotion is changed
                    cart.PromotionDiscount = promotion.Discount;
                }
            }
        }

        public bool SaveCart(int cartId, int institutionId, string cartName)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var cart = _cart.FirstOrDefault(x => x.InstitutionId == institutionId && x.Id == cartId);
                    if (cart != null)
                    {
                        if (cart.CartType == CartTypeEnum.Active)
                        {
                            var newDefaultCart = new Cart
                                { InstitutionId = institutionId, CartType = CartTypeEnum.Active };
                            uow.Save(newDefaultCart);
                        }

                        cart.ConvertDate = DateTime.Now;
                        if (cart.CartType == CartTypeEnum.Active)
                        {
                            cart.CartType = CartTypeEnum.Saved;
                        }

                        cart.CartName = cartName;

                        uow.Save(cart);
                        uow.Commit();
                        transaction.Commit();

                        uow.Evict(cart);

                        return true;
                    }
                }
            }

            return false;
        }

        public bool SaveCart(Cart cart)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    if (cart != null)
                    {
                        uow.SaveOrUpdate(cart);
                        uow.Commit();
                        transaction.Commit();

                        uow.Evict(cart);


                        return true;
                    }
                }
            }

            return false;
        }


        public void DeleteCart(int institutionId, int cartId, IUnitOfWork uow)
        {
            var cart = _cart.SingleOrDefault(x => x.InstitutionId == institutionId && x.Id == cartId);

            if (cart == null)
            {
                return;
            }

            uow.Delete(cart);
        }

        public PromotionAction ApplyPromotion(int institutionId, CachedPromotion promotion)
        {
            if (promotion == null)
            {
                return PromotionAction.PromotionNotFound;
            }

            if (promotion.EndDate < DateTime.Now)
            {
                return PromotionAction.PromotionExpired;
            }

            if (promotion.StartDate > DateTime.Now)
            {
                return PromotionAction.PromotionNotActive;
            }

            var promotionCarts = _cart.Where(x =>
                x.PromotionCode == promotion.Code && x.Processed && x.InstitutionId == institutionId);

            if (promotion.MaximumUses > promotionCarts.Count())
            {
                return PromotionAction.PromotionApplied;
            }

            foreach (var cart in promotionCarts)
            {
                _log.WarnFormat(
                    "ApplyPromotion() - Promotion previously applied, cartId: {0}, purchase date: {1}, promotion code: {2}",
                    cart.Id,
                    cart.PurchaseDate, cart.PromotionCode);
            }

            return PromotionAction.PromotionPreviouslyApplied;
        }

        public Cart GetUpdatedCartForCheckout(CheckoutRequest checkoutRequest)
        {
            var cart = _cart
                .FetchMany(x => x.CartItems)
                .SingleOrDefault(x => x.Id == checkoutRequest.CartId);
            if (cart != null)
            {
                cart.PurchaseOrderNumber = checkoutRequest.PurchaseOrderNumber;
                cart.PurchaseOrderComment = checkoutRequest.PurchaseOrderComment;
                cart.PurchaseDate = checkoutRequest.PurchaseDate;
                cart.BillingMethod = checkoutRequest.BillingMethod;
                cart.ForthcomingTitlesInvoicingMethod = checkoutRequest.ForthcomingTitlesInvoicingMethod;
                cart.Processed = checkoutRequest.Processed;
                cart.OrderNumber = checkoutRequest.OrderNumber;

                UpdateCart(cart);
            }

            return cart;
        }

        public IInstitution SetAnnualFeeIfNeeded(Cart c)
        {
            var institution = _institutionService.GetInstitutionForEdit(c.InstitutionId);
            institution.AccountStatusId = (int)AccountStatus.Active;

            if (institution.AnnualFee == null)
            {
                institution.AnnualFee = new AnnualFee { FeeDate = c.PurchaseDate };
            }

            return institution;
        }

        public Cart UpdateCartPromotion(int institutionId, CachedCart cachedCart, CachedPromotion promotion,
            IUnitOfWork uow)
        {
            var cart =
                _cart.FetchMany(x => x.CartItems)
                    .SingleOrDefault(x =>
                        x.InstitutionId == institutionId && !x.Processed && x.CartType == CartTypeEnum.Active);

            if (cart == null)
            {
                return null;
            }

            //If the cached cart is not the same as the default cart switch carts.
            if (cachedCart != null && cart.Id != cachedCart.Id)
            {
                var dbCart =
                    _cart.FetchMany(x => x.CartItems)
                        .SingleOrDefault(x =>
                            x.InstitutionId == institutionId && !x.Processed && x.Id == cachedCart.Id);
                //Only switch carts if the cached cart has not been processed.
                if (dbCart != null)
                {
                    cart = dbCart;
                }
            }

            if (promotion == null)
            {
                cart.PromotionCode = null;
                cart.PromotionDiscount = 0.0m;
                if (cart.CartType == CartTypeEnum.AutomatedCart)
                {
                    var automatedCartDiscount = _automatedCartFactory.GetAutomatedCartDiscount(cart.Id, institutionId);
                    cart.Discount = automatedCartDiscount;
                }
            }
            else
            {
                cart.PromotionCode = promotion.Code;
                cart.PromotionDiscount = promotion.Discount;
            }

            uow.SaveOrUpdate(cart);
            return cart;
        }


        public IList<string> GetAppliesPromotionCodes(int institutionId)
        {
            var codes = from c in _cart
                where c.PromotionCode != null && c.Processed && c.InstitutionId == institutionId
                select c.PromotionCode;
            return codes.ToList();
        }

        public IList<Promotion> GetAppliedPromotionCodes(int institutionId)
        {
            var codes = from c in _cart
                join p in _promotions on c.PromotionCode equals p.Code
                where c.Processed && c.InstitutionId == institutionId
                select p;
            return codes.ToList();
        }

        public IEnumerable<Promotion> GetAvailablePromotions()
        {
            var promotions = from p in _promotions
                where p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now
                select p;

            return promotions;
        }

        public List<Cart> GetAutoCartsToDelete(int olderThanDays)
        {
            return _cart.Where(x =>
                x.CartType == CartTypeEnum.AutomatedCart && !x.Processed && !x.ConvertDate.HasValue &&
                x.CreationDate <= DateTime.Now.AddDays(-olderThanDays)).ToList();
        }

        public bool DeleteCarts(IEnumerable<Cart> carts)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        foreach (var cart in carts)
                        {
                            uow.Delete(cart);
                        }

                        uow.Commit();
                        transaction.Commit();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                        transaction.Rollback();
                    }
                }
            }

            return false;
        }
    }
}