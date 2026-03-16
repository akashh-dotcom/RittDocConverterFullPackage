#region

using System.Collections.Generic;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Core.Search
{
    public interface ISearchRequest
    {
        string SearchTerm { get; set; }

        // resources
        IEnumerable<SearchResource> Resources { get; }
        IEnumerable<SearchResource> ActiveResources { get; }
        IEnumerable<SearchResource> ArchivedResources { get; }

        // paging
        int Page { get; set; }
        int PageSize { get; set; }

        // sorting
        SearchSortBy SortBy { get; set; }

        // limits/filters
        bool Active { get; set; }

        bool Archive { get; set; }

        //int Include { get; set; }
        //string PracticeArea { get; set; }
        //string Discipline { get; set; }
        IPracticeArea PracticeArea { get; set; }
        ISpecialty Specialty { get; set; }
        SearchFields Field { get; set; }
        string[] SearchWithinIsbns { get; }
        bool DrugMonograph { get; set; }
        bool IncludeTocResouces { get; set; }

        // advanced search items
        string Author { get; set; }
        string BookTitle { get; set; }
        string Publisher { get; set; }
        string Editor { get; set; }
        int PublicationYearMin { get; set; }
        int PublicationYearMax { get; set; }
        string[] Isbns { get; }
        int Year { get; set; }

        // methods
        void AddIsbn(string isbn);
        void AddIsbns(IEnumerable<string> isbn);
        void AddSearchWithinIsbn(string isbn);
        void AddSearchWithinIsbns(IEnumerable<string> isbn);
        string GetQuery();
        void AddSearchResource(SearchResource searchResource);
        void AddSearchResourceAdmin(SearchResource searchResource);

        SearchResource GetSearchResourceByIsbn(string isbn);
    }
}