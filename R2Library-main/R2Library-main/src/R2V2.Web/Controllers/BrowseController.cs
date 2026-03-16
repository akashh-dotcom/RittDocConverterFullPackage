#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Author;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Browse;
using R2V2.Web.Models.Collections;
using R2V2.Web.Models.Resource;
using R2V2.Web.Models.Shared;
using Filter = R2V2.Web.Models.Shared.Filter;

#endregion

namespace R2V2.Web.Controllers
{
    [RequestLoggerFilter(false)]
    public class BrowseController : R2BaseController, IInstitutionDisplay
    {
        private readonly IAdminSettings _adminSettings;
        private readonly IAuthorService _authorService;
        private readonly IClientSettings _clientSettings;
        private readonly ICollectionService _collectionService;
        private readonly InstitutionService _institutionService;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<BrowseController> _log;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly IPublisherService _publisherService;
        private readonly IResourceAccessService _resourceAccessService;
        private readonly IResourcesByInstitutionService _resourcesByInstitutionService;
        private readonly ISpecialtyService _specialtyService;
        private ICollection _filteredCollection;
        private IList<IAuthor> _filteredPrimaryAuthors;

        private IList<IResource> _filteredResources;
        private SortedDictionary<string, PublisherSummary> _filteredSortedPublishers;

        private int _guestInstitutionId;

        public BrowseController(
            ILog<BrowseController> log
            , IAuthenticationContext authenticationContext
            , IAuthorService authorService
            , ISpecialtyService specialtyService
            , IPublisherService publisherService
            , IClientSettings clientSettings
            , IInstitutionSettings institutionSettings
            , IResourcesByInstitutionService resourcesByInstitutionService
            , IPracticeAreaService practiceAreaService
            , InstitutionService institutionService
            , IResourceAccessService resourceAccessService
            , ICollectionService collectionService
            , IAdminSettings adminSettings
        ) : base(authenticationContext)
        {
            _log = log;
            _authorService = authorService;
            _specialtyService = specialtyService;
            _publisherService = publisherService;
            _clientSettings = clientSettings;
            _institutionSettings = institutionSettings;
            _resourcesByInstitutionService = resourcesByInstitutionService;
            _practiceAreaService = practiceAreaService;
            _institutionService = institutionService;
            _resourceAccessService = resourceAccessService;
            _collectionService = collectionService;
            _adminSettings = adminSettings;
        }

        private int GuestInstitutionId
        {
            get
            {
                if (_guestInstitutionId <= 0)
                {
                    _guestInstitutionId =
                        _institutionService.GetGuestInstitutionId(_institutionSettings.GuestAccountNumber);
                }

                return _guestInstitutionId;
            }
        }

        private int InstitutionId => AuthenticatedInstitution == null || AuthenticatedInstitution.Id <= 0
            ? GuestInstitutionId
            : AuthenticatedInstitution.Id;

        public bool DisplayTocAvailable()
        {
            return DisplayAllProducts() && InstitutionId != GuestInstitutionId;
        }

        [RequestLoggerFilter(false)]
        public ActionResult Index()
        {
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas()
                .Select(s => new PracticeArea { Id = s.Id, Name = s.Name }).ToList().OrderBy(x => x.Name);

            var model = new Browse
            {
                PracticeAreas = practiceAreas,
                PageSize = _clientSettings.BrowsePageSize,
                DisplayTocAvailable = DisplayTocAvailable()
            };

            if (AuthenticatedInstitution != null)
            {
                model.DefaultInclude = AuthenticatedInstitution.IncludeArchivedTitlesByDefault ? 3 : 1;
                model.DefaultType = AuthenticatedInstitution.HomePage.GetDescription();
                model.EnableCollectionLink = AuthenticatedInstitution.EnableHomePageCollectionLink;
                model.CollectionLinkName = _adminSettings.PublicCollectionTabName;

                if (model.EnableCollectionLink)
                {
                    var specialties = _specialtyService.GetAllSpecialties()
                        .Select(s => new Specialty { Id = s.Id, Name = s.Name }).ToList().OrderBy(x => x.Name);
                    var publishers = _publisherService.GetPublishers().ToList().OrderBy(x => x.DisplayName);
                    model.Disciplines = specialties;
                    model.Publishers = publishers;
                }
            }

            return View(model);
        }

