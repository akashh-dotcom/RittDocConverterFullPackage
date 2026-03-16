#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class ResourceDiscountService
    {
        //public const string ActiveCartKey = "Active.Cart.NoItems";
        public const string ActiveCartKey = "Active.Cart";
        private readonly CartFactory _cartFactory;
        private readonly PdaPromotionFactory _pdaPromotionFactory;

        private readonly PromotionsFactory _promotionsFactory;
        private readonly SpecialDiscountResourceFactory _specialDiscountResourceFactory;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public ResourceDiscountService(
            PromotionsFactory promotionsFactory
            , PdaPromotionFactory pdaPromotionFactory
            , SpecialDiscountResourceFactory specialDiscountResourceFactory
            , CartFactory cartFactory
            , IUserSessionStorageService userSessionStorageService
        )
        {
            _promotionsFactory = promotionsFactory;
            _pdaPromotionFactory = pdaPromotionFactory;
            _specialDiscountResourceFactory = specialDiscountResourceFactory;
            _cartFactory = cartFactory;
            _userSessionStorageService = userSessionStorageService;
        }

        public void SetDiscount(IDiscountResource item, IAdminInstitution adminInstitution)
        {
            if (item.IsBundle)
            {
                item.DiscountPrice = item.BundlePrice;
                item.Discount = 0;
            }
            else
            {
                var cart = GetCart(adminInstitution.Id, item.CartId);
                if (cart != null)
                {
                    SetDiscount(item, adminInstitution, cart);
                }
            }
        }

        private void SetDiscount(IDiscountResource item, IAdminInstitution adminInstitution, CachedCart cart)
        {
            var promotionCode = cart.Promotion?.Code;
            decimal discount = 0;
            decimal discountPrice = 0;
            var specialText = "";
            var specialIconName = "";
            var pdaPromotionApplied = false;

            var resourceId = item.ResourceId.GetValueOrDefault(0);
            var productId = item.ProductId.GetValueOrDefault(0);

            int sourceId = item.OriginalSourceId;
            var listPrice = item.ListPrice;

            var promotion = promotionCode != null
                ? _promotionsFactory.GetCachedPromotions().FirstOrDefault(x => x.Code == promotionCode)
                : null;
            var pdaPromotion = resourceId > 0 && sourceId == (short)LicenseOriginalSource.Pda
                ? _pdaPromotionFactory.GetCachedPdaPromotions()
                    .FirstOrDefault(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                : null;
            var cachedSpecialResource =
                resourceId > 0 ? _specialDiscountResourceFactory.GetCachedSpecialResource(resourceId) : null;

            if (productId > 0)
            {
                var productIds = promotion?.PromotionProductIds;
                decimal promotionDiscount = promotion?.Discount ?? 0;
                if (productIds == null || !productIds.Any())
                {
                    discountPrice = listPrice;
                }
                else if (productIds.Contains(productId))
                {
                    discountPrice = listPrice - promotionDiscount / 100 * listPrice;
                    discount = promotionDiscount;
                }
            }

            var pdaPromotionId = 0;
            var specialDiscountId = 0;

            if (resourceId > 0 && cart.ResellerId == 0)
            {
                var discountDictionary = new Dictionary<DiscountType, decimal>();
                if (cachedSpecialResource != null)
                {
                    discountDictionary.Add(DiscountType.Special, cachedSpecialResource.DiscountPercentage);
                }

                if (pdaPromotion != null && sourceId == (short)LicenseOriginalSource.Pda)
                {
                    discountDictionary.Add(DiscountType.PdaPromotion, pdaPromotion.Discount);
                }

                if (promotion != null)
                {
                    discountDictionary.Add(DiscountType.Promotion, promotion.Discount);
                }

                if (cart.CartType == CartTypeEnum.AutomatedCart)
                {
                    discountDictionary.Add(DiscountType.AutomatedCart, cart.Discount);
                }

                discountDictionary.Add(DiscountType.Institution, adminInstitution?.Discount ?? 0);

                var highestDiscount = discountDictionary.OrderByDescending(x => x.Value).FirstOrDefault();
                if (item.IsBundle)
                {
                    discountPrice = item.BundlePrice - highestDiscount.Value / 100 * item.BundlePrice;
                }
                else
                {
                    discountPrice = listPrice - highestDiscount.Value / 100 * listPrice;
                }

                //discountPrice = listPrice - (highestDiscount.Value / 100) * listPrice;
                discount = highestDiscount.Value;

                switch (highestDiscount.Key)
                {
                    case DiscountType.Special:
                        // ReSharper disable once PossibleNullReferenceException
                        specialText = cachedSpecialResource.SpecialText();
                        specialIconName = cachedSpecialResource.IconName;
                        specialDiscountId = cachedSpecialResource.SpecialDiscountId;
                        break;
                    case DiscountType.PdaPromotion:
                        // ReSharper disable once PossibleNullReferenceException
                        specialText = pdaPromotion.PromotionText;
                        specialIconName = null;
                        pdaPromotionApplied = true;
                        pdaPromotionId = pdaPromotion.Id;
                        break;
                    case DiscountType.Institution:
                    case DiscountType.Promotion:
                    case DiscountType.AutomatedCart:
                    default:
                        specialText = null;
                        specialIconName = null;
                        break;
                }
            }
            else if (resourceId > 0)
            {
                if (item.IsBundle)
                {
                    discountPrice = item.BundlePrice - cart.ResellerDiscount / 100 * item.BundlePrice;
                }
                else
                {
                    discountPrice = listPrice - cart.ResellerDiscount / 100 * listPrice;
                }


                specialText = null;
                specialIconName = null;
                discount = cart.ResellerDiscount;
            }

            item.DiscountPrice = discountPrice;
            item.Discount = discount;
            item.SpecialText = specialText;
            item.SpecialIconName = specialIconName;
            item.PdaPromotionApplied = pdaPromotionApplied;
            item.PdaPromotionId = pdaPromotionId;
            item.SpecialDiscountId = specialDiscountId;
        }

        private CachedCart GetCart(int institutionId, int cartId)
        {
            var cart = _userSessionStorageService.Get<CachedCart>(ActiveCartKey);
            if (cart == null
                || cart.InstitutionId != institutionId //Different Institution Cart
                || (cartId > 0 && cart.Id != cartId) //Cart is not the one that is already cached
                || (cartId == 0 && cart.CartType != CartTypeEnum.Active)) // The cached Cart is not default cart
            {
                if (institutionId == 0)
                {
                    return null;
                }

                var dbCart = _cartFactory.GetDatabaseCartOnlyNoItems(institutionId, cartId);
                var availablePromotions = _cartFactory.GetAvailablePromotions();
                var appliedPromotionCodes = _cartFactory.GetAppliedPromotionCodes(institutionId);

                // convert dbCart to cachedCart
                cart = new CachedCart(dbCart, availablePromotions, appliedPromotionCodes);

                if (_userSessionStorageService.Has(ActiveCartKey))
                {
                    _userSessionStorageService.Remove(ActiveCartKey);
                }

                _userSessionStorageService.Put(ActiveCartKey, cart);
            }

            return cart;
        }

        public void ClearCart()
        {
            _userSessionStorageService.Remove(ActiveCartKey);
        }

        private enum DiscountType
        {
            Special = 1,
            PdaPromotion = 2,
            Promotion = 3,
            Institution = 4,
            AutomatedCart = 5
        }
    }
}