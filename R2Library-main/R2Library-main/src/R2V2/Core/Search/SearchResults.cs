#region

using System;
using System.Collections.Generic;
using R2V2.Core.Search.FacetData;

#endregion

namespace R2V2.Core.Search
{
    public class SearchResults : ISearchResults
    {
        private readonly List<IFacetData> _facetData = new List<IFacetData>();
        private readonly List<ISearchResultsItem> _items = new List<ISearchResultsItem>();

        public ISearchRequest SearchRequest { get; set; }

        public int HitCount { get; set; }
        public int FileCount { get; set; }
        public TimeSpan SearchTimeSpan { get; set; }
        public ISearchHistory SearchHistory { get; set; }

        public IEnumerable<ISearchResultsItem> Items => _items;
        public IEnumerable<IFacetData> FacetDataList => _facetData;

        public void AddItem(ISearchResultsItem item)
        {
            _items.Add(item);
        }

        public void AddFacetData(IFacetData facetData)
        {
            _facetData.Add(facetData);
        }

        public void AddFacetDataRange(IEnumerable<IFacetData> facetDatas)
        {
            _facetData.AddRange(facetDatas);
        }
    }
}