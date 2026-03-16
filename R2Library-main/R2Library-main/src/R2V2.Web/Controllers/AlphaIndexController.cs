#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Resource.Topic;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Controllers.SuperTypes;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.AlphaIndex;
using R2V2.Web.Models.Resource;
using R2V2.Web.Models.Shared;
using Filter = R2V2.Web.Models.Shared.Filter;

#endregion

namespace R2V2.Web.Controllers
{
    [RequestLoggerFilter(false)]
    public class AlphaIndexController : R2BaseController
    {
        private readonly InstitutionService _institutionService;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<AlphaIndexController> _log;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly IResourcesByInstitutionService _resourcesByInstitutionService;
        private readonly ISpecialtyService _specialtyService;
        private readonly TopicService _topicService;

        public AlphaIndexController(ILog<AlphaIndexController> log
            , IAuthenticationContext authenticationContext
            , ISpecialtyService specialtyService
            , IInstitutionSettings institutionSettings
            , IPracticeAreaService practiceAreaService
            , IResourcesByInstitutionService resourcesByInstitutionService
            , TopicService topicService
            , InstitutionService institutionService
        )
            : base(authenticationContext)
        {
            _log = log;
            _specialtyService = specialtyService;
            _institutionSettings = institutionSettings;
            _practiceAreaService = practiceAreaService;
            _resourcesByInstitutionService = resourcesByInstitutionService;
            _topicService = topicService;
            _institutionService = institutionService;
        }

        [RequestLoggerFilter(false)]
        public ActionResult Index()
        {
            var stopwatch = new Stopwatch();

            // limit resources based on query
            var institutionsResources = GetResourceForInstitution(0);

            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().OrderBy(x => x.SequenceNumber).ToList();

            const int specialtyId = 0;
            var specialtySummaries = GetSpecialtySummaries(institutionsResources, specialtyId);
            stopwatch.Stop();
            _log.DebugFormat("GetSpecialtySummaries() - {0} ms", stopwatch.ElapsedMilliseconds);

            var alphaIndex = new AlphaIndex
            {
                PracticeAreas = practiceAreas,
                SpecialtySummaries = specialtySummaries
            };

            stopwatch.Stop();
            _log.DebugFormat("Index() - {0} ms", stopwatch.ElapsedMilliseconds);

            return View(alphaIndex);
        }

