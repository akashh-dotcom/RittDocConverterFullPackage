#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.Resource
{
    public static class ResourceExtensions
    {
        public static IEnumerable<IResource> FilterBy(this IEnumerable<IResource> resources, int publisherId)
        {
            if (publisherId > 0)
            {
                return resources.Where(x => x.PublisherId == publisherId ||
                                            (x.Publisher.ConsolidatedPublisher != null &&
                                             x.Publisher.ConsolidatedPublisher.Id == publisherId)
                );
            }

            return resources;
        }

        public static IEnumerable<IResource> FilterBy(this IEnumerable<IResource> resources,
            ResourceStatus resourceStatus)
        {
            switch (resourceStatus)
            {
                case ResourceStatus.Active:
                    return resources.Where(x => x.StatusId == (int)ResourceStatus.Active);
                case ResourceStatus.Archived:
                    return resources.Where(x => x.StatusId == (int)ResourceStatus.Archived);
                case ResourceStatus.Forthcoming:
                    return resources.Where(x => x.StatusId == (int)ResourceStatus.Forthcoming);
                case ResourceStatus.QANotApproved:
                    return resources.Where(x => x.StatusId == (int)ResourceStatus.Active && x.QaApprovalDate == null);
                case ResourceStatus.NotPromoted:
                    return resources.Where(x =>
                        x.StatusId == (int)ResourceStatus.Active && x.LastPromotionDate == null &&
                        x.QaApprovalDate != null);
                default:
                    return resources;
            }
        }

        public static IEnumerable<IResource> FilterBy(this IEnumerable<IResource> resources,
            ResourceFilterType resourceFilterType, List<CachedSpecialResource> specialResourceDiscounts)
        {
            switch (resourceFilterType)
            {
                case ResourceFilterType.SpecialOffer:
                    return specialResourceDiscounts != null
                        ? resources.Where(x => specialResourceDiscounts.Select(y => y.ResourceId).Contains(x.Id))
                        : resources;
                case ResourceFilterType.ContainsVideo:
                    return resources.Where(x => x.ContainsVideo > 0);
                case ResourceFilterType.FreeResources:
                    return resources.Where(x => x.IsFreeResource);
                default:
                    return resources;
            }
        }

        public static IEnumerable<IResource> CollectionFilterBy(this IEnumerable<IResource> resources,
            int collectionFilter)
        {
            if (collectionFilter > 0)
            {
                return resources.Where(x => x.Collections.Select(i => i.Id).Contains(collectionFilter));
            }

            return resources;
        }

        public static IEnumerable<IResource> CollectionFilterBy(this IEnumerable<IResource> resources,
            int[] collectionFilter)
        {
            return collectionFilter != null && collectionFilter.Any()
                ? resources.Where(x => x.Collections.Select(i => i.Id).Any(collectionFilter.Contains))
                : resources;
        }

        public static IEnumerable<IResource> CollectionListFilterBy(this IEnumerable<IResource> resources,
            int collectionListId)
        {
            return collectionListId > 0
                ? resources.Where(x => x.Collections.Select(i => i.Id).Contains(collectionListId) &&
                                       (x.StatusId == (int)ResourceStatus.Active ||
                                        x.StatusId == (int)ResourceStatus.Forthcoming))
                : resources;
        }

        public static IEnumerable<IResource> PracticeAreaFilterBy(this IEnumerable<IResource> resources,
            int practiceAreaFilter)
        {
            return practiceAreaFilter > 0
                ? resources.Where(x => x.PracticeAreas.Select(i => i.Id).Contains(practiceAreaFilter))
                : resources;
        }

        public static IEnumerable<IResource> PracticeAreaFilterBy(this IEnumerable<IResource> resources,
            int[] practiceAreaFilter)
        {
            return practiceAreaFilter != null && practiceAreaFilter.Any()
                ? resources.Where(x => x.PracticeAreas.Select(i => i.Id).Any(practiceAreaFilter.Contains))
                : resources;
        }

        public static IEnumerable<IResource> FreeResourcesFilterBy(this IEnumerable<IResource> resources,
            bool includeFreeResources)
        {
            return includeFreeResources
                ? resources.Where(x => x.IsFreeResource)
                : resources;
        }

        public static IEnumerable<IResource> SpecialtyFilterBy(this IEnumerable<IResource> resources,
            int specialtyFilter)
        {
            return specialtyFilter > 0
                ? resources.Where(x => x.Specialties.Select(i => i.Id).Contains(specialtyFilter))
                : resources;
        }

        public static IEnumerable<IResource> SpecialtyFilterBy(this IEnumerable<IResource> resources,
            int[] specialtyFilter)
        {
            return specialtyFilter != null && specialtyFilter.Any()
                ? resources.Where(x => x.Specialties.Select(i => i.Id).Any(specialtyFilter.Contains))
                : resources;
        }


        public static IEnumerable<IResource> SpecialDiscountResourcesFilterBy(this IEnumerable<IResource> resources,
            bool includeSpecialDiscounts, List<CachedSpecialResource> specialResourceDiscounts)
        {
            return specialResourceDiscounts != null && includeSpecialDiscounts
                ? resources.Where(x =>
                    specialResourceDiscounts.Select(y => y.ResourceId).Contains(x.Id) &&
                    (x.StatusId == (int)ResourceStatus.Active || x.StatusId == (int)ResourceStatus.Forthcoming))
                : resources;
        }

        public static IEnumerable<IResource> RecentResourcesFilterBy(this IEnumerable<IResource> resources,
            int[] recentResources, bool recentOnly)
        {
            if (recentOnly)
            {
                if (recentResources != null)
                {
                    return resources.Where(x => recentResources.Contains(x.Id));
                }

                return new List<IResource>();
            }

            return resources;
        }

        public static IEnumerable<IResource> TurnawayResourcesFilterBy(this IEnumerable<IResource> resources,
            Dictionary<int, int> resourcesIdsAndCount)
        {
            if (resourcesIdsAndCount != null)
            {
                return resources.Where(x => resourcesIdsAndCount.Keys.Contains(x.Id))
                    .OrderByDescending(x => resourcesIdsAndCount.Values);
            }

            return new List<IResource>();
        }

        public static IEnumerable<IResource> MaxPriceFilterBy(this IEnumerable<IResource> resources, decimal maxPrice,
            decimal institutionDiscount)
        {
            return maxPrice > 0
                ? resources.Where(x => x.ListPrice <= maxPrice)
                : resources;
        }


        public static IEnumerable<IResource> OrderBy(this IEnumerable<IResource> resources,
            IResourceQuery resourceQuery)
        {
            switch (resourceQuery.SortBy)
            {
                case "status":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.StatusId)
                        : resources.OrderByDescending(x => x.StatusId);

                case "releasedate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.ReleaseDate)
                        : resources.OrderByDescending(x => x.ReleaseDate);

                case "title":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.SortTitle == null ? "z" : x.SortTitle.Trim())
                        : resources.OrderByDescending(x => x.SortTitle == null ? "a" : x.SortTitle.Trim());

                case "publisher":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.Publisher == null ? "z" : x.Publisher.Name.Replace("The", "").Trim())
                        : resources.OrderByDescending(x =>
                            x.Publisher == null ? "z" : x.Publisher.Name.Replace("The", "").Trim());

                case "author":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.SortAuthor == null ? "z" : x.SortAuthor.Trim())
                        : resources.OrderByDescending(x => x.SortAuthor == null ? "a" : x.SortAuthor.Trim());

                case "duedate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.Where(x => x.StatusId == (int)ResourceStatus.Forthcoming)
                            .OrderBy(x => x.ForthcomingDate ?? "99/99")
                        : resources.Where(x => x.StatusId == (int)ResourceStatus.Forthcoming)
                            .OrderByDescending(x => x.ForthcomingDate ?? "01/01");

                case "price":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.ListPrice)
                        : resources.OrderByDescending(x => x.ListPrice);

                case "publicationdate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.PublicationDate).ThenBy(x => x.ReleaseDate)
                        : resources.OrderByDescending(x => x.PublicationDate).ThenByDescending(x => x.ReleaseDate);
                case "qaapprovaldate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.QaApprovalDate)
                        : resources.OrderByDescending(x => x.QaApprovalDate);
                case "promotiondate":
                    return resourceQuery.SortDirection == SortDirection.Ascending
                        ? resources.OrderBy(x => x.LastPromotionDate)
                        : resources.OrderByDescending(x => x.LastPromotionDate);
                default:
                    return resources.OrderByDescending(x => x.PublicationDate).ThenByDescending(x => x.ReleaseDate);
            }
        }

        public static IEnumerable<IResource> OrderByRecent(this IEnumerable<IResource> resources,
            IResourceQuery resourceQuery, int[] recentResourceIds)
        {
            var resourcesToSort = resources != null ? resources.ToList() : null;
            if (resourcesToSort == null || !resourcesToSort.Any() || !resourceQuery.RecentOnly)
            {
                return resourcesToSort;
            }

            var resourceDictionary = new Dictionary<int, IResource>();

            for (var i = 0; i < recentResourceIds.Count(); i++)
            {
                resourceDictionary.Add(i, resourcesToSort.FirstOrDefault(x => x.Id == recentResourceIds[i]));
            }

            return resourceDictionary.OrderBy(x => x.Key).Where(x => x.Value != null).Select(x => x.Value)
                .AsEnumerable();
        }

        public static string StatusToString(this IResource resource)
        {
            switch ((ResourceStatus)resource.StatusId)
            {
                case ResourceStatus.Active:
                    return "Active";
                case ResourceStatus.Archived:
                    return "Archived";
                case ResourceStatus.Forthcoming:
                    return "Pre-Order";
                case ResourceStatus.Inactive:
                    return "Not Available";
                default:
                    return "";
            }
        }
    }
}