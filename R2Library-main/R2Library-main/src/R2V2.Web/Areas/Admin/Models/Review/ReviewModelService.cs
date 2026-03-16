#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Email;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewModelService
    {
        private readonly IAdminContext _adminContext;
        private readonly ICartService _cartService;
        private readonly IClientSettings _clientSettings;
        private readonly ICollectionManagementService _collectionManagementService;
        private readonly ICollectionService _collectionService;
        private readonly RecommendationEmailBuildService _emailBuildService;
        private readonly EmailQueueService _emailQueueService;
        private readonly ILog<ReviewModelService> _log;
        private readonly OrderService _orderService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly RecommendationsService _recommendationsService;
        private readonly ReviewService _reviewService;
        private readonly ISearchService _searchService;
        private readonly ISpecialtyService _specialtyService;
        private readonly IWebImageSettings _webImageSettings;

        public ReviewModelService(ILog<ReviewModelService> log
            , IAdminContext adminContext
            , ISpecialtyService specialtyService
            , IPracticeAreaService practiceAreaService
            , IClientSettings clientSettings
            , OrderService orderService
            , ICollectionManagementService collectionManagementService
            , RecommendationsService recommendationsService
            , ReviewService reviewService
            , ICartService cartService
            , RecommendationEmailBuildService emailBuildService
            , EmailQueueService emailQueueService
            , IWebImageSettings webImageSettings
            , ICollectionService collectionService
            , ISearchService searchService
        )
        {
            _log = log;
            _adminContext = adminContext;
            _specialtyService = specialtyService;
            _practiceAreaService = practiceAreaService;
            _clientSettings = clientSettings;
            _orderService = orderService;
            _collectionManagementService = collectionManagementService;
            _recommendationsService = recommendationsService;
            _reviewService = reviewService;
            _cartService = cartService;
            _emailBuildService = emailBuildService;
            _emailQueueService = emailQueueService;
            _webImageSettings = webImageSettings;
            _collectionService = collectionService;
            _searchService = searchService;
        }

        public ReviewResources GetReviewResources(ReviewQuery reviewQuery)
        {
            var institution = _adminContext.GetAdminInstitution(reviewQuery.InstitutionId);
            if (institution == null)
            {
                return null;
            }

            int[] ids = { };
            var review = _reviewService.GetInstititionsReview(reviewQuery.InstitutionId, reviewQuery.ReviewId);
            if (review != null)
            {
                ids = review.ReviewResources.Select(x => x.ResourceId).ToArray();
            }

            var order = _orderService.GetOrderForInstitution(institution.Id);

            IEnumerable<CollectionManagementResource> collectionManagementResources = _collectionManagementService
                .GetCollectionManagementResourcesExcludeIds(reviewQuery, order, ids).ToList();

            int resourceCount;
            if (reviewQuery.Resources != null)
            {
                var resourceIds = reviewQuery.Resources.Split(',').Select(int.Parse);

                collectionManagementResources =
                    collectionManagementResources.Where(x => resourceIds.Contains(x.Resource.Id));

                resourceCount = collectionManagementResources.Count();
            }
            else
            {
                resourceCount = collectionManagementResources.Count();
                collectionManagementResources = resourceCount > reviewQuery.Page * reviewQuery.PageSize
                    ? collectionManagementResources.Skip((reviewQuery.Page - 1) * reviewQuery.PageSize)
                        .Take(reviewQuery.PageSize)
                    : collectionManagementResources.Skip((reviewQuery.Page - 1) * reviewQuery.PageSize);
            }

            var reviewResources = GetReviewResources(review, collectionManagementResources, institution);

            var reviewManagement = new ReviewResources(institution, reviewQuery, reviewResources, review,
                _practiceAreaService,
                _specialtyService, _collectionService, _clientSettings.DoodyReviewLink,
                _webImageSettings.SpecialIconBaseUrl);

            var currentCount = (reviewQuery.Page - 1) * reviewQuery.PageSize;
            reviewManagement.ResultsFirstItem = currentCount + 1;
            reviewManagement.ResultsLastItem = currentCount + reviewManagement.Resources.Count();

            reviewManagement.TotalCount = resourceCount;

            return reviewManagement;
        }

        private IEnumerable<ReviewResource> GetReviewResources(Core.Recommendations.Review review,
            IEnumerable<CollectionManagementResource> collectionManagementResources, IAdminInstitution adminInstitution)
        {
            return collectionManagementResources.Select(x => CreateReviewResource(x, review, adminInstitution));
        }

        private ReviewResource CreateReviewResource(CollectionManagementResource collectionManagementResource,
            Core.Recommendations.Review review, IAdminInstitution adminInstitution)
        {
            IEnumerable<Recommendation> recommendations =
                _recommendationsService.GetRecommendations(adminInstitution.Id);

            var reviewResource =
                review.ReviewResources.FirstOrDefault(x => x.ResourceId == collectionManagementResource.Resource.Id);

            return reviewResource == null
                ? new ReviewResource(collectionManagementResource, adminInstitution, recommendations, false, 0)
                : new ReviewResource(collectionManagementResource, adminInstitution, recommendations, true,
                    reviewResource.Id);
        }

        public ReviewResource GetReviewResource(int institutionId, int resourceId, bool isSelected,
            int reviewResourceId)
        {
            var cart = _cartService.GetInstitutionCartFromCache(institutionId);
            var collectionManagementResource =
                _collectionManagementService.GetCollectionManagementResource(institutionId, resourceId, cart);
            return collectionManagementResource == null
                ? null
                : GetReviewResource(institutionId, collectionManagementResource, isSelected, reviewResourceId);
        }

        public ReviewResource GetReviewResource(int institutionId,
            CollectionManagementResource collectionManagementResource, bool isSelected, int reviewResourceId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            IEnumerable<Recommendation> recommendations = _recommendationsService.GetRecommendations(institutionId);
            return collectionManagementResource.ToReviewResource(institution, recommendations, isSelected,
                reviewResourceId);
        }

        public string SendReviewEmailToUsers(int[] selectedExpertReviewerUserIds,
            List<Core.Authentication.User> expertReviewers, Core.Recommendations.Review review)
        {
            var msg = new StringBuilder();
            foreach (var expertReviewer in expertReviewers)
            {
                var selected = selectedExpertReviewerUserIds.Any(x => x == expertReviewer.Id);
                if (selected)
                {
                    _log.DebugFormat("send email to '{0}'", expertReviewer.Email);
                    msg.AppendFormat("{0}{1}", msg.Length == 0 ? "" : ", ", expertReviewer.Email);

                    var emailMessage = _emailBuildService.BuildReviewEmail(review, expertReviewer, false);
                    _emailQueueService.QueueEmailMessage(emailMessage);
                }
            }

            return msg.ToString();
        }
    }
}