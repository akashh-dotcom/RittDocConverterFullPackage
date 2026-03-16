#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml;
using R2Library.Data.ADO.R2.DataServices;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Utilities;
using R2V2.Core.MyR2;
using R2V2.Core.Resource;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Search;

#endregion

namespace R2Utilities.Tasks.DataConversion
{
    public class ConvertUserSearchDataTask : TaskBase
    {
        private readonly DisciplineToSpecialtyDataService _disciplineToSpecialtyDataService;
        private readonly IQueryable<PracticeArea> _practiceAreas;
        private readonly IQueryable<Resource> _resources;
        private readonly IQueryable<UserSavedSearch> _userSavedSearches;
        private readonly IQueryable<UserSearchHistory> _userSearchHistories;

        public ConvertUserSearchDataTask(IQueryable<UserSearchHistory> userSearchHistories
            , IQueryable<UserSavedSearch> userSavedSearches
            , IQueryable<Resource> resources
            , IQueryable<PracticeArea> practiceAreas
        )
            : base("ConvertUserSearchDataTask", "-ConvertUserSearchData", "x99", TaskGroup.Deprecated,
                "Converts user search data, only needed during conversion to R2v2", false)
        {
            //_sessionFactory = CreateR2SessionFactory();
            _userSearchHistories = userSearchHistories;
            _userSavedSearches = userSavedSearches;
            _resources = resources;
            _practiceAreas = practiceAreas;

            _disciplineToSpecialtyDataService = new DisciplineToSpecialtyDataService();
        }

