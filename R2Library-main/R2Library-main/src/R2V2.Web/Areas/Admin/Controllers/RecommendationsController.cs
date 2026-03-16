#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.Recommendations;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.ExpertReviewer })]
    [RequestLoggerFilter(true)]
    public class RecommendationsController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<RecommendationsController> _log;
        private readonly IOrderService _orderService;
        private readonly RecommendationsService _recommendationsService;
        private readonly RecommendedService _recommendedService;

        public RecommendationsController(IAuthenticationContext authenticationContext
            , ILog<RecommendationsController> log
            , RecommendationsService recommendationsService
            , IOrderService orderService
            , RecommendedService recommendedService
            , IAdminContext adminContext
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _log = log;
            _recommendationsService = recommendationsService;
            _orderService = orderService;
            _recommendedService = recommendedService;
            _adminContext = adminContext;
        }

        public ActionResult Recommend(CollectionManagementQuery collectionManagementQuery, Recommended recommended,
            string action, int? reviewId)
        {
            _log.DebugFormat("action: {0}, institutionId: {1}, resourceId: {2}, notes: {3}", action,
                collectionManagementQuery.InstitutionId, collectionManagementQuery.ResourceId,
                recommended != null ? recommended.Notes : "null");
            if (collectionManagementQuery.ReviewId > 0 && recommended?.Id == 0)
            {
                recommended = new Recommended { Id = collectionManagementQuery.ReviewId };
            }

            var institutionResource = _orderService.GetInstitutionResource(
                collectionManagementQuery.InstitutionId, collectionManagementQuery.ResourceId,
                collectionManagementQuery.CartId);

            var recommendations = _recommendationsService.GetRecommendations(collectionManagementQuery.InstitutionId,
                collectionManagementQuery.ResourceId);

            Recommendation recommendation = null;
            if (recommended != null && recommended.Id > 0)
            {
                recommendation = recommendations.FirstOrDefault(x => x.Id == recommended.Id);
            }

            var institution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var model = new RecommendationEdit(institution)
            {
                InstitutionResource = institutionResource,
                ResourceQuery = collectionManagementQuery,
                Recommended = recommendation != null ? new Recommended(recommendation) : null,
                ReviewId = reviewId != null && reviewId > 0 ? reviewId.Value : 0,
                Action = action.ToLower()
            };

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                foreach (var error in errors)
                {
                    _log.DebugFormat("ERROR --> {0} = {1}", error.Key, string.Join(",", error.Errors));
                }
            }

            if (Request.HttpMethod == "POST" && ModelState.IsValid && !string.IsNullOrEmpty(action) &&
                recommended != null)
            {
                var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

                if (action.ToLower() == "add")
                {
                    recommendation = _recommendationsService.SaveRecommendation(collectionManagementQuery.InstitutionId,
                        authenticatedInstitution.User.Id,
                        collectionManagementQuery.ResourceId, recommended.Notes);
                }
                else if (action.ToLower() == "update")
                {
                    recommendation = _recommendationsService.UpdateRecommendation(recommended.Id, recommended.Notes);
                }
                else if (action.ToLower() == "delete")
                {
                    recommendation =
                        _recommendationsService.DeleteRecommendation(recommended.Id, authenticatedInstitution.User.Id);
                }

                model.Recommended = recommendation != null ? new Recommended(recommendation) : null;

                if (model.ReviewId > 0)
                {
                    model.BackToListLink = Url.Action("Detail", "Review",
                        new { institutionId = collectionManagementQuery.InstitutionId, reviewId = model.ReviewId });
                }
                else
                {
                    model.BackToListLink = string.Format("{0}#{1}",
                        Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues()),
                        collectionManagementQuery.ResourceId);
                }

                model.ViewMyRecommendationsLink = Url.Action("List", "CollectionManagement",
                    new
                    {
                        collectionManagementQuery.InstitutionId,
                        RecommendationsOnly = true
                    });

                return View("Recommended", model);
            }

            // todo: replace magic string
            return View("Recommend", model);
        }


        public ActionResult DeleteRecommend(CollectionManagementQuery collectionManagementQuery,
            Recommended recommended)
        {
            _log.DebugFormat("institutionId: {0}, resourceId: {1}, notes: {2}", collectionManagementQuery.InstitutionId,
                collectionManagementQuery.ResourceId,
                recommended != null ? recommended.Notes : "null");

            var institutionResource = _orderService.GetInstitutionResource(
                collectionManagementQuery.InstitutionId, collectionManagementQuery.ResourceId,
                collectionManagementQuery.CartId);

            IList<Recommendation> recommendations =
                _recommendationsService.GetRecommendations(collectionManagementQuery.InstitutionId,
                    collectionManagementQuery.ResourceId).Where(x => x.DeletedDate == null).ToList();

            var institution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var model = new RecommendationEdit(institution)
            {
                InstitutionResource = institutionResource,
                ResourceQuery = collectionManagementQuery,
                //Recommended = (recommendation != null) ? new Recommended(recommendation) : null,
                RecommendedList = recommendations.Select(x => new Recommended(x)).ToList()
            };

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                foreach (var error in errors)
                {
                    _log.DebugFormat("ERROR --> {0} = {1}", error.Key, string.Join(",", error.Errors));
                }
            }

            if (Request.HttpMethod == "POST" && ModelState.IsValid && recommended != null)
            {
                var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

                var recommendation = _recommendationsService.DeleteRecommendation(recommended.Id,
                    authenticatedInstitution.User.Id, recommended.DeletedNotes);

                model.Recommended = recommendation != null ? new Recommended(recommendation) : null;

                model.BackToListLink = string.Format("{0}#{1}",
                    Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues()),
                    collectionManagementQuery.ResourceId);


                model.ViewMyRecommendationsLink = Url.Action("List", "CollectionManagement",
                    new
                    {
                        collectionManagementQuery.InstitutionId,
                        RecommendationsOnly = true
                    });

                return View("DeleteRecommended", model);
            }

            // todo: replace magic string
            return View("DeleteRecommend", model);
        }

        public ActionResult BulkRecommend(CollectionManagementQuery collectionManagementQuery, string bulkRecommend,
            string notes)
        {
            var model = new BulkAddToCart { ResourceQuery = collectionManagementQuery };
            List<string> isbnsNotFound;
            var institutionResources =
                _orderService.GetInstitutionResources(collectionManagementQuery, out isbnsNotFound);

            model.IsbnsNotFound = string.Join(", ", isbnsNotFound);

            foreach (var institutionResource in institutionResources)
            {
                if (institutionResource.IsForSale && institutionResource.LicenseType != LicenseType.Purchased &&
                    institutionResource.Recommendeds.Count == 0)
                {
                    _log.DebugFormat(
                        "Add - id: {0}, isbn: {1}, institutionResource.IsForSale: {2}, institutionResource.LicenseType: {3}, institutionResource.Recommendeds.Count: {4}"
                        , institutionResource.Id, institutionResource.Isbn, institutionResource.IsForSale,
                        institutionResource.LicenseType, institutionResource.Recommendeds.Count);
                    model.AddResource(institutionResource);
                }
                else
                {
                    _log.DebugFormat(
                        "Exclude - id: {0}, isbn: {1}, institutionResource.IsForSale: {2}, institutionResource.LicenseType: {3}, institutionResource.Recommendeds.Count: {4}"
                        , institutionResource.Id, institutionResource.Isbn, institutionResource.IsForSale,
                        institutionResource.LicenseType, institutionResource.Recommendeds.Count);
                    model.AddExcludedResource(institutionResource);
                }
            }

            if (bulkRecommend == "yes")
            {
                //_orderService.AddBulkItemsToOrder(collectionManagementQuery.InstitutionId, model.Resources);

                _recommendedService.BulkRecommend(collectionManagementQuery.InstitutionId, model.Resources, notes);

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}";

                model.CollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, RecommendationsOnly = true });

                //model.CartLink = Url.Action("ShoppingCart", "CollectionManagement", new { collectionManagementQuery.InstitutionId });

                return View("BulkRecommendConfirm", model);
            }

            return View(model);
        }
    }
}