#region

using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Contexts;
using R2V2.Core.MyR2;
using R2V2.Core.Resource;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Models.MyR2;
using R2V2.Web.Models.Search;
using R2V2.Web.Services;
using SearchHistory = R2V2.Web.Models.MyR2.SearchItem;

#endregion

namespace R2V2.Web.Controllers.MyR2
{
    public class MyR2Controller : R2BaseController
    {
        private readonly IContentSettings _contentSettings;
        private readonly ILog<MyR2Controller> _log;
        private readonly MyR2Service _myR2Service;
        private readonly IResourceService _resourceService;
        private readonly SearchQueryBuilder _searchQueryBuilder;
        private readonly SearchService _searchService;
        private readonly IUserContentService _userContentService;

        public MyR2Controller(ILog<MyR2Controller> log
            , IAuthenticationContext authenticationContext
            , IUserContentService userContentService
            , IResourceService resourceService
            , SearchQueryBuilder searchQueryBuilder
            , IContentSettings contentSettings
            , MyR2Service myR2Service
            , SearchService searchService
        )
            : base(authenticationContext)
        {
            _log = log;
            _userContentService = userContentService;
            _resourceService = resourceService;
            _searchQueryBuilder = searchQueryBuilder;
            _contentSettings = contentSettings;
            _myR2Service = myR2Service;
            _searchService = searchService;
        }

        public ActionResult Index(string type)
        {
            var userContentType = type.ToUserContentType();
            var isCourseLinks = userContentType == UserContentType.CourseLink;
            UserContent content;

            if (AuthenticatedInstitution != null)
            {
                var userContentFolders =
                    _myR2Service.GetUserContentFolders(userContentType, UserId, AuthenticatedInstitution.Id);
                var savedSearchResults = GetSavedSearchResults();
                var searchHistories = UserId > 0 ? GetSearchHistory() : GetSessionSearchHistory();
                var savedSearches = UserId > 0 ? GetSavedSearches() : GetSessionSavedSearches();
                content = new UserContent(userContentFolders, userContentType, searchHistories, savedSearches,
                    savedSearchResults);
            }
            else
            {
                //Not an institution user or user
                return RedirectToAction("Index", "Browse");
            }

            // todo: refactor post launch - JAH
            // centralize link url and citation generation
            foreach (var userContentFolder in content.UserContentFolders)
            {
                foreach (var userContentItem in userContentFolder.UserContentItems)
                {
                    var resource = _resourceService.GetResource(userContentItem.ResourceId);

                    if (resource == null)
                    {
                        continue;
                    }

                    var resourceUrl = !string.IsNullOrWhiteSpace(userContentItem.SectionId)
                        ? Url.Action("Detail", "Resource",
                            new { userContentItem.Resource.Isbn, section = userContentItem.SectionId },
                            HttpContext.Request.IsSecureConnection ? "https" : "http")
                        : Url.Action("Title", "Resource", new { userContentItem.Resource.Isbn },
                            HttpContext.Request.IsSecureConnection ? "https" : "http");

                    userContentItem.Resource.ApaCitation = _resourceService.GetCitation(resource, resourceUrl);

                    userContentItem.ImageUrl = userContentItem.Type == UserContentType.Image
                        ? $"{_contentSettings.ImageBaseUrl}/{userContentItem.Isbn}/{userContentItem.Filename}"
                        : userContentItem.Resource.ImageUrl;

                    if (isCourseLinks)
                    {
                        userContentItem.SetCourseLinksUrl(resourceUrl, AuthenticatedInstitution);
                    }
                }
            }

            return View(content);
        }

        private IList<SearchHistory> GetSearchHistory()
        {
            var userSearchHistories = _userContentService.GetUserSearchHistory(UserId, 20);

            var searchHistories = new List<SearchItem>();
            foreach (var userSearchHistory in userSearchHistories)
            {
                //_log.Debug(userSearchHistory.SearchQuery);

                var scriptSerializer = new JavaScriptSerializer();
                var searchSummary = scriptSerializer.Deserialize<SearchSummary>(userSearchHistory.SearchQuery);

                var searchQuery = _searchQueryBuilder.GetSearchQuery(searchSummary);

                var searchHistory = new SearchHistory
                {
                    SearchDate = userSearchHistory.CreationDate,
                    ResultsCount = userSearchHistory.ResultsCount,
                    Description = searchQuery.GetText(),
                    Href = searchQuery.GetSearchUrl(Url.Action("Index", "Search"), Url)
                };

                searchHistories.Add(searchHistory);
            }

            return searchHistories;
        }

