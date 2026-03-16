#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Promotion;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Resource;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Services;
using Resource = R2V2.Core.Resource.Resource;
using ResourceDetail = R2V2.Web.Areas.Admin.Models.Resource.ResourceDetail;
using UserService = R2V2.Core.UserService;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class ResourceController : R2AdminBaseController
    {
        private readonly ICartService _cartService;
        private readonly ICollectionService _collectionService;
        private readonly EmailSiteService _emailService;
        private readonly IFeaturedTitleService _featuredTitleService;
        private readonly ILog<ResourceController> _log;
        private readonly PdaRuleService _pdaRuleService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ResourcePromotionService _promotionService;
        private readonly PublisherService _publisherService;
        private readonly RecentCookieService _recentCookieService;
        private readonly ResourceAdminService _resourceAdminService;
        private readonly ResourceListService _resourceListService;
        private readonly IResourceService _resourceService;
        private readonly ISearchService _searchService;
        private readonly SpecialDiscountResourceService _specialDiscountResourceService;
        private readonly ISpecialtyService _specialtyService;
        private readonly UserService _userService;
        private readonly IWebImageSettings _webImageSettings;
        private readonly IWebSettings _webSettings;

        List<IFeaturedTitle> _featuredTitles;

        public ResourceController(ILog<ResourceController> log
            , IAuthenticationContext authenticationContext
            , IResourceService resourceService
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , PublisherService publisherService
            , IWebImageSettings webImageSettings
            , EmailSiteService emailService
            , IWebSettings webSettings
            , ResourcePromotionService promotionService
            , ResourceAdminService resourceAdminService
            , IFeaturedTitleService featuredTitleService
            , ICartService cartService
            , SpecialDiscountResourceService specialDiscountResourceService
            , RecentCookieService recentCookieService
            , ICollectionService collectionService
            , PdaRuleService pdaRuleService
            , ResourceListService resourceListService
            , UserService userService
            , ISearchService searchService
        )
            : base(authenticationContext)
        {
            _log = log;
            _resourceService = resourceService;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _publisherService = publisherService;
            _webImageSettings = webImageSettings;
            _emailService = emailService;
            _webSettings = webSettings;
            _promotionService = promotionService;
            _resourceAdminService = resourceAdminService;
            _featuredTitleService = featuredTitleService;
            _cartService = cartService;
            _specialDiscountResourceService = specialDiscountResourceService;
            _recentCookieService = recentCookieService;
            _collectionService = collectionService;
            _pdaRuleService = pdaRuleService;
            _resourceListService = resourceListService;
            _userService = userService;
            _searchService = searchService;
        }

        [HttpGet]
        public ActionResult List(ResourceQuery resourceQuery)
        {
            _log.DebugFormat(
                "resourceQuery = [ResourceStatus: {0}, ResourceFilterType: {1}, Query: {0}, Page: {0}, PageSize: {0}, SortBy: {0}, SortDirection: {0}]",
                resourceQuery.ResourceStatus, resourceQuery.ResourceFilterType, resourceQuery.Query, resourceQuery.Page,
                resourceQuery.PageSize, resourceQuery.SortBy, resourceQuery.SortDirection);

            var model = GetResourcesList(resourceQuery);

            model.ToolLinks = GetToolLinks(true, Url.Action("Export", model.ResourceQuery.ToRouteValues(true)));

            model.DisplayPromotionFields = _webSettings.DisplayPromotionFields && CurrentUser.EnablePromotion != null &&
                                           CurrentUser.EnablePromotion.Value > 0;

            return View(model);
        }

        [HttpPost]
        public ActionResult List(ResourceQuery resourceQuery, EmailPage emailPage)
        {
            if (emailPage.To == null)
            {
                return RedirectToAction("List", resourceQuery.ToRouteValues());
            }

            var resources = GetResourcesList(resourceQuery);

            var messageBody = RenderRazorViewToString("Resource", "_List", resources);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ResourcesList GetResourcesList(ResourceQuery resourceQuery)
        {
            var resources = GetResources(resourceQuery);
            var model = _resourceListService.GetResourcesList(resourceQuery, resources.ToList(), _featuredTitles, Url);
            return model;
        }

        private IEnumerable<IResource> GetResources(ResourceQuery resourceQuery)
        {
            var resources = _searchService.SearchAdmin(resourceQuery.Query, null);
            if (resources == null || !resources.Any())
            {
                resources = GetResourcesByIsbn(resourceQuery.Query);
            }

            IEnumerable<IResource> filteredResources =
                _resourceService.GetResources(
                    resources
                    , resourceQuery
                    , _publisherService.GetFeaturedPublisher()
                    , _featuredTitleService.GetFeaturedTitles()
                    , true
                    , resourceQuery.RecentOnly ? _recentCookieService.GetRecentResourceIds(Request) : null
                ).ToList();


            if (resourceQuery.ResourceFilterType == ResourceFilterType.FeaturedTitles)
            {
                _featuredTitles = _featuredTitleService.GetFeaturedTitles().ToList();
                if (_featuredTitles.Any())
                {
                    filteredResources =
                        filteredResources.Where(x => _featuredTitles.Select(ft => ft.ResourceId).Contains(x.Id));
                }
            }

            return filteredResources;
        }

        private List<IResource> GetResourcesByIsbn(string query)
        {
            var resources = _resourceService.GetAllResources();
            return resources.Where(x => x.Isbn == query || x.Isbn10 == query || x.Isbn13 == query || x.EIsbn == query)
                .ToList();
        }

        public ActionResult Export(ResourceQuery resourceQuery, string export)
        {
            var resources = GetResources(resourceQuery);

            var excelExport = new ResourceListExcelExport(resources,
                Url.Action("Title", "Resource", new { Area = "" }, _webSettings.RequireSsl ? "https" : "http"), false);
            var fileDownloadName = $"R2-ResourceList-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        public ActionResult Detail(ResourceQuery resourceQuery)
        {
            IResource resource = _resourceService.GetResourceForEdit(resourceQuery.ResourceId);
            if (resource == null || resource.Id == 0)
            {
                return RedirectToAction("List", resourceQuery.ToRouteValues());
            }

            var featuredTitle = _featuredTitleService.GetFeaturedTitle(resourceQuery.ResourceId);
            var specialsResource = _specialDiscountResourceService.GetResourceSpecial(resource.Id);

            _recentCookieService.SetRecentResourcesCookie(resource.Id, Response, Request);

            var resourcePromoteQueues = _promotionService.GetResourcePromoteQueue();
            var raPromotionUsers = _userService.GetRaUsersWhoCanPromote().ToList();
            var model = new ResourceDetail(resource, featuredTitle, resourceQuery, _webSettings, CurrentUser,
                specialsResource, resourcePromoteQueues, raPromotionUsers);

            // get promotion queue status if not defined
            if (string.IsNullOrEmpty(model.Resource.PromotionQueueStatus))
            {
                var resourcePromoteQueue = _promotionService.GetLatestResourcePromoteQueue(resource.Id);
                if (resourcePromoteQueue != null)
                {
                    model.Resource.PromotionQueueStatus = resourcePromoteQueue.PromoteStatus.ToString();
                    if (model.Resource.LastPromotionDate == null ||
                        (resourcePromoteQueue.LastUpdated != null &&
                         resourcePromoteQueue.LastUpdated.Value > model.Resource.LastPromotionDate.Value))
                    {
                        model.Resource.LastPromotionDate = resourcePromoteQueue.LastUpdated;
                    }
                }
            }

            return View(model);
        }


        public ActionResult Edit(ResourceQuery resourceQuery)
        {
            _log.DebugFormat(
                "resourceQuery: {0}, sort: {1}, page: {2}, ResourceStatus: {3}, ResourceFilterType: {4}, pageSize: {5}, resourceId: {6}",
                resourceQuery.Query, resourceQuery.SortBy, resourceQuery.Page, resourceQuery.ResourceStatus,
                resourceQuery.ResourceFilterType, resourceQuery.PageSize, resourceQuery.ResourceId);

            IResource resource = _resourceService.GetResourceForEdit(resourceQuery.ResourceId);
            var featuredTitle = _featuredTitleService.GetFeaturedTitle(resourceQuery.ResourceId);

            var specialsResource = _specialDiscountResourceService.GetResourceSpecials(resourceQuery.ResourceId);

            var specialAdminModels = _specialDiscountResourceService.GetAvailableAdminSpecials();

            // todo: need to handle null resource!
            var model = new ResourceEdit(resource, featuredTitle,
                _publisherService.GetActivePublishers(resource.StatusId),
                _publisherService.GetPublisher(resource.PublisherId), _specialtyService.GetAllSpecialties()
                , _practiceAreaService.GetAllPracticeAreas(), _collectionService.GetAllCollections(), resourceQuery,
                _webSettings, CurrentUser
                , specialsResource, specialAdminModels, null, null);

            model.SetBookCoverLimits(_webImageSettings);

            return View(model);
        }

        /// <param name="file"> </param>
        [HttpPost]
        public ActionResult Edit(ResourceEdit resourceEdit, HttpPostedFileBase file)
        {
            _log.DebugFormat(
                "resourceQuery: {0}, sort: {1}, page: {2}, ResourceStatus: {3}, ResourceFilterType: {4}, pageSize: {5}, resourceId: {6}",
                resourceEdit.ResourceQuery.Query, resourceEdit.ResourceQuery.SortBy, resourceEdit.ResourceQuery.Page,
                resourceEdit.ResourceQuery.ResourceStatus,
                resourceEdit.ResourceQuery.ResourceFilterType, resourceEdit.ResourceQuery.PageSize,
                resourceEdit.ResourceQuery.ResourceId);

            RemoveModelStateItems();

            _log.DebugFormat("IsValid: {0}", ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                RebuildResourceEdit(resourceEdit);
                return View(resourceEdit);
            }

            //Cover Image check and save
            foreach (var error in _resourceAdminService.SaveBookCoverImage(resourceEdit, file))
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            if (!ModelState.IsValid)
            {
                RebuildResourceEdit(resourceEdit);
                return View(resourceEdit);
            }

            //Resource check and save
            foreach (var error in _resourceAdminService.ValidateAndSaveResource(resourceEdit, CurrentUser,
                         AuthenticatedInstitution))
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            if (!ModelState.IsValid)
            {
                RebuildResourceEdit(resourceEdit);
                return View(resourceEdit);
            }

            _resourceService.GetAllResources(true);
            _publisherService.ClearPublisherCache();

            return RedirectToAction("Detail", resourceEdit.ResourceQuery);
        }

        public ActionResult Add()
        {
            IResource resource = new Resource { StatusId = 8 };
            var specialAdminModels = _specialDiscountResourceService.GetAvailableAdminSpecials();

            // todo: need to handle null resouce!
            var model = new ResourceEdit(resource, null
                , _publisherService.GetActivePublishers(resource.StatusId),
                _publisherService.GetPublisher(resource.PublisherId)
                , _specialtyService.GetAllSpecialties()
                , _practiceAreaService.GetAllPracticeAreas()
                , _collectionService.GetAllCollections()
                , new ResourceQuery(), _webSettings, CurrentUser, null, specialAdminModels, null, null);

            model.SetBookCoverLimits(_webImageSettings);

            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Add(ResourceEdit resourceEdit, HttpPostedFileBase file)
        {
            RemoveModelStateItems();

            foreach (var error in _resourceAdminService.ValidateAndSaveResource(resourceEdit, CurrentUser,
                         AuthenticatedInstitution))
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            if (ModelState.IsValid)
            {
                foreach (var error in _resourceAdminService.SaveBookCoverImage(resourceEdit, file))
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            if (!ModelState.IsValid)
            {
                RebuildResourceEdit(resourceEdit);
                return View("Edit", resourceEdit);
            }

            //reload the cache with the new resource
            _resourceService.GetAllResources(true);
            _publisherService.ClearPublisherCache();

            return RedirectToAction("Detail", new ResourceQuery { ResourceId = resourceEdit.Resource.Id });
        }


        private void RebuildResourceEdit(ResourceEdit resourceEdit)
        {
            if (resourceEdit.Resource.Id == 0) // New Resource
            {
                var allSpecialties = _specialtyService.GetAllSpecialties().ToList();
                var allPracticeAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
                var allCollections = _collectionService.GetAllCollections().ToList();
                var allAdminSpecials = _specialDiscountResourceService.GetAvailableAdminSpecials().ToList();

                if (resourceEdit.SpecialtiesSelected != null && resourceEdit.SpecialtiesSelected.Any())
                {
                    resourceEdit.Resource.Specialties =
                        allSpecialties.Where(x => resourceEdit.SpecialtiesSelected.Contains(x.Id));
                }

                if (resourceEdit.PracticeAreaSelected != null && resourceEdit.PracticeAreaSelected.Any())
                {
                    resourceEdit.Resource.PracticeAreas =
                        allPracticeAreas.Where(x => resourceEdit.PracticeAreaSelected.Contains(x.Id));
                }

                if (resourceEdit.CollectionsSelected != null && resourceEdit.CollectionsSelected.Any())
                {
                    resourceEdit.Resource.Collections =
                        allCollections.Where(x => resourceEdit.CollectionsSelected.Contains(x.Id));
                }

                resourceEdit.PopulatePublishersSelectList(
                    _publisherService.GetActivePublishers(resourceEdit.Resource.StatusId),
                    _publisherService.GetPublisher(resourceEdit.Resource.PublisherId));
                resourceEdit.PopulateSpecialtiesSelectListItems(allSpecialties.ToList());
                resourceEdit.PopulatePracticeAreaSelectListItems(allPracticeAreas.ToList());
                resourceEdit.PopulateCollectionsSelectListItems(allCollections.ToList());
                resourceEdit.PopulateSpecialsSelectListItems(null, allAdminSpecials);
                resourceEdit.QaApproval = resourceEdit.Resource != null && resourceEdit.Resource.QaApprovalDate != null;
            }
            else
            {
                var resourceQuery = resourceEdit.ResourceQuery;
                var currentSpecials = _specialDiscountResourceService.GetResourceSpecials(resourceQuery.ResourceId);
                var availableSpecials = _specialDiscountResourceService.GetAvailableAdminSpecials();

                var featuredTitle = _featuredTitleService.GetFeaturedTitle(resourceQuery.ResourceId);
                var resource = _resourceService.GetResourceForEdit(resourceEdit.Resource.Id);

                resourceEdit.Init(resource, featuredTitle, _publisherService.GetActivePublishers(resource.StatusId),
                    resource.Publisher,
                    _specialtyService.GetAllSpecialties(), _practiceAreaService.GetAllPracticeAreas(),
                    _collectionService.GetAllCollections(), resourceQuery, _webSettings, CurrentUser
                    , currentSpecials, availableSpecials, null, null);
            }
        }

        private void RemoveModelStateItems()
            //private void RemoveModelStateItems(ResourceEdit resourceEdit)
        {
            ModelState.Remove("CollectionsSelectListItems");
            ModelState.Remove("SpecialsSelectListItems");
            ModelState.Remove("SpecialtiesSelectListItems");
            ModelState.Remove("PracticeAreaSelectListItems");
            //ModelState.Remove("Resource.Title");
            //ModelState.Remove("Resource.Authors");
        }

        public ActionResult Delete(ResourceQuery resourceQuery)
        {
            _log.DebugFormat("Deleting resource id: {0}", resourceQuery.ResourceId);
            var resource = _resourceService.GetResourceForEdit(resourceQuery.ResourceId);

            if (resource != null)
            {
                resource.StatusId = 72;
                _resourceService.DeleteResource(resource);
            }

            if (resourceQuery.ResourceId > 0)
            {
                var featuredTitle = _featuredTitleService.GetFeaturedTitleForEdit(resourceQuery.ResourceId);
                if (featuredTitle != null)
                {
                    _featuredTitleService.DeleteFeaturedTitle(featuredTitle);
                }
            }

            _resourceService.GetAllResources(true);
            // Need to Clear Cart Cache so Deleted Resources are not getting counting in institution carts.
            _cartService.RemoveCartsFromCache();

            return RedirectToAction("List", new ResourceQuery());
        }

        public ActionResult AddToPromoteQueue(ResourceQuery resourceQuery)
        {
            var resource = _resourceService.GetResource(resourceQuery.ResourceId);
            _promotionService.AddResourceToPromoteQueue(resource.Id, resource.Isbn, CurrentUser,
                PromotionType.Production);
            var model = GetResourceDetail(resourceQuery);
            return View("Detail", model);
            //return View("Promote", model);
        }

        public ActionResult RemoveFromPromoteQueue(ResourceQuery resourceQuery)
        {
            _promotionService.RemoveResourceFromQueue(resourceQuery.ResourceId);
            var model = GetResourceDetail(resourceQuery);
            return View("Detail", model);
        }

        /// <summary>
        ///     Disabled link for now - SJS - 12/18/2015
        /// </summary>
        public ActionResult OngoingPdaEvent(ResourceQuery resourceQuery, OngoingPdaEventType ongoingEventType)
        {
            var model = GetResourceDetail(resourceQuery);

            var id = _pdaRuleService.SendOngoingPdaEventToMessageQueue(model.Resource.Isbn, ongoingEventType);

            model.ActionMessage = $"Your ongoing PDA request is being processed, id: {id}";

            return View(model);
        }

        public ResourceDetail GetResourceDetail(ResourceQuery resourceQuery)
        {
            var resource = _resourceService.GetResource(resourceQuery.ResourceId);

            var featuredTitle = _featuredTitleService.GetFeaturedTitle(resourceQuery.ResourceId);

            var specialsResource = _specialDiscountResourceService.GetResourceSpecial(resource.Id);

            _recentCookieService.SetRecentResourcesCookie(resource.Id, Response, Request);

            var resourcePromoteQueues = _promotionService.GetResourcePromoteQueue();
            var raPromotionUsers = _userService.GetRaUsersWhoCanPromote().ToList();
            var model = new ResourceDetail(resource, featuredTitle, resourceQuery, _webSettings, CurrentUser,
                specialsResource, resourcePromoteQueues, raPromotionUsers);

            return model;
        }
    }
}