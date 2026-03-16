#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Contexts;
using R2V2.Core.MyR2;
using R2V2.Core.RequestLogger;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Email.EmailBuilders;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Search;
using R2V2.Web.Models.Shared;
using R2V2.Web.Services;
using Filter = R2V2.Web.Models.Shared.Filter;
using SearchRequest = R2V2.Core.RequestLogger.SearchRequest;

#endregion

namespace R2V2.Web.Controllers
{
    [RequestLoggerFilter(false)]
    public class SearchController : R2BaseController
    {
        private const int MaxPages = 9;
        private readonly EmailSiteService _emailService;
        private readonly ILog<SearchController> _log;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly SearchFilterBuilder _searchFilterBuilder;
        private readonly SearchResultsEmailBuildService _searchResultsEmailBuildService;

        private readonly ISearchService _searchService;
        private readonly ISpecialtyService _specialtyService;
        private readonly IUserContentService _userContentService;
        private readonly IWebSettings _webSettings;

        /// <param name="searchFilterBuilder"> </param>
        /// <param name="authenticationContext"> </param>
        /// <param name="emailSettings"> </param>
        /// <param name="emailService"> </param>
        /// <param name="userContentService"> </param>
        /// <param name="practiceAreaService"> </param>
        /// <param name="specialtyService"> </param>
        public SearchController(ILog<SearchController> log
            , ISearchService searchService
            , SearchFilterBuilder searchFilterBuilder
            , IAuthenticationContext authenticationContext
            , EmailSiteService emailService
            , IUserContentService userContentService
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , SearchResultsEmailBuildService searchResultsEmailBuildService
            , IWebSettings webSettings
        )
            : base(authenticationContext)
        {
            _searchService = searchService;
            _searchFilterBuilder = searchFilterBuilder;
            _emailService = emailService;
            _userContentService = userContentService;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _searchResultsEmailBuildService = searchResultsEmailBuildService;
            _webSettings = webSettings;
            _log = log;
        }

        [RequestLoggerFilter(false)]
        public ActionResult Index(SearchQuery searchQuery)
        {
            //TestEmailSerialization();

            var advancedSearchModel = new AdvancedSearchModel
            {
                Author = searchQuery.Author,
                Title = searchQuery.Title,
                Publisher = searchQuery.Publisher,
                Editor = searchQuery.Editor,
                Isbn = searchQuery.Isbn,
                YearMin = searchQuery.MinYear,
                YearMax = searchQuery.MaxYear
            };

            var model = new SearchIndex
            {
                Q = searchQuery.Q,
                AdvancedSearch = advancedSearchModel,
                FieldFilterGroup = { Filters = _searchFilterBuilder.GetFieldFilters() },
                FilterByFilterGroup = { Filters = _searchFilterBuilder.GetFilterFilters() },
                PracticeAreaFilterGroup = { Filters = _searchFilterBuilder.GetPracticeAreaFilters() },
                PublicationDateFilterGroup = { Filters = _searchFilterBuilder.GetPublicationDateFilters() },
                DisciplineFilterGroup = { Filters = _searchFilterBuilder.GetDisciplineFilters() },
                EmailPage = new EmailPage { From = CurrentUser != null ? CurrentUser.Email : "" },
                SearchQuery = searchQuery,
                PubMedSearchUrl = string.Format(_webSettings.ExternalSearchPubMedUrl, searchQuery.Q),
                MeshSearchUrl = string.Format(_webSettings.ExternalSearchMeshUrl, searchQuery.Q)
            };

            if (AuthenticationContext.IsAuthenticated)
            {
                if (AuthenticatedInstitution != null)
                {
                    model.DisplayTocAvailable = AuthenticatedInstitution.DisplayAllProducts;

                    var currentUser = CurrentUser;
                    if (currentUser != null && currentUser.Id > 0)
                    {
                        model.DisplaySavedSearchResultLink = true;
                    }

                    model.Include = AuthenticatedInstitution.IncludeArchivedTitlesByDefault ? (short)3 : (short)1;
                }
                else
                {
                    model.DisplayTocAvailable = false;
                    model.Include = 1;
                }

                model.TocOnlyTitlesChecked = "checked=\"checked\"";
            }
            else
            {
                model.DisplayTocAvailable = false;
                model.Include = 1;
            }

            return View(model);
        }

