#region

using System;
using System.Web.Routing;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Promotion;
using R2V2.Core.Recommendations;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;
using R2V2.Web.Areas.Admin.Models.Dashboard;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    public static class QueryExtensions
    {
        #region Institution

        public static RouteValueDictionary ToRouteValues(this IInstitutionQuery institutionQuery, string page = null)
        {
            var routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" });

            if (page == null)
            {
                if (!string.IsNullOrEmpty(institutionQuery.Page))
                {
                    routeValueDictionary.Add("Page", institutionQuery.Page);
                }
            }
            else
            {
                routeValueDictionary.Add("Page", page);
            }


            if (!string.IsNullOrWhiteSpace(institutionQuery.Query))
            {
                routeValueDictionary.Add("Query", institutionQuery.Query);
            }

            if (!string.IsNullOrWhiteSpace(institutionQuery.SortBy))
            {
                routeValueDictionary.Add("SortBy", institutionQuery.SortBy);
            }

            routeValueDictionary.Add("SortDirection", institutionQuery.SortDirection);

            if (institutionQuery.AccountStatus > 0)
            {
                routeValueDictionary.Add("AccountStatus", institutionQuery.AccountStatus);
            }

            if (institutionQuery.TerritoryId > 0)
            {
                routeValueDictionary.Add("TerritoryId", institutionQuery.TerritoryId);
            }

            if (institutionQuery.InstitutionTypeId > 0)
            {
                routeValueDictionary.Add("InstitutionTypeId", institutionQuery.InstitutionTypeId);
            }

            if (institutionQuery.IncludeExpertReviewer)
            {
                routeValueDictionary.Add("IncludeExpertReviewer", true);
            }

            if (institutionQuery.ExcludeExpertReviewer)
            {
                routeValueDictionary.Add("ExcludeExpertReviewer", true);
            }

            if (page != "All" && (institutionQuery.Page == "Recent" || institutionQuery.RecentOnly))
            {
                routeValueDictionary.Add("RecentOnly", true);
            }

            return routeValueDictionary;
        }

        #endregion

        public static RouteValueDictionary ToRouteValues(this DashboardResource dashboardResource, string type)
        {
            var routeValueDictionary = new RouteValueDictionary
            {
                { "InstitutionId", dashboardResource.InstitutionId },
                { "DateRangeStart", dashboardResource.DateRangeStart.ToString("d") },
                { "DateRangeEnd", dashboardResource.DateRangeEnd.ToString("d") }
            };

            switch (type.ToLower())
            {
                case "purchased":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.Purchased);
                    break;
                case "archived":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.Archived);
                    break;
                case "neweditionpurchased":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.NewEditionPurchased);
                    break;
                case "pdaadded":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.PdaAdded);
                    break;
                case "pdaaddedtocart":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.PdaAddedToCart);
                    break;
                case "pdanewedition":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.PdaNewEdition);
                    break;
                case "featuredtitles":
                    routeValueDictionary.Add("ResourceListType", ResourceListType.FeaturedTitles);
                    break;
                case "specials":
                    routeValueDictionary.Add("IncludeSpecialDiscounts", true);
                    break;
                case "recommendations":
                    routeValueDictionary.Add("RecommendationsOnly", true);
                    break;
            }

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this DashboardResource dashboardResource, string type,
            string query)
        {
            var routeValueDictionary = dashboardResource.ToRouteValues(type);
            routeValueDictionary.Add("Query", query);

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this DashboardModel model, int specialtyId)
        {
            var routeValueDictionary = new RouteValueDictionary
            {
                { "InstitutionId", model.InstitutionId },
                { "DateRangeStart", model.DateRangeStart.ToString("d") },
                { "DateRangeEnd", model.DateRangeEnd.ToString("d") },
                { "SpecialtyFilter", specialtyId },
                { "SortBy", "releasedate" },
                { "SortDirection", "Descending" }
            };

            return routeValueDictionary;
        }

        #region Collection Management

        public static RouteValueDictionary ToMarcRouteValues(this ICollectionManagementQuery collectionManagementQuery,
            bool exportAll, bool isMarcDeleteRecord)
        {
            var routeValueDictionary = collectionManagementQuery.ToRouteValues(exportAll);

            if (exportAll)
            {
                routeValueDictionary.Remove("PageSize");
                routeValueDictionary.Add("PageSize", int.MaxValue);
            }

            if (isMarcDeleteRecord)
            {
                routeValueDictionary.Add("IsDeleteMarcRecord", true);
            }

            if (!string.IsNullOrWhiteSpace(collectionManagementQuery.Resources))
            {
                routeValueDictionary.Add("Resources", collectionManagementQuery.Resources);
            }

            return routeValueDictionary;
        }


        public static RouteValueDictionary ToRouteValues(this ICollectionManagementQuery collectionManagementQuery,
            bool toExport = false, bool isMarcDeleteRecord = false)
        {
            var resourceQuery = collectionManagementQuery as IResourceQuery;
            var routeValueDictionary = resourceQuery.ToRouteValues(toExport);

            if (collectionManagementQuery.InstitutionId > 0)
            {
                routeValueDictionary.Add("InstitutionId", collectionManagementQuery.InstitutionId);
            }

            if (collectionManagementQuery.ResourceListType != ResourceListType.All)
            {
                routeValueDictionary.Add("ResourceListType", collectionManagementQuery.ResourceListType);
            }


            if (collectionManagementQuery.CartId > 0)
            {
                routeValueDictionary.Add("CartId", collectionManagementQuery.CartId);
            }

            if (collectionManagementQuery.DateRangeStart != DateTime.MinValue)
            {
                routeValueDictionary.Add("DateRangeStart", $"{collectionManagementQuery.DateRangeStart:MM/dd/yyyy}");
            }

            if (collectionManagementQuery.DateRangeEnd != DateTime.MinValue)
            {
                routeValueDictionary.Add("DateRangeEnd", $"{collectionManagementQuery.DateRangeEnd:MM/dd/yyyy}");
            }

            if (collectionManagementQuery.TurnawayStartDate.HasValue)
            {
                routeValueDictionary.Add("TurnawayStartDate",
                    $"{collectionManagementQuery.TurnawayStartDate.Value:MM/dd/yyyy}");
            }

            if (collectionManagementQuery.TrialConvert)
            {
                routeValueDictionary.Add("TrialConvert", true);
            }

            if (collectionManagementQuery.EulaSigned)
            {
                routeValueDictionary.Add("EulaSigned", true);
            }

            if (collectionManagementQuery.PdaEulaSigned)
            {
                routeValueDictionary.Add("PdaEulaSigned", true);
            }

            if (collectionManagementQuery.IsPdaProfile)
            {
                routeValueDictionary.Add("IsPdaProfile", true);
            }


            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this ICollectionManagementQuery collectionManagementQuery,
            int resourceId)
        {
            ICollectionManagementQuery query = new CollectionManagementQuery(collectionManagementQuery);
            query.ResourceId = resourceId;
            return query.ToRouteValues();
        }

        public static RouteValueDictionary ToRouteValues(this ICollectionManagementQuery collectionManagementQuery,
            int resourceId, int reviewId)
        {
            ICollectionManagementQuery query = new CollectionManagementQuery(collectionManagementQuery);
            query.ResourceId = resourceId;
            query.ReviewId = reviewId;
            return query.ToRouteValues();
        }

        public static RouteValueDictionary ToDefaultQuery(this ICollectionManagementQuery query)
        {
            var routeValueDictionary = new RouteValueDictionary { { "InstitutionId", query.InstitutionId } };

            if (query.ResourceListType > 0)
            {
                routeValueDictionary.Add("ResourceListType", query.ResourceListType);
            }

            if (query.PurchasedOnly)
            {
                routeValueDictionary.Add("PurchasedOnly", query.PurchasedOnly);
            }

            if (query.IncludePdaResources)
            {
                routeValueDictionary.Add("IncludePdaResources", query.IncludePdaResources);
                if (query.IncludePdaHistory)
                {
                    routeValueDictionary.Add("IncludePdaHistory", query.IncludePdaHistory);
                }
            }

            if (query.IncludeSpecialDiscounts)
            {
                routeValueDictionary.Add("IncludeSpecialDiscounts", query.IncludeSpecialDiscounts);
            }

            if (query.RecommendationsOnly)
            {
                routeValueDictionary.Add("RecommendationsOnly", query.RecommendationsOnly);
            }

            if (query.DateRangeStart != DateTime.MinValue && query.DateRangeEnd != DateTime.MinValue)
            {
                routeValueDictionary.Add("DateRangeStart", query.DateRangeStart.ToString("d"));
                routeValueDictionary.Add("DateRangeEnd", query.DateRangeEnd.ToString("d"));
            }

            if (query.PublisherId > 0)
            {
                routeValueDictionary.Add("PublisherId", query.PublisherId);
            }

            if (query.IncludeFreeResources)
            {
                routeValueDictionary.Add("IncludeFreeResources", query.IncludeFreeResources);
            }

            if (query.CollectionListFilter > 0)
            {
                routeValueDictionary.Add("CollectionListFilter", query.CollectionListFilter);
            }

            return routeValueDictionary;
        }

        #endregion

        #region Reserve Shelf

        public static RouteValueDictionary ToRouteValues(this IReserveShelfQuery reserveShelfQuery)
        {
            var collectionManagementQuery = reserveShelfQuery as ICollectionManagementQuery;

            var routeValueDictionary = collectionManagementQuery.ToRouteValues();

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IReserveShelfQuery reserveShelfQuery, int resourceId,
            bool addInstitutionResource)
        {
            var collectionManagementQuery = reserveShelfQuery as ICollectionManagementQuery;

            collectionManagementQuery.ResourceId = resourceId;

            var routeValueDictionary = collectionManagementQuery.ToRouteValues();

            routeValueDictionary.Add("AddInstitutionResource", addInstitutionResource);

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IReserveShelfQuery reserveShelfQuery, string sPage)
        {
            var collectionManagementQuery = reserveShelfQuery as ICollectionManagementQuery;

            var routeValueDictionary = collectionManagementQuery.ToRouteValues();

            routeValueDictionary.Remove("Page");
            routeValueDictionary.Add("Page", int.Parse(sPage));

            return routeValueDictionary;
        }

        #endregion

        #region Resource

        public static RouteValueDictionary ToRouteValues(this IResourceQuery resourceQuery, SortDirection sortDirection)
        {
            var routeValueDictionary = resourceQuery.ToRouteValues();

            routeValueDictionary["SortDirection"] = sortDirection;

            var collectionManagementQuery = resourceQuery as ICollectionManagementQuery;
            if (collectionManagementQuery != null && collectionManagementQuery.ResourceListType != ResourceListType.All)
            {
                routeValueDictionary.Add("ResourceListType", collectionManagementQuery.ResourceListType);

                if (collectionManagementQuery.DateRangeStart != DateTime.MinValue)
                {
                    routeValueDictionary.Add("DateRangeStart", collectionManagementQuery.DateRangeStart);
                }

                if (collectionManagementQuery.DateRangeEnd != DateTime.MinValue)
                {
                    routeValueDictionary.Add("DateRangeEnd", collectionManagementQuery.DateRangeEnd);
                }
            }

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRecentRouteValues(this IResourceQuery resourceQuery)
        {
            var routeValueDictionary = resourceQuery.ToRouteValues();

            routeValueDictionary["RecentOnly"] = true;
            //routeValueDictionary["Page"] = 0;

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IResourceQuery resourceQuery, string sPage)
        {
            var routeValueDictionary = resourceQuery.ToRouteValues();

            routeValueDictionary["Page"] = int.Parse(sPage);

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IResourceQuery resourceQuery, string parameterName,
            int parameterValue)
        {
            var routeValueDictionary = resourceQuery.ToRouteValues();

            if (!routeValueDictionary.ContainsKey(parameterName))
            {
                routeValueDictionary.Add(parameterName, parameterValue);
            }

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IResourceQuery resourceQuery,
            OngoingPdaEventType ongoingPdaEventType)
        {
            var route = ToRouteValues(resourceQuery);
            route.Add("ongoingEventType", ongoingPdaEventType);
            return route;
        }

        // SJS - 2/1/2013 - why the hell didn't we create new methods instead of the optional parameters.  optional parameters seem like a hack to me!
        // If you wrote this and see this, fix it!
        // Also, can someone explain to me why these methods are not implemented inside the methods?
        public static RouteValueDictionary ToRouteValues(this IResourceQuery resourceQuery, bool toExport = false,
            bool excludeResourceId = false)
        {
            var routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" });

            if (!excludeResourceId && resourceQuery.ResourceId > 0)
            {
                routeValueDictionary.Add("ResourceId", resourceQuery.ResourceId);
            }

            if (!string.IsNullOrWhiteSpace(resourceQuery.Query))
            {
                routeValueDictionary.Add("Query", resourceQuery.Query);
            }

            if (!string.IsNullOrWhiteSpace(resourceQuery.SortBy))
            {
                routeValueDictionary.Add("SortBy", resourceQuery.SortBy);
            }

            if (resourceQuery.SortDirection == SortDirection.Descending)
            {
                routeValueDictionary.Add("SortDirection", resourceQuery.SortDirection);
            }


            if (!toExport)
            {
                if (resourceQuery.Page > 1)
                {
                    routeValueDictionary.Add("Page", resourceQuery.Page);
                }

                if (resourceQuery.PageSize > 10)
                {
                    routeValueDictionary.Add("PageSize", resourceQuery.PageSize);
                }
            }

            if (resourceQuery.ResourceStatus > 0)
            {
                routeValueDictionary.Add("ResourceStatus", resourceQuery.ResourceStatus);
            }

            if (resourceQuery.ResourceFilterType > 0)
            {
                routeValueDictionary.Add("ResourceFilterType", resourceQuery.ResourceFilterType);
            }

            if (resourceQuery.PurchasedOnly)
            {
                routeValueDictionary.Add("PurchasedOnly", resourceQuery.PurchasedOnly);
            }

            if (resourceQuery.IncludePdaResources)
            {
                routeValueDictionary.Add("IncludePdaResources", resourceQuery.IncludePdaResources);
            }

            if (resourceQuery.IncludePdaHistory)
            {
                routeValueDictionary.Add("IncludePdaHistory", resourceQuery.IncludePdaHistory);
            }

            if (resourceQuery.PracticeAreaFilter > 0)
            {
                routeValueDictionary.Add("PracticeAreaFilter", resourceQuery.PracticeAreaFilter);
            }

            if (resourceQuery.SpecialtyFilter > 0)
            {
                routeValueDictionary.Add("SpecialtyFilter", resourceQuery.SpecialtyFilter);
            }

            if (resourceQuery.CollectionFilter > 0)
            {
                routeValueDictionary.Add("CollectionFilter", resourceQuery.CollectionFilter);
            }

            if (resourceQuery.CollectionListFilter > 0)
            {
                routeValueDictionary.Add("CollectionListFilter", resourceQuery.CollectionListFilter);
            }

            if (resourceQuery.ReserveShelfId > 0)
            {
                routeValueDictionary.Add("ReserveShelfId", resourceQuery.ReserveShelfId);
            }

            if (resourceQuery.ReviewId > 0)
            {
                routeValueDictionary.Add("ReviewId", resourceQuery.ReviewId);
            }

            if (resourceQuery.RecentOnly)
            {
                routeValueDictionary.Add("RecentOnly", resourceQuery.RecentOnly);
            }

            if (resourceQuery.PublisherId > 0)
            {
                routeValueDictionary.Add("PublisherId", resourceQuery.PublisherId);
            }

            if (resourceQuery.RecommendationsOnly)
            {
                routeValueDictionary.Add("RecommendationsOnly", resourceQuery.RecommendationsOnly);
            }


            if (resourceQuery.IncludeSpecialDiscounts)
            {
                routeValueDictionary.Add("IncludeSpecialDiscounts", resourceQuery.IncludeSpecialDiscounts);
            }

            if (resourceQuery.IncludeFreeResources)
            {
                routeValueDictionary.Add("IncludeFreeResources", resourceQuery.IncludeFreeResources);
            }

            if (resourceQuery.PdaStatus != PdaStatus.None)
            {
                routeValueDictionary.Add("PdaStatus", resourceQuery.PdaStatus);
            }


            return routeValueDictionary;
        }

        #endregion

        #region User

        public static RouteValueDictionary ToRouteValues(this IUserQuery userQuery, SortDirection sortDirection)
        {
            var routeValueDictionary = userQuery.ToRouteValues();

            routeValueDictionary["SortDirection"] = sortDirection;

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IUserQuery userQuery, string sPage)
        {
            if (!string.IsNullOrWhiteSpace(sPage))
            {
                userQuery.Page = int.Parse(sPage);
            }

            return ToRouteValues(userQuery);
        }

        public static RouteValueDictionary ToRouteValues(this IUserQuery userQuery, bool toExport = false)
        {
            var routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" });

            if (userQuery.InstitutionId >= 0)
            {
                routeValueDictionary.Add("InstitutionId", userQuery.InstitutionId);
            }

            if (!string.IsNullOrWhiteSpace(userQuery.Query))
            {
                routeValueDictionary.Add("Query", userQuery.Query);
            }

            if (!string.IsNullOrWhiteSpace(userQuery.SortBy))
            {
                routeValueDictionary.Add("SortBy", userQuery.SortBy);
            }

            if (userQuery.UserStatus > 0)
            {
                routeValueDictionary.Add("UserStatus", userQuery.UserStatus);
            }

            if (userQuery.SortDirection != SortDirection.Ascending)
            {
                routeValueDictionary.Add("SortDirection", userQuery.SortDirection);
            }


            if (!toExport)
            {
                if (userQuery.Page > 0)
                {
                    routeValueDictionary.Add("Page", userQuery.Page);
                }

                if (userQuery.PageSize > 0)
                {
                    routeValueDictionary.Add("PageSize", userQuery.PageSize);
                }
            }


            if (userQuery.RoleCode > 0)
            {
                routeValueDictionary.Add("RoleCode", userQuery.RoleCode);
            }

            if (!string.IsNullOrWhiteSpace(userQuery.SearchType))
            {
                routeValueDictionary.Add("SearchType", userQuery.SearchType);
            }

            return routeValueDictionary;
        }

        #endregion

        #region Review List

        public static RouteValueDictionary ToRouteValues(this IReviewQuery query)
        {
            ICollectionManagementQuery collectionManagementQuery = query;

            var routeValueDictionary = collectionManagementQuery.ToRouteValues();

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IReviewQuery query, int resourceId,
            bool addInstitutionResource)
        {
            var collectionManagementQuery = query as ICollectionManagementQuery;

            collectionManagementQuery.ResourceId = resourceId;

            var routeValueDictionary = collectionManagementQuery.ToRouteValues();

            //routeValueDictionary.Add("ReviewId", query.ReviewId);

            routeValueDictionary.Add("AddInstitutionResource", addInstitutionResource);

            return routeValueDictionary;
        }

        public static RouteValueDictionary ToRouteValues(this IReviewQuery query, string page)
        {
            var collectionManagementQuery = query as ICollectionManagementQuery;

            var routeValueDictionary = collectionManagementQuery.ToRouteValues();

            routeValueDictionary.Remove("Page");
            routeValueDictionary.Add("Page", int.Parse(page));

            //routeValueDictionary.Add("ReviewId", query.ReviewId);

            return routeValueDictionary;
        }

        #endregion
    }
}