#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    public class BaseActionsMenuBuilder
    {
        protected IEnumerable<PageLink> BuildResourceFilterTypeFilterByLinks(UrlHelper urlHelper, IResourceQuery iQuery,
            IEnumerable<ICollection> collections, string actionName, string controllerName, bool includeFeaturedTitles,
            bool includeRecent)
        {
            var collectionFilters = collections.ToDictionary(x => x.Id, x => x.Name);

            var filters = new Dictionary<ResourceFilterType, string>
            {
                { ResourceFilterType.SpecialOffer, ResourceFilterType.SpecialOffer.ToDescription() },
                { ResourceFilterType.ContainsVideo, ResourceFilterType.ContainsVideo.ToDescription() },
                { ResourceFilterType.FreeResources, ResourceFilterType.FreeResources.ToDescription() }
            };
            var resourceQuery = iQuery;

            var pageLinks = new List<PageLink>();

            foreach (var collectionFilter in collectionFilters)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.CollectionFilter = collectionFilter.Key;
                query.ResourceFilterType = 0;

                pageLinks.Add(new PageLink
                {
                    Text = collectionFilter.Value,
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.CollectionFilter == collectionFilter.Key &&
                               resourceQuery.ResourceFilterType == 0
                });
            }

            foreach (var filter in filters)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.CollectionFilter = 0;
                query.ResourceFilterType = filter.Key;
                query.RecentOnly = false;

                pageLinks.Add(new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.ResourceFilterType == filter.Key && resourceQuery.CollectionFilter == 0
                });
            }

            if (includeFeaturedTitles)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.CollectionFilter = 0;
                query.CollectionListFilter = 0;
                query.ResourceFilterType = ResourceFilterType.FeaturedTitles;
                query.RecentOnly = false;

                pageLinks.Add(new PageLink
                {
                    Text = "Featured Titles",
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.ResourceFilterType == ResourceFilterType.FeaturedTitles &&
                               resourceQuery.CollectionFilter == 0
                });
            }

            if (includeRecent)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.CollectionFilter = 0;
                query.CollectionListFilter = 0;
                query.ResourceFilterType = 0;
                query.RecentOnly = true;

                pageLinks.Add(new PageLink
                {
                    Text = "Recently Viewed",
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Selected = resourceQuery.RecentOnly && resourceQuery.ResourceFilterType == 0 &&
                               resourceQuery.CollectionFilter == 0
                });
            }

            return pageLinks;
        }

        protected IEnumerable<PageLink> BuildPracticeAreaFilterByLinks(UrlHelper urlHelper, IResourceQuery iQuery,
            IEnumerable<IPracticeArea> practiceAreas, string actionName, string controllerName)
        {
            var practiceAreaFilters =
                practiceAreas.ToDictionary(practiceArea => practiceArea.Id, practiceArea => practiceArea.Name);

            var resourceQuery = iQuery;

            var pageLinks = new List<PageLink>();

            foreach (var practiceAreaFilter in practiceAreaFilters)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.PracticeAreaFilter = practiceAreaFilter.Key;

                pageLinks.Add(new PageLink
                {
                    Text = practiceAreaFilter.Value,
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.PracticeAreaFilter == practiceAreaFilter.Key
                });
            }

            return pageLinks;
        }

        protected IEnumerable<PageLink> BuildSpecialtyFilterByLinks(UrlHelper urlHelper, IResourceQuery iQuery,
            IEnumerable<ISpecialty> specialties, string actionName, string controllerName)
        {
            var specialtyFilters = specialties.ToDictionary(x => x.Id, x => x.Name);

            var resourceQuery = iQuery;

            var pageLinks = new List<PageLink>();

            foreach (var specialtyFilter in specialtyFilters)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.SpecialtyFilter = specialtyFilter.Key;

                pageLinks.Add(new PageLink
                {
                    Text = specialtyFilter.Value,
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.SpecialtyFilter == specialtyFilter.Key
                });
            }

            return pageLinks;
        }

        protected IEnumerable<PageLink> BuildResourceStatusFilterByLinks(UrlHelper urlHelper, IResourceQuery iQuery,
            Dictionary<ResourceStatus, string> filters, string actionName, string controllerName)
        {
            var resourceQuery = iQuery;

            var pageLinks = new List<PageLink>();


            foreach (var filter in filters)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.ResourceStatus = filter.Key;

                pageLinks.Add(new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.ResourceStatus == filter.Key
                });
            }

            return pageLinks;
        }

        protected IEnumerable<PageLink> BuildPdaStatusFilterByLinks(UrlHelper urlHelper, IResourceQuery iQuery,
            Dictionary<PdaStatus, string> filters, string actionName, string controllerName)
        {
            var resourceQuery = iQuery;

            var pageLinks = new List<PageLink>();

            foreach (var filter in filters)
            {
                var query = GetResourceQueryForFilter(resourceQuery);
                query.PdaStatus = filter.Key;

                pageLinks.Add(new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action(actionName, controllerName, query.ToRouteValues()),
                    Active = true,
                    Selected = resourceQuery.PdaStatus == filter.Key
                });
            }

            return pageLinks;
        }

        protected IEnumerable<SortLink> BuildSortByLinks(UrlHelper urlHelper, IResourceQuery iQuery,
            Dictionary<string, string> sorts, string actionName, string controllerName)
        {
            var resourceQuery = iQuery;
            var sortLinks = new List<SortLink>();

            foreach (var sort in sorts)
            {
                var query = GetResourceQueryForSort(resourceQuery);
                query.SortBy = sort.Key;

                sortLinks.Add(new SortLink
                {
                    Text = sort.Value,
                    HrefAscending = urlHelper.Action(actionName, controllerName,
                        query.ToRouteValues(SortDirection.Ascending)),
                    HrefDescending = urlHelper.Action(actionName, controllerName,
                        query.ToRouteValues(SortDirection.Descending)),
                    Selected = resourceQuery.SortBy == sort.Key
                });
            }

            return sortLinks;
        }


        private ResourceQuery GetResourceQueryForFilter(IResourceQuery resourceQuery)
        {
            return new ResourceQuery
            {
                Query = resourceQuery.Query,
                SortBy = resourceQuery.SortBy,
                Page = 1,
                PageSize = resourceQuery.PageSize,
                ResourceStatus = resourceQuery.ResourceStatus,
                PracticeAreaFilter = resourceQuery.PracticeAreaFilter,
                SpecialtyFilter = resourceQuery.SpecialtyFilter,
                CollectionFilter = resourceQuery.CollectionFilter,
                CollectionListFilter = resourceQuery.CollectionListFilter,
                ReserveShelfId = resourceQuery.ReserveShelfId,
                PurchasedOnly = resourceQuery.PurchasedOnly,
                IncludePdaResources = resourceQuery.IncludePdaResources,
                IncludePdaHistory = resourceQuery.IncludePdaHistory,
                IncludeSpecialDiscounts = resourceQuery.IncludeSpecialDiscounts,
                IncludeFreeResources = resourceQuery.IncludeFreeResources,
                RecentOnly = resourceQuery.RecentOnly,
                ResourceFilterType = resourceQuery.ResourceFilterType,
                ReviewId = resourceQuery.ReviewId,
                RecommendationsOnly = resourceQuery.RecommendationsOnly,
                PublisherId = resourceQuery.PublisherId,
                PdaStatus = resourceQuery.PdaStatus
            };
        }

        private ResourceQuery GetResourceQueryForSort(IResourceQuery resourceQuery)
        {
            var query = GetResourceQueryForFilter(resourceQuery);
            query.SortDirection = resourceQuery.SortDirection;
            query.PageSize = resourceQuery.PageSize;
            query.Page = resourceQuery.Page;
            query.DateRangeStart = resourceQuery.DateRangeStart;
            query.DateRangeEnd = resourceQuery.DateRangeEnd;
            query.PublisherId = resourceQuery.PublisherId;
            return query;
        }
    }
}