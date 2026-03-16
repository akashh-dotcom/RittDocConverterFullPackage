#region

using System;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.SuperType
{
    public interface IQuery
    {
        string Query { get; set; }

        SortDirection SortDirection { get; set; }

        bool PurchasedOnly { get; set; }

        bool DefaultQuery { get; }

        int ReserveShelfId { get; set; }
        int ReviewId { get; set; }

        ResourceListType ResourceListType { get; set; }
        DateTime DateRangeStart { get; set; }
        DateTime DateRangeEnd { get; set; }
        bool IncludePdaResources { get; set; }
        bool IncludePdaHistory { get; set; }
        bool IncludeFreeResources { get; set; }
        bool RecommendationsOnly { get; set; }
        int CollectionListFilter { get; set; }
        bool IncludeSpecialDiscounts { get; set; }
    }
}