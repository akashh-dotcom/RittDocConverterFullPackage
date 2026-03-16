#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Script.Serialization;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.MyR2;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Search;
using R2V2.DataAccess.DtSearch;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models.MyR2;
using R2V2.Web.Models.Search;
using R2V2.Web.Models.Search.Fields;

#endregion

namespace R2V2.Web.Services
{
    public class SearchService : ISearchService
    {
        private readonly AlternateSearchTermsService _alternateSearchTermsService;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IContentSettings _contentSettings;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<SearchService> _log;
        private readonly MyR2Service _myR2Service;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourceService _resourceService;
        private readonly ISearch _search;
        private readonly ISearchTypeaheadService _searchTypeaheadService;
        private readonly ISpecialtyService _specialtyService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserContentService _userContentService;

        private AuthenticatedInstitution _authenticatedInstitution;

        /// <param name="myR2Service"> </param>
        public SearchService(ILog<SearchService> log
            , ISearch search
            , IAuthenticationContext authenticationContext
            , AlternateSearchTermsService alternateSearchTermsService
            , IUnitOfWorkProvider unitOfWorkProvider
            , IResourceService resourceService
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , IInstitutionSettings institutionSettings
            , UserContentService userContentService
            , MyR2Service myR2Service
            , IResourceAccessService resourceAccessService
            , ISearchTypeaheadService searchTypeaheadService
            , IContentSettings contentSettings
        )
        {
            _log = log;
            _search = search;
            _authenticationContext = authenticationContext;
            _alternateSearchTermsService = alternateSearchTermsService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _resourceService = resourceService;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _institutionSettings = institutionSettings;
            _userContentService = userContentService;
            _myR2Service = myR2Service;
            _resourceAccessService = resourceAccessService;
            _searchTypeaheadService = searchTypeaheadService;
            _contentSettings = contentSettings;
        }

        private AuthenticatedInstitution AuthenticatedInstitution => _authenticatedInstitution ??
                                                                     (_authenticatedInstitution =
                                                                         _authenticationContext
                                                                             .AuthenticatedInstitution);

        public SearchResultSet Search(SearchQuery query)
        {
            var request = GetSearchRequest(query);
            SetSearchResources(request);

            ISearchResults results = _search.Execute(request);

            SaveUserSearchHistory(query, request, results);
            if (IsAuthenticatedUser())
            {
                SaveSearchHistory(results.SearchHistory);
            }
            else
            {
                if (AuthenticatedInstitution != null)
                {
                    _myR2Service.SaveSearchHistory(results.SearchHistory, query, AuthenticatedInstitution.Id);
                }
            }

            return GetSearchResults(results, request, query);
        }

        public List<IResource> SearchAdmin(string query, IEnumerable<License> licences)
        {
            var resources = _resourceService.GetAllResources();

            if (licences != null)
            {
                var resourceIds = licences.Where(y => y.LicenseCount > 0).Select(x => x.ResourceId);
                resources = resources.Where(x => !x.NotSaleable || (x.NotSaleable && resourceIds.Contains(x.Id)));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return resources.ToList();
            }

            ISearchRequest
                request = new SearchRequest { SearchTerm = GetCleanedSearchTerm(query) }; //;{}GetSearchRequest(query);}

            foreach (var resource in resources)
            {
                request.AddSearchResourceAdmin(new SearchResource { Resource = resource });
            }

            var results = _search.ExecuteAdmin(request);

            return results;
        }

        public SearchResultSet GetEmptySearchResultSet(SearchQuery query)
        {
            var request = GetSearchRequest(query);
            var searchResultSet =
                new SearchResultSet(request, null, query, AuthenticatedInstitution, _specialtyService);
            return searchResultSet;
        }

        public IEnumerable<string> GetTypeaheadSearchTerms(string searchInput)
        {
            return _searchTypeaheadService.GetTypeaheadSearchTerms(searchInput,
                _contentSettings.SearchTypeaheadResultLimit);
        }

        public void ClearTypeaheadCache()
        {
            _searchTypeaheadService.ClearCache();
        }

