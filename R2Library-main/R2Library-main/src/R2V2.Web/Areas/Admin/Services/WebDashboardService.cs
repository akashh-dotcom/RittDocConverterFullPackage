#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core;
using R2V2.Core.Admin;
using R2V2.Core.Cms;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource.Discipline;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.Dashboard;
using R2V2.Web.Areas.Admin.Models.Order;
using R2V2.Web.Infrastructure.Settings;
using InstitutionResource = R2V2.Web.Areas.Admin.Models.CollectionManagement.InstitutionResource;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class WebDashboardService
    {
        private readonly IClientSettings _clientSettings;
        private readonly CmsService _cmsService;
        private readonly CollectionManagementService _collectionManagementService;
        private readonly DashboardService _dashboardService;
        private readonly InstitutionService _institutionService;
        private readonly OrderService _orderService;
        private readonly RecommendationsService _recommendationsService;
        private readonly ISpecialtyService _specialtyService;

        public WebDashboardService(
            DashboardService dashboardService
            , InstitutionService institutionService
            , CollectionManagementService collectionManagementService
            , OrderService orderService
            , IClientSettings clientSettings
            , RecommendationsService recommendationsService
            , ISpecialtyService specialtyService
            , CmsService cmsService
        )
        {
            _dashboardService = dashboardService;
            _institutionService = institutionService;
            _collectionManagementService = collectionManagementService;
            _orderService = orderService;
            _clientSettings = clientSettings;
            _recommendationsService = recommendationsService;
            _specialtyService = specialtyService;
            _cmsService = cmsService;
        }

        /// <summary>
        ///     This will return only the highlights and account usage
        /// </summary>
        public DashboardModel GetBaseDashBoard(int institutionId, DateTime startDate, DateTime endDate)
        {
            var model = new DashboardModel();

            if (institutionId > 0)
            {
                var institution = new AdminInstitution(_institutionService.GetInstitutionForAdmin(institutionId));

                var highlights = _dashboardService.GetHighlights(institutionId, startDate, endDate);
                var accountUsage = _dashboardService.GetAccountUsage(institutionId, startDate, endDate);

                var stats = new InstitutionDashboardStatistics(institutionId, startDate, endDate, accountUsage,
                    highlights);

                var order = _orderService.GetOrderForInstitution(institutionId);
                var collectionManagementResources =
                    _collectionManagementService.GetCollectionManagementResources(stats.GetAllResourceIds(), order);

                var notes = _cmsService.GetDashboardQuickNotes();
                //notes = _dashboardService.GetQuickNoteTextList(_clientSettings.CmsHtmlContentUrl);

                model = new DashboardModel(institution, stats,
                    collectionManagementResources.ToInstitutionResources(institution, null).ToList(), notes, false);
                PopulateContentSpotLight(model, order, institution);

                var specialties = _specialtyService.GetAllSpecialties();
                if (specialties != null)
                {
                    var mostPopulateSpecialty =
                        specialties.FirstOrDefault(x => x.Name == model.MostPopularSpecialtyName);
                    var leastPopulateSpecialty =
                        specialties.FirstOrDefault(x => x.Name == model.LeastPopularSpecialtyName);

                    model.MostPopularSpecialtyId = mostPopulateSpecialty != null ? mostPopulateSpecialty.Id : 0;
                    model.LeastPopularSpecialtyId = leastPopulateSpecialty != null ? leastPopulateSpecialty.Id : 0;
                }
            }

            return model;
        }

        private void PopulateContentSpotLight(DashboardModel model, Order order, IAdminInstitution institution)
        {
            var featuredTitles = _collectionManagementService.GetCollectionManagementResources(
                new CollectionManagementQuery
                {
                    ResourceListType = ResourceListType.FeaturedTitles, SortBy = "releasedate",
                    SortDirection = SortDirection.Descending
                }, order);

            if (featuredTitles != null)
            {
                var featuredTitlesCount = featuredTitles.Count();
                if (featuredTitlesCount > 4)
                {
                    featuredTitles = featuredTitles.OrderByDescending(x => x.Resource.ReleaseDate).Take(4);
                }
                else if (featuredTitlesCount > 2)
                {
                    featuredTitles = featuredTitles.OrderByDescending(x => x.Resource.ReleaseDate).Take(2);
                }
            }

            var specials = _collectionManagementService.GetCollectionManagementResources(
                new CollectionManagementQuery
                {
                    ResourceFilterType = ResourceFilterType.SpecialOffer, SortBy = "releasedate",
                    SortDirection = SortDirection.Descending
                }, order);

            if (specials != null)
            {
                var specialsCount = specials.Count();
                if (specialsCount > 4)
                {
                    specials = specials.OrderByDescending(x => x.Resource.ReleaseDate).Take(4);
                }
                else if (specialsCount > 2)
                {
                    specials = specials.OrderByDescending(x => x.Resource.ReleaseDate).Take(2);
                }
            }

            var recommendations = _recommendationsService.GetRecommendations(model.InstitutionId);
            List<InstitutionResource> recommendedInstitutionResources = null;
            if (recommendations != null)
            {
                recommendations = recommendations.Where(x => x.PurchaseDate == null)
                    .OrderByDescending(y => y.CreationDate).ToList();
                var recommendationsCount = recommendations.Count();
                if (recommendationsCount > 4)
                {
                    recommendations = recommendations.Take(4).ToList();
                }
                else if (recommendationsCount > 2)
                {
                    recommendations = recommendations.Take(2).ToList();
                }

                var recommendationResourceIds = recommendations.Select(x => x.ResourceId);
                var recommendedCollectionManagementResource =
                    _collectionManagementService.GetCollectionManagementResources(recommendationResourceIds.ToArray(),
                        order);

                recommendedInstitutionResources = recommendedCollectionManagementResource
                    .ToInstitutionResources(institution, recommendations).ToList();
            }

            model.PopulateContentSpotLight(featuredTitles.ToInstitutionResources(institution, null).ToList(),
                specials.ToInstitutionResources(institution, null).ToList(), recommendedInstitutionResources);
        }

        /// <summary>
        ///     This will return only the PDA collection
        /// </summary>
        public DashboardModel GetPdaDashBoard(int institutionId, DateTime startDate, DateTime endDate)
        {
            var institution = new AdminInstitution(_institutionService.GetInstitutionForAdmin(institutionId));

            var resourceStatistics =
                _dashboardService.GetResourceStatisticsList(institutionId, startDate, endDate, true);
            var highlights = _dashboardService.GetHighlights(institutionId, startDate, endDate);

            var stats = new InstitutionDashboardStatistics(institutionId, startDate, endDate, resourceStatistics,
                highlights != null ? highlights.TotalResourceCount : 0);

            var order = _orderService.GetOrderForInstitution(institutionId);
            var collectionManagementResources =
                _collectionManagementService.GetCollectionManagementResources(stats.GetAllResourceIds(), order);

            var notes = _cmsService.GetDashboardQuickNotes();

            var recommendations = _recommendationsService.GetRecommendations(institutionId);

            return new DashboardModel(institution, stats,
                collectionManagementResources.ToInstitutionResources(institution, recommendations).ToList(), notes,
                true);
        }

        /// <summary>
        ///     This will return only the Purchased collection
        /// </summary>
        public DashboardModel GetEbookDashBoard(int institutionId, DateTime startDate, DateTime endDate)
        {
            var institution = new AdminInstitution(_institutionService.GetInstitutionForAdmin(institutionId));

            var resourceStatistics = _dashboardService.GetResourceStatisticsList(institutionId, startDate, endDate);
            var highlights = _dashboardService.GetHighlights(institutionId, startDate, endDate);

            var stats = new InstitutionDashboardStatistics(institutionId, startDate, endDate, resourceStatistics,
                highlights != null ? highlights.TotalResourceCount : 0);

            var order = _orderService.GetOrderForInstitution(institutionId);
            var collectionManagementResources =
                _collectionManagementService.GetCollectionManagementResources(stats.GetAllResourceIds(), order);

            var notes = _cmsService.GetDashboardQuickNotes();

            var recommendations = _recommendationsService.GetRecommendations(institutionId);

            return new DashboardModel(institution, stats,
                collectionManagementResources.ToInstitutionResources(institution, recommendations).ToList(), notes,
                true);
        }
    }
}