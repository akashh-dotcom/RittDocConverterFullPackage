#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.User
{
    public class InstitutionUsers : AdminBaseModel
    {
        private SelectList _pageSizeSelectList;

        public InstitutionUsers(IUserQuery userQuery)
        {
            UserQuery = userQuery;

            BuildSelectedFilters();
        }

        public InstitutionUsers(IAdminInstitution institution, IUserQuery userQuery)
            : base(institution)
        {
            UserQuery = userQuery;

            BuildSelectedFilters();
        }

        public IEnumerable<User> Users { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }

        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }

        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }

        public IUserQuery UserQuery { get; set; }

        public int TotalCount { get; set; }
        public int ResultsFirstItem { get; set; }
        public int ResultsLastItem { get; set; }

        public string SelectedFilters { get; set; }

        [Display(Name = " results per page")]
        public SelectList PageSizeSelectList =>
            _pageSizeSelectList ??
            (_pageSizeSelectList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "10", Value = "10" },
                new SelectListItem { Text = "25", Value = "25" },
                new SelectListItem { Text = "50", Value = "50" },
                new SelectListItem { Text = "100", Value = "100" },
                new SelectListItem { Text = "250", Value = "250" }
            }, "Value", "Text"));

        private void BuildSelectedFilters()
        {
            var selectedFilters = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(UserQuery.Query))
            {
                selectedFilters.AppendFormat("<li>Query: {0}</li>", UserQuery.Query);
            }

            if (!string.IsNullOrWhiteSpace(UserQuery.SortBy))
            {
                selectedFilters.AppendFormat("<li>Sorting by: {0} - {1}</li>", GetSortByDescription(UserQuery.SortBy),
                    UserQuery.SortDirection);
            }

            if (UserQuery.RoleCode != RoleCode.NoUser)
            {
                selectedFilters.AppendFormat("<li>Showing only: {0}</li>", UserQuery.RoleCode.ToUserRole().Description);
            }

            if (UserQuery.UserStatus > 0)
            {
                selectedFilters.AppendFormat("<li>User Status: {0}</li>",
                    UserQuery.UserStatus == 1 ? "Active" : "Disabled");
            }

            SelectedFilters = selectedFilters.ToString();
        }
    }
}