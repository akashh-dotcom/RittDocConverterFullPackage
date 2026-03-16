using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Resource;
using R2V2.Core.Search;
using R2V2.Core.Search.FacetData;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

namespace R2V2.DataAccess.DtSearch
{
    // Minimal no-op implementation to avoid loading native dtSearch binaries during debugging.
    public class NullSearch : ISearch
    {
        private readonly ILog<NullSearch> _log;
        private readonly string _indexLocation;

        public NullSearch(ILog<NullSearch> log, ContentSettings contentSettings)
        {
            _log = log;
            _indexLocation = contentSettings?.DtSearchIndexLocation;
            _log.Info("NullSearch initialized - dtSearch is disabled for debugging.");
        }

        private class NullSearchResults : ISearchResults
        {
            public ISearchRequest SearchRequest { get; set; }
            public int FileCount { get; set; }
            public int HitCount { get; set; }
            public TimeSpan SearchTimeSpan { get; set; } = TimeSpan.Zero;
            public ISearchHistory SearchHistory { get; set; }
            public IEnumerable<ISearchResultsItem> Items { get; }
            public IEnumerable<IFacetData> FacetDataList { get; } = Enumerable.Empty<IFacetData>();

            public void AddItem(ISearchResultsItem item)
            {
                throw new NotImplementedException();
            }

            public void AddFacetData(IFacetData facetData)
            {
                throw new NotImplementedException();
            }

            public void AddFacetDataRange(IEnumerable<IFacetData> facetDatas)
            {
                throw new NotImplementedException();
            }
        }

        public SearchResults Execute(ISearchRequest searchRequest)
        {
            // Return a valid (but empty) results object so callers like SearchService can continue.
            return new SearchResults
            {
                SearchRequest = searchRequest,
                FileCount = 0,
                HitCount = 0,
                SearchTimeSpan = TimeSpan.Zero,
                SearchHistory = null
            };
        }

        public List<IResource> ExecuteAdmin(ISearchRequest searchRequest)
        {
            return new List<IResource>();
        }

        public IndexStatus GetIndexStatus()
        {
            return new IndexStatus(null, _indexLocation);
        }
    }
}