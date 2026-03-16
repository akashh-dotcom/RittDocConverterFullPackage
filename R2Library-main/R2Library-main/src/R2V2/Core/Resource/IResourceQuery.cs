#region

using System;
using R2V2.Core.Institution;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public interface IResourceQuery : IQuery
    {
        int ResourceId { get; set; }

        string SortBy { get; set; }

        int Page { get; set; }
        int PageSize { get; set; }

        ResourceStatus ResourceStatus { get; set; }

        ResourceFilterType ResourceFilterType { get; set; }
        //CollectionIdentifier CollectionType { get; set; }

        int PracticeAreaFilter { get; set; }

        int CollectionFilter { get; set; }

        //bool IncludePdaResources { get; set; }
        //bool IncludePdaHistory { get; set; }
        int SpecialtyFilter { get; set; }

        DateTime? PdaDateAddedMin { get; set; }
        DateTime? PdaDateAddedMax { get; set; }
        bool RecentOnly { get; set; }

        DateTime? TurnawayStartDate { get; set; }

        int PublisherId { get; set; }

        PdaStatus PdaStatus { get; set; }
    }
}