#region

using System;
using System.Text;
using R2V2.Core.Institution;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceQuery : IResourceQuery
    {
        private const int DefaultPage = 1;
        private const int DefaultPageSize = 10;

        private int _page;

        private int _pageSize;

        public ResourceQuery()
        {
        }

        public ResourceQuery(IResourceQuery resourceQuery)
        {
            ResourceId = resourceQuery.ResourceId;
            Query = resourceQuery.Query;
            SortBy = resourceQuery.SortBy;
            SortDirection = resourceQuery.SortDirection;
            PracticeAreaFilter = resourceQuery.PracticeAreaFilter;
            SpecialtyFilter = resourceQuery.SpecialtyFilter;
            Page = resourceQuery.Page;
            PageSize = resourceQuery.PageSize;
            ResourceStatus = resourceQuery.ResourceStatus;
            ResourceFilterType = resourceQuery.ResourceFilterType;
            PurchasedOnly = resourceQuery.PurchasedOnly;
            IncludePdaResources = resourceQuery.IncludePdaResources;
            IncludePdaHistory = resourceQuery.IncludePdaHistory;
            ResourceListType = resourceQuery.ResourceListType;
            IncludeSpecialDiscounts = resourceQuery.IncludeSpecialDiscounts;
            IncludeFreeResources = resourceQuery.IncludeFreeResources;
            RecentOnly = resourceQuery.RecentOnly;
            TurnawayStartDate = resourceQuery.TurnawayStartDate;
            PublisherId = resourceQuery.PublisherId;
            CollectionFilter = resourceQuery.CollectionFilter;
            CollectionListFilter = resourceQuery.CollectionListFilter;
            PdaStatus = resourceQuery.PdaStatus;
        }

        public int ResourceId { get; set; }

        public string Query { get; set; }

        public string SortBy { get; set; }
        public SortDirection SortDirection { get; set; }

        public bool DefaultQuery =>
            string.IsNullOrWhiteSpace(Query) && string.IsNullOrWhiteSpace(SortBy) &&
            SortDirection == SortDirection.Ascending &&
            ResourceStatus == ResourceStatus.All && ResourceFilterType == ResourceFilterType.All && !RecentOnly &&
            PracticeAreaFilter <= 0 && SpecialtyFilter <= 0 && CollectionFilter <= 0 && PdaStatus == PdaStatus.None;

        //public CollectionIdentifier CollectionType { get; set; }
        public int PracticeAreaFilter { get; set; }
        public int CollectionFilter { get; set; }
        public int SpecialtyFilter { get; set; }

        public int Page
        {
            get => _page <= 0 ? DefaultPage : _page;
            set => _page = value;
        }

        public int PageSize
        {
            get => _pageSize <= 0 ? DefaultPageSize : _pageSize;
            set => _pageSize = value;
        }

        public ResourceStatus ResourceStatus { get; set; }
        public ResourceFilterType ResourceFilterType { get; set; }

        //TODO: Figure out way to remove these since they are only used for CollectionManagement
        public bool PurchasedOnly { get; set; }
        public bool IncludePdaResources { get; set; }
        public bool IncludePdaHistory { get; set; }
        public ResourceListType ResourceListType { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }

        public int CollectionListFilter { get; set; }
        //public bool IncludeClinicalCornerstone { get; set; }
        //public bool IncludeNoteworthyNursing { get; set; }
        //public bool IncludeBestOfYear { get; set; }
        //public bool IncludeAjnBooks2015 { get; set; }

        //public bool IncludeNursingEssentials { get; set; }
        //public bool IncludeHospitalEssentials { get; set; }
        //public bool IncludeMedicalEssentials { get; set; }
        //public bool IncludeAjnBooksOfYear { get; set; }
        public bool IncludeSpecialDiscounts { get; set; }
        public bool IncludeFreeResources { get; set; }
        public bool RecommendationsOnly { get; set; }

        //TODO: Figure out way to remove these since they are only used for ReserveShelfManagement
        public int ReserveShelfId { get; set; }
        public int ReviewId { get; set; }

        public DateTime? PdaDateAddedMin { get; set; }
        public DateTime? PdaDateAddedMax { get; set; }

        public bool RecentOnly { get; set; }
        public DateTime? TurnawayStartDate { get; set; }
        public int PublisherId { get; set; }

        public PdaStatus PdaStatus { get; set; }


        public string ToSelectedFilters(IPracticeAreaService practiceAreaService, ISpecialtyService specialtyService,
            ICollectionService collectionService, string sortString)
        {
            var selectedFilters = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Query))
            {
                selectedFilters.AppendFormat("<li>Query: {0} </li>", Query);
            }

            if (!string.IsNullOrWhiteSpace(sortString))
            {
                selectedFilters.AppendFormat("<li>Sorting by: {0} - {1} </li>", sortString, SortDirection);
            }

            if (ResourceStatus != ResourceStatus.All)
            {
                selectedFilters.AppendFormat("<li>Status: {0} </li>", ResourceStatus);
            }

            if (ResourceFilterType != ResourceFilterType.All || CollectionFilter > 0 || RecentOnly)
            {
                if (CollectionFilter > 0)
                {
                    selectedFilters.AppendFormat("<li>Show Only: {0} </li>",
                        collectionService.GetCollection(CollectionFilter).Name);
                }
                else if (RecentOnly)
                {
                    selectedFilters.Append("<li>Showing Only: Recently Viewed </li>");
                }
                else
                {
                    selectedFilters.AppendFormat("<li>Showing Only: {0} </li>", ResourceFilterType.ToDescription());
                }
            }

            if (PracticeAreaFilter > 0)
            {
                selectedFilters.AppendFormat("<li>Practice Area: {0} </li>",
                    practiceAreaService.GetPracticeAreaById(PracticeAreaFilter).Name);
            }

            if (SpecialtyFilter > 0)
            {
                selectedFilters.AppendFormat("<li>Discipline: {0} </li>",
                    specialtyService.GetSpecialty(SpecialtyFilter).Name);
            }

            if (PdaStatus != PdaStatus.None)
            {
                selectedFilters.AppendFormat("<li>PDA Status: {0} </li>", PdaStatus.ToDescription());
            }

            return selectedFilters.ToString();
        }
    }
}