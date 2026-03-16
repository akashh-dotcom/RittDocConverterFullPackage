#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Search
{
    public interface ISearchTypeaheadService
    {
        IEnumerable<string> GetTypeaheadSearchTerms(string searchInput, int searchTypeaheadResultLimit);
        void ClearCache();
    }

    public class SearchTypeaheadService : ISearchTypeaheadService
    {
        private readonly IQueryable<SearchTypeahead> _searchTypeaheads;

        public SearchTypeaheadService(IQueryable<SearchTypeahead> searchTypeaheads)
        {
            _searchTypeaheads = searchTypeaheads;

            if (Cache == null)
            {
                InitializeCache();
            }
        }

        private static Dictionary<string, IEnumerable<string>> Cache { get; set; }

        public IEnumerable<string> GetTypeaheadSearchTerms(string searchInput, int searchTypeaheadResultLimit)
        {
            if (string.IsNullOrEmpty(searchInput))
            {
                return null;
            }

            if (!Cache.ContainsKey(searchInput))
            {
                Cache[searchInput] = _searchTypeaheads
                    .Where(t => t.SearchTerm.Contains(searchInput) /*&& t.SearchTerm.Length > searchInput.Length*/)
                    .OrderByDescending(t => t.SearchTerm.StartsWith(searchInput) ? 1 : 0)
                    .ThenByDescending(t => t.Rank)
                    .ThenBy(t => t.SearchTerm.Length)
                    .Take(searchTypeaheadResultLimit)
                    .Select(t => t.SearchTerm)
                    .ToList();
            }

            return Cache[searchInput];
        }

        public void ClearCache()
        {
            InitializeCache();
        }

        private static void InitializeCache()
        {
            Cache = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}