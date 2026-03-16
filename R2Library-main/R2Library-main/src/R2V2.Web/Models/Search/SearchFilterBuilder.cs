#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Models.Shared;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchFilterBuilder
    {
        private readonly Dictionary<string, string> _fieldCodes = new Dictionary<string, string>
        {
            { "IndexTerms", "index-terms" },
            { "FullText", "full-text" },
            { "BookTitle", "book-title" },
            { "ChapterTitle", "chapter-title" },
            { "SectionTitle", "section-title" },
            { "ImageTitle", "image-title" },
            { "VideoSection", "video-section" }
        };

        private readonly Dictionary<string, string> _filterCodes = new Dictionary<string, string>
        {
            { "drug", "drug" }
        };

        private readonly ILog<SearchFilterBuilder> _log;
        private readonly PracticeAreaService _practiceAreaService;
        private readonly ResourceService _resourceService;
        private readonly SpecialtyService _specialtyService;

        /// <summary>
        /// </summary>
        public SearchFilterBuilder(PracticeAreaService practiceAreaService
            , ResourceService resourceService
            , SpecialtyService specialtyService
            , ILog<SearchFilterBuilder> log)
        {
            _practiceAreaService = practiceAreaService;
            _resourceService = resourceService;
            _specialtyService = specialtyService;
            _log = log;
        }


        public Dictionary<string, Filter> GetFieldFilters()
        {
            return new Dictionary<string, Filter>
            {
                { "index-terms", new Filter { Name = "Indexed Terms", Count = "0" } },
                { "full-text", new Filter { Name = "Full Text", Count = "0" } },
                { "book-title", new Filter { Name = "Book Title", Count = "0" } },
                { "chapter-title", new Filter { Name = "Chapter Title", Count = "0" } },
                { "section-title", new Filter { Name = "Section Title", Count = "0" } },
                { "image-title", new Filter { Name = "Images", Count = "0" } },
                { "video-section", new Filter { Name = "Videos", Count = "0" } }
            };
        }

        public Dictionary<string, Filter> GetFieldFilters(IEnumerable<SearchResultOption> searchResultOptions)
        {
            var filters = GetFieldFilters();

            foreach (var searchResultOption in searchResultOptions)
            {
                if (_fieldCodes.ContainsKey(searchResultOption.Code))
                {
                    var code = _fieldCodes[searchResultOption.Code];
                    var filter = filters[code];
                    filter.Count = searchResultOption.CountText;
                }
            }

            return filters;
        }

        public Dictionary<string, Filter> GetFilterFilters()
        {
            return new Dictionary<string, Filter> { { "drug", new Filter { Name = "Drug", Count = "0" } } };
        }

        public Dictionary<string, Filter> GetFilterFilters(IEnumerable<SearchResultOption> searchResultOptions)
        {
            var filters = GetFilterFilters();

            foreach (var searchResultOption in searchResultOptions)
            {
                var code = _filterCodes[searchResultOption.Code];
                var filter = filters[code];
                filter.Count = searchResultOption.CountText;
                //_log.DebugFormat("code: {0}, filter.Name: {1}, filter.Count: {2}", code, filter.Name, filter.Count);
                //_log.DebugFormat("searchResultOption.Code: {0}, searchResultOption.Count: {1}, searchResultOption.CountText: {2}", searchResultOption.Code, searchResultOption.Count, searchResultOption.CountText);
            }

            return filters;
        }

        public Dictionary<string, Filter> GetPracticeAreaFilters()
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();
            return practiceAreas.ToDictionary(practiceArea => $"{practiceArea.Id}",
                practiceArea => new Filter { Name = practiceArea.Name, Count = "0" });
        }

        public Dictionary<string, Filter> GetPracticeAreaFilters(IEnumerable<SearchResultOption> searchResultOptions)
        {
            var filters = GetPracticeAreaFilters();

            foreach (var searchResultOption in searchResultOptions)
            {
                if (searchResultOption.Id != 0)
                {
                    var code = $"{searchResultOption.Id}";
                    var filter = filters[code];
                    filter.Count = searchResultOption.CountText;
                }
            }

            return filters;
        }

        public Dictionary<string, Filter> GetPublicationDateFilters()
        {
            var years = _resourceService.GetResourcePublicationYears();
            return years.ToDictionary(year => $"{year}", year => new Filter { Name = $"{year}", Count = "0" });
        }

        public Dictionary<string, Filter> GetPublicationDateFilters(IEnumerable<SearchResultOption> searchResultOptions)
        {
            var filters = GetPublicationDateFilters();

            foreach (var searchResultOption in searchResultOptions)
            {
                var filter = filters[searchResultOption.Code];
                filter.Count = searchResultOption.CountText;
            }

            return filters;
        }

        public Dictionary<string, Filter> GetDisciplineFilters()
        {
            var specialties = _specialtyService.GetAllSpecialties();
            return specialties.ToDictionary(specialty => $"{specialty.Id}",
                specialty => new Filter { Name = specialty.Name, Count = "0" });
        }

        public Dictionary<string, Filter> GetDisciplineFilters(IEnumerable<SearchResultOption> searchResultOptions)
        {
            var filters = GetDisciplineFilters();
            var sb = new StringBuilder();
            foreach (var searchResultOption in searchResultOptions)
            {
                var code = $"{searchResultOption.Id}";
                if (filters.ContainsKey(code))
                {
                    var filter = filters[code];
                    filter.Count = searchResultOption.CountText;
                }
                else
                {
                    sb.Append($"{(sb.Length == 0 ? "" : ", ")}[{searchResultOption.ToDebug()}]");
                }
            }

            if (sb.Length > 0)
            {
                _log.Error($"No Specialty found for the following: [{sb}]");
            }

            return filters;
        }
    }
}