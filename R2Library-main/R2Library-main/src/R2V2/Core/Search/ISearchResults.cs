#region

using System;
using System.Collections.Generic;
using R2V2.Core.Search.FacetData;

#endregion

namespace R2V2.Core.Search
{
    public interface ISearchResults
    {
        ISearchRequest SearchRequest { get; set; }
        int HitCount { get; set; }
        int FileCount { get; set; }
        TimeSpan SearchTimeSpan { get; set; }
        IEnumerable<ISearchResultsItem> Items { get; }
        IEnumerable<IFacetData> FacetDataList { get; }
        ISearchHistory SearchHistory { get; set; }

        void AddItem(ISearchResultsItem item);
        void AddFacetData(IFacetData facetData);
        void AddFacetDataRange(IEnumerable<IFacetData> facetDatas);
    }
}