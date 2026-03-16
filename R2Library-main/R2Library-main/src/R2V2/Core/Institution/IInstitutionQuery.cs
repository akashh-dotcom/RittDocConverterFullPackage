#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    public interface IInstitutionQuery : IQuery
    {
        string SortBy { get; set; }

        string Page { get; set; }

        AccountStatus AccountStatus { get; set; }

        int TerritoryId { get; set; }

        int InstitutionTypeId { get; set; }

        bool IncludeExpertReviewer { get; set; }
        bool ExcludeExpertReviewer { get; set; }

        bool DefaultQueryExceptSearch { get; }

        bool RecentOnly { get; set; }
        bool AlphaFilter { get; }

        string ToDebugInfo();
    }
}