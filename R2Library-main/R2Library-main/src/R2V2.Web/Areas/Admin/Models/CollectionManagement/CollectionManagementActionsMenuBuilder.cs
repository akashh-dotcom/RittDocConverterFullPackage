#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class CollectionManagementActionsMenuBuilder : BaseActionsMenuBuilder, IActionsMenuBuilder
    {
        private readonly ICollectionService _collectionService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ISpecialtyService _specialtyService;

        public CollectionManagementActionsMenuBuilder(IPracticeAreaService practiceAreaService,
            ISpecialtyService specialtyService, ICollectionService collectionService)
        {
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
        }

        #region Implementation of IActionsMenuBuilder

        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu();

            var institutionResources = adminBaseModel as InstitutionResources;
            if (institutionResources != null)
            {
                var collectionManagementQuery =
                    new CollectionManagementQuery(institutionResources.CollectionManagementQuery);

                actionsMenu.Query = collectionManagementQuery;

                actionsMenu.SearchMenu = new SearchMenu
                    { Label = GetSearchLabel(collectionManagementQuery), Query = collectionManagementQuery };

                //Need to reset the Page to the first incase someone was at end of list and filters
                collectionManagementQuery.Page = 1;

                actionsMenu.Sorts = new PageLinkSection
                {
                    Title = "Sort by:",
                    PageLinks = BuildSortByLinks(urlHelper, collectionManagementQuery)
                };

                actionsMenu.AddFilter(typeof(ResourceStatus), new PageLinkSection
                {
                    Title = "Status:",
                    PageLinks = BuildResourceStatusFilterByLinks(urlHelper, collectionManagementQuery)
                });

                actionsMenu.SecondRowFilter(typeof(PdaStatus), new PageLinkSection
                {
                    Title = "PDA Status:",
                    PageLinks = BuildPdaStatusFilterByLinks(urlHelper, collectionManagementQuery)
                });

                actionsMenu.AddFilter(typeof(ResourceFilterType), new PageLinkSection
                {
                    Title = "Show Only:",
                    PageLinks = BuildResourceFilterTypeFilterByLinks(urlHelper, collectionManagementQuery)
                });

                actionsMenu.AddFilter(typeof(PracticeArea), new PageLinkSection
                {
                    Title = "Practice Area:",
                    PageLinks = BuildPracticeAreaFilterByLinks(urlHelper, institutionResources)
                });

                actionsMenu.AddFilter(typeof(Specialty), new PageLinkSection
                {
                    Title = "Discipline:",
                    PageLinks = BuildSpecialtyFilterByLinks(urlHelper, institutionResources)
                });
            }

            actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
            return actionsMenu;
        }

        #endregion

        private static string GetSearchLabel(IResourceQuery resourceQuery)
        {
            if (!resourceQuery.PurchasedOnly)
            {
                return "Search for eBooks";
            }

            return resourceQuery.ResourceStatus == ResourceStatus.Archived
                ? "Search My eBook Archive"
                : "Search My R2 Collection";
        }

        private IEnumerable<SortLink> BuildSortByLinks(UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery)
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

            if (collectionManagementQuery.IncludePdaResources)
            {
                sorts.Add("pdadateadded", "PDA Date Added");
                sorts.Add("pdaviewcount", "PDA View Count");
            }

            if (collectionManagementQuery.IncludePdaHistory)
            {
                sorts.Add("pdadatedeleted", "PDA Date Removed");
            }

            return BuildSortByLinks(urlHelper, collectionManagementQuery, sorts, "List", "CollectionManagement");
        }

        private IEnumerable<PageLink> BuildResourceStatusFilterByLinks(UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery)
        {
            var filters = new Dictionary<ResourceStatus, string>
            {
                { 0, "All" },
                { ResourceStatus.Active, "Active" },
                { ResourceStatus.Archived, "Archived" },
                { ResourceStatus.Forthcoming, "Pre-Order" }
            };
            return BuildResourceStatusFilterByLinks(urlHelper, collectionManagementQuery, filters, "List",
                "CollectionManagement");
        }

        private IEnumerable<PageLink> BuildPdaStatusFilterByLinks(UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery)
        {
            var filters = new Dictionary<PdaStatus, string>
            {
                { PdaStatus.Active, PdaStatus.Active.ToDescription() },
                { PdaStatus.NotPurchased, PdaStatus.NotPurchased.ToDescription() },
                { PdaStatus.Purchased, PdaStatus.Purchased.ToDescription() },
                { PdaStatus.Deleted, PdaStatus.Deleted.ToDescription() }
            };
            return BuildPdaStatusFilterByLinks(urlHelper, collectionManagementQuery, filters, "List",
                "CollectionManagement");
        }

        private IEnumerable<PageLink> BuildResourceFilterTypeFilterByLinks(UrlHelper urlHelper,
            ICollectionManagementQuery collectionManagementQuery)
        {
            var collections = _collectionService.GetAllCollections().Where(x => !x.HideInFilter);

            return BuildResourceFilterTypeFilterByLinks(urlHelper, collectionManagementQuery, collections, "List",
                "CollectionManagement", false, false);
        }

        private IEnumerable<PageLink> BuildPracticeAreaFilterByLinks(UrlHelper urlHelper,
            InstitutionResources institutionResources)
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();
            return BuildPracticeAreaFilterByLinks(urlHelper, institutionResources.CollectionManagementQuery,
                practiceAreas, "List", "CollectionManagement");
        }

        private IEnumerable<PageLink> BuildSpecialtyFilterByLinks(UrlHelper urlHelper,
            InstitutionResources institutionResources)
        {
            var specialties = _specialtyService.GetAllSpecialties();

            return BuildSpecialtyFilterByLinks(urlHelper, institutionResources.CollectionManagementQuery, specialties,
                "List", "CollectionManagement");
        }
    }
}