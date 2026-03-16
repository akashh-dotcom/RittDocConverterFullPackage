#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.ExpressCheckout;
using R2V2.Web.Areas.Admin.Models.PdaRules;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.ExpertReviewer })]
    public class ExpressCheckoutController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly GoogleService _googleService;
        private readonly IOrderService _orderService;
        private readonly PatronDrivenAcquisitionService _patronDrivenAcquisitionService;
        private readonly PdaService _pdaService;

        public ExpressCheckoutController(
            IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IOrderService orderService
            , PatronDrivenAcquisitionService patronDrivenAcquisitionService
            , PdaService pdaService
            , GoogleService googleService
        ) : base(authenticationContext)
        {
            _adminContext = adminContext;
            _orderService = orderService;
            _patronDrivenAcquisitionService = patronDrivenAcquisitionService;
            _pdaService = pdaService;
            _googleService = googleService;
        }

        public ActionResult Index(CollectionManagementQuery collectionManagementQuery)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("Index", collectionManagementQuery.ToRouteValues());
            }

            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);
            var model = new ExpressCheckout(adminInstitution) { CollectionManagementQuery = collectionManagementQuery };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(CollectionManagementQuery collectionManagementQuery, string action)
        {
            switch (action.ToLower())
            {
                case "addtocart":
                    TempData.AddItem("CartIsbns", collectionManagementQuery.Isbns);
                    return RedirectToAction("BulkAddToCartVerify", collectionManagementQuery.ToRouteValues());
                case "addtopda":
                    TempData.AddItem("PdaIsbns", collectionManagementQuery.Isbns);
                    return RedirectToAction("BulkAddPdaVerify", collectionManagementQuery.ToRouteValues());
                default:
                    return RedirectToAction("Index",
                        collectionManagementQuery.ToRouteValues()); // Index(collectionManagementQuery);
            }
        }

        public ActionResult BulkAddPdaVerify(CollectionManagementQuery collectionManagementQuery, string bulkAddPda)
        {
            var tempCollectionManagementQuery =
                TempData.GetItem<CollectionManagementQuery>("CollectionManagementQuery");

            if (tempCollectionManagementQuery != null)
            {
                collectionManagementQuery = tempCollectionManagementQuery;
            }

            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var model = new BulkAddToCart(adminInstitution) { ResourceQuery = collectionManagementQuery };

            var pdaIsbns = TempData.GetItem<string>("PdaIsbns");

            if (collectionManagementQuery.Isbns == null && pdaIsbns != null)
            {
                collectionManagementQuery.Isbns = pdaIsbns;
            }

            List<string> isbnsNotFound;
            var institutionResources =
                _orderService.GetInstitutionResourcesWithoutDatabase(collectionManagementQuery, out isbnsNotFound);

            model.IsbnsNotFound = string.Join(", ", isbnsNotFound);
            model.IsbnsNotFoundCount = isbnsNotFound.Count;
            foreach (var institutionResource in institutionResources)
            {
                if (institutionResource.IsPdaEligible && institutionResource.IsForSale)
                {
                    model.AddResource(institutionResource);
                }
                else
                {
                    model.AddExcludedResource(institutionResource);
                }
            }

            if (bulkAddPda == "yes")
            {
                if (_pdaService.ShowPdaTrialConvert(adminInstitution, collectionManagementQuery))
                {
                    return View("PdaTrialConvertModal",
                        new CollectionAdd(adminInstitution) { ResourceQuery = collectionManagementQuery });
                }

                if (_pdaService.ShowEula(adminInstitution, collectionManagementQuery))
                {
                    return View("EulaModal",
                        new CollectionAdd(adminInstitution) { ResourceQuery = collectionManagementQuery });
                }

                if (_pdaService.ShowPdaEula(adminInstitution, collectionManagementQuery))
                {
                    return View("PdaEulaModal",
                        new CollectionAdd(adminInstitution) { ResourceQuery = collectionManagementQuery });
                }

                IList<BulkAddResource> bulkAddResources = model.Resources.Select(resource => new BulkAddResource
                {
                    ResourceId = resource.Id,
                    InstitutionId = collectionManagementQuery.InstitutionId,
                    NumberOfLicenses = 1,
                    OriginalSource = LicenseOriginalSource.Pda
                }).ToList();

                _pdaService.ConvertAndSignEulasIfNeeded(adminInstitution, collectionManagementQuery);

                _patronDrivenAcquisitionService.AddBuildPdaLicenses(bulkAddResources,
                    collectionManagementQuery.InstitutionId, AuthenticatedInstitution.User.Id);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId,
                    AuthenticatedInstitution.User.Id);

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}";
                model.CollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, IncludePdaResources = true });

                return View("BulkAddPdaConfirm", model);
            }

            return View(model);
        }

        public ActionResult BulkAddToCartVerify(CollectionManagementQuery collectionManagementQuery,
            string bulkAddToCart)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var model = new BulkAddToCart(adminInstitution) { ResourceQuery = collectionManagementQuery };

            var cartIsbns = TempData.GetItem<string>("CartIsbns");

            if (collectionManagementQuery.Isbns == null && cartIsbns != null)
            {
                collectionManagementQuery.Isbns = cartIsbns;
            }

            List<string> isbnsNotFound;
            var institutionResources =
                _orderService.GetInstitutionResourcesWithoutDatabase(collectionManagementQuery,
                    out isbnsNotFound);

            model.IsbnsNotFound = string.Join(", ", isbnsNotFound);
            model.IsbnsNotFoundCount = isbnsNotFound.Count;
            
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

            if (bulkAddToCart == "yes")
            {
                _orderService.AddBulkItemsToOrder(collectionManagementQuery.InstitutionId, model.Resources);

                _googleService.LogBulkAddToCart(model.Resources.ToList(), "Express Check Out");

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}";

                model.CollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, PurchasedOnly = true });

                model.CartLink = Url.Action("ShoppingCart", "Cart", new { collectionManagementQuery.InstitutionId });

                return View("BulkAddToCartConfirm", model);
            }

            return View(model);
        }

        public ActionResult PdaTrialConvertModal(CollectionManagementQuery collectionManagementQuery)
        {
            collectionManagementQuery.TrialConvert = true;
            var pdaRuleModel = TempData.GetItem<PdaRuleModel>("PdaRule");
            if (pdaRuleModel != null)
            {
                return RedirectToAction("SaveRule", "Pda", collectionManagementQuery.ToRouteValues());
            }

            TempData.AddItem("CollectionManagementQuery", collectionManagementQuery);
            return RedirectToAction("BulkAddPdaVerify", new { bulkAddPda = "yes" });
        }

        public ActionResult EulaModal(CollectionManagementQuery collectionManagementQuery)
        {
            collectionManagementQuery.EulaSigned = true;

            var pdaRuleModel = TempData.GetItem<PdaRuleModel>("PdaRule");
            if (pdaRuleModel != null)
            {
                return RedirectToAction("SaveRule", "Pda", collectionManagementQuery.ToRouteValues());
            }

            TempData.AddItem("CollectionManagementQuery", collectionManagementQuery);
            return RedirectToAction("BulkAddPdaVerify", new { bulkAddPda = "yes" });
        }

        public ActionResult PdaEulaModal(CollectionManagementQuery collectionManagementQuery)
        {
            collectionManagementQuery.PdaEulaSigned = true;

            var pdaRuleModel = TempData.GetItem<PdaRuleModel>("PdaRule");
            if (pdaRuleModel != null)
            {
                return RedirectToAction("SaveRule", "Pda", collectionManagementQuery.ToRouteValues());
            }

            TempData.AddItem("CollectionManagementQuery", collectionManagementQuery);
            return RedirectToAction("BulkAddPdaVerify", new { bulkAddPda = "yes" });
        }
    }
}