        [RequestLoggerFilter(true)]
        public ActionResult JsonResults(BrowseQuery browseQuery)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var json = new FacetedResultsJson();

            try
            {
                if (browseQuery.Type == BrowseType.Collections)
                {
                    if (_filteredCollection == null)
                    {
                        _filteredCollection = _collectionService.GetPublicCollection();
                    }

                    browseQuery.CollectionId = _filteredCollection.Id;
                }

                // limit resources based on query
                _filteredResources = GetResourceForInstitution(browseQuery);

                json.FilterGroups.Add(
                    "toc-available",
                    new FilterGroup
                    {
                        Code = "toc-available",
                        Name = "Table of Contents Available",
                        Filters = new Dictionary<string, Filter>
                            { { "true", new Filter { Count = "0", Name = "Yes" } } }
                    });

                if (browseQuery.Type == BrowseType.Authors)
                {
                    _filteredPrimaryAuthors = GetPrimaryAuthors();
                    json.FilterGroups.Add("author",
                        new FilterGroup { Code = "author", Name = "Author", Filters = GetAuthorFilters() });
                }

                if (browseQuery.Type == BrowseType.Disciplines)
                {
                    json.FilterGroups.Add("discipline",
                        new FilterGroup { Code = "discipline", Name = "Discipline", Filters = GetSpecialtyFilters() });
                }

                if (browseQuery.Type == BrowseType.Publishers)
                {
                    _filteredSortedPublishers = GetFilteredSortedPublishers();
                    json.FilterGroups.Add("publisher",
                        new FilterGroup { Code = "publisher", Name = "Publisher", Filters = GetPublisherFilters() });
                }

                if (browseQuery.Type == BrowseType.Collections)
                {
                    json.FilterGroups.Add("disciplineId",
                        new FilterGroup
                            { Code = "disciplineId", Name = "Discipline", Filters = GetSpecialtyFilters() });
                    //_filteredSortedPublishers = GetFilteredSortedPublishers();
                    json.FilterGroups.Add("publisherId",
                        new FilterGroup
                        {
                            Code = "publisherId", Name = "Publisher", Filters = GetPublisherFiltersForCollections()
                        });
                }


                json.FilterGroups.Add("practice-area",
                    new FilterGroup
                    {
                        Code = "practice-area", Name = "Practice Area", Filters = GetPracticeAreaFilters(browseQuery)
                    });

                json.SortGroups.Add("sort-by",
                    string.IsNullOrWhiteSpace(browseQuery.Id)
                        ? null
                        : new SortGroup { Code = "sort-by", Name = "Sort by", Filters = GetSortFilters(browseQuery) });

                json.HtmlSnippets.Add("results", GetHtmlResults(browseQuery));

                json.Successful = true;

                if (browseQuery.Page > 1)
                {
                    SuppressRequestLogging();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json.ErrorMessage = "We are sorry, an error occurred while your were browsing. Please try again.";
                json.Successful = false;
            }

            stopwatch.Stop();
            _log.DebugFormat("JsonResults() - {0} ms", stopwatch.ElapsedMilliseconds);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private Dictionary<string, Filter> GetAuthorFilters()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var authorCounts = new SortedDictionary<string, int>();

            foreach (var author in _filteredPrimaryAuthors)
            {
                if (authorCounts.ContainsKey(author.LastName))
                {
                    var count = authorCounts[author.LastName];
                    authorCounts[author.LastName] = count + 1;
                }
                else
                {
                    authorCounts.Add(author.LastName, 1);
                }
            }

            var filters = new Dictionary<string, Filter>();

            foreach (var author in authorCounts)
            {
                filters[author.Key] = new Filter { Name = author.Key, Count = $"{author.Value}" };
            }

            stopwatch.Stop();
            _log.DebugFormat("GetAuthorFilters() - {0} ms, filters.Count: {1}", stopwatch.ElapsedMilliseconds,
                filters.Count);
            return filters;
        }

        private Dictionary<string, Filter> GetSpecialtyFilters()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var specialtyCounts = new SortedDictionary<int, int>();

