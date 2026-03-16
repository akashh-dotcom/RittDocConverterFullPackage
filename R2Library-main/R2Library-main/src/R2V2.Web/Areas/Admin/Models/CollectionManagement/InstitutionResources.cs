#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class InstitutionResources : AdminBaseModel
    {
        private readonly string _specialIconBaseUrl;

        private SelectList _pageSizeSelectList;

        public InstitutionResources()
        {
        }

        public InstitutionResources(IAdminInstitution institution
            , CollectionManagementQuery collectionManagementQuery
            , IEnumerable<InstitutionResource> institutionResourcesList
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , ICollectionService collectionService
            , string doodyReviewUrl, string specialIconBaseUrl)
            : base(institution, collectionManagementQuery)
        {
            CollectionManagementQuery = collectionManagementQuery;
            InstitutionResourcesList = institutionResourcesList;

            SelectedFilters = CollectionManagementQuery.ToSelectedFilters(practiceAreaService, specialtyService,
                collectionService, GetSortByDescription(CollectionManagementQuery.SortBy));

            PageTitle = SetPageTitle(collectionService);

            DoodyReviewUrl = doodyReviewUrl;
            SpecialIconBaseUrl = specialIconBaseUrl;
        }

        public string DoodyReviewUrl { get; set; }
        public string SpecialIconBaseUrl { get; set; }

        public IEnumerable<InstitutionResource> InstitutionResourcesList { get; set; }

        public string PageTitle { get; set; }

        public string PageDescription { get; set; }

        public string NoTitlesAvailableMessage
        {
            get
            {
                if (ResourceFilterType.SpecialOffer == CollectionManagementQuery.ResourceFilterType)
                {
                    return "There are no special offers at this time.";
                }

                if (CollectionManagementQuery.RecommendationsOnly)
                {
                    return "No recommended Titles found.";
                }

                return Institution.AccountStatus == InstitutionAccountStatus.Active
                    ? "No Titles Available."
                    : "No titles found. Search again or click on Purchase eBooks to browse eBooks and start a shopping cart.";
            }
        }

        public CollectionManagementQuery CollectionManagementQuery { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }

        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }

        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }

        public int TotalCount { get; set; }
        public int ResultsFirstItem { get; set; }
        public int ResultsLastItem { get; set; }

        public string SelectedFilters { get; set; }

        public bool IsPdaEnabled { get; set; }

        [Display(Name = @" results per page")]
        public SelectList PageSizeSelectList =>
            _pageSizeSelectList ??
            (_pageSizeSelectList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = @"10", Value = "10" },
                new SelectListItem { Text = @"25", Value = "25" },
                new SelectListItem { Text = @"50", Value = "50" },
                new SelectListItem { Text = @"100", Value = "100" },
                new SelectListItem { Text = @"250", Value = "250" }
            }, "Value", "Text"));

        public bool DisplayBulkPdaOption
        {
            get
            {
                if (CollectionManagementQuery == null)
                {
                    return false;
                }

                return !CollectionManagementQuery.IncludePdaResources && !CollectionManagementQuery.PurchasedOnly;
            }
        }

        public ActivePublisher Publisher { get; set; }

        private string SetPageTitle(ICollectionService collectionService)
        {
            if (CollectionManagementQuery.PublisherId > 0)
            {
                return "Purchase by Publisher";
            }

            if (CollectionManagementQuery.ResourceListType != ResourceListType.All)
            {
                return CollectionManagementQuery.ResourceListType.ToDescription();
            }

            if (CollectionManagementQuery.IncludePdaResources && !CollectionManagementQuery.IncludePdaHistory)
            {
                return "My PDA Collection";
            }

            if (CollectionManagementQuery.IncludePdaResources && CollectionManagementQuery.IncludePdaHistory)
            {
                return "My PDA History";
            }

            if (CollectionManagementQuery.CollectionListFilter > 0)
            {
                var collection = collectionService.GetCollectionById(CollectionManagementQuery.CollectionListFilter);

                PageDescription = collection.Description;

                return collection.Name;
            }

            if (CollectionManagementQuery.IncludeFreeResources)
            {
                return "Open Access Resources";
            }

            if (CollectionManagementQuery.IncludeSpecialDiscounts)
            {
                return "Special Offer";
            }

            if (CollectionManagementQuery.RecommendationsOnly)
            {
                return "Recommended eBooks";
            }

            if (!CollectionManagementQuery.PurchasedOnly)
            {
                return IsExpertReviewer ? "Browse eBooks" : "Purchase eBooks";
            }

            return CollectionManagementQuery.ResourceStatus == ResourceStatus.Archived
                ? "Archived Titles"
                : "My R2 Collection";
        }

        public RouteValueDictionary GetQueryWithParentPageTitle(string parentPage)
        {
            var routeValues = CollectionManagementQuery.ToRouteValues();
            routeValues.Add("ParentPageTitle", parentPage);
            return routeValues;
        }
    }
}