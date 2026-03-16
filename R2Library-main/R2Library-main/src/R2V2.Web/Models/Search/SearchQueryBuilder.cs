#region

using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Models.Search.Fields;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchQueryBuilder
    {
        private readonly ILog<SearchQueryBuilder> _log;
        private readonly PracticeAreaService _practiceAreaService;
        private readonly SpecialtyService _specialtyService;

        public SearchQueryBuilder(ILog<SearchQueryBuilder> log
            , PracticeAreaService practiceAreaService
            , SpecialtyService specialtyService
        )
        {
            _log = log;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
        }

        public SearchQuery GetSearchQuery(SearchSummary searchSummary)
        {
            var searchQuery = new SearchQuery
            {
                Page = 1,
                PageSize = searchSummary.PageSize < 10 ? 10 : searchSummary.PageSize,
                Q = searchSummary.Term,
                Author = searchSummary.Author,
                Title = searchSummary.BookTitle,
                Publisher = searchSummary.Publisher,
                Editor = searchSummary.Editor,
                Isbn = searchSummary.Isbns == null ? null : string.Join(",", searchSummary.Isbns),
                Within = searchSummary.SearchWithinIsbns == null
                    ? null
                    : string.Join(",", searchSummary.SearchWithinIsbns),
                Filter = searchSummary.DrugMonograph ? "drug" : null,
                Include = (searchSummary.Active ? 1 : 0) + (searchSummary.Archive ? 2 : 0),
                TocAvailable = searchSummary.IncludeTocResouces,
                Year = searchSummary.Years
            };

            if (searchQuery.Include == 0)
            {
                searchQuery.Include = 1;
            }

            var searchField = SearchFieldsFactory.GetSearchFieldByValue(searchSummary.Field);
            searchQuery.Field = searchField.Code;

            if (!string.IsNullOrWhiteSpace(searchSummary.PracticeAreaCode))
            {
                var practiceArea = _practiceAreaService.GetPracticeAreaByCode(searchSummary.PracticeAreaCode);
                if (practiceArea != null)
                {
                    searchQuery.PracticeArea = practiceArea.Id.ToString();
                }
                else
                {
                    _log.WarnFormat("practice area is null, code: {0}", searchSummary.PracticeAreaCode);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchSummary.SpecialtyCode))
            {
                var specialty = _specialtyService.GetSpecialtyByCode(searchSummary.SpecialtyCode);
                if (specialty != null)
                {
                    searchQuery.Disciplines = specialty.Id.ToString();
                }
                else
                {
                    _log.WarnFormat("specialty is null, code: {0}", searchSummary.SpecialtyCode);
                }
            }

            return searchQuery;
        }
    }
}