#region

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Core.Promotion;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Resource;
using R2V2.Web.Areas.Admin.Models.Special;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using Resource = R2V2.Web.Areas.Admin.Models.Resource.Resource;

#endregion

namespace R2V2.Web.Services
{
    public class ResourceListService
    {
        protected const int MaxPages = 9;
        private readonly ICollectionService _collectionService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ResourcePromotionService _promotionService;
        private readonly SpecialDiscountResourceService _specialDiscountResourceService;
        private readonly ISpecialtyService _specialtyService;
        private readonly Core.UserService _userService;
        private readonly IWebImageSettings _webImageSettings;

        public ResourceListService(IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , ICollectionService collectionService
            , ResourcePromotionService promotionService
            , Core.UserService userService
            , SpecialDiscountResourceService specialDiscountResourceService
            , IWebImageSettings webImageSettings
        )
        {
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
            _promotionService = promotionService;
            _userService = userService;
            _specialDiscountResourceService = specialDiscountResourceService;
            _webImageSettings = webImageSettings;
        }

        public ResourcesList GetResourcesList(ResourceQuery resourceQuery, IEnumerable<IResource> resources,
            List<IFeaturedTitle> featuredTitles, UrlHelper urlHelper)
        {
            var resourcesArray = resources as IResource[] ?? resources.ToArray();
            var resourceCount = resourcesArray.Count();

            if (resourceCount > resourceQuery.Page * resourceQuery.PageSize)
            {
                resources = resourcesArray.Skip((resourceQuery.Page - 1) * resourceQuery.PageSize)
                    .Take(resourceQuery.PageSize);
            }
            else if (resourceCount < resourceQuery.Page * resourceQuery.PageSize &&
                     resourceCount > resourceQuery.PageSize)
            {
                var takeSize = resourceCount - (resourceQuery.Page - 1) * resourceQuery.PageSize;
                resources = resourcesArray.Skip(resourceCount - takeSize);
            }
            else
            {
                resources = resourcesArray.Skip((resourceQuery.Page - 1) * resourceQuery.PageSize);
            }

            var resourcesList = resources.ToList();

            var currentCount = (resourceQuery.Page - 1) * resourceQuery.PageSize;

            var resourceSpecials = _specialDiscountResourceService.GetSpecialResourcesForAdminResource();

            var model = new ResourcesList
            {
                ResourceQuery = resourceQuery,
                TotalCount = resourceCount,
                SpecialIconBaseUrl = _webImageSettings.SpecialIconBaseUrl,
                IsRittenhouseAdmin = true,
                PreviousLink = urlHelper.PreviousPageLink(resourceQuery, resourceCount),
                NextLink = urlHelper.NextPageLink(resourceQuery, resourceCount),
                FirstLink = urlHelper.FirstPageLink(resourceQuery),
                LastLink = urlHelper.LastPageLink(resourceQuery, resourceCount),
                ResultsFirstItem = currentCount == 0 ? 0 : currentCount + 1,
                ResultsLastItem = currentCount + resourcesList.Count(),
                SelectedFilters =
                    resourceQuery.ToSelectedFilters(_practiceAreaService, _specialtyService, _collectionService,
                        AdminBaseModel.GetSortByDescription(resourceQuery.SortBy))
            };

            SetResources(model, resources, featuredTitles, resourceSpecials);

            return model;
        }


        private void SetResources(ResourcesList resourcesList, IEnumerable<IResource> resources,
            List<IFeaturedTitle> featuredTitles, List<SpecialResourceModel> specialResources)
        {
            var pageCount = resourcesList.TotalCount / resourcesList.ResourceQuery.PageSize +
                            (resourcesList.TotalCount % resourcesList.ResourceQuery.PageSize > 0 ? 1 : 0);

            var resourcePromoteQueues = _promotionService.GetResourcePromoteQueue();
            var users = _userService.GetRaUsersWhoCanPromote().ToList();

            foreach (var resource in resources)
            {
                IFeaturedTitle featuredTitle = null;
                if (featuredTitles != null)
                {
                    featuredTitle = featuredTitles.FirstOrDefault(ft => ft.ResourceId == resource.Id);
                }

                SpecialResourceModel specialResource = null;
                if (specialResources != null)
                {
                    specialResource = specialResources.FirstOrDefault(x => x.ResourceId == resource.Id);
                }

                var resourceModel = specialResource != null
                    ? new Resource(resource, featuredTitle, specialResource.SpecialText, specialResource.IconName)
                    : new Resource(resource, featuredTitle, null, null);

                resourceModel.PromotionQueueStatus =
                    GetResourcePromotionQueueStatus(resource.Id, resourcePromoteQueues, users);

                resourcesList.AddResource(resourceModel);
            }

            resourcesList.PageLinks = GetPageLinks(resourcesList.ResourceQuery.Page, pageCount);
        }

        public string GetResourcePromotionQueueStatus(int resourceId, IList<ResourcePromoteQueue> resourcePromoteQueues,
            List<User> users)
        {
            var resourcePromotionQueue = resourcePromoteQueues.FirstOrDefault(x => x.ResourceId == resourceId);

            if (resourcePromotionQueue == null)
            {
                return null;
            }

            var user = users.FirstOrDefault(x => x.Id == resourcePromotionQueue.AddedByUserId);
            return string.Format("Added to queue by {0} on {1}",
                user == null ? $"User Id: {resourcePromotionQueue.AddedByUserId}" : user.ToFullName(),
                resourcePromotionQueue.CreationDate);
        }

        protected static IEnumerable<PageLink> GetPageLinks(int currentPage, int pageCount)
        {
            int lastPage;
            int firstPage;

            if (pageCount <= MaxPages || currentPage <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = currentPage - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            for (var p = firstPage; p <= lastPage; p++)
            {
                yield return new PageLink
                    { Selected = p == currentPage, Text = p.ToString(CultureInfo.InvariantCulture) };
            }
        }
    }
}