        [RequestLoggerFilter]
        public ActionResult Link(SearchQuery searchQuery)
        {
            if (Request.Url != null)
            {
                var redirectTo =
                    $"http://{Request.Url.Host}{searchQuery.GetSearchUrl(Url.Action("Index", "Search"), Url)}";
                _log.DebugFormat("redirectTo: {0}", redirectTo);
                return Redirect(redirectTo);
            }

            return Redirect(searchQuery.GetSearchUrl(Url.Action("Index", "Search"), Url));
        }


        [RequestLoggerFilter(true, true)]
        public ActionResult JsonResults(SearchQuery searchQuery)
        {
            _log.Debug(searchQuery.ToDebugString());
            var json = new SearchResultsJson();

            try
            {
                var searchResultSet = _searchService.Search(searchQuery);

                SetPagingLinks(searchResultSet);

                json.TotalSearchTime = searchResultSet.TotalSearchTime;
                json.TotalResults = searchResultSet.TotalResultsCount;
                json.TocSelected = searchQuery.TocAvailable;

                // active/archive
                var includeFilterGroup = new FilterGroup { Name = "Include", Code = "include" };
                var includeFilters = new Dictionary<string, Filter>();
                includeFilterGroup.Filters = includeFilters;
                includeFilters.Add("active",
                    new Filter { Count = $"{searchResultSet.ActiveCount:#,###}", Name = "Active" });
                includeFilters.Add("archive",
                    new Filter { Count = $"{searchResultSet.ArchiveCount:#,###}", Name = "Archive" });
                json.FilterGroups.Add(includeFilterGroup.Code, includeFilterGroup);

                // toc
                var tocFilterGroup = new FilterGroup { Name = "Table of Contents Available", Code = "toc-available" };
                var tocFilters = new Dictionary<string, Filter>();
                tocFilterGroup.Filters = tocFilters;
                tocFilters.Add("true", new Filter { Count = "0", Name = "Yes" });
                json.FilterGroups.Add(tocFilterGroup.Code, tocFilterGroup);

                var fieldFilterGroup = new FilterGroup { Name = "Show results from", Code = "field" };
                var filterByFilterGroup = new FilterGroup { Name = "Filter by", Code = "filter-by" };
                var practiceAreaFilterGroup = new FilterGroup { Name = "Practice Area", Code = "practice-area" };
                var publicationDateFilterGroup = new FilterGroup { Name = "Publication Date", Code = "year" };
                var disciplineFilterGroup = new FilterGroup { Name = "Discipline", Code = "disciplines" };

                json.FilterGroups.Add(fieldFilterGroup.Code, fieldFilterGroup);
                json.FilterGroups.Add(filterByFilterGroup.Code, filterByFilterGroup);
                json.FilterGroups.Add(practiceAreaFilterGroup.Code, practiceAreaFilterGroup);
                json.FilterGroups.Add(publicationDateFilterGroup.Code, publicationDateFilterGroup);
                json.FilterGroups.Add(disciplineFilterGroup.Code, disciplineFilterGroup);

                fieldFilterGroup.Filters = _searchFilterBuilder.GetFieldFilters(searchResultSet.FieldOptions);
                var filters = new List<SearchResultOption>();

                filters.Add(searchResultSet.DrugMonographOption);
                filterByFilterGroup.Filters = _searchFilterBuilder.GetFilterFilters(filters);

                practiceAreaFilterGroup.Filters =
                    _searchFilterBuilder.GetPracticeAreaFilters(searchResultSet.PracticeAreaResultOptions);

                publicationDateFilterGroup.Filters =
                    _searchFilterBuilder.GetPublicationDateFilters(searchResultSet.YearOptions);
                disciplineFilterGroup.Filters =
                    _searchFilterBuilder.GetDisciplineFilters(searchResultSet.SpecialtyOptions);

                // option groups (within, author, title, publisher, editor, isbn, year min/max)
                var withinOptionGroup = new OptionGroup
                {
                    Name = "Within", Code = "within",
                    Value = string.IsNullOrEmpty(searchQuery.Within) ? "" : searchResultSet.SearchWithinTitle
                };
                json.OptionGroups.Add(withinOptionGroup.Code, withinOptionGroup);
                var authorOptionGroup = new OptionGroup
                {
                    Name = "Author", Code = "author",
                    Value = string.IsNullOrEmpty(searchQuery.Author) ? "" : searchQuery.Author
                };
                json.OptionGroups.Add(authorOptionGroup.Code, authorOptionGroup);
                var titleOptionGroup = new OptionGroup
                {
                    Name = "Title", Code = "title",
                    Value = string.IsNullOrEmpty(searchQuery.Title) ? "" : searchQuery.Title
                };
                json.OptionGroups.Add(titleOptionGroup.Code, titleOptionGroup);
                var publisherOptionGroup = new OptionGroup
                {
                    Name = "Publisher", Code = "publisher",
                    Value = string.IsNullOrEmpty(searchQuery.Publisher) ? "" : searchQuery.Publisher
                };
                json.OptionGroups.Add(publisherOptionGroup.Code, publisherOptionGroup);
                var editorOptionGroup = new OptionGroup
                {
                    Name = "Editor", Code = "editor",
                    Value = string.IsNullOrEmpty(searchQuery.Editor) ? "" : searchQuery.Editor
                };
                json.OptionGroups.Add(editorOptionGroup.Code, editorOptionGroup);
                var isbnOptionGroup = new OptionGroup
                {
                    Name = "Isbn", Code = "isbn", Value = string.IsNullOrEmpty(searchQuery.Isbn) ? "" : searchQuery.Isbn
                };
                json.OptionGroups.Add(isbnOptionGroup.Code, isbnOptionGroup);
                var yearRangeOptionGroup = new OptionGroup
                {
                    Name = "Publication Date", Code = "year",
                    Value = searchQuery.MinYear.Equals(0) && searchQuery.MaxYear.Equals(0)
                        ? ""
                        : searchQuery.MinYear.ToString(CultureInfo.InvariantCulture) + "&#8211;" +
                          searchQuery.MaxYear.ToString(CultureInfo.InvariantCulture)
                };
                json.OptionGroups.Add(yearRangeOptionGroup.Code, yearRangeOptionGroup);

                // HTML snippets
                json.HtmlSnippets.Add("paging", RenderPartialViewToString("_Paging", searchResultSet));
                json.HtmlSnippets.Add("totals", RenderPartialViewToString("_Totals", searchResultSet));
                json.HtmlSnippets.Add("results", RenderPartialViewToString("_TitleList", searchResultSet));

                json.Status = "ok";
                json.Successful = true;

                if (searchQuery.Page > 1)
                {
                    SuppressRequestLogging();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json.ErrorMessage = "We are sorry, an error occurred while executing your search.  Please try again.";
                json.Status = "error";
                json.Successful = false;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <param name="json"> </param>
        /// <param name="html"> </param>
        [RequestLoggerFilter(true, true)]
        public ActionResult Federated(SearchQuery searchQuery, bool json = true, bool html = false)
        {
            SearchResultSet model;
            if (AuthenticatedInstitution != null && AuthenticatedInstitution.Id > 0)
            {
                model = _searchService.Search(searchQuery);
                try
                {
                    if (searchQuery.PageSize > 500)
                    {
                        searchQuery.PageSize = 500;
                    }
                    else if (searchQuery.PageSize < 10)
                    {
                        searchQuery.PageSize = 10;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    model.FederatedSearchMessage = $"Exception: {ex.Message}";
                }
            }
            else
            {
                model = _searchService.GetEmptySearchResultSet(searchQuery);
                model.FederatedSearchMessage =
                    "ACCESS DENIED! Federated Search is only accessible by licensed institutions.<br/>For testing, please login using username and password.<br/>IP authentication is recommended for production use.";
            }

            model.DisplayHtml = html;
            model.DisplayJson = json;
            return View(model);
        }

        [RequestLoggerFilter(true, true)]
        public ActionResult Email(SearchQuery searchQuery, EmailPage emailPage)
        {
            _log.Debug(searchQuery.ToDebugString());
            var json = new JsonResponse { Status = "failure", Successful = false };
            try
            {
                var searchResultSet = _searchService.Search(searchQuery);
                if (searchResultSet.TotalResultsCount > 0)
                {
                    var emailMessageHtmlBody = _searchResultsEmailBuildService.GetMessageBody(Request, searchResultSet,
                        Url, AuthenticatedInstitution,
                        emailPage.From, emailPage.Comments, HttpContext.Request.IsSecureConnection);

                    var emailStatus = _emailService.SendEmailMessageToQueue(emailMessageHtmlBody, emailPage);
                    if (emailStatus)
                    {
                        json = new JsonResponse { Status = "success", Successful = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [RequestLoggerFilter]
        public ActionResult LogExternalSearch(SearchType searchType)
        {
            SetRequestLoggerSearchRequest(searchType);
            return Json(new JsonResponse { Status = "success", Successful = true }, JsonRequestBehavior.AllowGet);
        }

        private void SetRequestLoggerSearchRequest(SearchType searchType)
        {
            var searchRequest = new SearchRequest { IsExternalSearch = true, SearchTypeId = (int)searchType };
            var requestData = HttpContext.RequestStorage().Get<RequestData>(RequestLoggerFilter.RequestDataKey);
            requestData.SearchRequest = searchRequest;
        }

        private void SetPagingLinks(SearchResultSet searchResultSet)
        {
            var query = searchResultSet.Query;
            var pageCount = searchResultSet.TotalResultsCount / query.PageSize +
                            (searchResultSet.TotalResultsCount % query.PageSize > 0 ? 1 : 0);
            var currentPage = query.Page;

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || currentPage <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = currentPage - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            _log.DebugFormat(
                "pageCount: {0}, currentPage: {1}, firstPage: {2}, lastPage: {3}, TotalResultsCount: {4}, PageSize: {5}",
                pageCount, currentPage, firstPage, lastPage, searchResultSet.TotalResultsCount, query.PageSize);

            searchResultSet.PreviousLink = new JumpToLink
            {
                Current = false,
                Enabled = currentPage > 1,
                Label = "Previous",
                Value = $"{currentPage - 1}"
            };

            searchResultSet.NextLink = new JumpToLink
            {
                Current = false,
                Enabled = pageCount > currentPage,
                Label = "Next",
                Value = $"{currentPage + 1}"
            };

            var jumpToLinks = new List<JumpToLink>();
            for (var p = firstPage; p <= lastPage; p++)
            {
                var jumpToLink = new JumpToLink
                {
                    Current = p == currentPage,
                    Enabled = true,
                    Label = $"{p}",
                    Value = $"{p}"
                };
                jumpToLinks.Add(jumpToLink);
            }

            searchResultSet.JumpToLinks = jumpToLinks;
        }

        [RequestLoggerFilter]
        public ActionResult SavedSearchResultList(int savedSearchResultId)
        {
            var userSavedSearch = _userContentService.GetUserSavedSearchResult(savedSearchResultId, UserId);

            if (userSavedSearch == null)
            {
                return RedirectToAction("Index", "MyR2", new { type = "bookmark" });
            }

            var scriptSerializer = new JavaScriptSerializer();
            var savedSearchResultSet =
                scriptSerializer.Deserialize<SavedSearchResultSet>(userSavedSearch.SearchResultSet);

            if (!string.IsNullOrWhiteSpace(savedSearchResultSet.SearchQuery.PracticeArea))
            {
                savedSearchResultSet.PracticeArea = _practiceAreaService
                    .GetPracticeAreaById(savedSearchResultSet.SearchQuery.PracticeArea).Name;
            }

            if (!string.IsNullOrWhiteSpace(savedSearchResultSet.SearchQuery.Disciplines))
            {
                savedSearchResultSet.Discipline =
                    _specialtyService.GetSpecialty(savedSearchResultSet.SearchQuery.Disciplines).Name;
            }

            savedSearchResultSet.ResultsCount = userSavedSearch.ResultsCount;
            savedSearchResultSet.CreationDate = userSavedSearch.CreationDate;
            savedSearchResultSet.Title = userSavedSearch.Title;

            return View(savedSearchResultSet);
        }

        [RequestLoggerFilter(true, true)]
        public ActionResult Typeahead(string searchInput)
        {
            if (!string.IsNullOrEmpty(searchInput))
            {
                var searchTerms = _searchService.GetTypeaheadSearchTerms(searchInput);
                return ContentResult(searchTerms);
            }

            return new ContentResult();
        }

        [RequestLoggerFilter]
        public ActionResult ClearTypeaheadCache()
        {
            _searchService.ClearTypeaheadCache();
            return RedirectToAction("Index", "Home");
        }

        private static ContentResult ContentResult(object obj)
        {
            return
                new ContentResult
                {
                    ContentType = "application/json",
                    Content = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }
                        .Serialize(obj)
                };
        }
    }
}