        private ISearchRequest GetSearchRequest(SearchQuery query)
        {
            ISearchRequest request = new SearchRequest
            {
                SearchTerm = GetCleanedSearchTerm(query.Q),
                Page = query.Page,
                PageSize = query.PageSize,
                SortBy = SearchRequest.ConvertSortBy(query.SortBy),
                PracticeArea = _practiceAreaService.GetPracticeAreaById(query.PracticeArea),
                Specialty = _specialtyService.GetSpecialty(query.Disciplines),
                Author = GetCleanedSearchTerm(query.Author),
                BookTitle = GetCleanedSearchTerm(query.Title),
                Editor = GetCleanedSearchTerm(query.Editor),
                Publisher = GetCleanedSearchTerm(query.Publisher),
                DrugMonograph = query.Filter == "drug",
                Active = (query.Include & 0x1) == 0x1,
                Archive = (query.Include & 0x2) == 0x2
            };


            // todo: long term, this needs to be changed to a more eligant solution - SJS - 7/20/2012 - OK for the beta
            if (_authenticationContext.IsAuthenticated)
            {
                if (AuthenticatedInstitution != null &&
                    AuthenticatedInstitution.AccountNumber != _institutionSettings.GuestAccountNumber)
                {
                    request.IncludeTocResouces = query.TocAvailable;
                }
                else
                {
                    request.IncludeTocResouces = true;
                }
            }
            else
            {
                request.IncludeTocResouces = true;
            }

            var searchField = SearchFieldsFactory.GetSearchFieldByCode(query.Field);
            request.Field = searchField != null ? searchField.SearchField : SearchFields.All;

            if (!string.IsNullOrEmpty(query.Isbn))
            {
                request.AddIsbns(query.Isbn.Split(','));
            }

            if (!string.IsNullOrEmpty(query.Within))
            {
                request.AddSearchWithinIsbns(query.Within.Split(','));
            }

            var publicationYears = _resourceService.GetResourcePublicationYears();
            var publicationYearMin = publicationYears.Min();

            request.Year = query.FilterYear;
            request.PublicationYearMin = query.MinYear;
            request.PublicationYearMax = query.MaxYear;

            if (request.PublicationYearMin > 1900 && request.PublicationYearMax == 0)
            {
                request.PublicationYearMax = DateTime.Now.Year;
            }
            else if (request.PublicationYearMin == 0 && request.PublicationYearMax >= publicationYearMin)
            {
                request.PublicationYearMin = publicationYearMin;
            }

            if (request.PublicationYearMin > request.PublicationYearMax)
            {
                var min = request.PublicationYearMax;
                request.PublicationYearMax = request.PublicationYearMin;
                request.PublicationYearMin = min;
            }

            return request;
        }

        private SearchResultSet GetSearchResults(ISearchResults results, ISearchRequest request, SearchQuery query)
        {
            var searchResultSet =
                new SearchResultSet(request, results, query, AuthenticatedInstitution, _specialtyService);

            SetAlternateSearchTerms(searchResultSet);

            if (_authenticationContext.IsAuthenticated)
            {
                searchResultSet.DisplayTocOnlyTitlesOption =
                    AuthenticatedInstitution != null && AuthenticatedInstitution.DisplayAllProducts;
            }
            else
            {
                searchResultSet.DisplayTocOnlyTitlesOption = false;
            }

            return searchResultSet;
        }

