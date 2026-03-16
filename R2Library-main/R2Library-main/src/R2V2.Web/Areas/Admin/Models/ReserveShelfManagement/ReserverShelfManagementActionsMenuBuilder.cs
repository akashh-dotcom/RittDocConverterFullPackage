#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ReserveShelfManagement
{
    public class ReserverShelfManagementActionsMenuBuilder : BaseActionsMenuBuilder, IActionsMenuBuilder
    {
        private readonly ICollectionService _collectionService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ISpecialtyService _specialtyService;

        public ReserverShelfManagementActionsMenuBuilder(IPracticeAreaService practiceAreaService,
            ISpecialtyService specialtyService, ICollectionService collectionService)
        {
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
        }

        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu();

            var reserveShelfManagement = adminBaseModel as ReserveShelfManagement;
            if (reserveShelfManagement != null)
            {
                var reserveShelfQuery = reserveShelfManagement.ReserveShelfQuery;

                actionsMenu.Query = reserveShelfQuery;

                actionsMenu.SearchMenu = new SearchMenu { Label = GetSearchLabel(), Query = reserveShelfQuery };

                reserveShelfQuery.Page = 1;


                actionsMenu.Sorts = new PageLinkSection
                {
                    Title = "Sort by:",
                    PageLinks = BuildSortByLinks(urlHelper, reserveShelfQuery)
                };

                actionsMenu.AddFilter(typeof(ResourceStatus), new PageLinkSection
                {
                    Title = "Status:",
                    PageLinks = BuildResourceStatusFilterByLinks(urlHelper, reserveShelfQuery)
                });
                actionsMenu.AddFilter(typeof(ResourceFilterType), new PageLinkSection
                {
                    Title = "Show Only:",
                    PageLinks = BuildResourceFilterTypeFilterByLinks(urlHelper, reserveShelfQuery)
                });

                actionsMenu.AddFilter(typeof(PracticeArea), new PageLinkSection
                {
                    Title = "Practice Area:",
                    PageLinks = BuildPracticeAreaFilterByLinks(urlHelper, reserveShelfQuery)
                });

                actionsMenu.AddFilter(typeof(Specialty), new PageLinkSection
                {
                    Title = "Discipline:",
                    PageLinks = BuildSpecialtyFilterByLinks(urlHelper, reserveShelfQuery)
                });
            }

            actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
            return actionsMenu;
        }

        private static string GetSearchLabel()
        {
            return "Search My R2 Collection";
        }

        private IEnumerable<SortLink> BuildSortByLinks(UrlHelper urlHelper, IReserveShelfQuery reserveShelfQuery)
        {
            var sorts = new Dictionary<string, string>
            {
                { "status", "Status" },
                { "publicationdate", "Publication Date" },
                { "releasedate", "R2 Release Date" },
                { "title", "Book Title" },
                { "publisher", "Publisher" },
                { "author", "Author" },
                { "duedate", "Due Date" },
                { "price", "Price" }
            };

            return BuildSortByLinks(urlHelper, reserveShelfQuery, sorts, "List", "Resource");
        }

        private IEnumerable<PageLink> BuildResourceStatusFilterByLinks(UrlHelper urlHelper,
            IReserveShelfQuery reserveShelfQuery)
        {
            var filters = new Dictionary<ResourceStatus, string>
            {
                { 0, "All" },
                { ResourceStatus.Active, "Active" },
                { ResourceStatus.Archived, "Archived" },
                { ResourceStatus.Forthcoming, "Pre-Order" }
            };

            return BuildResourceStatusFilterByLinks(urlHelper, reserveShelfQuery, filters, "ManageResources",
                "ReserveShelfManagement");
        }

        private IEnumerable<PageLink> BuildResourceFilterTypeFilterByLinks(UrlHelper urlHelper,
            IReserveShelfQuery reserveShelfQuery)
        {
            var collections = _collectionService.GetAllCollections().Where(x => !x.HideInFilter);

            return BuildResourceFilterTypeFilterByLinks(urlHelper, reserveShelfQuery, collections, "ManageResources",
                "ReserveShelfManagement", false, false);
        }

        private IEnumerable<PageLink> BuildPracticeAreaFilterByLinks(UrlHelper urlHelper,
            IReserveShelfQuery reserveShelfQuery)
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();
            return BuildPracticeAreaFilterByLinks(urlHelper, reserveShelfQuery, practiceAreas, "ManageResources",
                "ReserveShelfManagement");
        }

        private IEnumerable<PageLink> BuildSpecialtyFilterByLinks(UrlHelper urlHelper,
            IReserveShelfQuery reserveShelfQuery)
        {
            var specialties = _specialtyService.GetAllSpecialties();
            return BuildSpecialtyFilterByLinks(urlHelper, reserveShelfQuery, specialties, "ManageResources",
                "ReserveShelfManagement");
        }
    }
}