        private IList<SearchHistory> GetSessionSearchHistory()
        {
            var sessionSearchHistories = _myR2Service.GetSearchHistory(AuthenticatedInstitution.Id);

            var searchHistories = new List<SearchItem>();
            foreach (var savedSearch in sessionSearchHistories)
            {
                var searchSummary = _searchService.ProcessSavedSearchToSearchSummary(savedSearch);
                var searchQuery = _searchQueryBuilder.GetSearchQuery(searchSummary);

                var searchHistory = new SearchItem
                {
                    Id = savedSearch.Id,
                    SearchDate = savedSearch.SearchDate,
                    ResultsCount = savedSearch.Total,
                    Description = searchQuery.GetText(),
                    Href = searchQuery.GetSearchUrl(Url.Action("Index", "Search"), Url),
                    Name = savedSearch.Name
                };

                searchHistories.Add(searchHistory);
            }

            return searchHistories;
        }

        private IList<SearchHistory> GetSessionSavedSearches()
        {
            var sessionSavedSearches = new List<SearchHistory>();
            var savedSearches = _myR2Service.GetSavedSearches(AuthenticatedInstitution.Id);

            foreach (var savedSearch in savedSearches)
            {
                var searchSummary = _searchService.ProcessSavedSearchToSearchSummary(savedSearch);
                var searchQuery = _searchQueryBuilder.GetSearchQuery(searchSummary);
                var item = new SearchItem
                {
                    Id = savedSearch.Id,
                    Description = searchQuery.GetText(),
                    SearchDate = savedSearch.SearchDate,
                    ResultsCount = savedSearch.Total,
                    Href = searchQuery.GetSearchUrl(Url.Action("Index", "Search"), Url),
                    Name = savedSearch.Name
                };
                sessionSavedSearches.Add(item);
            }

            return sessionSavedSearches;
        }

        private IList<SearchHistory> GetSavedSearches()
        {
            var userSavedSearches = _userContentService.GetUserSavedSearch(UserId);

            var savedSearches = new List<SearchItem>();
            foreach (var userSavedSearch in userSavedSearches)
            {
                //_log.Debug(userSavedSearch.SearchQuery);

                var scriptSerializer = new JavaScriptSerializer();
                var searchSummary = scriptSerializer.Deserialize<SearchSummary>(userSavedSearch.SearchQuery);

                var searchQuery = _searchQueryBuilder.GetSearchQuery(searchSummary);

                var item = new SearchItem
                {
                    Id = userSavedSearch.Id,
                    SearchDate = userSavedSearch.CreationDate,
                    ResultsCount = userSavedSearch.ResultsCount,
                    Description = searchQuery.GetText(),
                    Href = searchQuery.GetSearchUrl(Url.Action("Index", "Search"), Url),
                    Name = userSavedSearch.Title
                };

                savedSearches.Add(item);
            }

            return savedSearches;
        }

        private IList<SearchHistory> GetSavedSearchResults()
        {
            var userSavedSearches = _userContentService.GetUserSavedSearchResults(UserId);
            if (userSavedSearches == null)
            {
                return null;
            }

            var savedSearches = new List<SearchItem>();
            //TODO: Ok This is working as a resultSet and the query. I may need to add more to this, but for now I should get to the Display of it.
            foreach (var userSavedSearch in userSavedSearches)
            {
                var scriptSerializer = new JavaScriptSerializer();
                var savedSearchResultSet =
                    scriptSerializer.Deserialize<SavedSearchResultSet>(userSavedSearch.SearchResultSet);

                var item = new SearchItem
                {
                    Id = userSavedSearch.Id,
                    SearchDate = userSavedSearch.CreationDate,
                    ResultsCount = savedSearchResultSet.SearchResultList.Count,
                    TotalResultsCount = userSavedSearch.ResultsCount,
                    Description = savedSearchResultSet.SearchQuery.GetText(),
                    Href = Url.Action("SavedSearchResultList", "Search",
                        new { savedSearchResultId = userSavedSearch.Id }),
                    Name = userSavedSearch.Title
                };

                savedSearches.Add(item);
            }

            return savedSearches;
        }
    }
}