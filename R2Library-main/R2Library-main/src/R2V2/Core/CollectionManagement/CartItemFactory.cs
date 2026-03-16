#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CartItemFactory
    {
        private readonly IQueryable<CartItem> _cartItems;
        private readonly IQueryable<Cart> _carts;
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly InstitutionResourceAuditFactory _institutionResourceAuditFactory;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly IQueryable<Institution.Institution> _institutions;
        private readonly ILog<CartItemFactory> _log;
        private readonly IQueryable<Product> _products;
        private readonly ResourceDiscountService _resourceDiscountService;
        private readonly IResourceService _resourceService;

        public CartItemFactory(
            ILog<CartItemFactory> log
            , IResourceService resourceService
            , ResourceDiscountService resourceDiscountService
            , ICollectionManagementSettings collectionManagementSettings
            , IQueryable<Cart> carts
            , IQueryable<CartItem> cartItems
            , IQueryable<Product> products
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , InstitutionResourceAuditFactory institutionResourceAuditFactory
            , IQueryable<Institution.Institution> institutions
        )
        {
            _log = log;
            _resourceService = resourceService;
            _resourceDiscountService = resourceDiscountService;
            _collectionManagementSettings = collectionManagementSettings;
            _carts = carts;
            _cartItems = cartItems;
            _products = products;
            _institutionResourceLicenses = institutionResourceLicenses;
            _institutionResourceAuditFactory = institutionResourceAuditFactory;
            _institutions = institutions;
        }

        public void UpdateCartItems(Cart cart, IAdminInstitution institution)
        {
            var hasInstitutionSignedEula = HasInstitutionSignedEula(institution.Id);
            var annualMaintenanceFeeProductId = _collectionManagementSettings.AnnualMaintenanceFeeProductId;

            if (institution.AccountStatus != InstitutionAccountStatus.Active && !hasInstitutionSignedEula &&
                !cart.CartItems.Any(x => x.Product != null && x.Product.Id == annualMaintenanceFeeProductId))
            {
                var annualMaintenanceFee = _products.FirstOrDefault(x => x.Id == annualMaintenanceFeeProductId);
                if (annualMaintenanceFee != null)
                {
                    cart.AddProduct(annualMaintenanceFee);
                }
            }

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.ResourceId != null)
                {
                    var resource = _resourceService.GetResource(cartItem.ResourceId.Value);

                    if (resource != null)
                    {
                        //Zero out Archived, Inactive, and Not Saleable titles
                        if (
                            resource.StatusId == (int)ResourceStatus.Archived || resource.StatusId ==
                                                                              (int)ResourceStatus.Inactive
                                                                              ||
                                                                              resource.NotSaleable ||
                                                                              resource.NotSaleableDate != null
                        )
                        {
                            cartItem.ListPrice = 0;
                            cartItem.DiscountPrice = 0;
                            cartItem.NumberOfLicenses = 0;
                            cartItem.Include = false;
                        }
                        else
                        {
                            //If its free make sure there is no price
                            if (resource.IsFreeResource)
                            {
                                cartItem.ListPrice = 0;
                                cartItem.Include = true;
                            }
                            else
                            {
                                //If no liceses or an expired PDA resource remove it
                                if (cartItem.NumberOfLicenses == 0 ||
                                    (cartItem.OriginalSourceId == (short)LicenseOriginalSource.Pda &&
                                     cartItem.CreationDate.AddDays(30) < DateTime.Now &&
                                     cart.CartType != CartTypeEnum.Saved))
                                {
                                    cartItem.RecordStatus = false;
                                }
                                else
                                {
                                    //Update Price
                                    if (cartItem.IsBundle)
                                    {
                                        cartItem.ListPrice = resource.BundlePrice3.GetValueOrDefault();
                                    }
                                    else
                                    {
                                        cartItem.ListPrice = resource.ListPrice;
                                    }


                                    cartItem.Include = true;
                                }
                            }
                        }
                    }
                }

                if (cartItem.ProductId != null)
                {
                    if (cartItem.Product.Id == _collectionManagementSettings.AnnualMaintenanceFeeProductId)
                    {
                        if (hasInstitutionSignedEula)
                        {
                            cartItem.Include = false;
                            cartItem.RecordStatus = false;
                        }
                    }
                    else if (!cartItem.Product.Optional)
                    {
                        cartItem.Include = true;
                    }
                }


                _resourceDiscountService.SetDiscount(cartItem, institution);
            }
            //_resourceDiscountService.ClearCart();
        }

        public bool HasInstitutionPurchasedProduct(int productId, int institutionId)
        {
            try
            {
                var count = (from c in _carts
                        join ci in _cartItems on c.Id equals ci.Cart.Id
                        where ci.ProductId == productId && c.InstitutionId == institutionId && c.Processed
                        select c
                    ).Count();

                _log.DebugFormat("HasInstitutionPurchasedProduct(productId: {0}, institutionId: {1}) - count: {2}",
                    productId, institutionId, count);

                return count > 0;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("HasInstitutionPurchasedProduct(productId: {0}, institutionId: {1}) - EXCEPTION: {2}",
                    ex, productId, institutionId, ex.Message);
                return false;
            }
        }

        public bool HasInstitutionSignedEula(int institutionId)
        {
            try
            {
                var institution = _institutions.Fetch(y => y.AnnualFee).FirstOrDefault(x => x.Id == institutionId);
                return institution != null && institution.AnnualFee != null && institution.AnnualFee.HasAnnualFee;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public void CheckOutCartItems(Cart cart, IUnitOfWork uow)
        {
            foreach (var cartItem in cart.CartItems.Where(x =>
                         ((x.ResourceId != null && x.NumberOfLicenses > 0) || x.ProductId != null) && x.Include))
            {
                //Only Set a cartItem Purchase date if it is product. Will set it after the license is saved. 
                if (cartItem.ProductId != null)
                {
                    cartItem.PurchaseDate = cart.PurchaseDate;
                }


                var license = GetUpdatedInstitutionResourceLicenseForCheckout(cartItem, uow);
                if (license != null)
                {
                    var resource = _resourceService.GetResource(license.ResourceId);

                    if (resource != null)
                    {
                        uow.SaveOrUpdate(license);
                        cartItem.PurchaseDate = cart.PurchaseDate;
                        cartItem.InstitutionResourceLicenseId = license.Id;
                        try
                        {
                            var audit = _institutionResourceAuditFactory.BuildAuditRecord(
                                InstitutionResourceAuditType.PurchasedResource,
                                license.InstitutionId, license.ResourceId,
                                cartItem.NumberOfLicenses, cart.PurchaseOrderNumber,
                                cartItem.DiscountPrice,
                                license.LicenseCount);
                            uow.Save(audit);
                        }
                        catch (Exception ex)
                        {
                            var msg = new StringBuilder()
                                .AppendFormat("Error saving cart item to tInstitutionResourceAudit. Exception: {0}",
                                    ex.Message)
                                .AppendLine()
                                .Append(license.ToDebugString())
                                .ToString();
                            _log.Error(msg, ex);
                        }
                    }
                    else
                    {
                        uow.Delete(license);
                    }
                }
            }
        }

        private InstitutionResourceLicense GetUpdatedInstitutionResourceLicenseForCheckout(CartItem cartItem,
            IUnitOfWork uow)
        {
            uow.IncludeSoftDeletedValues();
            try
            {
                InstitutionResourceLicense institutionResourceLicense = null;

                var cart = cartItem.Cart;

                if (cartItem.ResourceId != null)
                {
                    institutionResourceLicense =
                        _institutionResourceLicenses.SingleOrDefault(x =>
                            x.InstitutionId == cart.InstitutionId && x.ResourceId == cartItem.ResourceId) ??
                        new InstitutionResourceLicense
                        {
                            Id = 0,
                            InstitutionId = cart.InstitutionId,
                            ResourceId = (int)cartItem.ResourceId,
                            LicenseCount = 0,
                            LicenseTypeId = (short)LicenseType.Purchased,
                            OriginalSourceId = (short)LicenseOriginalSource.FirmOrder,
                            FirstPurchaseDate = DateTime.Now,
                            PdaAddedDate = null,
                            PdaAddedToCartDate = null,
                            PdaAddedToCartById = null,
                            PdaViewCount = 0,
                            PdaMaxViews = 0,
                            RecordStatus = true,
                            AveragePrice = cartItem.DiscountPrice
                        };

                    if (!institutionResourceLicense.RecordStatus)
                    {
                        // handle soft deleted records
                        institutionResourceLicense.RecordStatus = true;
                        institutionResourceLicense.LicenseCount = 0;
                        institutionResourceLicense.FirstPurchaseDate = DateTime.Now;
                    }

                    var currentLicenseCount = institutionResourceLicense.LicenseCount;
                    var currentAveragePrice = institutionResourceLicense.AveragePrice;

                    institutionResourceLicense.LicenseCount += cartItem.NumberOfLicenses;
                    institutionResourceLicense.LicenseTypeId = (short)LicenseType.Purchased;
                    institutionResourceLicense.FirstPurchaseDate =
                        institutionResourceLicense.FirstPurchaseDate ?? DateTime.Now;

                    var newAveragePrice = cart.Reseller != null && cart.Reseller.Id > 0
                        ? cartItem.ListPrice
                        : cartItem.DiscountPrice;
                    if (currentLicenseCount != 0)
                    {
                        if (currentAveragePrice > 0)
                        {
                            newAveragePrice = (currentLicenseCount * (currentAveragePrice ?? 0.0m) +
                                               cartItem.NumberOfLicenses * cartItem.DiscountPrice) /
                                              institutionResourceLicense.LicenseCount;
                        }
                    }

                    institutionResourceLicense.AveragePrice = newAveragePrice;
                }

                return institutionResourceLicense;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                uow.ExcludeSoftDeletedValues();
            }
        }

        public bool DeleteResourceFromAllCarts(int resourceId, IUnitOfWork uow)
        {
            var cartItems = _cartItems.Where(x => x.ResourceId == resourceId);
            if (cartItems.Any())
            {
                foreach (var cartItem in cartItems)
                {
                    uow.Delete(cartItem);
                }
            }

            return true;
        }

        public void CopyCartItems(int originalCartId, Cart newCart, IAdminInstitution institution, IUnitOfWork uow)
        {
            var cartItems = _cartItems.Where(x => x.Cart.Id == originalCartId);

            foreach (var item in cartItems)
            {
                if (item.ResourceId.HasValue)
                {
                    var resource = _resourceService.GetResource(item.ResourceId.Value);

                    var cartItem = new CartItem
                    {
                        Cart = newCart,
                        NumberOfLicenses = item.NumberOfLicenses,
                        ListPrice = resource.ListPrice,
                        ResourceId = item.ResourceId.Value,
                        Include = true,
                        OriginalSourceId = item.OriginalSourceId
                    };

                    _resourceDiscountService.SetDiscount(cartItem, institution);


                    var audit =
                        _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.ResourceAddedToCart, institution.Id, item.ResourceId.Value,
                            item.NumberOfLicenses,
                            null, cartItem.DiscountPrice, -1);
                    uow.Save(cartItem);
                    uow.Save(audit);
                }
                else
                {
                    var product = _products.FirstOrDefault(x => x.Id == item.ProductId);
                    newCart.AddProduct(product);
                }
            }
        }

        public void CopyCartItemsIntoNewCart(Cart newCart, Cart originalCart, IAdminInstitution institution,
            IUnitOfWork uow)
        {
            foreach (var item in originalCart.CartItems.Where(x => x.ResourceId != null && x.ResourceId > 0))
            {
                var resource = _resourceService.GetResource(item.ResourceId.GetValueOrDefault());
                var cartItem = new CartItem
                {
                    Cart = newCart,
                    NumberOfLicenses = item.NumberOfLicenses,
                    ListPrice = resource.ListPrice,
                    ResourceId = item.ResourceId.GetValueOrDefault(),
                    Include = true,
                    OriginalSourceId = item.OriginalSourceId
                };
                _resourceDiscountService.SetDiscount(cartItem, institution);

                var audit =
                    _institutionResourceAuditFactory.BuildAuditRecord(
                        InstitutionResourceAuditType.ResourceAddedToCart, institution.Id,
                        item.ResourceId.GetValueOrDefault(), item.NumberOfLicenses,
                        null, cartItem.DiscountPrice, -1);

                uow.SaveOrUpdate(cartItem);
                uow.Save(audit);
            }
        }

        public bool MergeCartItems(Cart currentCart, Cart cartToMergeInto, IAdminInstitution institution,
            IUnitOfWork uow)
        {
            var lastItem = new CartItem();
            try
            {
                foreach (var currentCartItem in currentCart.CartItems.Where(x =>
                             x.ResourceId != null && x.ResourceId > 0))
                {
                    var resource = _resourceService.GetResource(currentCartItem.ResourceId.GetValueOrDefault());
                    var mergedCartItem = cartToMergeInto.CartItems.FirstOrDefault(x =>
                        x.ResourceId == currentCartItem.ResourceId.GetValueOrDefault());
                    lastItem = mergedCartItem;
                    if (mergedCartItem != null)
                    {
                        if (!resource.IsFreeResource)
                        {
                            mergedCartItem.NumberOfLicenses += currentCartItem.NumberOfLicenses;
                        }
                    }
                    else
                    {
                        mergedCartItem = new CartItem
                        {
                            Cart = cartToMergeInto,
                            NumberOfLicenses = currentCartItem.NumberOfLicenses,
                            ListPrice = resource.ListPrice,
                            ResourceId = currentCartItem.ResourceId.GetValueOrDefault(),
                            Include = currentCartItem.Include,
                            OriginalSourceId = currentCartItem.OriginalSourceId
                        };
                        _resourceDiscountService.SetDiscount(mergedCartItem, institution);
                    }

                    var audit =
                        _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.ResourceAddedToCart, institution.Id,
                            currentCartItem.ResourceId.GetValueOrDefault(), currentCartItem.NumberOfLicenses,
                            null, mergedCartItem.DiscountPrice, -1);

                    uow.Merge(mergedCartItem);
                    uow.Save(audit);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"{ex.Message}\r\n{lastItem?.ToDebugString()}";

                _log.Error(errorMessage, ex);
                return false;
            }

            return true;
        }

        public void AddItemToCart(Cart cart, int resourceId, int numberOfLicenses, LicenseOriginalSource originalSource,
            IAdminInstitution institution, IUnitOfWork uow, bool is3BundlePurchase = false)
        {
            var resource = _resourceService.GetResource(resourceId);
            var cartItem = new CartItem
            {
                Cart = cart,
                NumberOfLicenses = numberOfLicenses,
                ListPrice = resource.ListPrice,
                ResourceId = resourceId,
                Include = true,
                OriginalSourceId = (short)originalSource,
                IsBundle = is3BundlePurchase
            };

            if (is3BundlePurchase)
            {
                cartItem.NumberOfLicenses = 3;
                cartItem.ListPrice =
                    resource.BundlePrice3.GetValueOrDefault(); //resource.BundlePrice3.GetValueOrDefault() / 3;
                cartItem.BundlePrice = resource.BundlePrice3.GetValueOrDefault();
            }

            _resourceDiscountService.SetDiscount(cartItem, institution);

            var audit =
                _institutionResourceAuditFactory.BuildAuditRecord(
                    InstitutionResourceAuditType.ResourceAddedToCart, institution.Id, resourceId, numberOfLicenses,
                    null, cartItem.DiscountPrice, -1);

            uow.SaveOrUpdate(cartItem);
            uow.Save(audit);
        }

        public void AddBulkItemsToCart(Cart cart, IEnumerable<BulkAddResource> bulkAddResources,
            IAdminInstitution institution, IUnitOfWork uow)
        {
            foreach (var bulkAddResource in bulkAddResources)
            {
                if (bulkAddResource.NumberOfLicenses <= 0)
                {
                    continue;
                }

                var resource = _resourceService.GetResource(bulkAddResource.ResourceId);
                var cartItem = new CartItem
                {
                    Cart = cart,
                    NumberOfLicenses = bulkAddResource.NumberOfLicenses,
                    ListPrice = resource.ListPrice,
                    ResourceId = bulkAddResource.ResourceId,
                    Include = true,
                    OriginalSourceId = (short)bulkAddResource.OriginalSource
                };
                _resourceDiscountService.SetDiscount(cartItem, institution);

                var audit =
                    _institutionResourceAuditFactory.BuildAuditRecord(
                        InstitutionResourceAuditType.ResourceAddedToCart, institution.Id, bulkAddResource.ResourceId,
                        bulkAddResource.NumberOfLicenses,
                        null, cartItem.DiscountPrice, -1);

                uow.SaveOrUpdate(cartItem);
                uow.Save(audit);
            }
        }

        public void UpdateCartItemLicenseInCart(Cart cart, int numberOfLicenses, int cartItemId,
            IAdminInstitution institution, IUnitOfWork uow)
        {
            var cartItem = cart.CartItems.SingleOrDefault(x => x.Id == cartItemId);
            if (cartItem != null)
            {
                cartItem.NumberOfLicenses = numberOfLicenses;

                if (cartItem.ResourceId != null)
                {
                    var audit =
                        _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.CartResourceUpdated, institution.Id, cartItem.ResourceId.Value,
                            numberOfLicenses);
                    uow.Save(audit);
                }

                uow.SaveOrUpdate(cartItem);
            }
        }

        public void UpdateResourceLicenseInCart(Cart cart, int numberOfLicenses, int resourceId,
            IAdminInstitution institution, IUnitOfWork uow)
        {
            var cartItem = cart.CartItems.SingleOrDefault(x => x.ResourceId == resourceId);
            if (cartItem != null)
            {
                cartItem.NumberOfLicenses = numberOfLicenses;

                if (cartItem.ResourceId != null)
                {
                    var audit =
                        _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.CartResourceUpdated, institution.Id, cartItem.ResourceId.Value,
                            numberOfLicenses);
                    uow.Save(audit);
                }

                uow.SaveOrUpdate(cartItem);
            }
        }

        public void UpdateFreeResourceLicensesInAllCarts(int resourceId, bool isFreeResource, IUnitOfWork uow)
        {
            var cartItems = _cartItems.Where(y => y.ResourceId == resourceId && !y.Cart.Processed);
            if (cartItems.Any())
            {
                foreach (var item in cartItems)
                {
                    if (isFreeResource)
                    {
                        item.NumberOfLicenses = 500;
                    }
                    else
                    {
                        item.NumberOfLicenses -= 500;
                        if (item.NumberOfLicenses <= 0)
                        {
                            item.NumberOfLicenses = 1;
                        }
                    }

                    uow.Update(item);
                    var audit =
                        _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.CartResourceUpdated, item.Cart.InstitutionId,
                            item.ResourceId.GetValueOrDefault(),
                            item.NumberOfLicenses);

                    uow.Save(audit);
                }
            }
        }

        public void UpdateProductInCart(Cart cart, IProductOrderItem productOrderItem, IUnitOfWork uow)
        {
            var cartItem = cart.CartItems.SingleOrDefault(x => x.Id == productOrderItem.ItemId);
            if (cartItem != null)
            {
                cartItem.Include = productOrderItem.Include;
                cartItem.Agree = productOrderItem.Agree;

                uow.SaveOrUpdate(cartItem);
            }
        }

        public void RemoveItemFromCart(Cart cart, int itemId, int institutionId, IUnitOfWork uow)
        {
            var cartItem = cart.CartItems.SingleOrDefault(x => x.Id == itemId);
            if (cartItem != null)
            {
                if (cartItem.ResourceId != null)
                {
                    var audit =
                        _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.ResourceDeletedFromCart, institutionId,
                            cartItem.ResourceId.Value);
                    uow.Save(audit);
                }

                uow.Delete(cartItem);
            }
            else
            {
                _log.ErrorFormat("CartItem not found, cartItemId: {0}, cartId: {1}", itemId, cart.Id);
            }
        }

        public bool RemoveAllResourcesFromCart(int institutionId, int cartId, CartTypeEnum cartType, string auditId,
            IUnitOfWork uow)
        {
            var update = new StringBuilder();
            update.Append("update tCartItem ");
            update.Append("set    tiRecordStatus = 0, vchUpdaterId = :updatedBy, dtLastUpdate = getdate() ");
            update.Append(
                "where  iCartId = :cartId and tiRecordStatus = 1 and iResourceId is not null and iProductId is null; ");

            var query = uow.Session.CreateSQLQuery(update.ToString());
            query.SetParameter("updatedBy", auditId);
            query.SetParameter("cartId", cartId);
            var rows = query.ExecuteUpdate();


            _log.DebugFormat(
                "RemoveAllItemsFromCart(institutionId: {0}, int cartId: {1}) - rows updated: {2}, updatedBy: {3}",
                institutionId, cartId, rows, auditId);

            return rows > 0;
        }

        public bool RemoveResourcesFromCart(int institutionId, int cartId, int[] resourceIds, string auditId,
            IUnitOfWork uow)
        {
            var resourceIdString = string.Join(",", resourceIds);
            var update = new StringBuilder();
            update.Append("update tCartItem ");
            update.Append($"set    tiRecordStatus = 0, vchUpdaterId = '{auditId}', dtLastUpdate = getdate() ");
            update.Append(
                $"where  iCartId = {cartId} and tiRecordStatus = 1 and iResourceId in ({resourceIdString}); ");

            var query = uow.Session.CreateSQLQuery(update.ToString());
            var rows = query.ExecuteUpdate();

            _log.Debug(
                $"RemoveResourcesFromCart(institutionId: {institutionId}, int cartId: {cartId}, int resourceIds: {resourceIdString}) - rows updated: {rows}, updatedBy: {auditId}");

            return rows > 0;
        }
    }
}