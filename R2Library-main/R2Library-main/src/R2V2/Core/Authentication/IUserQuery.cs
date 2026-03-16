#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    public interface IUserQuery : IQuery
    {
        int InstitutionId { get; set; }

        string SortBy { get; set; }

        int Page { get; set; }
        int PageSize { get; set; }

        RoleCode RoleCode { get; set; }

        string SearchType { get; set; }

        bool LoadAllUsers { get; set; }
        bool IsUserIa { get; }

        new string Query { get; set; }

        int UserStatus { get; set; }
    }
}