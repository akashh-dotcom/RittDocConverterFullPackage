#region

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Search;
using R2V2.Core.Search.FacetData;
using R2V2.Infrastructure.Authentication;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchResultSet : BaseModel
    {
        private static readonly List<KeyValuePair<int, int>> PageOptions = new List<KeyValuePair<int, int>>
        {
            new KeyValuePair<int, int>(10, 10),
            new KeyValuePair<int, int>(25, 25),
            new KeyValuePair<int, int>(50, 50)
        };

        private readonly List<string> _alternateTerms = new List<string>();


        private readonly ISearchRequest _searchRequest;
        private readonly List<SearchResult> _searchResults = new List<SearchResult>();
        private List<SearchResultOption> _fieldOptions;
        private List<SearchResultOption> _practiceAreaResultOptions;
        private List<SearchResultOption> _searchResultOptions;
        private List<SearchResultOption> _specialtyOptions;
        private List<SearchResultOption> _yearOptions;

        public SearchResultSet()
        {
        }

        public SearchResultSet(ISearchRequest searchRequest, ISearchResults results, SearchQuery query,
            AuthenticatedInstitution authenticatedInstitution, ISpecialtyService specialtyService)
        {
            Q = searchRequest.SearchTerm;
            _searchRequest = searchRequest;
            PageSize = _searchRequest.PageSize;
            Active = _searchRequest.Active;
            Archive = _searchRequest.Archive;
            PageSizeOptions = new SelectList(PageOptions, "Key", "Value");

            IncludeTocOnlyTitles = searchRequest.IncludeTocResouces;
            DisplayTocOnlyTitlesOption = true;

            Query = query;

            if (results == null)
            {
                TotalResultsCount = 0;
                TotalSearchTime = "0.000";
                return;
            }

            PopulateResults(results, authenticatedInstitution);
            TotalResultsCount = results.FileCount;
            TotalSearchTime = $"{results.SearchTimeSpan.TotalSeconds:0.000}";

            SetSearchResultOptions(results.FacetDataList, specialtyService);

            if (searchRequest.SearchWithinIsbns.Length != 1)
            {
                return;
            }

            var searchResource = searchRequest.GetSearchResourceByIsbn(searchRequest.SearchWithinIsbns[0]);
            if (searchResource != null)
            {
                SearchWithinTitle = searchResource.Resource.Title;
            }
        }

        public SearchQuery Query { get; private set; }
        public IEnumerable<SearchResult> SearchResults => _searchResults;
        public int TotalResultsCount { get; private set; }
        public string TotalSearchTime { get; private set; }

        public JumpToLink PreviousLink { get; set; }
        public JumpToLink NextLink { get; set; }
        public IEnumerable<JumpToLink> JumpToLinks { get; set; }

        public int ActiveCount { get; private set; }
        public int ArchiveCount { get; private set; }
        public SearchResultOption DrugMonographOption { get; private set; }

        public SelectList PageSizeOptions { get; private set; }

        public int PageSize { get; private set; }
        public bool Active { get; private set; }
        public bool Archive { get; private set; }

        public bool DisplayTocOnlyTitlesOption { get; set; }
        public bool IncludeTocOnlyTitles { get; private set; }

        public string SearchWithinTitle { get; private set; }
        public string[] AlternateTerms => _alternateTerms.ToArray();

        public bool DisplayJson { get; set; }
        public bool DisplayHtml { get; set; }
        public string FederatedSearchMessage { get; set; }

        public IEnumerable<SearchResultOption> SearchResultOptions => _searchResultOptions;

        public IEnumerable<SearchResultOption> PracticeAreaResultOptions => _practiceAreaResultOptions;

        public IEnumerable<SearchResultOption> FieldOptions => _fieldOptions;

        public IEnumerable<SearchResultOption> YearOptions => _yearOptions;

        public IEnumerable<SearchResultOption> SpecialtyOptions => _specialtyOptions;

        public int AlternateTermCount => _alternateTerms.Count;

        public AlternateTerm[] GetAlternateTerms()
        {
            return _alternateTerms.Select(alternateTerm => new AlternateTerm(alternateTerm)).ToArray();
        }

        private void SetSearchResultOptions(IEnumerable<IFacetData> facetDataList, ISpecialtyService specialtyService)
        {
            _searchResultOptions = new List<SearchResultOption>();
            _practiceAreaResultOptions = new List<SearchResultOption>();
            _fieldOptions = new List<SearchResultOption>();
            _yearOptions = new List<SearchResultOption>();
            _specialtyOptions = new List<SearchResultOption>();

            var selectedSpecialtyCodes = new List<string>();
            var specialties = specialtyService.GetAllSpecialties().ToList();
            foreach (var facetData in facetDataList)
            {
                if (facetData.Count < 1)
                {
                    continue;
                }

                var option = new SearchResultOption
                    { Count = facetData.Count, Name = facetData.Name, Id = facetData.Id, Code = facetData.Code };

                if (facetData is PracticeAreaFacetData)
                {
                    option.Group = "practice-area";
                    option.Selected = _searchRequest.PracticeArea != null &&
                                      option.Code == _searchRequest.PracticeArea.Code;
                    _practiceAreaResultOptions.Add(option);
                }

                if (facetData is SpecialtyFacetData)
                {
                    option.Group = "specialty";
                    option.Selected = _searchRequest.Specialty != null && option.Code != null &&
                                      _searchRequest.Specialty.Code.Contains(option.Code);
                    if (option.Selected)
                    {
                        //selectedSpecialtyCodes.AppendFormat("{0},", option.Code);
                        selectedSpecialtyCodes.Add(option.Code);
                    }

                    //R2D0070 R2D0099
                    //Results with mulitple specialies are not showing up.
                    if (option.Id == 0)
                    {
                        var specialtyCodeNameArray = option.Name.Split(' ');
                        foreach (var s in specialtyCodeNameArray)
                        {
                            var specialty = specialties.FirstOrDefault(x => x.Code == s);
                            if (specialty != null)
                            {
                                var option2 = new SearchResultOption
                                {
                                    Count = facetData.Count,
                                    Name = specialty.Name,
                                    Id = specialty.Id,
                                    Code = specialty.Code,
                                    Group = "specialty",
                                    Selected = _searchRequest.Specialty != null &&
                                               _searchRequest.Specialty.Code.Contains(specialty.Code)
                                };
                                _specialtyOptions.Add(option2);
                            }
                        }
                    }
                    else
                    {
                        _specialtyOptions.Add(option);
                    }
                }
                else if (facetData is FieldFacetData)
                {
                    option.Group = "field";
                    option.Selected = option.Id == (int)_searchRequest.Field;
                    _fieldOptions.Add(option);
                }
                else if (facetData is YearFacetData)
                {
                    option.Group = "year";
                    option.Selected = option.Name == _searchRequest.Year.ToString(CultureInfo.InvariantCulture);
                    _yearOptions.Add(option);
                }
                else if (facetData is DrugMonographFacetData)
                {
                    option.Group = "drug";
                    option.Selected = _searchRequest.DrugMonograph;
                    DrugMonographOption = option;
                }
                else if (option.Name == "Active")
                {
                    option.Group = "BookStatus";
                    option.Selected = _searchRequest.Active;
                    ActiveCount = option.Count;
                }
                else if (option.Name == "Archive")
                {
                    option.Selected = _searchRequest.Archive;
                    option.Group = "BookStatus";
                    ArchiveCount = option.Count;
                }

                _searchResultOptions.Add(option);
            }

            _yearOptions.Sort();

            _specialtyOptions.Sort();
            var specialtyCodes = string.Join(",", selectedSpecialtyCodes);
            if (specialtyCodes.Length > 0)
            {
                specialtyCodes = $"{specialtyCodes},";
            }

            // ReSharper disable ForCanBeConvertedToForeach
            for (var i = 0; i < _specialtyOptions.Count; i++)
            {
                if (!_specialtyOptions[i].Selected)
                {
                    _specialtyOptions[i].Code = $"{specialtyCodes}{_specialtyOptions[i].Code}";
                }
                else
                {
                    var codes = new StringBuilder();
                    foreach (var selectedSpecialtyCode in selectedSpecialtyCodes)
                    {
                        if (selectedSpecialtyCode != _specialtyOptions[i].Code)
                        {
                            codes.AppendFormat("{0}{1}", codes.Length > 0 ? "," : "", selectedSpecialtyCode);
                        }
                    }

                    _specialtyOptions[i].Code = codes.ToString();
                }
            }
            // ReSharper restore ForCanBeConvertedToForeach

            if (DrugMonographOption == null)
            {
                DrugMonographOption = new SearchResultOption { Count = 0, Name = "Drug", Id = 1, Code = "drug" };
            }
        }

        public void AddAlernateTerms(IEnumerable<string> alternateTerms)
        {
            _alternateTerms.AddRange(alternateTerms);
        }

        private void PopulateResults(ISearchResults results, AuthenticatedInstitution authenticatedInstitution)
        {
            _searchResults.Clear();

            foreach (SearchResultsItem searchResultsItem in results.Items)
            {
                var licenseCount = 0;
                if (searchResultsItem.SearchResource == null)
                {
                    continue;
                }

                var resource = searchResultsItem.SearchResource.Resource;
                var license = authenticatedInstitution?.GetResourceLicense(resource.Id);
                if (license != null)
                {
                    licenseCount = license.LicenseCount;
                }

                _searchResults.Add(new SearchResult(searchResultsItem, licenseCount, authenticatedInstitution));
            }
        }

        public string ResultsAsJson()
        {
            var scriptSerializer = new JavaScriptSerializer();
            return scriptSerializer.Serialize(SearchResults);
        }
    }
}