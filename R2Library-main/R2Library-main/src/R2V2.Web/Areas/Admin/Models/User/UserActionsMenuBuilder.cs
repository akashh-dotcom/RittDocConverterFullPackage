#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public class UserActionsMenuBuilder : IActionsMenuBuilder
    {
        #region Implementation of IActionsMenuBuilder

        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu();

            var institutionUsers = adminBaseModel as InstitutionUsers;
            if (institutionUsers != null)
            {
                actionsMenu.SearchMenu = new SearchMenu { Label = "Find Users" };

                var userQuery = institutionUsers.UserQuery;

                actionsMenu.Query = userQuery;

                actionsMenu.Sorts = new PageLinkSection
                {
                    Title = "Sort By:",
                    PageLinks = BuildSortByLinks(urlHelper, userQuery)
                };

                actionsMenu.AddFilter(typeof(RoleCode), new PageLinkSection
                {
                    Title = "Show Only:",
                    PageLinks = BuildFilterByLinks(urlHelper, userQuery)
                });

                actionsMenu.AddFilter(typeof(int), new PageLinkSection
                {
                    Title = "Status: ",
                    PageLinks = BuildUserStatusLinks(urlHelper, userQuery)
                });

                actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
            }

            return actionsMenu;
        }

        #endregion

        private static IEnumerable<SortLink> BuildSortByLinks(UrlHelper urlHelper, IUserQuery userQuery)
        {
            var sorts = new Dictionary<string, string>
            {
                { "lastname", "Last Name" },
                { "firstname", "First Name" },
                { "email", "Email" },
                { "role", "Role" }
            };
            if (userQuery.LoadAllUsers)
            {
                sorts.Add("Institution", "Institution");
            }

            return from sort in sorts
                let query = new UserQuery
                {
                    Query = userQuery.Query,
                    SortBy = sort.Key,
                    SortDirection = userQuery.SortDirection,
                    PageSize = userQuery.PageSize,
                    RoleCode = userQuery.RoleCode,
                    SearchType = userQuery.SearchType,
                    InstitutionId = userQuery.InstitutionId,
                    UserStatus = userQuery.UserStatus
                }
                select new SortLink
                {
                    Text = sort.Value,
                    HrefAscending = urlHelper.Action("List", "User", query.ToRouteValues(SortDirection.Ascending)),
                    HrefDescending = urlHelper.Action("List", "User", query.ToRouteValues(SortDirection.Descending)),
                    Selected = userQuery.SortBy == sort.Key
                };
        }

        private static IEnumerable<PageLink> BuildFilterByLinks(UrlHelper urlHelper, IUserQuery userQuery)
        {
            var filters = new Dictionary<RoleCode, string>
            {
                { RoleCode.INSTADMIN, "Institution Administrator" },
                { RoleCode.SALESASSOC, "Sales Associate" },
                { RoleCode.USERS, "Users" }
            };
            if (userQuery.LoadAllUsers || userQuery.InstitutionId == 1)
            {
                filters.Add(RoleCode.RITADMIN, "Rittenhouse Administrator");
            }

            return from filter in filters
                let query = new UserQuery
                {
                    Query = userQuery.Query,
                    SortBy = userQuery.SortBy,
                    SortDirection = userQuery.SortDirection,
                    PageSize = userQuery.PageSize,
                    RoleCode = filter.Key,
                    SearchType = userQuery.SearchType,
                    InstitutionId = userQuery.InstitutionId,
                    UserStatus = userQuery.UserStatus
                }
                select new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action("List", "User", query.ToRouteValues()),
                    Selected = userQuery.RoleCode == filter.Key
                };
        }

        private static IEnumerable<PageLink> BuildUserStatusLinks(UrlHelper urlHelper, IUserQuery userQuery)
        {
            var filters = new Dictionary<int, string>
            {
                { 0, "All" },
                { 1, "Active" },
                { 2, "Disabled" }
            };
            return from filter in filters
                let query = new UserQuery
                {
                    Query = userQuery.Query,
                    SortBy = userQuery.SortBy,
                    SortDirection = userQuery.SortDirection,
                    PageSize = userQuery.PageSize,
                    RoleCode = userQuery.RoleCode,
                    SearchType = userQuery.SearchType,
                    UserStatus = filter.Key,
                    InstitutionId = userQuery.InstitutionId
                }
                select new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action("List", "User", query.ToRouteValues()),
                    Selected = userQuery.UserStatus == filter.Key
                };
        }
    }
}