#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Review
{
    public class ReviewActionsMenuBuilder : BaseActionsMenuBuilder, IActionsMenuBuilder
    {
        private readonly ICollectionService _collectionService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ISpecialtyService _specialtyService;

        /// <summary>
        /// </summary>
        public ReviewActionsMenuBuilder(IPracticeAreaService practiceAreaService, ISpecialtyService specialtyService,
            ICollectionService collectionService)
        {
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
        }

        public ActionsMenu Build(AuthenticatedInstitution authenticatedInstitution, UrlHelper urlHelper,
            AdminBaseModel adminBaseModel)
        {
            var actionsMenu = new ActionsMenu();

            var reviewManagement = adminBaseModel as ReviewResources;
            if (reviewManagement != null)
            {
                var reviewQuery = reviewManagement.ReviewQuery;

                actionsMenu.Query = reviewQuery;

                actionsMenu.SearchMenu = new SearchMenu { Label = GetSearchLabel(), Query = reviewQuery };

                reviewQuery.Page = 1;

                actionsMenu.Sorts = new PageLinkSection
                {
                    Title = "Sort by:",
                    PageLinks = BuildSortByLinks(urlHelper, reviewQuery)
                };

                actionsMenu.AddFilter(typeof(ResourceStatus), new PageLinkSection
                {
                    Title = "Status:",
                    PageLinks = BuildResourceStatusFilterByLinks(urlHelper, reviewQuery)
                });
                actionsMenu.AddFilter(typeof(ResourceFilterType), new PageLinkSection
                {
                    Title = "Show Only:",
                    PageLinks = BuildResourceFilterTypeFilterByLinks(urlHelper, reviewQuery)
                });

                actionsMenu.AddFilter(typeof(ResourceFilterType), new PageLinkSection
                {
                    Title = "Show Only:",
                    PageLinks = BuildResourceFilterTypeFilterByLinks(urlHelper, reviewQuery)
                });

                actionsMenu.AddFilter(typeof(PracticeArea), new PageLinkSection
                {
                    Title = "Practice Area:",
                    PageLinks = BuildPracticeAreaFilterByLinks(urlHelper, reviewQuery)
                });

                actionsMenu.AddFilter(typeof(Specialty), new PageLinkSection
                {
                    Title = "Discipline:",
                    PageLinks = BuildSpecialtyFilterByLinks(urlHelper, reviewQuery)
                });
            }

            actionsMenu.ToolLinks = adminBaseModel.ToolLinks;
            return actionsMenu;
        }

        private static string GetSearchLabel()
        {
            return "Search For Resources:";
        }

        /// <summary>
        /// </summary>
        private IEnumerable<SortLink> BuildSortByLinks(UrlHelper urlHelper, IReviewQuery reviewQuery)
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

            return BuildSortByLinks(urlHelper, reviewQuery, sorts, "Resources", "Review");
        }

        /// <summary>
        /// </summary>
        private IEnumerable<PageLink> BuildResourceStatusFilterByLinks(UrlHelper urlHelper, IReviewQuery reviewQuery)
        {
            // todo: we may want to change the filter parameter from an int to a string
            var filters = new Dictionary<ResourceStatus, string>
            {
                { 0, "All" },
                { ResourceStatus.Active, "Active" },
                { ResourceStatus.Archived, "Archived" },
                { ResourceStatus.Forthcoming, "Pre-Order" }
            };

            return BuildResourceStatusFilterByLinks(urlHelper, reviewQuery, filters, "Resources", "Review");
        }

        /// <summary>
        /// </summary>
        private IEnumerable<PageLink> BuildResourceFilterTypeFilterByLinks(UrlHelper urlHelper,
            IReviewQuery reviewQuery)
        {
            var collections = _collectionService.GetAllCollections().Where(x => !x.HideInFilter);

            return BuildResourceFilterTypeFilterByLinks(urlHelper, reviewQuery, collections, "Resources", "Review",
                false, false);
        }

        /// <summary>
        /// </summary>
        private IEnumerable<PageLink> BuildPracticeAreaFilterByLinks(UrlHelper urlHelper, IReviewQuery reviewQuery)
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();

            return BuildPracticeAreaFilterByLinks(urlHelper, reviewQuery, practiceAreas, "Resources", "Review");
        }

        /// <summary>
        /// </summary>
        private IEnumerable<PageLink> BuildSpecialtyFilterByLinks(UrlHelper urlHelper, IReviewQuery reviewQuery)
        {
            var specialties = _specialtyService.GetAllSpecialties();
            return BuildSpecialtyFilterByLinks(urlHelper, reviewQuery, specialties, "Resources", "Review");
        }
    }
}