#region

using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public class ResourceActionsMenuBuilder : BaseActionsMenuBuilder, IActionsMenuBuilder
    {
        private static IWebSettings _webSettings;
        private readonly ICollectionService _collectionService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ISpecialtyService _specialtyService;

        public ResourceActionsMenuBuilder(IPracticeAreaService practiceAreaService, ISpecialtyService specialtyService,
            ICollectionService collectionService, IWebSettings webSettings)
        {
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _webSettings = webSettings;
            _collectionService = collectionService;
        }

        #region Implementation of IActionsMenuBuilder

        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu { SearchMenu = new SearchMenu { Label = "Find Resources" } };

            IUser currentUser = null;

            if (authenticatedInstitution != null)
            {
                if (authenticatedInstitution.User != null)
                {
                    currentUser = authenticatedInstitution.User;
                }
            }

            var manageResources = adminBaseModel as ResourcesList;
            if (manageResources != null)
            {
                actionsMenu.Query = manageResources.ResourceQuery;

                actionsMenu.Sorts = new PageLinkSection
                {
                    Title = "Sort by:",
                    PageLinks = BuildSortByLinks(urlHelper, manageResources, currentUser)
                };

                actionsMenu.AddFilter(typeof(ResourceStatus), new PageLinkSection
                {
                    Title = "Status:",
                    PageLinks = BuildResourceStatusFilterByLinks(urlHelper, manageResources, currentUser)
                });

                actionsMenu.AddFilter(typeof(bool), new PageLinkSection
                {
                    Title = "Show Only:",
                    PageLinks = BuildResourceFilterTypeFilterByLinks(urlHelper, manageResources)
                });

                actionsMenu.AddFilter(typeof(PracticeArea), new PageLinkSection
                {
                    Title = "Practice Area:",
                    PageLinks = BuildPracticeAreaFilterByLinks(urlHelper, manageResources)
                });

                actionsMenu.AddFilter(typeof(Specialty), new PageLinkSection
                {
                    Title = "Discipline:",
                    PageLinks = BuildSpecialtyFilterByLinks(urlHelper, manageResources)
                });

                if (adminBaseModel.ToolLinks != null)
                {
                    actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
                }
            }

            return actionsMenu;
        }

        #endregion

        private IEnumerable<SortLink> BuildSortByLinks(UrlHelper urlHelper, ResourcesList resourcesList, IUser user)
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
            //TODO: Find way to get access to the user
            //{ "qaapprovaldate", "QA Approval Date" },
            //{ "promotiondate", "Promotion Date" }
            if (user != null && user.EnablePromotion != null && user.EnablePromotion.Value > 0 &&
                _webSettings.DisplayPromotionFields)
            {
                sorts.Add("qaapprovaldate", "QA Approval Date");
                sorts.Add("promotiondate", "Promotion Date");
            }

            return BuildSortByLinks(urlHelper, resourcesList.ResourceQuery, sorts, "List", "Resource");
        }

        private IEnumerable<PageLink> BuildResourceStatusFilterByLinks(UrlHelper urlHelper, ResourcesList resourcesList,
            IUser user)
        {
            var filters = new Dictionary<ResourceStatus, string>
            {
                { 0, "All" },
                { ResourceStatus.Active, "Active" },
                { ResourceStatus.Archived, "Archived" },
                { ResourceStatus.Forthcoming, "Pre-Order" }
            };

            if (user != null && user.IsRittenhouseAdmin() && _webSettings.DisplayPromotionFields)
            {
                filters.Add(ResourceStatus.QANotApproved, "Awaiting QA Approved");
                filters.Add(ResourceStatus.NotPromoted, "Ready for Promotion");
            }

            return BuildResourceStatusFilterByLinks(urlHelper, resourcesList.ResourceQuery, filters, "List",
                "Resource");
        }

        private IEnumerable<PageLink> BuildResourceFilterTypeFilterByLinks(UrlHelper urlHelper,
            ResourcesList resourcesList)
        {
            var collections = _collectionService.GetAllCollections();
            return BuildResourceFilterTypeFilterByLinks(urlHelper, resourcesList.ResourceQuery, collections, "List",
                "Resource", true, true);
        }

        private IEnumerable<PageLink> BuildPracticeAreaFilterByLinks(UrlHelper urlHelper, ResourcesList resourcesList)
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();

            return BuildPracticeAreaFilterByLinks(urlHelper, resourcesList.ResourceQuery, practiceAreas, "List",
                "Resource");
        }

        private IEnumerable<PageLink> BuildSpecialtyFilterByLinks(UrlHelper urlHelper, ResourcesList resourcesList)
        {
            var specialties = _specialtyService.GetAllSpecialties();
            return BuildSpecialtyFilterByLinks(urlHelper, resourcesList.ResourceQuery, specialties, "List", "Resource");
        }
    }
}