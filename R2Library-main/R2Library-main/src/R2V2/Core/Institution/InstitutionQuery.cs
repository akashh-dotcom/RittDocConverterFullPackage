#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class InstitutionQuery : IInstitutionQuery
    {
        private const string DefaultPage = "A";

        private const string DefaultSearchPage = "All";

        private string _page;

        public string Query { get; set; }

        public string SortBy { get; set; }
        public SortDirection SortDirection { get; set; }

        public bool RecentOnly { get; set; }

        public bool DefaultQuery =>
            string.IsNullOrWhiteSpace(Query)
            && string.IsNullOrWhiteSpace(SortBy)
            && SortDirection == SortDirection.Ascending
            && AccountStatus == AccountStatus.All
            && TerritoryId == 0
            && InstitutionTypeId == 0
            && !IncludeExpertReviewer
            && !ExcludeExpertReviewer;

        public bool AlphaFilter => string.IsNullOrWhiteSpace(Query);

        public bool DefaultQueryExceptSearch =>
            string.IsNullOrWhiteSpace(SortBy)
            && SortDirection == SortDirection.Ascending
            && AccountStatus == AccountStatus.All
            && TerritoryId == 0
            && InstitutionTypeId == 0
            && !IncludeExpertReviewer
            && !ExcludeExpertReviewer
            && Page == DefaultPage;

        public string Page
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_page) && string.IsNullOrWhiteSpace(Query))
                {
                    return DefaultPage;
                }

                if (string.IsNullOrWhiteSpace(_page) && !string.IsNullOrWhiteSpace(Query))
                {
                    return DefaultSearchPage;
                }

                return _page;
            }
            set => _page = value;
        }

        public AccountStatus AccountStatus { get; set; }
        public int TerritoryId { get; set; }
        public int InstitutionTypeId { get; set; }

        //TODO: Figure out way to remove these since they are only used for CollectionManagement
        public bool PurchasedOnly { get; set; }
        public ResourceListType ResourceListType { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public bool IncludePdaResources { get; set; }
        public bool IncludePdaHistory { get; set; }
        public int CollectionListFilter { get; set; }
        public bool IncludeSpecialDiscounts { get; set; }
        public bool IncludeFreeResources { get; set; }
        public bool RecommendationsOnly { get; set; }
        public bool IncludeExpertReviewer { get; set; }
        public bool ExcludeExpertReviewer { get; set; }


        //TODO: Figure out way to remove these since they are only used for ReserveShelfManagement
        public int ReserveShelfId { get; set; }
        public int ReviewId { get; set; }

        public string ToDebugInfo()
        {
            return new StringBuilder()
                .AppendFormat("InstitutionQuery = [Query = {0}", Query)
                .AppendFormat(", SortBy = {0}", SortBy)
                .AppendFormat(", SortDirection = {0}", SortDirection)
                .AppendFormat(", DefaultQuery = {0}", DefaultQuery)
                .AppendFormat(", Page = {0}", Page)
                .AppendFormat(", AccountStatus = {0}", AccountStatus)
                .AppendFormat(", TerritoryId = {0}", TerritoryId)
                .AppendFormat(", InstitutionTypeId = {0}", InstitutionTypeId)
                .Append("]")
                .ToString();
        }
    }
}