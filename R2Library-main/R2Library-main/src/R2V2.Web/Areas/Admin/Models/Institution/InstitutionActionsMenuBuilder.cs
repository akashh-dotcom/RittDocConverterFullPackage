#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class InstitutionActionsMenuBuilder : IActionsMenuBuilder
    {
        private static ITerritoryService _territoryService;
        private static IInstitutionTypeService _institutionTypeService;

        public InstitutionActionsMenuBuilder(ITerritoryService territoryService,
            IInstitutionTypeService institutionTypeService)
        {
            _territoryService = territoryService;
            _institutionTypeService = institutionTypeService;
        }

        #region Implementation of IActionsMenuBuilder

        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu();

            var institutionList = adminBaseModel as InstitutionList;
            if (institutionList != null)
            {
                actionsMenu.SearchMenu = new SearchMenu { Label = "Find Institutions" };
                actionsMenu.Query = institutionList.InstitutionQuery;

                actionsMenu.AddFilter(typeof(AccountStatus), new PageLinkSection
                {
                    Title = "Status:",
                    PageLinks = BuildFilterByLinks(urlHelper, institutionList)
                });

                actionsMenu.AddFilter(typeof(Territory), new PageLinkSection
                {
                    Title = "Territory:",
                    PageLinks = BuildTerritoryFilterByLinks(urlHelper, institutionList)
                });

                actionsMenu.AddFilter(typeof(InstitutionType), new PageLinkSection
                {
                    Title = "Institution Type:",
                    PageLinks = BuildInstitutionTypeFilterByLinks(urlHelper, institutionList)
                });


                actionsMenu.AddFilter(typeof(bool), new PageLinkSection
                {
                    Title = "Expert Reviewer:",
                    PageLinks = BuildReviewerFilterByLinks(urlHelper, institutionList)
                });

                actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
            }

            return actionsMenu;
        }

        #endregion

        private static IEnumerable<PageLink> BuildFilterByLinks(UrlHelper urlHelper, InstitutionList institutionList)
        {
            var filters = new Dictionary<AccountStatus, string>
            {
                { AccountStatus.Active, "Active" },
                { AccountStatus.Trial, "Trial" },
                { AccountStatus.TrialExpired, "Trial Expired" },
                { AccountStatus.Disabled, "Disabled" },
                { AccountStatus.PdaOnly, "PDA" },
                { AccountStatus.All, "All" }
            };

            var institutionQuery = institutionList.InstitutionQuery;

            return from filter in filters
                let query = new InstitutionQuery
                {
                    Query = institutionQuery.Query,
                    SortBy = institutionQuery.SortBy,
                    SortDirection = institutionQuery.SortDirection,
                    TerritoryId = institutionQuery.TerritoryId,
                    InstitutionTypeId = institutionQuery.InstitutionTypeId,
                    AccountStatus = filter.Key,
                    IncludeExpertReviewer = institutionQuery.IncludeExpertReviewer,
                    ExcludeExpertReviewer = institutionQuery.ExcludeExpertReviewer,
                    Page = institutionQuery.Page,
                    RecentOnly = institutionQuery.RecentOnly
                }
                select new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action("List", "Institution", query.ToRouteValues()),
                    Selected = institutionList.InstitutionQuery.AccountStatus == filter.Key
                };
        }

        private static IEnumerable<PageLink> BuildTerritoryFilterByLinks(UrlHelper urlHelper,
            InstitutionList institutionList)
        {
            var territories = _territoryService.GetAllTerritories();
            var territoryFilters = territories.ToDictionary(territory => territory.Id, territory => territory.Name);

            territoryFilters.Add(0, "All");

            var institutionQuery = institutionList.InstitutionQuery;

            return from filter in territoryFilters
                let query = new InstitutionQuery
                {
                    Query = institutionQuery.Query,
                    SortBy = institutionQuery.SortBy,
                    SortDirection = institutionQuery.SortDirection,
                    AccountStatus = institutionQuery.AccountStatus,
                    TerritoryId = filter.Key,
                    IncludeExpertReviewer = institutionQuery.IncludeExpertReviewer,
                    ExcludeExpertReviewer = institutionQuery.ExcludeExpertReviewer,
                    Page = institutionQuery.Page,
                    RecentOnly = institutionQuery.RecentOnly,
                    InstitutionTypeId = institutionQuery.InstitutionTypeId
                }
                select new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action("List", "Institution", query.ToRouteValues()),
                    Selected = institutionList.InstitutionQuery.TerritoryId == filter.Key
                };
        }

        private static IEnumerable<PageLink> BuildInstitutionTypeFilterByLinks(UrlHelper urlHelper,
            InstitutionList institutionList)
        {
            var institutionTypes = _institutionTypeService.GetAllInstitutionTypes();
            var institutionTypeFilters = institutionTypes.ToDictionary(x => x.Id, x => x.Name);

            institutionTypeFilters.Add(0, "All");

            var institutionQuery = institutionList.InstitutionQuery;

            return from filter in institutionTypeFilters
                let query = new InstitutionQuery
                {
                    Query = institutionQuery.Query,
                    SortBy = institutionQuery.SortBy,
                    SortDirection = institutionQuery.SortDirection,
                    AccountStatus = institutionQuery.AccountStatus,
                    InstitutionTypeId = filter.Key,
                    IncludeExpertReviewer = institutionQuery.IncludeExpertReviewer,
                    ExcludeExpertReviewer = institutionQuery.ExcludeExpertReviewer,
                    TerritoryId = institutionQuery.TerritoryId,
                    Page = institutionQuery.Page,
                    RecentOnly = institutionQuery.RecentOnly
                }
                select new PageLink
                {
                    Text = filter.Value,
                    Href = urlHelper.Action("List", "Institution", query.ToRouteValues()),
                    Selected = institutionList.InstitutionQuery.InstitutionTypeId == filter.Key
                };
        }

        private static IEnumerable<PageLink> BuildReviewerFilterByLinks(UrlHelper urlHelper,
            InstitutionList institutionList)
        {
            var pageLinks = new List<PageLink>();

            var institutionQuery = institutionList.InstitutionQuery;

            var newInstitutionQuery = new InstitutionQuery
            {
                Query = institutionQuery.Query,
                SortBy = institutionQuery.SortBy,
                SortDirection = institutionQuery.SortDirection,
                AccountStatus = institutionQuery.AccountStatus,
                IncludeExpertReviewer = true,
                ExcludeExpertReviewer = false,
                Page = institutionQuery.Page,
                TerritoryId = institutionQuery.TerritoryId,
                InstitutionTypeId = institutionQuery.InstitutionTypeId,
                RecentOnly = institutionQuery.RecentOnly
            };

            pageLinks.Add(new PageLink
            {
                Text = "Include",
                Href = urlHelper.Action("List", "Institution", newInstitutionQuery.ToRouteValues()),
                Selected = institutionQuery.IncludeExpertReviewer
            });

            newInstitutionQuery.IncludeExpertReviewer = false;
            newInstitutionQuery.ExcludeExpertReviewer = true;

            pageLinks.Add(new PageLink
            {
                Text = "Exclude",
                Href = urlHelper.Action("List", "Institution", newInstitutionQuery.ToRouteValues()),
                Selected = institutionQuery.ExcludeExpertReviewer
            });

            newInstitutionQuery.IncludeExpertReviewer = false;
            newInstitutionQuery.ExcludeExpertReviewer = false;

            pageLinks.Add(new PageLink
            {
                Text = "All",
                Href = urlHelper.Action("List", "Institution", newInstitutionQuery.ToRouteValues()),
                Selected = !institutionQuery.IncludeExpertReviewer && !institutionQuery.ExcludeExpertReviewer
            });

            return pageLinks;
        }

        private static IEnumerable<PageLink> BuildPdaAccountsFilterByLinks(UrlHelper urlHelper,
            InstitutionList institutionList)
        {
            var pageLinks = new List<PageLink>();

            var institutionQuery = institutionList.InstitutionQuery;

            var newInstitutionQuery = new InstitutionQuery
            {
                Query = institutionQuery.Query,
                SortBy = institutionQuery.SortBy,
                SortDirection = institutionQuery.SortDirection,
                AccountStatus = institutionQuery.AccountStatus,
                Page = institutionQuery.Page,
                TerritoryId = institutionQuery.TerritoryId,
                RecentOnly = institutionQuery.RecentOnly,
                IncludeExpertReviewer = institutionQuery.IncludeExpertReviewer,
                ExcludeExpertReviewer = institutionQuery.ExcludeExpertReviewer
            };

            pageLinks.Add(new PageLink
            {
                Text = "PDA Accounts Only",
                Href = urlHelper.Action("List", "Institution", newInstitutionQuery.ToRouteValues())
            });

            return pageLinks;
        }
    }
}