        private void SetSearchResources(ISearchRequest request)
        {
            var methodTimer = new Stopwatch();
            var loopTimer = new Stopwatch();
            methodTimer.Start();
            var resources = _resourceService.GetAllResources();

            //_log.DebugFormat("_authenticationContext.IsAuthenticated: {0}", _authenticationContext.IsAuthenticated);
            if (_authenticationContext.IsAuthenticated)
            {
                if (AuthenticatedInstitution != null &&
                    AuthenticatedInstitution.AccountNumber != _institutionSettings.GuestAccountNumber)
                {
                    if (AuthenticatedInstitution.DisplayAllProducts)
                    {
                        loopTimer.Start();
                        // annoy the user and search all resources!
                        foreach (var resource in resources)
                        {
                            // the business rules when display all products is selected
                            // 1. Only show active and archived titles
                            // 2. Show if title is saleable
                            // 3. If not saleable, include only if the institution has a license for the title (IsFullTextAvailable)
                            if (resource.StatusId != (int)ResourceStatus.Active &&
                                resource.StatusId != (int)ResourceStatus.Archived)
                            {
                                //_log.DebugFormat("resource.Id: {0}, isbn: {1}, StatusId: {2}", resource.Id, resource.Isbn, resource.StatusId);
                                continue;
                            }

                            var license = AuthenticatedInstitution.GetResourceLicense(resource.Id);
                            var searchResource = new SearchResource
                            {
                                Resource = resource,
                                FullTextAvailable = _resourceAccessService.IsFullTextAvailable(license)
                            };
                            if (!resource.NotSaleable || searchResource.FullTextAvailable)
                            {
                                request.AddSearchResource(searchResource);
                            }
                        }

                        loopTimer.Stop();
                        methodTimer.Stop();
                        _log.DebugFormat(
                            "SetSearchResources() method time: {0} ms, loop timer: {1}, resource count: {2}, DisplayAllProducts: {3}",
                            methodTimer.ElapsedMilliseconds, loopTimer.ElapsedMilliseconds, request.Resources.Count(),
                            AuthenticatedInstitution.DisplayAllProducts);
                        return;
                    }

                    // search only resources the institution has access to
                    loopTimer.Start();
                    foreach (var resource in resources)
                    {
                        var license = AuthenticatedInstitution.GetResourceLicense(resource.Id);
                        if (!_resourceAccessService.IsFullTextAvailable(license))
                        {
                            continue;
                        }

                        var searchResource = new SearchResource { Resource = resource, FullTextAvailable = true };
                        request.AddSearchResource(searchResource);
                    }

                    loopTimer.Stop();
                    methodTimer.Stop();
                    _log.DebugFormat(
                        "SetSearchResources() method time: {0} ms, loop timer: {1}, resource count: {2}, DisplayAllProducts: {3}",
                        methodTimer.ElapsedMilliseconds, loopTimer.ElapsedMilliseconds, request.Resources.Count(),
                        AuthenticatedInstitution.DisplayAllProducts);
                    return;
                }
            }

            // trial - guest - default
            loopTimer.Start();
            foreach (var resource in resources)
            {
                if (resource.IsActive() && !resource.NotSaleable)
                {
                    var searchResource = new SearchResource { Resource = resource, FullTextAvailable = false };
                    request.AddSearchResource(searchResource);
                }
            }

            loopTimer.Stop();
            methodTimer.Stop();
            _log.DebugFormat("SetSearchResources() method time: {0} ms, loop timer: {1}, resource count: {2}, trial",
                methodTimer.ElapsedMilliseconds, loopTimer.ElapsedMilliseconds, request.Resources.Count());
        }

        private void SetAdminSearchResources(ISearchRequest request)
        {
            var resources = _resourceService.GetAllResources();
            foreach (var resource in resources.Where(x => !x.NotSaleable))
            {
                request.AddSearchResource(new SearchResource { Resource = resource, FullTextAvailable = true });
            }
        }

        private void SetAlternateSearchTerms(SearchResultSet searchResultSet)
        {
            var alternateTerms = _alternateSearchTermsService.GetAlternateTerms(searchResultSet.Q);
            searchResultSet.AddAlernateTerms(alternateTerms);
        }

