#region

using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Recommendations;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    public static class UrlExtensions
    {
        public static PageLink NextPageLink(this UrlHelper urlHelper, string actionName, string controllerName,
            int page, int pageCount, int pageSize)
        {
            var link = NextPageLink(page, pageCount);
            link.Href = urlHelper.Action(actionName, controllerName, new { page = page + 1, pageSize });
            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper, string actionName, string controllerName,
            int page, int pageCount, int pageSize)
        {
            var link = PreviousPageLink(page, pageCount);
            link.Href = urlHelper.Action(actionName, controllerName, new { page = page + 1, pageSize });
            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper, string actionName, string controllerName,
            int page, int pageCount, int pageSize)
        {
            var link = FirstPageLink(page);
            link.Href = urlHelper.Action(actionName, controllerName, new { page = 1, pageSize });
            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper, string actionName, string controllerName,
            int page, int pageCount, int pageSize)
        {
            var link = LastPageLink(page, pageCount);
            link.Href = urlHelper.Action(actionName, controllerName, new { page = pageCount, pageSize });
            return link;
        }


        private static PageLink NextPageLink(int currentPage, int pageCount)
        {
            return new PageLink { Active = pageCount > 1 && currentPage < pageCount, Text = "Next" };
        }

        private static PageLink PreviousPageLink(int currentPage, int pageCount)
        {
            return new PageLink { Active = currentPage > 1 && pageCount > 1, Text = "Previous" };
        }

        private static PageLink FirstPageLink(int currentPage)
        {
            return new PageLink { Active = currentPage == 1, Text = "First" };
        }

        private static PageLink LastPageLink(int currentPage, int pageCount)
        {
            return new PageLink { Active = currentPage == pageCount, Text = "Last" };
        }

        #region Collection Management

        public static PageLink NextPageLink(this UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery, int pageCount)
        {
            var link = NextPageLink(collectionManagementQuery.Page, pageCount);

            var query = new CollectionManagementQuery(collectionManagementQuery);
            query.Page += 1;

            link.Href = urlHelper.Action("List", "CollectionManagement", query.ToRouteValues());

            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery, int pageCount)
        {
            var link = FirstPageLink(collectionManagementQuery.Page);

            var query = new CollectionManagementQuery(collectionManagementQuery) { Page = 1 };

            link.Href = urlHelper.Action("List", "CollectionManagement", query.ToRouteValues());

            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery, int pageCount)
        {
            var link = LastPageLink(collectionManagementQuery.Page, pageCount);

            var query = new CollectionManagementQuery(collectionManagementQuery) { Page = pageCount };

            link.Href = urlHelper.Action("List", "CollectionManagement", query.ToRouteValues());

            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery, int pageCount)
        {
            var link = PreviousPageLink(collectionManagementQuery.Page, pageCount);

            var query = new CollectionManagementQuery(collectionManagementQuery);
            query.Page -= 1;

            link.Href = urlHelper.Action("List", "CollectionManagement", query.ToRouteValues());

            return link;
        }

        #endregion

        #region Reserve Shelf Management

        public static PageLink NextPageLink(this UrlHelper urlHelper, IReserveShelfQuery reserveShelfQuery,
            int pageCount)
        {
            var link = NextPageLink(reserveShelfQuery.Page, pageCount);

            var query = new ReserveShelfQuery(reserveShelfQuery);
            query.Page += 1;

            link.Href = urlHelper.Action("ManageResources", "ReserveShelfManagement", query.ToRouteValues());

            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper, IReserveShelfQuery reserveShelfQuery,
            int pageCount)
        {
            var link = FirstPageLink(reserveShelfQuery.Page);

            var query = new ReserveShelfQuery(reserveShelfQuery) { Page = 1 };

            link.Href = urlHelper.Action("ManageResources", "ReserveShelfManagement", query.ToRouteValues());

            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper, IReserveShelfQuery reserveShelfQuery,
            int pageCount)
        {
            var link = LastPageLink(reserveShelfQuery.Page, pageCount);

            var query = new ReserveShelfQuery(reserveShelfQuery) { Page = pageCount };

            link.Href = urlHelper.Action("ManageResources", "ReserveShelfManagement", query.ToRouteValues());

            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper, IReserveShelfQuery reserveShelfQuery,
            int pageCount)
        {
            var link = PreviousPageLink(reserveShelfQuery.Page, pageCount);

            var query = new ReserveShelfQuery(reserveShelfQuery);
            query.Page -= 1;

            link.Href = urlHelper.Action("ManageResources", "ReserveShelfManagement", query.ToRouteValues());

            return link;
        }

        #endregion

        #region Resource

        public static PageLink NextPageLink(this UrlHelper urlHelper, IResourceQuery resourceQuery, int pageCount)
        {
            var link = NextPageLink(resourceQuery.Page, pageCount);

            var query = new ResourceQuery(resourceQuery);
            query.Page += 1;

            link.Href = urlHelper.Action("List", "Resource", query.ToRouteValues());

            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper, IResourceQuery resourceQuery, int pageCount)
        {
            var link = PreviousPageLink(resourceQuery.Page, pageCount);

            var query = new ResourceQuery(resourceQuery);
            query.Page -= 1;

            link.Href = urlHelper.Action("List", "Resource", query.ToRouteValues());

            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper, IResourceQuery resourceQuery)
        {
            var link = FirstPageLink(resourceQuery.Page);

            var query = new ResourceQuery(resourceQuery) { Page = 1 };

            link.Href = urlHelper.Action("List", "Resource", query.ToRouteValues());

            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper, IResourceQuery resourceQuery, int resourceCount)
        {
            var totalPages = resourceCount % resourceQuery.PageSize > 0
                ? resourceCount / resourceQuery.PageSize + 1
                : resourceCount / resourceQuery.PageSize;

            var link = LastPageLink(resourceQuery.Page, totalPages);

            var query = new ResourceQuery(resourceQuery) { Page = totalPages };

            link.Href = urlHelper.Action("List", "Resource", query.ToRouteValues());

            return link;
        }

        #endregion

        #region User

        public static PageLink NextPageLink(this UrlHelper urlHelper, IUserQuery userQuery, int pageCount)
        {
            var link = NextPageLink(userQuery.Page, pageCount);

            var query = new UserQuery(userQuery);
            query.Page += 1;

            link.Href = urlHelper.Action("List", "User", query.ToRouteValues());

            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper, IUserQuery userQuery, int pageCount)
        {
            var link = PreviousPageLink(userQuery.Page, pageCount);

            var query = new UserQuery(userQuery);
            query.Page -= 1;

            link.Href = urlHelper.Action("List", "User", query.ToRouteValues());

            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper, IUserQuery userQuery, int pageCount)
        {
            var link = FirstPageLink(userQuery.Page);

            var query = new UserQuery(userQuery) { Page = 1 };

            link.Href = urlHelper.Action("List", "User", query.ToRouteValues());

            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper, IUserQuery userQuery, int pageCount)
        {
            var link = LastPageLink(userQuery.Page, pageCount);

            var query = new UserQuery(userQuery) { Page = pageCount };

            link.Href = urlHelper.Action("List", "User", query.ToRouteValues());

            return link;
        }

        #endregion

        #region ResourceUsage

        public static PageLink NextPageLink(this UrlHelper urlHelper, ReportQuery reportQuery, int pageCount)
        {
            var link = NextPageLink(reportQuery.Page, pageCount);

            var query = new ReportQuery(reportQuery);
            query.Page += 1;

            link.Href = urlHelper.Action("ResourceUsage", "Report", query.ToRouteValues());

            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper, ReportQuery reportQuery, int pageCount)
        {
            var link = FirstPageLink(reportQuery.Page);

            var query = new ReportQuery(reportQuery) { Page = 1 };

            link.Href = urlHelper.Action("ResourceUsage", "Report", query.ToRouteValues());

            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper, ReportQuery reportQuery, int pageCount)
        {
            var link = LastPageLink(reportQuery.Page, pageCount);

            var query = new ReportQuery(reportQuery) { Page = pageCount };

            link.Href = urlHelper.Action("ResourceUsage", "Report", query.ToRouteValues());

            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper, ReportQuery reportQuery, int pageCount)
        {
            var link = PreviousPageLink(reportQuery.Page, pageCount);

            var query = new ReportQuery(reportQuery);
            query.Page -= 1;

            link.Href = urlHelper.Action("ResourceUsage", "Report", query.ToRouteValues());

            return link;
        }

        public static PageLink PageLink(this UrlHelper urlHelper, ReportQuery reportQuery, int pageCount,
            int currentPage)
        {
            var pageLink = new PageLink
            {
                Text = $"{reportQuery.Page}",
                Active = true,
                Selected = reportQuery.Page == currentPage,
                Href = urlHelper.Action("ResourceUsage", "Report", reportQuery.ToRouteValues())
            };
            return pageLink;
        }

        public static PageLink PageLinkPublisherUsage(this UrlHelper urlHelper, ReportQuery reportQuery, int pageCount,
            int currentPage)
        {
            var pageLink = new PageLink
            {
                Text = $"{reportQuery.Page}",
                Active = true,
                Selected = reportQuery.Page == currentPage,
                Href = urlHelper.Action("PublisherUsage", "Report", reportQuery.ToRouteValues())
            };
            return pageLink;
        }

        #endregion


        #region Review

        public static PageLink NextPageLink(this UrlHelper urlHelper, IReviewQuery reviewQuery, int pageCount)
        {
            var link = NextPageLink(reviewQuery.Page, pageCount);

            var query = new ReviewQuery(reviewQuery);
            query.Page += 1;

            link.Href = urlHelper.Action("Resources", "Review", query.ToRouteValues());

            return link;
        }

        public static PageLink FirstPageLink(this UrlHelper urlHelper, IReviewQuery reviewQuery, int pageCount)
        {
            var link = FirstPageLink(reviewQuery.Page);

            var query = new ReviewQuery(reviewQuery) { Page = 1 };

            link.Href = urlHelper.Action("Resources", "Review", query.ToRouteValues());

            return link;
        }

        public static PageLink LastPageLink(this UrlHelper urlHelper, IReviewQuery reviewQuery, int pageCount)
        {
            var link = LastPageLink(reviewQuery.Page, pageCount);

            var query = new ReviewQuery(reviewQuery) { Page = pageCount };

            link.Href = urlHelper.Action("Resources", "Review", query.ToRouteValues());

            return link;
        }

        public static PageLink PreviousPageLink(this UrlHelper urlHelper, IReviewQuery reviewQuery, int pageCount)
        {
            var link = PreviousPageLink(reviewQuery.Page, pageCount);

            var query = new ReviewQuery(reviewQuery);
            query.Page -= 1;

            link.Href = urlHelper.Action("Resources", "Review", query.ToRouteValues());

            return link;
        }

        #endregion
    }
}