#region

using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.ReserveShelf;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Order;
using R2V2.Web.Areas.Admin.Models.ReserveShelfManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public interface IOrderService
    {
        Order GetOrder(int orderId, IAdminInstitution adminInstitution);
        //Order GetOrderHistory(int orderId, IAdminInstitution adminInstitution);

        Order GetOrderForInstitution(int institutionId, int cartId = 0);

        Order GetOrderFromDatabaseForInstitution(int institutionId, int cartId = 0);
        IEnumerable<IOrder> GetOrdersForInstitution(IAdminInstitution adminInstitution);

        bool AddItemToOrder(CollectionAdd collectionAdd);
        bool AddBulkItemsToOrder(int institutionId, IEnumerable<InstitutionResource> institutionResources);
        bool UpdateOrderItem(int institutionId, ResourceOrderItem resourceOrderItem, int cartId);
        bool UpdateOrderItem(int institutionId, IProductOrderItem productOrderItem);
        bool RemoveItemFromOrder(int institutionId, int itemId, int cartId);
        bool RemoveAllResourcesFromOrder(int institutionId, int cartId);

        bool RemoveResourcesFromOrder(int institutionId, int cartId, int[] resourceIds);

        int MergeCarts(int currentCartId, int mergeIntoCartId, int institutionId);
        int CopyCart(int cartId, int institutionId, string cartName);

        IEnumerable<IProductOrderItem> GetProductsRequiringAgreements(Order order);
        bool AgreeToProductLicense(Order order, int productId);

        bool PlaceOrder(Order order, IUser currentUser, bool sendNewAccountEmail);
        int PlaceOrder(SubscriptionOrderHistory order, IUser currentUser);

        ShoppingCartExcelExport GetShoppingCartExcelExport(int institutionId, string bookPrefixUrl,
            string bookSuffixUrl, string bookUrl);

        bool SaveCart(int cartId, int institutionId, string cartName);


        InstitutionResource GetInstitutionResource(int institutionId, int resourceId, int cartId);
        InstitutionResource GetInstitutionResource(int institutionId, string isbn, int cartId);
        InstitutionResources GetInstitutionResources(CollectionManagementQuery collectionManagementQuery, IUser user);

        IEnumerable<InstitutionResource> GetInstitutionResources(CollectionManagementQuery collectionManagementQuery,
            out List<string> isbnsNotFound);

        IEnumerable<InstitutionResource> GetInstitutionResourcesWithoutDatabase(
            CollectionManagementQuery collectionManagementQuery, out List<string> isbnsNotFound);


        ReserveShelfManagement GetReserveShelfResources(ReserveShelfQuery reserveShelfQuery);

        List<CollectionManagementResource> GetCollectionManagementResources(
            CollectionManagementQuery collectionManagementQuery, bool isExpertReviewer);

        bool UpdateLicenseCount(CollectionEdit collectionEdit);

        PromotionAction ApplyPromotion(int institutionId, CachedPromotion promotion);
        PromotionAction RemovePromotion(int institutionId);

        bool AddItemToOrder2(CollectionAdd collectionAdd);
    }
}