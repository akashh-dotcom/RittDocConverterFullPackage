#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Core.Search
{
    public class SearchRequest : ISearchRequest
    {
        private readonly IList<SearchResource> _activeResources = new List<SearchResource>();

        private readonly IList<SearchResource> _archivedResources = new List<SearchResource>();

        //private int _include = 1;
        private readonly List<string> _isbns = new List<string>();
        private readonly IList<SearchResource> _resources = new List<SearchResource>();
        private readonly List<string> _searchWithInIsbns = new List<string>();

        public string SearchTerm { get; set; }

        public IEnumerable<SearchResource> Resources => _resources;
        public IEnumerable<SearchResource> ActiveResources => _activeResources;
        public IEnumerable<SearchResource> ArchivedResources => _archivedResources;

        public int Page { get; set; }
        public int PageSize { get; set; }
        public SearchSortBy SortBy { get; set; }
        public bool Active { get; set; }
        public bool Archive { get; set; }
        public IPracticeArea PracticeArea { get; set; }
        public ISpecialty Specialty { get; set; }
        public string Author { get; set; }
        public string BookTitle { get; set; }
        public string Publisher { get; set; }
        public string Editor { get; set; }
        public int PublicationYearMin { get; set; }
        public int PublicationYearMax { get; set; }
        public SearchFields Field { get; set; }
        public bool DrugMonograph { get; set; }
        public int Year { get; set; }
        public bool IncludeTocResouces { get; set; } = true;

        public string[] Isbns => _isbns.ToArray();

        public string[] SearchWithinIsbns => _searchWithInIsbns.ToArray();

        public void AddIsbn(string isbn)
        {
            _isbns.Add(isbn);
        }

        public void AddIsbns(IEnumerable<string> isbns)
        {
            _isbns.AddRange(isbns);
        }

        public void AddSearchWithinIsbn(string isbn)
        {
            _searchWithInIsbns.Add(isbn);
        }

        public void AddSearchWithinIsbns(IEnumerable<string> isbns)
        {
            _searchWithInIsbns.AddRange(isbns);
        }

        public string GetQuery()
        {
            var query = new StringBuilder();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query.Append(SearchTerm);
            }

            AppendFieldToQuery(query, "author", Author);
            AppendFieldToQuery(query, "title", BookTitle);
            AppendFieldToQuery(query, "publisher", Publisher);
            AppendFieldToQuery(query, "editor", Editor);

            if (PublicationYearMax > 0 && PublicationYearMin > 0)
            {
                AppendFieldToQuery(query, "years", $"{PublicationYearMin}-{PublicationYearMax}");
            }

            if (_isbns.Count > 0)
            {
                AppendFieldToQuery(query, "isbns", string.Join(",", _isbns.ToArray()));
            }

            return query.ToString();
        }

        public SearchResource GetSearchResourceByIsbn(string isbn)
        {
            var searchResource = _resources.SingleOrDefault(r => r.Resource.Isbn == isbn);
            return searchResource;
        }

        public void AddSearchResource(SearchResource searchResource)
        {
            if (searchResource.Resource.IsActive())
            {
                _activeResources.Add(searchResource);
                _resources.Add(searchResource);
            }
            else if (searchResource.Resource.IsArchive())
            {
                _archivedResources.Add(searchResource);
                _resources.Add(searchResource);
            }
        }

        public void AddSearchResourceAdmin(SearchResource searchResource)
        {
            _resources.Add(searchResource);
        }

        // default to true, institution must have access to search all content

        //public SearchResource[] ActiveResources { get; set; }
        //public SearchResource[] ArchivedResources { get; set; }

        public static SearchSortBy ConvertSortBy(string sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return SearchSortBy.Relevance;
            }

            if (sortBy.ToLower() == "booktitle")
            {
                return SearchSortBy.BookTitle;
            }

            if (sortBy.ToLower() == "author")
            {
                return SearchSortBy.Author;
            }

            return sortBy.ToLower() == "publisher" ? SearchSortBy.Publisher : SearchSortBy.Relevance;
        }

        private void AppendFieldToQuery(StringBuilder query, string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(Author))
            {
                query.AppendFormat("{0}{1}:{2}", query.Length > 0 ? "; " : string.Empty, fieldName, fieldValue);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("SearchRequest = [");
            sb.AppendFormat("SearchTerm: {0}", SearchTerm);
            sb.AppendFormat(", Page: {0}", Page);
            sb.AppendFormat(", PageSize: {0}", PageSize);
            sb.AppendFormat(", SortBy: {0}", SortBy);
            sb.AppendFormat(", Active: {0}", Active);
            sb.AppendFormat(", Archive: {0}", Archive);
            sb.AppendFormat(", IncludeTocResouces: {0}", IncludeTocResouces);
            sb.AppendFormat(", PracticeArea: {0}", PracticeArea);
            sb.AppendFormat(", Discipline: {0}", Specialty);
            sb.AppendFormat(", Field: {0}", Field);
            sb.AppendFormat(", Isbn: {0}", string.Join(",", _isbns.ToArray()));
            sb.AppendFormat(", SearchWithInIsbns: {0}", string.Join(",", _searchWithInIsbns.ToArray()));
            sb.AppendFormat(", Year: {0}", Year);
            sb.AppendFormat(", DrugMonograph: {0}", DrugMonograph);
            sb.AppendFormat(", PublicationYearMin: {0}", PublicationYearMin);
            sb.AppendFormat(", PublicationYearMax: {0}", PublicationYearMax);
            sb.AppendFormat(", Author: {0}", Author);
            sb.AppendFormat(", BookTitle: {0}", BookTitle);
            sb.AppendFormat(", Publisher: {0}", Publisher);
            sb.AppendFormat(", Editor: {0}", Editor);
            sb.AppendFormat(", _resources.Count: {0}", _resources.Count);
            sb.AppendFormat(", _activeResources.Count: {0}", _activeResources.Count);
            sb.AppendFormat(", _archivedResources.Count(): {0}", _archivedResources.Count);
            sb.Append("]");
            return sb.ToString();
        }
    }
}