        [RequestLoggerFilter(true, false)]
        public ActionResult JsonResults(AlphaQuery alphaQuery)
        {
            _log.InfoFormat("JsonResults() - Alpha: {0}, Disciplines: {1}, PracticeArea: {2}, Show: {3}",
                alphaQuery.Alpha, alphaQuery.Disciplines, alphaQuery.PracticeArea, alphaQuery.Show);

            var json = new FacetedResultsJson();

            try
            {
                // limit resources based on query
                var institutionsResources = GetResourceForInstitution(alphaQuery.PracticeAreaId);

                var specialtyFilters = GetSpecialtyFilters(institutionsResources);
                json.FilterGroups.Add("disciplines",
                    new FilterGroup { Code = "disciplines", Name = "Discipline", Filters = specialtyFilters });

                var practiceAreaFilters = GetPracticeAreaFilters(institutionsResources, alphaQuery);
                json.FilterGroups.Add("practice-area",
                    new FilterGroup { Code = "practice-area", Name = "Practice Area", Filters = practiceAreaFilters });

                if (string.IsNullOrWhiteSpace(alphaQuery.Alpha))
                {
                    alphaQuery.Alpha = "A";
                }

                var allAlphaKeys = _topicService.GetAllAlphaKeys();
                var pageLinks = PaginationHelper.GetAlphaPageLinks(allAlphaKeys, alphaQuery.Alpha, false);
                var topicList = GetTopics(alphaQuery);

                var topics = new Topics { AlphaQuery = alphaQuery, PageLinks = pageLinks, TopicList = topicList };

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                json.HtmlSnippets.Add("results", RenderPartialViewToString("_Topics", topics));
                stopwatch.Stop();
                _log.InfoFormat("JsonResults() - RenderPartialViewToString {0} ms", stopwatch.ElapsedMilliseconds);

                json.Successful = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json.ErrorMessage = "We are sorry, an error occurred while your were browsing. Please try again.";
                json.Successful = false;
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private Dictionary<string, Filter> GetPracticeAreaFilters(IEnumerable<IResource> institutionsResources,
            AlphaQuery alphaQuery)
        {
            // get practice areas
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas();
            var filters = practiceAreas.ToDictionary(practiceArea => $"{practiceArea.Id}",
                practiceArea => new Filter { Count = "0", Name = practiceArea.Name });

            var selectedPracticeAreaId = GetId(alphaQuery.PracticeArea);

            // get all the practice areas with counts
            var practiceAreaCounts = new Dictionary<int, int>();
            foreach (var filteredResource in institutionsResources)
            {
                var practiceAreaIds = filteredResource.GetDistinctPracticeAreaIds();
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

            return filters;
        }

        private Dictionary<string, Filter> GetSpecialtyFilters(IEnumerable<IResource> institutionsResources)
        {
            var sortedSpecialtySummeries = GetSpecialties(institutionsResources);

            var filters = new Dictionary<string, Filter>();

            foreach (var specialtySummary in sortedSpecialtySummeries.Values)
            {
                filters[$"{specialtySummary.Id}"] = new Filter
                {
                    Name = specialtySummary.Name, Count =
                        $"{specialtySummary.ResourceCount}"
                };
            }

            return filters;
        }

        private IEnumerable<Topic> GetTopics(AlphaQuery alphaQuery)
        {
            var institutionId = AuthenticatedInstitution == null || AuthenticatedInstitution.Id <= 0
                ? _institutionService.GetGuestInstitutionId(_institutionSettings.GuestAccountNumber)
                : AuthenticatedInstitution.Id;

            //DisplayType displayType = GetDisplayType(alphaQuery.Show);
            var practiceAreaId = GetId(alphaQuery.PracticeArea);
            var specialtyId = GetId(alphaQuery.Disciplines);


            var topics = _topicService.GetTopics(institutionId, alphaQuery.Alpha, DisplayAllProducts(),
                practiceAreaId, specialtyId,
                GetAtoZIndexType(alphaQuery.Show));

            return topics.Select(topic => new Topic { Name = topic });
        }

        private bool DisplayAllProducts()
        {
            if (AuthenticatedInstitution != null)
            {
                if (AuthenticatedInstitution.IsPublisherUser())
                {
                    return true;
                }

                if (AuthenticatedInstitution.AccountStatus == InstitutionAccountStatus.Trial)
                {
                    return true;
                }

                return AuthenticatedInstitution.DisplayAllProducts;
            }

            return true;
        }

        private static int GetId(string s)
        {
            int id;
            int.TryParse(s, out id);

            return id;
        }


        private IList<IResource> GetResourceForInstitution(int practiceAreaId)
        {
            // get all the resource an institution has access to
            var resources =
                _resourcesByInstitutionService.GetResourcesForActiveInstitution(_institutionSettings.GuestAccountNumber,
                    DisplayAllProducts());
            _log.DebugFormat("resources.Count: {0}", resources.Count());

            var filteredResources = new List<IResource>();

            foreach (var resource in resources)
            {
                // filter by active/archive (include)
                if (resource.StatusId != (int)ResourceStatus.Active &&
                    resource.StatusId != (int)ResourceStatus.Archived)
                {
                    continue;
                }

                if (practiceAreaId > 0)
                {
                    var practiceArea = resource.PracticeAreas.FirstOrDefault(x => x.Id == practiceAreaId);
                    if (practiceArea == null)
                    {
                        continue;
                    }
                }

                filteredResources.Add(resource);
            }

            _log.DebugFormat("filteredResources.Count: {0}", filteredResources.Count());
            return filteredResources;
        }


        private SpecialtySummaries GetSpecialtySummaries(IEnumerable<IResource> institutionsResources,
            int selectedSpecialtyId)
        {
            var sortedSpecialtySummeries = GetSpecialties(institutionsResources);
            return new SpecialtySummaries
                { Specialties = sortedSpecialtySummeries.Values, SelectedSpecialtyId = selectedSpecialtyId };
        }

        private SortedDictionary<string, SpecialtySummary> GetSpecialties(IEnumerable<IResource> institutionsResources)
        {
            var sortedSpecialtySummeries = new SortedDictionary<string, SpecialtySummary>();

            var specialties = _specialtyService.GetAllSpecialties();
            foreach (var specialtySummary in specialties.Select(specialty => new SpecialtySummary
                         { Id = specialty.Id, Name = specialty.Name, ResourceCount = 0 }))
            {
                if (sortedSpecialtySummeries.ContainsKey(specialtySummary.Name))
                {
                    _log.WarnFormat("Duplicate Specialty Name found: {0}", specialtySummary.Name);
                    continue;
                }
                sortedSpecialtySummeries.Add(specialtySummary.Name, specialtySummary);
            }

            foreach (var filteredResource in institutionsResources)
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
                        _log.WarnFormat("Specialty Name not found: {0}", specialty.Name);
                    }
                }
            }

            return sortedSpecialtySummeries;
        }

        private static AtoZIndexType GetAtoZIndexType(string type)
        {
            switch (type)
            {
                case "drug-names":
                    return AtoZIndexType.Drug;

                case "diseases":
                    return AtoZIndexType.Disease;

                case "azIndex":
                    return AtoZIndexType.Keyword;

                // case "all":
                default:
                    return AtoZIndexType.All;
            }
        }
    }
}