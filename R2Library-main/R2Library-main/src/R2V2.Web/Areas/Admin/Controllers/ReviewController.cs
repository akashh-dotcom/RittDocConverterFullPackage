#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Review;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.ExpertReviewer })]
    public class ReviewController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IClientSettings _clientSettings;
        private readonly ILog<ReviewController> _log;
        private readonly IResourceService _resourceService;
        private readonly ReviewModelService _reviewModelService;
        private readonly ReviewService _reviewService;
        private readonly UserService _userService;
        private readonly IWebImageSettings _webImageSettings;
        private readonly IWebSettings _webSettings;


        public ReviewController(
            IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , ILog<ReviewController> log
            , ReviewService reviewService
            , IResourceService resourceService
            , IWebSettings webSettings
            , ReviewModelService reviewModelService
            , UserService userService
            , IClientSettings clientSettings
            , IWebImageSettings webImageSettings
        ) : base(authenticationContext)
        {
            _adminContext = adminContext;
            _log = log;
            _reviewService = reviewService;
            _resourceService = resourceService;
            _webSettings = webSettings;
            _reviewModelService = reviewModelService;
            _userService = userService;
            _clientSettings = clientSettings;
            _webImageSettings = webImageSettings;
        }

        /// <summary>
        /// </summary>
        public ActionResult List(int institutionId)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                institutionId != CurrentUser.InstitutionId)
            {
                institutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("List", new { institutionId });
            }

            var institution = _adminContext.GetAdminInstitution(institutionId);
            var reviews = _reviewService.GetInstititionsReviews(institutionId);

            var reviewList = new ReviewList(institution, reviews.ToReviews().ToList());

            return View(reviewList);
        }

        public ActionResult Detail(int institutionId, int reviewId, string resourceTitleRemoved)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var review = _reviewService.GetInstititionsReview(institutionId, reviewId);

            var expertReviewer = _userService.GetExpertReviewers(institutionId);
            var reviewEdit = new ReviewEdit(institution, review, expertReviewer, _webImageSettings.SpecialIconBaseUrl);

            if (review != null && review.Id > 0)
            {
                foreach (var coreReviewResource in review.ReviewResources)
                {
                    var reviewResource = _reviewModelService.GetReviewResource(institutionId,
                        coreReviewResource.ResourceId, true, coreReviewResource.Id);
                    if (reviewResource != null)
                    {
                        reviewEdit.Review.AddReviewResource(reviewResource);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(resourceTitleRemoved))
            {
                reviewEdit.Review.ResourceTitleRemoved = resourceTitleRemoved;
            }

            reviewEdit.CollectionManagementQuery = new CollectionManagementQuery
            {
                InstitutionId = institutionId
            };
            reviewEdit.IsPdaEnabled = _webSettings.EnablePatronDrivenAcquisitions;
            reviewEdit.DoodyReviewUrl = _clientSettings.DoodyReviewLink;

            return View(reviewEdit);
        }

        public ActionResult Edit(int institutionId, int reviewId, string resourceTitleRemoved)
        {
            var reviewEdit = GetReviewEditModel(institutionId, reviewId, resourceTitleRemoved);
            return View(reviewEdit);
        }

        private ReviewModel PopulateReviewList(Review review)
        {
            var reviewModel = review.ToReview();
            if (review != null && review.Id > 0)
            {
                foreach (var reviewResource in review.ReviewResources)
                {
                    reviewModel.AddReviewResource(_reviewModelService.GetReviewResource(review.InstitutionId,
                        reviewResource.ResourceId, true, reviewResource.Id));
                }
            }

            return reviewModel;
        }

        private ReviewEdit GetReviewEditModel(int institutionId, int reviewId, string resourceTitleRemoved)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);

            var review = _reviewService.GetInstititionsReview(institutionId, reviewId) ?? new Review();

            var expertReviewer = _userService.GetExpertReviewers(institutionId);
            var reviewEdit = new ReviewEdit(institution, review, expertReviewer, _webImageSettings.SpecialIconBaseUrl)
                { Review = PopulateReviewList(review) };

            if (!string.IsNullOrWhiteSpace(resourceTitleRemoved))
            {
                reviewEdit.Review.ResourceTitleRemoved = resourceTitleRemoved;
            }

            reviewEdit.CollectionManagementQuery = new CollectionManagementQuery
            {
                InstitutionId = institutionId
            };
            reviewEdit.IsPdaEnabled = _webSettings.EnablePatronDrivenAcquisitions;
            reviewEdit.DoodyReviewUrl = _clientSettings.DoodyReviewLink;
            return reviewEdit;
        }

        [HttpPost]
        public ActionResult Edit(ReviewEdit reviewEdit)
        {
            if (reviewEdit.Review != null)
            {
                var expertReviewer = _userService.GetExpertReviewers(reviewEdit.InstitutionId);
                if (!string.IsNullOrWhiteSpace(reviewEdit.ActionType) &&
                    reviewEdit.ActionType.ToLower() == "emailselectedusers")
                {
                    // send emails
                    var review = _reviewService.GetInstititionsReview(reviewEdit.InstitutionId, reviewEdit.Review.Id);
                    var model = GetReviewEditModel(reviewEdit.InstitutionId, reviewEdit.Review.Id, null);
                    if (review.ReviewResources != null && review.ReviewResources.Any())
                    {
                        var msg = _reviewModelService.SendReviewEmailToUsers(reviewEdit.SelectedExpertReviewerUserIds,
                            expertReviewer, review);
                        model.ActionResultsMessage = $"Review email sent to {msg}";
                    }
                    else
                    {
                        model.ActionResultsMessage = @"Cannot Send Email until the review list contains Resources";
                    }


                    return View(model);
                }

                //List<User> expertReviewer = _userService.GetExpertReviewers(reviewEdit.InstitutionId);

                var reviewId = _reviewService.SaveReviewList(reviewEdit.Review.Id, reviewEdit.InstitutionId,
                    reviewEdit.Review.Name,
                    reviewEdit.Review.Description, reviewEdit.SelectedExpertReviewerUserIds,
                    AuthenticatedInstitution.User.Id, expertReviewer);

                return RedirectToAction("Detail",
                    new { reviewEdit.InstitutionId, reviewId, resourceTitleRemoved = "" });
            }

            return RedirectToAction("List", new { reviewEdit.InstitutionId });
        }

        public ActionResult Delete(int institutionId, int reviewId)
        {
            if (reviewId != 0)
            {
                _reviewService.DeleteReviewList(reviewId, institutionId, AuthenticatedInstitution.User.Id);
            }

            return RedirectToAction("List", new { institutionId });
        }

        /// <summary>
        /// </summary>
        public ActionResult DeleteResource(ReviewQuery reviewQuery, int resourceId, int reviewResourceId)
        {
            _reviewService.DeleteReviewResource(reviewResourceId, reviewQuery.ReviewId, resourceId,
                AuthenticatedInstitution.User.Id);

            var resource = _resourceService.GetResource(resourceId);

            return RedirectToAction("Detail",
                new { reviewQuery.InstitutionId, reviewQuery.ReviewId, resourceTitleRemoved = resource.Title });
        }

        public ActionResult AddResource(ReviewQuery reviewQuery, int resourceId)
        {
            _reviewService.AddReviewResource(reviewQuery.ReviewId, resourceId, AuthenticatedInstitution.User.Id);
            return RedirectToAction("Resources", reviewQuery.ToRouteValues());
        }

        public ActionResult BulkAddResources(ReviewQuery reviewQuery, string bulkAddToReview)
        {
            var model = new BulkAddToReview { ResourceQuery = reviewQuery };
            List<string> isbnsNotFound;
            var reviewResourcesModel = _reviewModelService.GetReviewResources(reviewQuery);

            foreach (InstitutionResource institutionResource in reviewResourcesModel.Resources)
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


            if (bulkAddToReview == "yes")
            {
                var resourceIds = reviewQuery.Resources.Split(',').Select(int.Parse).ToArray();
                _reviewService.AddReviewResources(reviewQuery.ReviewId, resourceIds, AuthenticatedInstitution.User.Id);

                return View("BulkAddResourcesConfirm", model);
            }

            return View(model);
        }

        public ActionResult Resources(ReviewQuery reviewQuery)
        {
            var reviewManagement = _reviewModelService.GetReviewResources(reviewQuery);

            SetReviewResourcePaging(reviewManagement, reviewQuery, reviewManagement.TotalCount);

            return View(reviewManagement);
        }

        [HttpPost]
        public ActionResult Resources(ReviewQuery reviewQuery, EmailPage emailPage)
        {
            object json;

            try
            {
                if (emailPage.To == null)
                {
                    return RedirectToAction("Resources", reviewQuery.ToRouteValues());
                }

                json = new JsonResponse { Status = "success", Successful = true };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private void SetReviewResourcePaging(InstitutionResources reviewManagement, IReviewQuery reviewQuery,
            int resourceCount)
        {
            var pageCount = resourceCount / reviewQuery.PageSize +
                            (resourceCount % reviewQuery.PageSize > 0 ? 1 : 0);

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || reviewQuery.Page <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = reviewQuery.Page - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            reviewManagement.PreviousLink = Url.PreviousPageLink(reviewQuery, pageCount);
            reviewManagement.NextLink = Url.NextPageLink(reviewQuery, pageCount);

            reviewManagement.FirstLink = Url.FirstPageLink(reviewQuery, pageCount);
            reviewManagement.LastLink = Url.LastPageLink(reviewQuery, pageCount);

            var pageLinks = new List<PageLink>();
            for (var p = firstPage; p <= lastPage; p++)
            {
                pageLinks.Add(new PageLink
                    { Selected = p == reviewQuery.Page, Text = p.ToString(CultureInfo.InvariantCulture) });
            }

            reviewManagement.PageLinks = pageLinks;
        }
    }
}