        public override void Run()
        {
            try
            {
                var step2 = new TaskResultStep { Name = "SearchHistoryTask", StartTime = DateTime.Now };
                TaskResult.AddStep(step2);
                UpdateTaskResult();

                var count = 0;
                var userSavedSearches = GetUserSavedSearch();
                foreach (var userSavedSearch in userSavedSearches)
                {
                    count++;
                    Log.InfoFormat("{0} of {1}", count, userSavedSearches.Count());
                    Log.DebugFormat("Id: {0}, CreationDate: {1}, Xml: {2}", userSavedSearch.Id,
                        userSavedSearch.CreationDate, userSavedSearch.Xml);
                    //Log.DebugFormat("Folder.Id: {0}, User.Id: {1}, FolderName: {2}", userSavedSearch.Folder.Id, userSavedSearch.Folder.User.Id, userSavedSearch.Folder.FolderName);
                    var searchSummary = GetSearchSummary(userSavedSearch.Xml);

                    if (searchSummary != null)
                    {
                        var scriptSerializer = new JavaScriptSerializer();
                        var json = scriptSerializer.Serialize(searchSummary);
                        Log.Debug(json);

                        userSavedSearch.SearchQuery = json;
                        userSavedSearch.ResultsCount = searchSummary.ResultsCount;
                    }
                }

                var userSavedSearchService = new UserSavedSearchService();
                foreach (var userSavedSearch in userSavedSearches)
                {
                    userSavedSearchService.UpdateSearchQuery(userSavedSearch.Id, userSavedSearch.SearchQuery,
                        userSavedSearch.ResultsCount);
                }

                var userSearchHistories = GetUserSearchHistory(new DateTime(2011, 1, 1, 0, 0, 0, 0), DateTime.Now);

                count = 0;
                foreach (var userSearchHistory in userSearchHistories)
                {
                    count++;
                    Log.InfoFormat("{0} of {1}", count, userSearchHistories.Count());
                    Log.DebugFormat("Id: {0}, CreationDate: {1}, SearchXml: {2}", userSearchHistory.Id,
                        userSearchHistory.CreationDate, userSearchHistory.SearchXml);
                    var searchSummary = GetSearchSummary(userSearchHistory.SearchXml);

                    if (searchSummary != null)
                    {
                        var scriptSerializer = new JavaScriptSerializer();
                        var json = scriptSerializer.Serialize(searchSummary);
                        Log.Debug(json);

                        userSearchHistory.SearchQuery = json;
                        userSearchHistory.ResultsCount = searchSummary.ResultsCount;
                    }
                }

                var userSearchHistoryService = new UserSearchHistoryService();
                foreach (var userSearchHistory in userSearchHistories)
                {
                    userSearchHistoryService.UpdateSearchQuery(userSearchHistory.Id, userSearchHistory.SearchQuery,
                        userSearchHistory.ResultsCount);
                }

                UpdateTaskResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        private IList<UserSavedSearch> GetUserSavedSearch()
        {
            var userSavedSearches = _userSavedSearches
                //.Fetch(x => x.Folder)
                //.ThenFetch(x => x.User)
                .Where(x => x.SearchQuery == null)
                .OrderByDescending(x => x.CreationDate)
                //.Take(1000)
                .ToList();

            return userSavedSearches;
        }

        private IList<UserSearchHistory> GetUserSearchHistory(DateTime minDate, DateTime maxDate)
        {
            var userSearchHistory = _userSearchHistories
                .Where(x => x.CreationDate >= minDate && x.CreationDate <= maxDate)
                //.Fetch(x => x.User)
                //.Eager
                .Where(x => x.SearchQuery == null)
                .OrderByDescending(x => x.CreationDate)
                //.Take(1000)
                .ToList();

            return userSearchHistory;
        }


        private SearchSummary GetSearchSummary(string searchXml)
        {
            try
            {
                var searchSummary = new SearchSummary();

                var xmlDoc = new XmlDocument();
                var cleanXml = searchXml.Replace(" & ", " &amp; ");
                xmlDoc.LoadXml(cleanXml);

                var searchNode = XmlHelper.GetXmlNode(xmlDoc, "//searchroot/search");
                var searchType = searchNode.Attributes["type"].Value; // 1 = quick, 2 = advanced
                searchSummary.Advanced = searchType == "2";

                var resultsNode = XmlHelper.GetXmlNode(xmlDoc, "//searchroot/searchresults");
                if (resultsNode != null)
                {
                    var searchResults = resultsNode.InnerText;
                    int.TryParse(searchResults, out var count);
                    searchSummary.ResultsCount = count;
                }
                else
                {
                    searchSummary.ResultsCount = 0;
                }

                foreach (XmlNode childNode in searchNode.ChildNodes)
                {
                    //Log.DebugFormat("childNode: {0}", childNode.Name);

                    switch (childNode.Name)
                    {
                        case "searchresources":
                            // All = 1, FullTextOnly = 2, FullTextDefault = 3
                            // SJS - I think we can ignore - 8/10/2012
                            var resources = childNode.InnerText;
                            break;

                        case "searchonly":
                            // Images = 1, Multimedia = 2, DrugMonograph = 3
                            var searchOnly = childNode.InnerText;
                            if (searchOnly == "3")
                            {
                                searchSummary.DrugMonograph = true;
                                searchSummary.Field = SearchFields.All;
                            }
                            else if (searchOnly == "1")
                            {
                                searchSummary.Field = SearchFields.ImageTitle;
                            }
                            else
                            {
                                searchSummary.Field = SearchFields.All;
                            }

                            break;

                        case "searcharchive":
                            // yes = 1, no = 0
                            var archive = childNode.InnerText;
                            searchSummary.Archive = archive == "1";
                            searchSummary.Active = archive != "1";
                            break;

                        case "searchcriteria":
                            foreach (XmlNode criteriaNode in childNode.ChildNodes)
                            {
                                if (criteriaNode.Name == "criteria")
                                {
                                    //SearchCriteria searchCriteria = new SearchCriteria();
                                    //searchData.SearchCriteria.Add(searchCriteria);
                                    if (criteriaNode.Attributes != null)
                                    {
                                        var type = criteriaNode.Attributes["type"].Value.ToLower();
                                        var phrase = criteriaNode.Attributes["phrase"].Value.Trim();

                                        switch (type)
                                        {
                                            case "keyword":
                                                searchSummary.Term = phrase;
                                                break;
                                            case "isbn":
                                                searchSummary.Isbns = new[] { phrase };
                                                break;
                                            case "booktitle":
                                                searchSummary.BookTitle = phrase;
                                                break;
                                            case "author":
                                                searchSummary.Author = phrase;
                                                break;
                                            case "publisher":
                                                searchSummary.Publisher = phrase;
                                                break;
                                            case "editor":
                                                searchSummary.Editor = phrase;
                                                break;
                                            default:
                                                Log.WarnFormat("criteria type not supported: {0}", type);
                                                break;
                                        }

                                        foreach (XmlAttribute attribute in criteriaNode.Attributes)
                                        {
                                            if (attribute.Name != "type" && attribute.Name != "phrase")
                                            {
                                                Log.WarnFormat("criteria attribute not supported: {0}", attribute.Name);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Log.Warn("criteria attribute is null");
                                    }
                                }
                                else
                                {
                                    Log.WarnFormat("searchcriteria node not supported: {0}", criteriaNode.Name);
                                }
                            }

                            break;

                        case "limitcriteria":
                            var limitCriteriaType =
                                childNode.Attributes["type"].Value; // 0 = ???, Library = 1, ReservedShelfList = 2
                            foreach (XmlNode limitNode in childNode.ChildNodes)
                            {
                                switch (limitNode.Name)
                                {
                                    case "limitdiscipline":
                                        var disciplineId = int.Parse(limitNode.InnerText);
                                        if (disciplineId > 0)
                                        {
                                            Log.WarnFormat("DISIPLINE ID: {0}", disciplineId);

                                            var specialtyCode =
                                                _disciplineToSpecialtyDataService.GetSpecialtyCodeByDisciplineId(
                                                    disciplineId);
                                            if (specialtyCode != null)
                                            {
                                                searchSummary.SpecialtyCode = specialtyCode;
                                            }
                                            else
                                            {
                                                Log.Warn("Specialty code was null");
                                                return null;
                                            }
                                        }

                                        break;

                                    case "limitresource":
                                        var resourceId = int.Parse(limitNode.InnerText);
                                        if (resourceId > 0)
                                        {
                                            var resource = GetResourceById(resourceId);
                                            searchSummary.SearchWithinIsbns = new[] { resource.Isbn };
                                        }

                                        break;

                                    case "limitlibrary":
                                        var libraryId = int.Parse(limitNode.InnerText);
                                        if (libraryId > 0)
                                        {
                                            var practiceArea = _practiceAreas.SingleOrDefault(x => x.Id == libraryId);
                                            searchSummary.PracticeAreaCode = practiceArea.Code;
                                        }

                                        break;

                                    case "limitreserveshelf":
                                        var reserveShelfId = int.Parse(limitNode.InnerText);
                                        if (reserveShelfId > 0)
                                        {
                                            searchSummary.ReserveShelfId = reserveShelfId;
                                        }

                                        break;
                                    default:
                                        Log.WarnFormat("limit criteria node not supported: {0}", limitNode.Name);
                                        break;
                                }
                            }

                            break;

                        default:
                            Log.WarnFormat("search node not supported: {0}", childNode.Name);
                            break;
                    }
                }


                return searchSummary;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message, ex);
                return null;
            }
        }


        private SearchData ParseSearchXml(UserSearchHistory userSearchHistory)
        {
            var searchData = new SearchData
            {
                UserSearchHistoryId = userSearchHistory.Id,
                UserId = userSearchHistory.UserId,
                CreatedBy = userSearchHistory.CreatedBy,
                DateCreated = userSearchHistory.CreationDate,
                SearchXml = userSearchHistory.SearchXml
            };
            Log.DebugFormat("UserSearchHistoryId: {0}, DateCreated: {1}, SearchXml: {2}",
                searchData.UserSearchHistoryId, searchData.DateCreated, searchData.SearchXml);

            var xmlDoc = new XmlDocument();
            var cleanXml = userSearchHistory.SearchXml.Replace(" & ", " &amp; ");
            xmlDoc.LoadXml(cleanXml);

            var searchNode = XmlHelper.GetXmlNode(xmlDoc, "//searchroot/search");
            searchData.SearchType = searchNode.Attributes["type"].Value;

            var resultsNode = XmlHelper.GetXmlNode(xmlDoc, "//searchroot/searchresults");
            searchData.Results = resultsNode.InnerText;

            foreach (XmlNode childNode in searchNode.ChildNodes)
            {
                //Log.DebugFormat("childNode: {0}", childNode.Name);

                switch (childNode.Name)
                {
                    case "searchresources":
                        searchData.Resources = childNode.InnerText;
                        break;

                    case "searchonly":
                        searchData.SearchOnly = childNode.InnerText;
                        break;

                    case "searcharchive":
                        searchData.Archive = childNode.InnerText;
                        break;

                    case "searchcriteria":
                        foreach (XmlNode criteriaNode in childNode.ChildNodes)
                        {
                            if (criteriaNode.Name == "criteria")
                            {
                                var searchCriteria = new SearchCriteria();
                                searchData.SearchCriteria.Add(searchCriteria);
                                if (criteriaNode.Attributes != null)
                                {
                                    foreach (XmlAttribute attribute in criteriaNode.Attributes)
                                    {
                                        if (attribute.Name == "type")
                                        {
                                            searchCriteria.Type = attribute.Value;
                                        }
                                        else if (attribute.Name == "phrase")
                                        {
                                            searchCriteria.Phrase = attribute.Value.Trim();
                                        }
                                        else
                                        {
                                            Log.WarnFormat("criteria attribute not supported: {0}", attribute.Name);
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(childNode.InnerText))
                                    {
                                        Log.WarnFormat("criteria inner text not supported: '{0}'", childNode.InnerText);
                                    }
                                }
                            }
                            else
                            {
                                Log.WarnFormat("searchcriteria node not supported: {0}", criteriaNode.Name);
                            }
                        }

                        break;

                    case "limitcriteria":
                        searchData.LimitCriteriaType = childNode.Attributes["type"].Value;
                        foreach (XmlNode limitNode in childNode.ChildNodes)
                        {
                            var limitCriteria = new LimitCriteria();
                            searchData.LimitCriteria.Add(limitCriteria);
                            switch (limitNode.Name)
                            {
                                case "limitdiscipline":
                                    limitCriteria.Discipline = int.Parse(limitNode.InnerText);
                                    break;
                                case "limitresource":
                                    limitCriteria.Resource = int.Parse(limitNode.InnerText);
                                    break;
                                case "limitlibrary":
                                    limitCriteria.Library = int.Parse(limitNode.InnerText);
                                    break;
                                case "limitreserveshelf":
                                    limitCriteria.ReserverShelf = int.Parse(limitNode.InnerText);
                                    break;
                                default:
                                    Log.WarnFormat("limit criteria node not supported: {0}", limitNode.Name);
                                    break;
                            }
                        }

                        break;

                    default:
                        Log.WarnFormat("search node not supported: {0}", childNode.Name);
                        break;
                }
            }

            return searchData;
        }

        private Resource GetResourceById(int resourceId)
        {
            var resource = _resources
                .SingleOrDefault(x => x.Id == resourceId);
            return resource;
        }
    }
}