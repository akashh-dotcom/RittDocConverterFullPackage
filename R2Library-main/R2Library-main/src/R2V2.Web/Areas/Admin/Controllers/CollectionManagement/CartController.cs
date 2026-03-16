#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Order;
using R2V2.Web.Areas.Admin.Models.Promotion;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC })]
    public class CartController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly ICartService _cartService;
        private readonly EmailSiteService _emailService;
        private readonly GoogleService _googleService;
        private readonly ILog<CartController> _log;
        private readonly IOrderService _orderService;
        private readonly PatronDrivenAcquisitionService _pdaService;
        private readonly PromotionsService _promotionsService;
        private readonly RecommendationsService _recommendationsService;
        private readonly IResourceService _resourceService;
        private readonly IWebImageSettings _webImageSettings;

        private bool _sendNewAccountEmail;

        public CartController(ILog<CartController> log
            , IAuthenticationContext authenticationContext
            , IOrderService orderService
            , EmailSiteService emailService
            , PromotionsService promotionsService
            , RecommendationsService recommendationsService
            , PatronDrivenAcquisitionService pdaService
            , IAdminContext adminContext
            , ICartService cartService
            , GoogleService googleService
            , IResourceService resourceService
            , IWebImageSettings webImageSettings
        )
            : base(authenticationContext)
        {
            _log = log;
            _orderService = orderService;
            _emailService = emailService;
            _promotionsService = promotionsService;
            _recommendationsService = recommendationsService;
            _pdaService = pdaService;
            _adminContext = adminContext;
            _cartService = cartService;
            _googleService = googleService;
            _resourceService = resourceService;
            _webImageSettings = webImageSettings;
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
        public ActionResult CreateResellerCart(int institutionId, int resellerId)
        {
            var newCart = _cartService.CreateResellerCart(institutionId, resellerId);
            return RedirectToAction("ShoppingCart",
                new { InstitutionId = institutionId, CartId = newCart?.Id.ToString() });
        }

        public ActionResult ShoppingCart(CollectionManagementQuery collectionManagementQuery)
        {
            var order = _orderService.GetOrderFromDatabaseForInstitution(collectionManagementQuery.InstitutionId,
                collectionManagementQuery.CartId);

            if (order.PurchaseDate != null)
            {
                return RedirectToAction("Detail", "OrderHistory",
                    new { institutionId = order.InstitutionId, id = order.OrderHistoryId });
            }

            order.CollectionManagementQuery = collectionManagementQuery;

            var excelUrl = Url.Action("ExportShoppingCart", new { institutionId = order.InstitutionId });
            var marcExportUrl = Url.Action("ShoppingCart", "MarcExport",
                new { institutionId = order.InstitutionId, id = order.OrderId });

            order.ToolLinks = GetToolLinks(true, excelUrl, marcExportUrl);
            order.Editable = true;
            order.PromotionStatusMessage = TempData.GetItem<string>("PromotionStatusMessage");
            order.PromotionErrorMessage = TempData.GetItem<string>("PromotionErrorMessage");
            order.MergeCartsErrorMessage = TempData.GetItem<string>("MergeCartsErrorMessage");

            _googleService.LogCheckoutStep(order.PurchasableItems.ToList(), 1);

            return View(order);
        }

        [HttpPost]
        public ActionResult ShoppingCart(CollectionManagementQuery collectionManagementQuery, EmailPage emailPage)
        {
            object json;

            try
            {
                if (emailPage.To == null)
                {
                    return RedirectToAction("List", "CollectionManagement", collectionManagementQuery.ToRouteValues());
                }

                var order = _orderService.GetOrderForInstitution(collectionManagementQuery.InstitutionId);
                order.CollectionManagementQuery = collectionManagementQuery;

                var messageBody = RenderRazorViewToString("Cart", "_Cart", order);

                var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                json = emailStatus
                    ? new JsonResponse { Status = "success", Successful = true }
                    : new JsonResponse { Status = "failure", Successful = false };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveCart(Order order)
        {
            if (string.IsNullOrWhiteSpace(order.CartName) || order.InstitutionId == 0 || order.OrderId == 0)
            {
                return RedirectToAction("ShoppingCart", new { order.InstitutionId });
            }

            int cartId;
            if (order.CartType == CartTypeEnum.Saved && !order.IsCartRename)
            {
                cartId = _orderService.CopyCart(order.OrderId, order.InstitutionId, order.CartName);
            }
            else
            {
                _orderService.SaveCart(order.OrderId, order.InstitutionId, order.CartName);
                cartId = order.OrderId;
            }

            return RedirectToAction("ShoppingCart", new { order.InstitutionId, cartId });
        }

        public ActionResult ExportShoppingCart(int institutionId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var export = _orderService.GetShoppingCartExcelExport(institutionId, adminInstitution.ProxyPrefix,
                adminInstitution.UrlSuffix,
                Url.Action("Title", "Resource", new { Area = "" },
                    HttpContext.Request.IsSecureConnection ? "https" : "http"));
            var fileDownloadName = $"R2-ShoppingCart-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(export.Export(), export.MimeType, fileDownloadName);
        }

        [HttpGet]
        public ActionResult Add(int institutionId, int resourceId, int cartId, string parentPageTitle)
        {
            _log.DebugFormat("Add(institutionId: {0}, resourceId: {1})", institutionId, resourceId);
            try
            {
                var institutionResource = _orderService.GetInstitutionResource(institutionId, resourceId, cartId);
                var licenseCount = institutionResource.IsFreeResource ? 500 :
                    institutionResource.CartLicenseCount > 0 ? institutionResource.CartLicenseCount : 1;
                return PartialView("_Add",
                    new CollectionAdd
                    {
                        InstitutionResource = institutionResource,
                        NumberOfLicenses = licenseCount,
                        CartId = cartId,
                        ParentPageTitle = parentPageTitle,
                        BaseImageUrl = _webImageSettings.SpecialIconBaseUrl
                    });
            }
            catch (Exception ex)
            {
                return Content($"<html><body>EXCEPTION: {ex.Message}</body></html>");
            }
        }

        [HttpPost]
        public ActionResult Add(CollectionAdd collectionAdd)
        {
            object json;

            try
            {
                var itemAdded = _orderService.AddItemToOrder(collectionAdd);

                var resourceId = collectionAdd.InstitutionResource.Id;
                var institutionId = collectionAdd.InstitutionId;

                if (itemAdded && _recommendationsService.HasRecommendation(collectionAdd.InstitutionId, resourceId))
                {
                    _recommendationsService.RecommendationAddedToCart(institutionId, resourceId, CurrentUser);
                }

                _googleService.LogAddToCart(collectionAdd, collectionAdd.ParentPageTitle);

                json = new JsonResponse { Status = itemAdded ? "success" : "failure", Successful = itemAdded };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CalculateResourceCost(int institutionId, int resourceId, int cartId, int numberOfLicenses,
            string parentPageTitle)
        {
            _log.DebugFormat("Add(institutionId: {0}, resourceId: {1})", institutionId, resourceId);
            try
            {
                var institutionResource = _orderService.GetInstitutionResource(institutionId, resourceId, cartId);

                var allCarts = _cartService.GetAllInstitutionCartsFromCache(institutionId);
                var savedCarts = allCarts
                    .Where(x => x.CartType == CartTypeEnum.Saved || x.CartType == CartTypeEnum.AutomatedCart).ToList();

                var licenseCount = institutionResource.IsFreeResource ? 500 :
                    institutionResource.CartLicenseCount > 0 ? institutionResource.CartLicenseCount : 1;
                return PartialView("_CalculateResourceCost",
                    new CollectionAdd
                    {
                        InstitutionResource = institutionResource,
                        NumberOfLicenses = licenseCount,
                        CartId = cartId,
                        ParentPageTitle = parentPageTitle,
                        DisplayAddToSavedCart = savedCarts.Any()
                    });
            }
            catch (Exception ex)
            {
                return Content($"<html><body>EXCEPTION: {ex.Message}</body></html>");
            }
        }

        public ActionResult AddResourceToCart(int institutionId, int resourceId, int cartId, int numberOfLicenses,
            string parentPageTitle)
        {
            object json;

            try
            {
                var institutionResource = _orderService.GetInstitutionResource(institutionId, resourceId, 0);
                var collectionAdd = new CollectionAdd
                {
                    InstitutionResource = institutionResource,
                    NumberOfLicenses = numberOfLicenses,
                    CartId = cartId,
                    ParentPageTitle = parentPageTitle,
                    InstitutionId = institutionId
                };

                var itemAdded = _orderService.AddItemToOrder2(collectionAdd);

                if (itemAdded && _recommendationsService.HasRecommendation(collectionAdd.InstitutionId, resourceId))
                {
                    _recommendationsService.RecommendationAddedToCart(institutionId, resourceId, CurrentUser);
                }

                _googleService.LogAddToCart(collectionAdd, collectionAdd.ParentPageTitle);

                json = new JsonResponse { Status = itemAdded ? "success" : "failure", Successful = itemAdded };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddToSavedCart(int institutionId, int resourceId, string parentPageTitle,
            int numberOfLicenses)
        {
            _log.DebugFormat("AddToSavedCart(institutionId: {0}, resourceId: {1})", institutionId, resourceId);
            try
            {
                var institutionResource = new InstitutionResource { Id = resourceId, InstitutionId = institutionId };

                var allCarts = _cartService.GetAllInstitutionCartsFromCache(institutionId);
                var savedCarts = allCarts.Where(x => x.CartType == CartTypeEnum.Saved).ToList();

                return PartialView("_AddToSavedCart",
                    new CollectionAdd
                    {
                        InstitutionResource = institutionResource,
                        NumberOfLicenses = numberOfLicenses,
                        ParentPageTitle = parentPageTitle,
                        DisplayAddToSavedCart = savedCarts.Any(),
                        CachedCarts = savedCarts
                    });
            }
            catch (Exception ex)
            {
                return Content($"<html><body>EXCEPTION: {ex.Message}</body></html>");
            }
        }

        [HttpPost]
        public ActionResult Update(int institutionId, ResourceOrderItem resourceOrderItem, int cartId)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
            var cartItem = cart.CartItems.FirstOrDefault(x => x.Id == resourceOrderItem.ItemId);
            if (cartItem == null)
            {
                return RedirectToAction("ShoppingCart", new { institutionId, cartId });
            }

            var resourceId = cartItem.ResourceId ?? 0;

            if (resourceOrderItem.NumberOfLicenses == 0 && resourceId > 0)
            {
                _pdaService.DeletePartonDrivenAcquisitionFromCart(resourceId, institutionId, CurrentUser);
            }

            if (resourceId > 0)
            {
                var resource = _resourceService.GetResource(resourceId);
                resourceOrderItem.PopulateResource(resource, cartItem);
            }

            _orderService.UpdateOrderItem(institutionId, resourceOrderItem, cartId);

            if (resourceId > 0)
            {
                if (cartItem.NumberOfLicenses > resourceOrderItem.NumberOfLicenses)
                {
                    _googleService.LogAddToCart(resourceOrderItem, "Shopping Cart");
                }
                else if (cartItem.NumberOfLicenses < resourceOrderItem.NumberOfLicenses)
                {
                    _googleService.LogRemoveFromCart(resourceOrderItem, "Shopping Cart");
                }
            }

            _adminContext.ReloadAdminInstitution(institutionId, CurrentUser.Id);
            _cartService.GetInstitutionCartFromCache(institutionId, cartId);

            return RedirectToAction("ShoppingCart", new { institutionId, cartId });
        }

        [HttpPost]
        public ActionResult Include(int institutionId, ProductOrderItem productOrderItem)
        {
            _orderService.UpdateOrderItem(institutionId, productOrderItem);

            return RedirectToAction("ShoppingCart", new { institutionId });
        }

        public ActionResult Delete(int institutionId, int itemId, int cartId)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId, cartId);
            var cartItem = cart.CartItems.FirstOrDefault(x => x.Id == itemId);
            if (cartItem?.ResourceId != null)
            {
                var resource = _resourceService.GetResource(cartItem.ResourceId.Value);
                var resourceOrderItem = new ResourceOrderItem(cartItem, resource, null,
                    cart.ConvertDate.HasValue && cart.ConvertDate.GetValueOrDefault() != DateTime.MinValue);

                _googleService.LogRemoveFromCart(resourceOrderItem, "Shopping Cart");
                _pdaService.DeletePartonDrivenAcquisitionFromCart(cartItem.ResourceId.Value, institutionId,
                    CurrentUser);
            }

            _orderService.RemoveItemFromOrder(institutionId, itemId, cartId);

            _adminContext.ReloadAdminInstitution(institutionId, CurrentUser.Id);
            _cartService.GetInstitutionCartFromCache(institutionId, cartId);

            return RedirectToAction("ShoppingCart", new { institutionId, cartId });
        }

        public ActionResult DeleteNonPurchaseable(int institutionId, int cartId, string nonPurchaseType)
        {
            if (string.IsNullOrWhiteSpace(nonPurchaseType))
            {
                return RedirectToAction("ShoppingCart", new { institutionId, cartId });
            }

            var order = _orderService.GetOrderFromDatabaseForInstitution(institutionId, cartId);
            int[] resourceIdsToDelete;
            switch (nonPurchaseType.ToLower())
            {
                case "archived":
                    //cartItemsToDelete = cart.CartItems.Where(x => x. .Resource.StatusId == (int)ResourceStatus.Archived || x.Resource.StatusId == (int)ResourceStatus.Inactive)
                    resourceIdsToDelete = order.ArchivedItems.Select(x => x.Resource.Id).ToArray();
                    break;
                default:
                    resourceIdsToDelete = order.NotForSaleItems.Select(x => x.Resource.Id).ToArray();
                    break;
            }

            _orderService.RemoveResourcesFromOrder(institutionId, cartId, resourceIdsToDelete);
            return RedirectToAction("ShoppingCart", new { institutionId, cartId });
        }

        public ActionResult ClearCart(int institutionId, int cartId)
        {
            var order = _orderService.GetOrderForInstitution(institutionId, cartId);

            _pdaService.DeletePartonDrivenAcquisitionsFromCart(cartId, institutionId, CurrentUser);

            _orderService.RemoveAllResourcesFromOrder(institutionId, cartId);

            foreach (var item in order.Items)
            {
                var productItem = item as ProductOrderItem;
                if (productItem == null)
                {
                    continue;
                }

                if (productItem.Product != null)
                {
                    if (productItem.Product.Optional)
                    {
                        productItem.Include = false;
                        _orderService.UpdateOrderItem(institutionId, productItem);
                    }

                    continue;
                }

                _log.ErrorFormat("ClearCart Error: ProductOrderItem.Product is null.");
            }

            _googleService.LogBulkRemoveFromCart(order.Items.ToList(), "Shopping Cart");

            _adminContext.ReloadAdminInstitution(institutionId, CurrentUser.Id);
            _cartService.GetInstitutionCartFromCache(institutionId, cartId);

            return RedirectToAction("ShoppingCart", new { institutionId });
        }

        public ActionResult BulkAddToCart(CollectionManagementQuery collectionManagementQuery, string bulkAddToCart,
            string parentPageTitle)
        {
            var model = new BulkAddToCart { ResourceQuery = collectionManagementQuery };
            List<string> isbnsNotFound;
            var institutionResources =
                _orderService.GetInstitutionResources(collectionManagementQuery, out isbnsNotFound).ToList();

            model.IsbnsNotFound = string.Join(", ", isbnsNotFound);

            foreach (var institutionResource in institutionResources)
            {
                if (institutionResource.IsForSale)
                {
                    model.AddResource(institutionResource);
                }
                else
                {
                    model.AddExcludedResource(institutionResource);
                }
            }

            model.ParentPageTitle = parentPageTitle;
            if (bulkAddToCart == "yes")
            {
                var itemsAdded =
                    _orderService.AddBulkItemsToOrder(collectionManagementQuery.InstitutionId, model.Resources);

                if (itemsAdded)
                {
                    var resourceIds = model.Resources.Select(x => x.Id).ToArray();
                    _recommendationsService.BulkRecommendationsAddedToCart(collectionManagementQuery.InstitutionId,
                        resourceIds, CurrentUser);
                }

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}";

                model.CollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, PurchasedOnly = true });

                model.CartLink = Url.Action("ShoppingCart", new { collectionManagementQuery.InstitutionId });

                _googleService.LogBulkAddToCart(institutionResources, model.ParentPageTitle);

                return View("BulkAddToCartConfirm", model);
            }

            return View(model);
        }

        public ActionResult ApplyPromotion(int institutionId, string promotionCode)
        {
            var promotion = _promotionsService.GetPromotion(promotionCode);
            var action = _orderService.ApplyPromotion(institutionId, promotion);
            string statusMessage = null;
            string errorMessage = null;
            switch (action)
            {
                case PromotionAction.PromotionApplied:
                    statusMessage = "Promotion code successfully applied.";
                    break;
                case PromotionAction.PromotionError:
                    errorMessage = $"An error occurred while applying promotion code '{promotionCode}'.";
                    break;
                case PromotionAction.PromotionExpired:
                    errorMessage = $"Promotion expired on {promotion.EndDate:d}, promotion code '{promotionCode}'.";
                    break;
                case PromotionAction.PromotionNotActive:
                    errorMessage =
                        $"Promotion cannot be used before {promotion.StartDate:d}, promotion code '{promotionCode}'.";
                    break;
                case PromotionAction.PromotionPreviouslyApplied:
                    errorMessage = $"Promotion code '{promotionCode}' was previously applied to an R2 Library order.";
                    break;
                case PromotionAction.PromotionNotFound:
                    errorMessage = $"Invalid promotion code '{promotionCode}'.";
                    break;
                case PromotionAction.PromotionLowerThenCurrent:
                    errorMessage = "Promotion discount is lower than the current cart discount";
                    break;
            }

            TempData.AddItem("PromotionStatusMessage", statusMessage);
            TempData.AddItem("PromotionErrorMessage", errorMessage);

            return RedirectToAction("ShoppingCart", new { institutionId });
        }

        public ActionResult RemovePromotion(int institutionId)
        {
            _orderService.RemovePromotion(institutionId);
            return RedirectToAction("ShoppingCart", new { institutionId });
        }

        public ActionResult MergeCarts(int currentCartId, int mergeIntoCartId, int institutionId)
        {
            var cartId = _orderService.MergeCarts(currentCartId, mergeIntoCartId, institutionId);
            if (cartId == 0)
            {
                TempData.AddItem("MergeCartsErrorMessage",
                    "There was a problem merging the carts. Please try again later.");
                return RedirectToAction("ShoppingCart", new { institutionId, cartId = currentCartId });
            }

            return RedirectToAction("ShoppingCart", new { institutionId, cartId });
        }
    }
}