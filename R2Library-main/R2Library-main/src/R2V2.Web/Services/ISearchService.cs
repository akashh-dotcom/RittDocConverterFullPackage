#region

using System.Collections.Generic;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Web.Models.Search;

#endregion

namespace R2V2.Web.Services
{
    public interface ISearchService
    {
        SearchResultSet Search(SearchQuery query);
        SearchResultSet GetEmptySearchResultSet(SearchQuery query);
        IEnumerable<string> GetTypeaheadSearchTerms(string searchInput);
        List<IResource> SearchAdmin(string query, IEnumerable<License> licences);

        void ClearTypeaheadCache();
    }
}