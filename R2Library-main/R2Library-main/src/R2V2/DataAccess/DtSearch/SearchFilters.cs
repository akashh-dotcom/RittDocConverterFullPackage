#region

using R2V2.Core.Search;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class SearchFilters
    {
        /// <summary>
        /// </summary>
        public SearchFilters(ISearchRequest searchRequest, string indexLocation, bool isAdminSearch)
        {
            if (isAdminSearch)
            {
                PrimaryFilter = new ResourceFilter(searchRequest, indexLocation);
                AlternateFilter = new ResourceFilter(searchRequest, indexLocation);
            }
            else
            {
                PrimaryFilter = new ResourceFilter(searchRequest, indexLocation, true);
                AlternateFilter = new ResourceFilter(searchRequest, indexLocation, false);
            }
        }

        public ResourceFilter PrimaryFilter { get; private set; }
        public ResourceFilter AlternateFilter { get; private set; }
    }
}