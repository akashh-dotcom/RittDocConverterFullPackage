#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.Institution;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface ICartService
    {
        List<Reseller> GetResellers();
        CachedCart CreateResellerCart(int institutionId, int resellerId);

        bool AddItemToCart(int institutionId, int resourceId, int numberOfLicenses,
            LicenseOriginalSource originalSource, int cartId, bool is3BundlePurchase = false);

        //bool Checkout(Cart cart, int userId);
        CheckoutResult Checkout(CheckoutRequest request, IUnitOfWork uow, IAdminInstitution adminInstitution);
        CachedCart GetInstitutionCartFromCache(int institutionId, int cartId = 0);
        CachedCart GetInstitutionCartFromDatabase(int institutionId, int cartId = 0);

        List<CachedCart> GetAllInstitutionCartsFromCache(int institutionId);
        bool UpdateItemInCart(int institutionId, int itemId, int numberOfLicenses, int cartId);
        bool UpdateItemInCart(int institutionId, IProductOrderItem productOrderItem);
        bool UpdateLicenseCountInCart(int institutionId, int resourceId, int newLicenseCount, int cartId);
        bool RemoveItemFromCart(int institutionId, int itemId, int cartId);

        bool RemoveResourcesFromCart(int institutionId, int cartId, int[] resourceIds);
        bool AddBulkItemsToCart(int institutionId, IEnumerable<BulkAddResource> bulkAddResources);
        void RemoveCartsFromCache();
        bool RemoveAllResourcesFromCart(int institutionId, int cartId, CartTypeEnum cartType);

        PromotionAction ApplyPromotion(int institutionId, CachedPromotion promotion);
        PromotionAction RemovePromotion(int institutionId);

        bool DeleteResourceFromAllCarts(int resourceId);

        bool SaveCart(int cartId, int institutionId, string cartName);

        int MergeCarts(int currentCartId, int cartToMergeIntoId, int institutionId);


        int CopyCart(int cartId, int institutionId, string cartName);

        bool UpdateFreeResourceLicenseCountInCart(int resourceId, bool isFreeResource);
    }
}