            foreach (var resource in _filteredResources)
            {
                foreach (var specialty in resource.Specialties)
                {
                    var specialtyId = specialty.Id;
                    if (specialtyCounts.ContainsKey(specialtyId))
                    {
                        var count = specialtyCounts[specialtyId];
                        specialtyCounts[specialtyId] = count + 1;
                    }
                    else
                    {
                        specialtyCounts.Add(specialtyId, 1);
                    }
                }
            }

            var filters = new Dictionary<string, Filter>();

            foreach (var pair in specialtyCounts)
            {
                var specialty = _specialtyService.GetSpecialty(pair.Key);
                filters[$"{pair.Key}"] = new Filter { Name = specialty.Name, Count = $"{pair.Value}" };
            }

            stopwatch.Stop();
            _log.DebugFormat("GetSpecialtyFilters() - {0} ms, filters.Count: {1}", stopwatch.ElapsedMilliseconds,
                filters.Count);
            return filters;
        }

        private Dictionary<string, Filter> GetPublisherFiltersForCollections()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var publisherCounts = new SortedDictionary<int, int>();
            var publishers = new SortedDictionary<int, IPublisher>();
            foreach (var resource in _filteredResources)
            {
                var publisherId = resource.Publisher.Id;
                if (publisherCounts.ContainsKey(publisherId))
                {
                    var count = publisherCounts[publisherId];
                    publisherCounts[publisherId] = count + 1;
                }
                else
                {
                    publisherCounts.Add(publisherId, 1);
                }

                if (!publishers.ContainsKey(publisherId))
                {
                    publishers.Add(publisherId, resource.Publisher);
                }
            }

            var filters = new Dictionary<string, Filter>();

            foreach (var pair in publisherCounts)
            {
                var pub = publishers[pair.Key];
                var pubName = string.IsNullOrWhiteSpace(pub.DisplayName) ? pub.Name : pub.DisplayName;
                filters[$"{pair.Key}"] = new Filter { Name = pubName, Count = $"{pair.Value}" };
            }

