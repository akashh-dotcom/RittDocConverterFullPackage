#region

using System.Collections.Generic;
using System.Text;
using dtSearch.Engine;
using R2V2.Core;
using R2V2.Core.Search;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class ResourceFilter : IDebugInfo
    {
        protected int _fileCount;
        protected int _indexId;
        protected int _resourceCount;
        protected ISearchRequest _searchRequest;

        /// <param name="primary"> </param>
        public ResourceFilter(ISearchRequest searchRequest, string indexLocation, bool primary)
        {
            SearchFilter = new SearchFilter();
            _indexId = SearchFilter.AddIndex(indexLocation);
            _searchRequest = searchRequest;

            if (primary)
            {
                if (searchRequest.Active)
                {
                    SetFilter(searchRequest.ActiveResources);
                }

                if (searchRequest.Archive)
                {
                    SetFilter(searchRequest.ArchivedResources);
                }
            }
            else
            {
                if (!searchRequest.Active)
                {
                    SetFilter(searchRequest.ActiveResources);
                }

                if (!searchRequest.Archive)
                {
                    SetFilter(searchRequest.ArchivedResources);
                }
            }
        }

        public ResourceFilter(ISearchRequest searchRequest, string indexLocation)
        {
            SearchFilter = new SearchFilter();
            _indexId = SearchFilter.AddIndex(indexLocation);
            _searchRequest = searchRequest;

            SetFilter2(searchRequest.Resources);
        }

        public SearchFilter SearchFilter { get; }
        public int ResourceCount => _resourceCount;
        public int FileCount => _fileCount;

        public string ToDebugString()
        {
            var sb = new StringBuilder("ResourceFilter = [");
            sb.AppendFormat("_resourceCount: {0}", _resourceCount);
            sb.AppendFormat(", _fileCount: {0}", _fileCount);
            sb.AppendFormat(", _indexId: {0}", _indexId);
            return sb.ToString();
        }

        protected void SetFilter(IEnumerable<SearchResource> searchResources)
        {
            foreach (var searchResource in searchResources)
            {
                if (!_searchRequest.IncludeTocResouces && !searchResource.FullTextAvailable)
                {
                    continue;
                }

                //int minDocId = searchResource.Resource.GetMinDocumentId();
                //int maxDocId = searchResource.Resource.GetMaxDocumentId();
                var minDocId = searchResource.Resource.DocumentIdMin;
                var maxDocId = searchResource.Resource.DocumentIdMax;
                //_log.DebugFormat("archived resource: {0} - {1}, docIds: {2} - {3}", searchResource.Resource.Id, searchResource.Resource.Isbn, minDocId, maxDocId);
                if (minDocId > 0 && maxDocId >= minDocId)
                {
                    //_log.DebugFormat("archived resource: {0} - {1}, docIds: {2} - {3}", searchResource.Resource.Id, searchResource.Resource.Isbn, minDocId, maxDocId);
                    SearchFilter.SelectItems(_indexId, minDocId, maxDocId, true);
                    _resourceCount++;
                    _fileCount += maxDocId - (minDocId - 1);
                }
            }
        }

        protected void SetFilter2(IEnumerable<SearchResource> searchResources)
        {
            foreach (var searchResource in searchResources)
            {
                var minDocId = searchResource.Resource.DocumentIdMin;
                var maxDocId = searchResource.Resource.DocumentIdMax;
                if (minDocId > 0 && maxDocId >= minDocId)
                {
                    SearchFilter.SelectItems(_indexId, minDocId, maxDocId, true);
                    _resourceCount++;
                    _fileCount += maxDocId - (minDocId - 1);
                }
            }
        }
    }
}