#region

using System;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.Authentication
{
    public class UserQuery : IUserQuery
    {
        private const int DefaultPage = 1;
        private const int DefaultPageSize = 10;

        private int _page;

        private int _pageSize;

        public UserQuery()
        {
        }

        public UserQuery(IUserQuery userQuery)
        {
            InstitutionId = userQuery.InstitutionId;
            Query = userQuery.Query;
            SortBy = userQuery.SortBy;
            SortDirection = userQuery.SortDirection;
            Page = userQuery.Page;
            PageSize = userQuery.PageSize;
            RoleCode = userQuery.RoleCode;
            SearchType = userQuery.SearchType;
            LoadAllUsers = userQuery.LoadAllUsers;
            UserStatus = userQuery.UserStatus;
        }

        public int InstitutionId { get; set; }

        public string Query { get; set; }

        public string SortBy { get; set; }
        public SortDirection SortDirection { get; set; }

        public bool DefaultQuery =>
            string.IsNullOrWhiteSpace(Query)
            && string.IsNullOrWhiteSpace(SortBy)
            && SortDirection == SortDirection.Ascending
            && RoleCode == RoleCode.NoUser
            && (string.IsNullOrWhiteSpace(SearchType) || SearchType.ToLower() == "all")
            && UserStatus == 0;

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

        public RoleCode RoleCode { get; set; }

        public string SearchType { get; set; }

        public bool LoadAllUsers { get; set; }

        public bool IsUserIa => RoleCode == RoleCode.INSTADMIN;

        public int UserStatus { get; set; }


        //TODO: Figure out way to remove these since they are only used for CollectionManagement
        public bool PurchasedOnly { get; set; }
        public ResourceListType ResourceListType { get; set; }
        public bool IncludePdaResources { get; set; }
        public bool IncludePdaHistory { get; set; }
        public int CollectionListFilter { get; set; }
        public bool IncludeSpecialDiscounts { get; set; }
        public bool IncludeFreeResources { get; set; }
        public bool RecommendationsOnly { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }

        //TODO: Figure out way to remove these since they are only used for ReserveShelfManagement
        public int ReserveShelfId { get; set; }
        public int ReviewId { get; set; }
    }
}