            stopwatch.Stop();
            _log.DebugFormat("GetPublisherFiltersForCollections() - {0} ms, filters.Count: {1}",
                stopwatch.ElapsedMilliseconds, filters.Count);
            return filters;
        }

        private Dictionary<string, Filter> GetPublisherFilters()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var filters = new Dictionary<string, Filter>();
            foreach (var filteredSortedPublisher in _filteredSortedPublishers)
            {
                var publisherSummary = filteredSortedPublisher.Value;
                var filter = new Filter { Count = null, Name = publisherSummary.Name };
                filters.Add($"{publisherSummary.Id}", filter);
            }

            stopwatch.Stop();
            _log.DebugFormat("GetPublisherFilters() - {0} ms, filters.Count: {1}", stopwatch.ElapsedMilliseconds,
                filters.Count);
            return filters;
        }

        private Dictionary<string, Filter> GetPracticeAreaFilters(BrowseQuery browseQuery)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // get practice areas
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();
            var filters = practiceAreas.ToDictionary(practiceArea => $"{practiceArea.Id}",
                practiceArea => new Filter { Count = "0", Name = practiceArea.Name });

            var selectedPracticeAreaId = GetId(browseQuery.PracticeArea);

            // get all the practice areas with counts
            var practiceAreaCounts = new Dictionary<int, int>();
            foreach (var filteredResource in _filteredResources)
            {
                var practiceAreaIds = GetDistinctPracticeAreaIds(filteredResource.PracticeAreas);
                foreach (var practiceAreaId in practiceAreaIds)
                {
                    if (selectedPracticeAreaId > 0 && selectedPracticeAreaId != practiceAreaId)
                    {
                        continue;
                    }

                    if (practiceAreaCounts.ContainsKey(practiceAreaId))
                    {
                        var count = practiceAreaCounts[practiceAreaId];
                        practiceAreaCounts[practiceAreaId] = count + 1;
                    }
                    else
                    {
                        practiceAreaCounts.Add(practiceAreaId, 1);
                    }
                }
            }

            foreach (var practiceAreaCount in practiceAreaCounts)
            {
                var filter = filters[$"{practiceAreaCount.Key}"];
                filter.Count = $"{practiceAreaCount.Value}";
            }

            stopwatch.Stop();
            _log.DebugFormat("GetPracticeAreaFilters() - {0} ms, filters.Count: {1}, selectedPracticeAreaId: {2}",
                stopwatch.ElapsedMilliseconds, filters.Count, selectedPracticeAreaId);
            return filters;
        }

        private IEnumerable<int> GetDistinctPracticeAreaIds(IEnumerable<IPracticeArea> practiceAreas)
        {
            var ids = new List<int>();
            foreach (var practiceArea in practiceAreas)
            {
                if (!ids.Contains(practiceArea.Id))
                {
                    ids.Add(practiceArea.Id);
                }
            }

            return ids;
        }

        private IList<IResource> GetResourceForInstitution(BrowseQuery browseQuery)
        {
            // get all the resource an institution has access to
            var resources = _resourcesByInstitutionService.GetResourcesForActiveInstitution(
                _institutionSettings.GuestAccountNumber, browseQuery.TocAvailable, browseQuery.CollectionId);

            var publisherId = browseQuery.Type == BrowseType.Publishers ? GetId(browseQuery.Id) : 0;

            if (!string.IsNullOrWhiteSpace(browseQuery.PublisherId))
            {
                publisherId = GetId(browseQuery.PublisherId);
            }

            var filteredResources = new List<IResource>();

            var queryPracticeAreaId = GetId(browseQuery.PracticeArea);
            foreach (var resource in resources)
            {
                // filter by active/archive (include)
                if (resource.StatusId == (int)ResourceStatus.Active && browseQuery.Include != Include.Active &&
                    browseQuery.Include != Include.ActiveAndArchive)
                {
                    continue;
                }

                if (resource.StatusId == (int)ResourceStatus.Archived && browseQuery.Include != Include.Archive &&
                    browseQuery.Include != Include.ActiveAndArchive)
                {
                    continue;
                }

                if (queryPracticeAreaId > 0)
                {
                    var practiceArea = resource.PracticeAreas.FirstOrDefault(x => x.Id == queryPracticeAreaId);
                    if (practiceArea == null)
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(browseQuery.DisciplineId))
                {
                    var specialty = resource.Specialties.FirstOrDefault(x => x.Id == GetId(browseQuery.DisciplineId));
                    if (specialty == null)
                    {
                        continue;
                    }
                }

                if (publisherId > 0 && publisherId != resource.PublisherId &&
                    (resource.Publisher.ConsolidatedPublisher == null ||
                     resource.Publisher.ConsolidatedPublisher.Id != publisherId))
                {
                    continue;
                }

                filteredResources.Add(resource);
            }

            _log.DebugFormat("filteredResources.Count: {0}", filteredResources.Count());
            return filteredResources;
        }

        private static Dictionary<string, Filter> GetSortFilters(BrowseQuery browseQuery)
        {
            var sortFilters = new Dictionary<string, Filter> { { "title", new Filter { Name = "Title" } } };

            if (browseQuery.Type != BrowseType.Publishers)
            {
                sortFilters.Add("publisher", new Filter { Name = "Publisher" });
            }

            sortFilters.Add("publication-date", new Filter { Name = "Publication Date" });

            return sortFilters;
        }

        private string GetHtmlResults(BrowseQuery browseQuery)
        {
            if (browseQuery.Page <= 0)
            {
                browseQuery.Page = 1;
            }

            switch (browseQuery.Type)
            {
                case BrowseType.Disciplines:
                    return string.IsNullOrWhiteSpace(browseQuery.Id)
                        ? RenderPartialViewToString("_BrowseDisciplines", GetSpecialtySummaries())
                        : RenderPartialViewToString("_Discipline", GetSpecialtyDetail(browseQuery));

                case BrowseType.Authors:
                    return string.IsNullOrWhiteSpace(browseQuery.Id)
                        ? RenderPartialViewToString("_BrowseAuthors", GetAuthorSummaries())
                        : RenderPartialViewToString("_Author", GetAuthorDetail(browseQuery));

                case BrowseType.Publishers:
                    return string.IsNullOrWhiteSpace(browseQuery.Id)
                        ? RenderPartialViewToString("_BrowsePublishers", GetPublisherSummaries())
                        : RenderPartialViewToString("_Publisher", GetPublisherDetail(browseQuery));
                case BrowseType.Collections:
                    return RenderPartialViewToString("_Collection", GetCollectionSummary(browseQuery));
                default:
                    return RenderPartialViewToString("_Publications", GetResourceSummaries(browseQuery));
            }
        }

        private AuthorSummaries GetAuthorSummaries()
        {
            //IEnumerable<AuthorSummary> authors = _authorService.GetAuthors(InstitutionId, browseQuery.Include, browseQuery.PracticeArea, DisplayAllProducts(browseQuery.TocAvailable)).ToAuthorSummaries();
            var authors = GetAuthorSummariesFilteredResources();
            return new AuthorSummaries { Authors = authors };
        }

        private AuthorDetail GetAuthorDetail(BrowseQuery browseQuery)
        {
            var authorDetail = _authorService.GetAuthor(browseQuery.Id).ToAuthorDetail();
            authorDetail.BrowseQuery = browseQuery;
            authorDetail.Resources = GetResources(null, null, browseQuery.Id, "", browseQuery);

            return authorDetail;
        }

        private SpecialtySummaries GetSpecialtySummaries()
        {
            var sortedSpecialtySummeries = new SortedDictionary<string, SpecialtySummary>();

            foreach (var filteredResource in _filteredResources)
            {
                if (filteredResource.Specialties == null)
                {
                    _log.DebugFormat("GetSpecialtySummaries() - {0}", filteredResource.ToDebugInfo());
                }
                else
                {
                    foreach (var specialty in filteredResource.Specialties)
                    {
                        if (sortedSpecialtySummeries.ContainsKey(specialty.Name))
                        {
                            var specialtySummary = sortedSpecialtySummeries[specialty.Name];
                            specialtySummary.ResourceCount++;
                        }
                        else
                        {
                            var specialtySummary = new SpecialtySummary
                                { Id = specialty.Id, Name = specialty.Name, ResourceCount = 1 };
                            sortedSpecialtySummeries.Add(specialtySummary.Name, specialtySummary);
                        }
                    }
                }
            }

            return new SpecialtySummaries { Specialties = sortedSpecialtySummeries.Values };
        }

        private SpecialtyDetail GetSpecialtyDetail(BrowseQuery browseQuery)
        {
            int id;
            int.TryParse(browseQuery.Id, out id);
            //int practiceAreaId = GetId(browseQuery.PracticeArea);
            var specialtyDetail = _specialtyService.GetSpecialty(browseQuery.Id).ToSpecialtyDetail();
            specialtyDetail.BrowseQuery = browseQuery;
            specialtyDetail.Resources = GetResources(id, null, "", "", browseQuery);

            return specialtyDetail;
        }

        private ResourceSummaries GetResourceSummaries(BrowseQuery browseQuery)
        {
            IEnumerable<PageLink> pageLinks = null;

            var institutionResourceCount = _filteredResources.Count();
            if (institutionResourceCount >= _institutionSettings.MinimumResourceCountForPaging ||
                browseQuery.TocAvailable)
            {
                if (string.IsNullOrWhiteSpace(browseQuery.Alpha))
                {
                    browseQuery.Alpha = "All";
                }

                pageLinks = GetPageLinks(browseQuery.Alpha);
            }

            var resources = GetResources(null, null, "", browseQuery.Alpha, browseQuery);
            return new ResourceSummaries { BrowseQuery = browseQuery, PageLinks = pageLinks, Resources = resources };
        }

        private CollectionDetail GetCollectionSummary(BrowseQuery browseQuery)
        {
            browseQuery.ShowAll = true;
            return new CollectionDetail(_filteredCollection, GetResources(null, null, "", "All", browseQuery));
        }

        private PublisherSummaries GetPublisherSummaries()
        {
            IList<PublisherSummary> publishers = _filteredSortedPublishers.Values.ToList();
            var publisherSummaries = new PublisherSummaries { Publishers = publishers };
            return publisherSummaries;
        }

        private PublisherDetail GetPublisherDetail(BrowseQuery browseQuery)
        {
            int id;
            int.TryParse(browseQuery.Id, out id);

            IList<IPublisher> publishers = _publisherService.GetPublishers().ToList();
            var publisher = publishers.SingleOrDefault(x => x.Id == id);
            var publisherDetail = publisher.ToPublisherDetail();

            publisherDetail.BrowseQuery = browseQuery;
            publisherDetail.Resources = GetResources(null, id, "", "", browseQuery);

            return publisherDetail;
        }

        private bool DisplayAllProducts()
        {
            return AuthenticatedInstitution == null || AuthenticatedInstitution.DisplayAllProducts;
        }

        private IEnumerable<PageLink> GetPageLinks(string page)
        {
            var keys = GetKeys();
            return PaginationHelper.GetAlphaPageLinks(keys, page, true);
        }

        private IEnumerable<string> GetKeys()
        {
            var alphaKeys = new List<string>();
            foreach (var resource in _filteredResources)
            {
                if (!alphaKeys.Contains(resource.AlphaKey))
                {
                    alphaKeys.Add(resource.AlphaKey);
                }
            }

            alphaKeys.Sort();

            return alphaKeys;
        }

        private IEnumerable<ResourceSummary> GetResources(int? specialtyId, int? publisherId, string authorLastName,
            string alpha, BrowseQuery browseQuery)
        {
            var resources = FilterResourcesBySpecialty(_filteredResources, specialtyId);
            resources = FilterResourcesByPublisher(resources, publisherId);
            resources = FilterResourcesByAuthorLastName(resources, authorLastName);
            resources = FilterResourcesByAlphaKey(resources, alpha);

            resources = OrderResources(resources, browseQuery.SortBy).ToList();

            var resourceSummaries = resources.ToResourceSummaries().ToList();
            foreach (var resourceSummary in resourceSummaries)
            {
                resourceSummary.Url = Url.Action("Title", "Resource", new { resourceSummary.Isbn });
            }

            if (AuthenticatedInstitution != null)
            {
                foreach (var resourceSummary in resourceSummaries)
                {
                    var institutionResourceLicense = AuthenticatedInstitution.GetResourceLicense(resourceSummary.Id);
                    resourceSummary.IsFullTextAvailable =
                        _resourceAccessService.IsFullTextAvailable(resourceSummary.Id);

                    if (institutionResourceLicense != null)
                    {
                        resourceSummary.LicenseCount = institutionResourceLicense.LicenseCount;
                        resourceSummary.SetShowLicenseCount(AuthenticatedInstitution);
                    }
                }
            }

            if (browseQuery.PageSize <= 0)
            {
                browseQuery.PageSize = _clientSettings.BrowsePageSize;
            }

            var startIndex = (browseQuery.Page - 1) * browseQuery.PageSize;

            return browseQuery.ShowAll
                ? resourceSummaries
                : resourceSummaries.Skip(startIndex).Take(browseQuery.PageSize);
        }

        private static int GetId(string s)
        {
            int id;
            int.TryParse(s, out id);

            return id;
        }

        private IList<IResource> FilterResourcesBySpecialty(IList<IResource> resources, int? specialtyId)
        {
            var filteredResources = new List<IResource>();
            if (specialtyId.HasValue)
            {
                var id = specialtyId.Value;
                foreach (var resource in resources)
                {
                    if (resource.Specialties.Any(x => x.Id == id))
                    {
                        filteredResources.Add(resource);
                    }
                }

                return filteredResources;
            }

            return resources;
        }

        private IList<IResource> FilterResourcesByPublisher(IList<IResource> resources, int? publisherId)
        {
            var filteredResources = new List<IResource>();
            if (publisherId.HasValue)
            {
                IList<IPublisher> publishers = _publisherService.GetPublishers().ToList();
                var publisher = publishers.SingleOrDefault(x => x.Id == publisherId);

                if (publisher != null)
                {
                    _log.DebugFormat("FilterResourcesByPublisher() - , publisher.Id: {0}, publisher.Name: {1}",
                        publisher.Id, publisher.Name);
                }
                else
                {
                    _log.Debug("FilterResourcesByPublisher() - publisher is null");
                }


                var id = publisherId.Value;

                IList<int> consolidatedPublisherIds = (from pub in publishers
                    where pub.ConsolidatedPublisher != null && pub.ConsolidatedPublisher.Id == id
                    select pub.Id).ToList();


                _log.DebugFormat("FilterResourcesByPublisher() - publisherId.Value: {0}", id);
                _log.DebugFormat("FilterResourcesByPublisher() - consolidatedPublisherIds: {0}",
                    string.Join(",", consolidatedPublisherIds));
                foreach (var resource in resources)
                {
                    if (resource.PublisherId == id || consolidatedPublisherIds.Contains(resource.PublisherId))
                    {
                        filteredResources.Add(resource);
                    }
                }

                return filteredResources;
            }

            return resources;
        }

        private IList<IResource> FilterResourcesByAuthorLastName(IList<IResource> resources, string authorLastName)
        {
            var filteredResources = new List<IResource>();
            if (!string.IsNullOrWhiteSpace(authorLastName))
            {
                foreach (var resource in resources)
                {
                    if (resource.AuthorList.Any(author => author.LastName == authorLastName && author.Order == 1))
                    {
                        filteredResources.Add(resource);
                    }
                }

                return filteredResources;
            }

            return resources;
        }

        private IList<IResource> FilterResourcesByAlphaKey(IList<IResource> resources, string alphaKey)
        {
            var filteredResources = new List<IResource>();
            if (!string.IsNullOrWhiteSpace(alphaKey) && alphaKey != "All")
            {
                foreach (var resource in resources)
                {
                    if (resource.AlphaKey == alphaKey || (alphaKey == "09" && !char.IsLetter(resource.AlphaKey[0])))
                    {
                        filteredResources.Add(resource);
                    }
                }

                return filteredResources;
            }

            return resources;
        }

        public static IEnumerable<IResource> OrderResources(IList<IResource> resources, string sortOrder)
        {
            switch (sortOrder)
            {
                case "author":
                    return resources.OrderBy(x => x.Authors);

                case "publication-date":
                    return resources.OrderByDescending(x => x.PublicationDate);

                case "publisher":
                    return resources.OrderBy(x => x.Publisher.Name);

                case "release-date":
                    return resources.OrderBy(x => x.ReleaseDate);

                default:
                    return resources.OrderBy(x => x.SortTitle);
            }
        }

        private IEnumerable<AuthorSummary> GetAuthorSummariesFilteredResources()
        {
            _log.DebugFormat("GetAuthorSummariesFilteredResources() - _filteredPrimaryAuthors.Count(): {0}",
                _filteredPrimaryAuthors.Count());
            var authorCounts = new SortedDictionary<string, AuthorSummary>();

            foreach (var author in _filteredPrimaryAuthors)
            {
                if (authorCounts.ContainsKey(author.LastName))
                {
                    var authorSummary = authorCounts[author.LastName];
                    authorSummary.ResourceCount++;
                }
                else
                {
                    var authorSummary = new AuthorSummary { Name = author.LastName, ResourceCount = 1 };
                    authorCounts.Add(author.LastName, authorSummary);
                }
            }

            return authorCounts.Values;
        }

        private List<IAuthor> GetPrimaryAuthors()
        {
            _log.DebugFormat("GetPrimaryAuthors() - filteredResources.Count(): {0}", _filteredResources.Count());
            var primaryAuthors = new List<IAuthor>();

            foreach (var resource in _filteredResources)
            {
                primaryAuthors.AddRange(resource.AuthorList.Where(author =>
                    !string.IsNullOrWhiteSpace(author.LastName) && author.Order == 1));
            }

            return primaryAuthors;
        }

        private SortedDictionary<string, PublisherSummary> GetFilteredSortedPublishers()
        {
            var sortedPublishers = new SortedDictionary<string, PublisherSummary>();

            IList<IPublisher> publishers = _publisherService.GetPublishers().ToList();

            // Get a list of publisher ids and counts for the publishers the institution has access to
            foreach (var resource in _filteredResources)
            {
                var publisherId = resource.Publisher.ConsolidatedPublisher == null
                    ? resource.PublisherId
                    : resource.Publisher.ConsolidatedPublisher.Id;
                var publisher = publishers.SingleOrDefault(x => x.Id == publisherId);
                if (publisher != null)
                {
                    if (sortedPublishers.ContainsKey(publisher.Name))
                    {
                        var publisherSummary = sortedPublishers[publisher.Name];
                        publisherSummary.ResourceCount++;
                    }
                    else
                    {
                        var publisherSummary = new PublisherSummary
                            { Id = publisher.Id, Name = publisher.Name, ResourceCount = 1 };
                        sortedPublishers.Add(publisher.Name, publisherSummary);
                    }
                }
            }

            return sortedPublishers;
        }
    }
}