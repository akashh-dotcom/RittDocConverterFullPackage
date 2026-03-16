#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dtSearch.Engine;
using log4net.Core;
using R2V2.Core.Resource;
using R2V2.Core.Search;
using R2V2.Core.Search.FacetData;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using SearchResults = R2V2.Core.Search.SearchResults;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class Search : ISearch
    {
        private readonly FacetDataService _facetDataService;

        private readonly string _indexLocation;

        // dependency injected
        private readonly ILog<Search> _log;
        private readonly SearchRequestBuilder _searchRequestBuilder;
        private readonly SearchResultItemFactory _searchResultItemFactory;

        private SearchFilters _searchFilters;

        /// <param name="facetDataService"> </param>
        /// <param name="searchResultItemFactory"> </param>
        public Search(ILog<Search> log
            , ContentSettings contentSettings
            , SearchRequestBuilder searchRequestBuilder
            , FacetDataService facetDataService
            , SearchResultItemFactory searchResultItemFactory
        )
        {
            _log = log;
            _indexLocation = contentSettings.DtSearchIndexLocation;
            _searchRequestBuilder = searchRequestBuilder;
            _facetDataService = facetDataService;
            _searchResultItemFactory = searchResultItemFactory;
        }

        public SearchResults Execute(ISearchRequest searchRequest)
        {
            var totalSearchStopwatch = new Stopwatch();
            var filterStopwatch = new Stopwatch();
            var searchJobStopwatch = new Stopwatch();
            var executeJobStopwatch = new Stopwatch();
            var reportJobStopwatch = new Stopwatch();
            var resultsStopwatch = new Stopwatch();
            var wordlistStopwatch = new Stopwatch();
            var buildRequestStopwatch = new Stopwatch();
            var itemsFound = 0;

            var searchResults = new SearchResults { SearchRequest = searchRequest };
            var searchHistory = new SearchHistory
            {
                // SJS - 9/4/2012 - limit search to to 255 for logging purposes
                SearchTerm =
                    searchRequest.SearchTerm != null && searchRequest.SearchTerm.Length > 255
                        ? searchRequest.SearchTerm.Substring(0, 255)
                        : searchRequest.SearchTerm
            };
            searchResults.SearchHistory = searchHistory;

            var searchStatusHandler = new SearchStatusHandler(searchRequest.Field == SearchFields.FullText);

            try
            {
                totalSearchStopwatch.Start();

                buildRequestStopwatch.Start();
                var dtSearchRequestString = _searchRequestBuilder.GetRequest(searchRequest, out var searchTermToLog);
                buildRequestStopwatch.Stop();
                searchHistory.SearchTerm = searchTermToLog;
                searchHistory.SearchRequest = dtSearchRequestString;

                filterStopwatch.Start();
                InitSearchFilters(searchRequest, false);
                filterStopwatch.Stop();

                searchJobStopwatch.Start();
                using (var dtSearchJob = new SearchJob())
                {
                    dtSearchJob.SetIndexCache(SearchIndexCache.IndexCache);
                    dtSearchJob.IndexesToSearch.Add(_indexLocation);

                    dtSearchJob.SetFilter(_searchFilters.PrimaryFilter.SearchFilter);

                    dtSearchJob.Request = dtSearchRequestString;

                    dtSearchJob.SearchFlags = SearchFlags.dtsSearchDelayDocInfo
                                              | SearchFlags.dtsSearchWantHitDetails
                                              | SearchFlags.dtsSearchStemming;

                    dtSearchJob.FieldWeights =
                        "r2IndexTerms:500,r2BookSearch:200,r2BookTitle:50,r2BookSubTitle:40,r2Author:100";
                    dtSearchJob.AutoStopLimit = 500000;
                    dtSearchJob.StatusHandler = searchStatusHandler;
                    dtSearchJob.WantResultsAsFilter = true;

                    var maxFiles = searchRequest.Page * searchRequest.PageSize;
                    if (searchRequest.SortBy != SearchSortBy.Relevance && maxFiles < 500)
                    {
                        maxFiles = 500;
                    }

                    dtSearchJob.MaxFilesToRetrieve = maxFiles;
                    executeJobStopwatch.Start();
                    dtSearchJob.Execute();
                    executeJobStopwatch.Stop();
                    _log.DebugFormat(
                        "dtSearchJob.Execute() run time: {0} ms, MaxFilesToRetrieve: {1}, AutoStopLimit: {2}",
                        executeJobStopwatch.ElapsedMilliseconds,
                        dtSearchJob.MaxFilesToRetrieve, dtSearchJob.AutoStopLimit);

                    using (var jobErrorInfo = dtSearchJob.Errors)
                    {
                        //JobErrorInfo jobErrorInfo = dtSearchJob.Errors;
                        if (jobErrorInfo != null && jobErrorInfo.Count > 0)
                        {
                            _log.InfoFormat("SEARCH ERROR - count: {0}", jobErrorInfo.Count);
                            for (var i = 0; i < jobErrorInfo.Count; i++)
                            {
                                _log.WriteLogMessage(
                                    jobErrorInfo.Code(i) == 122 || jobErrorInfo.Code(i) == 117
                                        ? Level.Info
                                        : Level.Warn,
                                    $"Message: {jobErrorInfo.Message(i)}, Code: {jobErrorInfo.Code(i)}, Arg1: {jobErrorInfo.Arg1(i)}, Arg2: {jobErrorInfo.Arg2(i)}");
                            }
                        }
                    }

                    searchJobStopwatch.Stop();

                    // word list
                    wordlistStopwatch.Start();
                    using (var wordListBuilder = new WordListBuilder())
                    {
                        wordListBuilder.OpenIndex(_indexLocation);
                        wordListBuilder.SetFilter(dtSearchJob.ResultsAsFilter);
                        searchResults.AddFacetDataRange(
                            _facetDataService.GetFacetData(wordListBuilder, searchStatusHandler));
                        wordlistStopwatch.Stop();
                    }

                    resultsStopwatch.Start();
                    using (var dtSearchResults = dtSearchJob.Results)
                    {
                        SetSortBy(searchRequest, dtSearchResults);


                        var firstIndex = (searchRequest.Page - 1) * searchRequest.PageSize;
                        var lastIndex = searchRequest.Page * searchRequest.PageSize - 1;

                        using (var dtSearchReportJob = dtSearchResults.NewSearchReportJob())
                        {
                            dtSearchReportJob.Flags = ReportFlags.dtsReportByWordExact
                                                      | ReportFlags.dtsReportStoreInResults
                                                      | ReportFlags.dtsReportWholeFile
                                                      | ReportFlags.dtsReportGetFromCache;

                            dtSearchReportJob.SelectItems(firstIndex, lastIndex);
                            dtSearchReportJob.OutputFormat = OutputFormats.itUTF8;
                            dtSearchReportJob.BeforeHit = "<strong>";
                            dtSearchReportJob.AfterHit = "</strong>";
                            dtSearchReportJob.WordsOfContext = 40;
                            dtSearchReportJob.MaxContextBlocks = 1;
                            reportJobStopwatch.Start();
                            dtSearchReportJob.Execute();
                            reportJobStopwatch.Stop();
                        }

                        searchResults.HitCount = dtSearchJob.HitCount;
                        searchResults.FileCount = dtSearchJob.FileCount;
                        searchHistory.FileCount = dtSearchJob.FileCount;
                        searchHistory.HitCount = dtSearchJob.HitCount;

                        itemsFound = dtSearchResults.Count;
                        for (var i = firstIndex; i < itemsFound && i <= lastIndex; i++)
                        {
                            dtSearchResults.GetNthDoc(i);
                            var dtSearchItem = dtSearchResults.CurrentItem;
                            var searchResultsItem = _searchResultItemFactory.GetSearchResultsItem(dtSearchItem,
                                searchRequest.Resources.ToList(),
                                i + 1, searchRequest);
                            if (searchResultsItem == null)
                            {
                                continue;
                            }

                            searchResults.AddItem(searchResultsItem);
                        }
                    }

                    resultsStopwatch.Stop();

                    if (searchRequest.Active && !searchRequest.Archive && searchRequest.ArchivedResources.Any())
                    {
                        var archiveCount = ExecuteCountSearch(searchRequest, dtSearchRequestString,
                            _searchFilters.AlternateFilter.SearchFilter);
                        var statusFacetData = new StatusFacetData { Name = "Archive", Count = archiveCount };
                        searchResults.AddFacetData(statusFacetData);
                    }
                    else if (!searchRequest.Active && searchRequest.Archive)
                    {
                        var activeCount = ExecuteCountSearch(searchRequest, dtSearchRequestString,
                            _searchFilters.AlternateFilter.SearchFilter);
                        var statusFacetData = new StatusFacetData { Name = "Active", Count = activeCount };
                        searchResults.AddFacetData(statusFacetData);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                totalSearchStopwatch.Stop();
                searchResults.SearchTimeSpan = totalSearchStopwatch.Elapsed;
                _log.DebugFormat("number of resources searched: {0}, searchJob.Request: {1}",
                    _searchFilters.PrimaryFilter.ResourceCount, searchHistory.SearchRequest);
                _log.DebugFormat("search request: {0} returned {1} items in {2:0.000}", searchRequest.ToString(),
                    itemsFound, totalSearchStopwatch.Elapsed.TotalSeconds);
                _log.DebugFormat(
                    "total search time: {0:0.000}, filter time: {1:0.000}, searchJob time: {2:0.000}, reportJob time: {3:0.000}, results time: {4:0.000}, wordlist time: {5:0.000}",
                    totalSearchStopwatch.Elapsed.TotalSeconds, filterStopwatch.Elapsed.TotalSeconds,
                    searchJobStopwatch.Elapsed.TotalSeconds,
                    reportJobStopwatch.Elapsed.TotalSeconds, resultsStopwatch.Elapsed.TotalSeconds,
                    wordlistStopwatch.Elapsed.TotalSeconds);

                _log.Debug(searchStatusHandler.ToString());

                searchHistory.TotalSearchTime = totalSearchStopwatch.ElapsedMilliseconds;
                searchHistory.FilterTime = filterStopwatch.ElapsedMilliseconds;
                searchHistory.BuildRequestTime = buildRequestStopwatch.ElapsedMilliseconds;
                searchHistory.SearchJobTime = searchJobStopwatch.ElapsedMilliseconds;
                searchHistory.ReportJobTime = reportJobStopwatch.ElapsedMilliseconds;
                searchHistory.ResultsJobTime = resultsStopwatch.ElapsedMilliseconds;
                searchHistory.WordListJobTime = wordlistStopwatch.ElapsedMilliseconds;
                searchHistory.Timestamp = DateTime.Now;
                searchHistory.ResourceCount = _searchFilters.PrimaryFilter.ResourceCount;
            }

            return searchResults;
        }

        public List<IResource> ExecuteAdmin(ISearchRequest searchRequest)
        {
            var totalSearchStopwatch = new Stopwatch();
            var filterStopwatch = new Stopwatch();
            var searchJobStopwatch = new Stopwatch();
            var executeJobStopwatch = new Stopwatch();
            var reportJobStopwatch = new Stopwatch();
            var resultsStopwatch = new Stopwatch();
            var wordlistStopwatch = new Stopwatch();
            var buildRequestStopwatch = new Stopwatch();
            const int itemsFound = 0;

            //SearchResults searchResults = new SearchResults { SearchRequest = searchRequest };


            var searchStatusHandler = new SearchStatusHandler(searchRequest.Field == SearchFields.FullText);
            var resourcesToReturn = new List<IResource>();
            try
            {
                totalSearchStopwatch.Start();

                buildRequestStopwatch.Start();
                var dtSearchRequestString = _searchRequestBuilder.GetAdminRequest(searchRequest, out _);
                buildRequestStopwatch.Stop();


                searchJobStopwatch.Start();
                using (var dtSearchJob = new SearchJob())
                {
                    dtSearchJob.SetIndexCache(SearchIndexCache.IndexCache);
                    dtSearchJob.IndexesToSearch.Add(_indexLocation);

                    dtSearchJob.Request = dtSearchRequestString;

                    dtSearchJob.SearchFlags = SearchFlags.dtsSearchDelayDocInfo
                                              | SearchFlags.dtsSearchWantHitDetails
                                              | SearchFlags.dtsSearchStemming;

                    dtSearchJob.FieldWeights =
                        "r2IndexTerms:10,r2BookSearch:200,r2BookTitle:50,r2BookSubTitle:40,r2Author:100";
                    dtSearchJob.AutoStopLimit = 500000;
                    dtSearchJob.StatusHandler = searchStatusHandler;
                    dtSearchJob.WantResultsAsFilter = true;

                    executeJobStopwatch.Start();
                    dtSearchJob.Execute();
                    executeJobStopwatch.Stop();
                    _log.DebugFormat(
                        "dtSearchJob.Execute() run time: {0} ms, MaxFilesToRetrieve: {1}, AutoStopLimit: {2}",
                        executeJobStopwatch.ElapsedMilliseconds,
                        dtSearchJob.MaxFilesToRetrieve, dtSearchJob.AutoStopLimit);

                    using (var jobErrorInfo = dtSearchJob.Errors)
                    {
                        if (jobErrorInfo != null && jobErrorInfo.Count > 0)
                        {
                            _log.InfoFormat("SEARCH ERROR - count: {0}", jobErrorInfo.Count);
                            for (var i = 0; i < jobErrorInfo.Count; i++)
                            {
                                _log.WriteLogMessage(
                                    jobErrorInfo.Code(i) == 122 || jobErrorInfo.Code(i) == 117
                                        ? Level.Info
                                        : Level.Warn,
                                    $"Message: {jobErrorInfo.Message(i)}, Code: {jobErrorInfo.Code(i)}, Arg1: {jobErrorInfo.Arg1(i)}, Arg2: {jobErrorInfo.Arg2(i)}");
                            }
                        }
                    }

                    searchJobStopwatch.Stop();


                    resultsStopwatch.Start();
                    using (var dtSearchResults = dtSearchJob.Results)
                    {
                        SetSortBy(searchRequest, dtSearchResults);

                        IList<SearchResource> searchResourceList = searchRequest.Resources.ToList();
                        //TODO: Parse ISBNs here and return them.
                        for (var i = 0; i < dtSearchJob.FileCount; i++)
                        {
                            dtSearchResults.GetNthDoc(i);
                            var dtSearchItem = dtSearchResults.CurrentItem;


                            var parts = dtSearchItem.DisplayName.Split('.');
                            var isbn = parts[1];
                            var searchResource = searchResourceList.FirstOrDefault(x => x.Resource.Isbn == isbn);
                            if (searchResource != null)
                            {
                                resourcesToReturn.Add(searchResource.Resource);
                            }
                        }
                    }

                    resultsStopwatch.Stop();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                totalSearchStopwatch.Stop();
                //searchResults.SearchTimeSpan = totalSearchStopwatch.Elapsed;
                //_log.DebugFormat("number of resources searched: {0}, searchJob.Request: NULL", _searchFilters.PrimaryFilter.ResourceCount);
                _log.DebugFormat("search request: {0} returned {1} items in {2:0.000}", searchRequest.ToString(),
                    itemsFound, totalSearchStopwatch.Elapsed.TotalSeconds);
                _log.DebugFormat(
                    "total search time: {0:0.000}, filter time: {1:0.000}, searchJob time: {2:0.000}, reportJob time: {3:0.000}, results time: {4:0.000}, wordlist time: {5:0.000}",
                    totalSearchStopwatch.Elapsed.TotalSeconds, filterStopwatch.Elapsed.TotalSeconds,
                    searchJobStopwatch.Elapsed.TotalSeconds,
                    reportJobStopwatch.Elapsed.TotalSeconds, resultsStopwatch.Elapsed.TotalSeconds,
                    wordlistStopwatch.Elapsed.TotalSeconds);

                _log.Debug(searchStatusHandler.ToString());
            }

            return resourcesToReturn;
        }

        public IndexStatus GetIndexStatus()
        {
            _log.DebugFormat("_indexLocation: {0}", _indexLocation);
            var indexInfo = IndexJob.GetIndexInfo(_indexLocation);
            return new IndexStatus(indexInfo, _indexLocation);
        }


        private void InitSearchFilters(ISearchRequest searchRequest, bool isAdminSearch)
        {
            if (_searchFilters == null)
            {
                if (isAdminSearch)
                {
                    _searchFilters = new SearchFilters(searchRequest, _indexLocation, true);
                }
                else
                {
                    _searchFilters = new SearchFilters(searchRequest, _indexLocation, false);
                }
            }
        }

        private void SetSortBy(ISearchRequest searchRequest, dtSearch.Engine.SearchResults dtSearchResults)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            switch (searchRequest.SortBy)
            {
                case SearchSortBy.BookTitle:
                    dtSearchResults.Sort(SortFlags.dtsSortByField | SortFlags.dtsSortAscending, "r2booktitle");
                    break;
                case SearchSortBy.Author:
                    dtSearchResults.Sort(SortFlags.dtsSortByField | SortFlags.dtsSortAscending, "r2primaryauthor");
                    break;
                case SearchSortBy.Publisher:
                    dtSearchResults.Sort(SortFlags.dtsSortByField | SortFlags.dtsSortAscending, "r2publisher");
                    break;
            }

            stopwatch.Stop();
            _log.DebugFormat("SetSortBy: {0} ms, SortBy: {1}", stopwatch.ElapsedMilliseconds, searchRequest.SortBy);
        }

        private int ExecuteCountSearch(ISearchRequest searchRequest, string dtSearchRequestString,
            SearchFilter searchFilter)
        {
            var searchJobStopwatch = new Stopwatch();
            var hitCount = 0;
            var fileCount = 0;
            try
            {
                searchJobStopwatch.Start();
                using (var dtSearchJob = new SearchJob())
                {
                    dtSearchJob.SetIndexCache(SearchIndexCache.IndexCache);
                    dtSearchJob.IndexesToSearch.Add(_indexLocation);
                    dtSearchJob.SetFilter(searchFilter);

                    dtSearchJob.Request = dtSearchRequestString;
                    dtSearchJob.SearchFlags = SearchFlags.dtsSearchDelayDocInfo
                                              | SearchFlags.dtsSearchFastSearchFilterOnly;

                    dtSearchJob.MaxFilesToRetrieve = searchRequest.PageSize * searchRequest.Page;
                    dtSearchJob.WantResultsAsFilter = true;
                    dtSearchJob.Execute();

                    using (var jobErrorInfo = dtSearchJob.Errors)
                    {
                        if (jobErrorInfo != null && jobErrorInfo.Count > 0)
                        {
                            _log.InfoFormat("SEARCH ERROR - count: {0}", jobErrorInfo.Count);
                            for (var i = 0; i < jobErrorInfo.Count; i++)
                            {
                                _log.WriteLogMessage(
                                    jobErrorInfo.Code(i) == 122 || jobErrorInfo.Code(i) == 117
                                        ? Level.Info
                                        : Level.Warn,
                                    $"Message: {jobErrorInfo.Message(i)}, Code: {jobErrorInfo.Code(i)}, Arg1: {jobErrorInfo.Arg1(i)}, Arg2: {jobErrorInfo.Arg2(i)}");
                            }
                        }
                    }

                    fileCount = dtSearchJob.FileCount;
                    hitCount = dtSearchJob.HitCount;
                    return fileCount;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                searchJobStopwatch.Stop();
                _log.DebugFormat("searchJob time: {0:0.000}, HitCount: {1}, FileCount: {2}",
                    searchJobStopwatch.Elapsed.TotalSeconds, hitCount, fileCount);
            }
        }
    }
}