        private void SaveSearchHistory(ISearchHistory searchHistory)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Clear();
                    uow.Save(searchHistory);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                // TODO - swallow exception for testing
                _log.Error(ex.Message, ex);
            }
        }

        private void SaveUserSearchHistory(SearchQuery query, ISearchRequest searchRequest, ISearchResults results)
        {
            try
            {
                if (!_authenticationContext.IsAuthenticated || AuthenticatedInstitution == null)
                {
                    _log.Debug("SaveUserSearchHistory() - unauthenticated - don't save search");
                    return;
                }

                if (AuthenticatedInstitution.User == null)
                {
                    _log.Debug("SaveUserSearchHistory() - AuthenticatedInstitution.User is null - don't save search");
                    return;
                }

                if (AuthenticatedInstitution.User.Id < 1)
                {
                    _log.DebugFormat(
                        "SaveUserSearchHistory() - AuthenticatedInstitution.User is not defined, AuthenticatedInstitution.User.Id: {0} - don't save search",
                        AuthenticatedInstitution.User.Id);
                    return;
                }

                if (AuthenticatedInstitution.User.IsPublisherUser())
                {
                    _log.DebugFormat(
                        "SaveUserSearchHistory() - AuthenticatedInstitution.User is defined as a Publisher User, AuthenticatedInstitution.User.Id: {0} - don't save search",
                        AuthenticatedInstitution.User.Id);
                    return;
                }

                _log.DebugFormat("SaveUserSearchHistory() - AuthenticatedInstitution.User.Id: {0}",
                    AuthenticatedInstitution.User.Id);

                var searchSummary = new SearchSummary
                {
                    Term = query.Q,
                    Author = query.Author,
                    BookTitle = query.Title,
                    Publisher = query.Publisher,
                    Editor = query.Editor,
                    Active = searchRequest.Active,
                    Archive = searchRequest.Active,

                    PageSize = query.PageSize,
                    SortBy = searchRequest.SortBy,
                    DrugMonograph = searchRequest.DrugMonograph,

                    Field = searchRequest.Field,
                    Years = query.Year,
                    IncludeTocResouces = query.TocAvailable,

                    Isbns = searchRequest.Isbns,
                    SearchWithinIsbns = searchRequest.SearchWithinIsbns,

                    ResultsCount = results.FileCount,

                    ReserveShelfId = 0
                };

                if (!string.IsNullOrWhiteSpace(query.PracticeArea))
                {
                    var practiceArea = _practiceAreaService.GetPracticeAreaById(query.PracticeArea);
                    if (practiceArea != null)
                    {
                        searchSummary.PracticeAreaCode = practiceArea.Code;
                    }
                }

                if (!string.IsNullOrWhiteSpace(query.Disciplines))
                {
                    var specialty = _specialtyService.GetSpecialty(query.Disciplines);
                    if (specialty != null)
                    {
                        searchSummary.SpecialtyCode = specialty.Code;
                    }
                }

                searchSummary.Advanced = !string.IsNullOrWhiteSpace(searchSummary.Author) &&
                                         !string.IsNullOrWhiteSpace(searchSummary.Publisher) &&
                                         !string.IsNullOrWhiteSpace(searchSummary.BookTitle) &&
                                         !string.IsNullOrWhiteSpace(searchSummary.Editor) &&
                                         !string.IsNullOrWhiteSpace(query.Isbn);

                var scriptSerializer = new JavaScriptSerializer();
                var json = scriptSerializer.Serialize(searchSummary);
                _log.Debug(json);

                var userSearchHistory = new UserSearchHistory
                {
                    RecordStatus = true,
                    UserId = AuthenticatedInstitution.User.Id,
                    SearchQuery = json,
                    SearchXml = "<Deplicated/>",
                    ResultsCount = results.FileCount
                };

                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Save(userSearchHistory);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                // TODO - swallow exception for testing
                _log.Error(ex.Message, ex);
            }
        }

        public int SaveUserSearch(SavedSearch savedSearch, int userId)
        {
            try
            {
                if (userId < 1)
                {
                    _log.DebugFormat("Invalid User Id: {0}", userId);
                    return -4;
                }

                _log.DebugFormat("userId: {0}", userId);

                var searchSummary = ProcessSavedSearchToSearchSummary(savedSearch);
                var scriptSerializer = new JavaScriptSerializer();
                var json = scriptSerializer.Serialize(searchSummary);
                _log.Debug(json);

                var userSavedSearch = new UserSavedSearch
                {
                    RecordStatus = true,
                    //UserId = identity.User.Id,
                    SearchQuery = json,
                    Xml = "<Deplicated/>",
                    ResultsCount = savedSearch.Total,
                    Title = savedSearch.Name
                };

                var id = _userContentService.SaveUserSearchIntoDefaultFolder(userSavedSearch, userId);
                return id;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return -1;
            }
        }

        public int SaveUserSearchResult(SavedSearchResult savedSearchResult, int userId)
        {
            var scriptSerializer = new JavaScriptSerializer();


            var userSavedSearchResult = new UserSavedSearchResult
            {
                SearchResultSet = scriptSerializer.Serialize(savedSearchResult.SavedSearchResultSet),
                Title = savedSearchResult.Title,
                ResultsCount = savedSearchResult.ResultsCount
            };

            return _userContentService.SaveUserSearchResultIntoDefaultFolder(userSavedSearchResult, userId);
        }

        public SearchSummary ProcessSavedSearchToSearchSummary(SavedSearch savedSearch)
        {
            var searchSummary = new SearchSummary
            {
                Term = savedSearch.Q,
                Author = savedSearch.Author,
                BookTitle = savedSearch.Title,
                Publisher = savedSearch.Publisher,
                Editor = savedSearch.Editor,
                Active = (savedSearch.Include & 0x1) == 0x1,
                Archive = (savedSearch.Include & 0x2) == 0x2,
                PageSize = savedSearch.PageSize,
                SortBy = SearchRequest.ConvertSortBy(savedSearch.SortBy),
                DrugMonograph = savedSearch.Filter == "drug",
                Years = savedSearch.Year,
                IncludeTocResouces = savedSearch.TocAvailable,
                ReserveShelfId = 0
            };

            var searchField = SearchFieldsFactory.GetSearchFieldByCode(savedSearch.Field);
            searchSummary.Field = searchField != null ? searchField.SearchField : SearchFields.All;

            searchSummary.ResultsCount = savedSearch.Total;

            if (!string.IsNullOrEmpty(savedSearch.Isbn))
            {
                searchSummary.Isbns = savedSearch.Isbn.Split(',');
            }

            if (!string.IsNullOrEmpty(savedSearch.Within))
            {
                searchSummary.SearchWithinIsbns = savedSearch.Within.Split(',');
            }

            if (!string.IsNullOrWhiteSpace(savedSearch.PracticeArea))
            {
                var practiceArea = _practiceAreaService.GetPracticeAreaById(savedSearch.PracticeArea);
                if (practiceArea != null)
                {
                    searchSummary.PracticeAreaCode = practiceArea.Code;
                }
            }

            if (!string.IsNullOrWhiteSpace(savedSearch.Disciplines))
            {
                var specialty = _specialtyService.GetSpecialty(savedSearch.Disciplines);
                if (specialty != null)
                {
                    searchSummary.SpecialtyCode = specialty.Code;
                }
            }

            searchSummary.Advanced = !string.IsNullOrWhiteSpace(searchSummary.Author) &&
                                     !string.IsNullOrWhiteSpace(searchSummary.Publisher) &&
                                     !string.IsNullOrWhiteSpace(searchSummary.BookTitle) &&
                                     !string.IsNullOrWhiteSpace(searchSummary.Editor) &&
                                     !string.IsNullOrWhiteSpace(savedSearch.Isbn);

            return searchSummary;
        }

        private string GetCleanedSearchTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return term;
            }

            //string cleanedTerm = term.Replace("'", "").Replace("+", " and ").Replace("|", " or ");
            var cleanedTerm = term.Replace("+", " and ").Replace("|", " or ");
            _log.DebugFormat("term: {0}, cleanedTerm: {1}", term, cleanedTerm);
            return cleanedTerm;
        }

        private bool IsAuthenticatedUser()
        {
            if (!_authenticationContext.IsAuthenticated || AuthenticatedInstitution == null ||
                AuthenticatedInstitution.User == null || AuthenticatedInstitution.IsPublisherUser())
            {
                return false;
            }

            return AuthenticatedInstitution.User.Id > 0;
        }
    }
}