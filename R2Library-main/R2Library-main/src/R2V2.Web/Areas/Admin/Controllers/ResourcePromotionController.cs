#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Authentication;
using R2V2.Core.Promotion;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.ResourcePromotion;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Models;
using Resource = R2V2.Web.Areas.Admin.Models.Resource.Resource;

#endregion


namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class ResourcePromotionController : R2AdminBaseController
    {
        private readonly ILog<ResourcePromotionController> _log;
        private readonly ResourcePromotionService _promotionService;
        private readonly IResourceService _resourceService;
        private readonly UserService _userService;

        public ResourcePromotionController(ILog<ResourcePromotionController> log
            , IAuthenticationContext authenticationContext
            , ResourcePromotionService promotionService
            , IResourceService resourceService
            , UserService userService
        )
            : base(authenticationContext)
        {
            _log = log;
            _promotionService = promotionService;
            _resourceService = resourceService;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult Queue()
        {
            var model = new ResourcePromotionViewModel
            {
                Resources = GetPromoteQueueResource(),
                BatchName = $"Batch {DateTime.Now:yyyy.MM.dd}"
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Queue(string batchName)
        {
            _log.DebugFormat("batchName: {0}", batchName);
            var errorMessage = "Invalid promotion name";
            if (ModelState.IsValid)
            {
                if (_promotionService.IsBatchNameUnique(batchName))
                {
                    if (_promotionService.InitialBatchPromotion(batchName, CurrentUser.Id))
                    {
                        return RedirectToAction("History");
                    }

                    errorMessage = "Promotion failed, please try again.";
                }
                else
                {
                    errorMessage = "'Promotion Batch Name' has already need used, please enter a unique name";
                }
            }

            var model = new ResourcePromotionViewModel
            {
                Resources = GetPromoteQueueResource(),
                BatchName = batchName,
                ErrorMessage = errorMessage
            };
            return View("Queue", model);
        }

        public ActionResult Remove(int resourceId)
        {
            _promotionService.RemoveResourceFromQueue(resourceId);
            var model = new ResourcePromotionViewModel
            {
                Resources = GetPromoteQueueResource()
            };
            return View("Queue", model);
        }

        public ActionResult History(int page = 1, int pageSize = 25)
        {
            var total = _promotionService.GetResourcePromoteQueueSize();
            var pageCount = total / pageSize + (total % pageSize > 0 ? 1 : 0);

            var lastPage = 0;
            var firstPage = 0;

            SetFirstLastPage(pageCount, page, ref firstPage, ref lastPage);

            var model = new ResourcePromotionViewModel
            {
                Resources = GetPromoteHistoryResource(page, pageSize),
                PreviousLink = Url.PreviousPageLink("History", "ResourcePromotion", page, pageCount, pageSize),
                NextLink = Url.NextPageLink("History", "ResourcePromotion", page, pageCount, pageSize),
                FirstLink = Url.FirstPageLink("History", "ResourcePromotion", page, pageCount, pageSize),
                LastLink = Url.LastPageLink("History", "ResourcePromotion", page, pageCount, pageSize),
                PageLinks = GetPageLinks(firstPage, lastPage, page),
                Page = page,
                PageSize = pageSize
            };


            return View(model);
        }

        private static void SetFirstLastPage(int pageCount, int currentPage, ref int firstPage, ref int lastPage)
        {
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
        }

        private static IEnumerable<PageLink> GetPageLinks(int firstPage, int lastPage, int currentPage)
        {
            for (var p = firstPage; p <= lastPage; p++)
            {
                yield return new PageLink
                    { Selected = p == currentPage, Text = p.ToString(CultureInfo.InvariantCulture) };
            }
        }

        private List<PromoteQueueResource> GetPromoteQueueResource()
        {
            var resources = _promotionService.GetResourcePromoteQueue();

            var users = _userService.GetRaUsersWhoCanPromote().ToList();

            var promoteQueueResources = new List<PromoteQueueResource>();

            foreach (var resourcePromoteQueue in resources)
            {
                var resource = new Resource(_resourceService.GetResource(resourcePromoteQueue.ResourceId));

                var promoteQueueResource = new PromoteQueueResource
                {
                    ResourcePromoteQueue = resourcePromoteQueue,
                    Resource = resource,
                    AddedByUserFullName = GetUserFullName(users, resourcePromoteQueue.AddedByUserId),
                    PromotedByUserFullName = GetUserFullName(users,
                        resourcePromoteQueue.PromotedByUserId != null ? resourcePromoteQueue.PromotedByUserId.Value : 0)
                };
                promoteQueueResources.Add(promoteQueueResource);
            }

            return promoteQueueResources;
        }

        private List<PromoteQueueResource> GetPromoteHistoryResource(int page, int pageSize)
        {
            var resources = _promotionService.GetResourcePromoteQueueHistory(page, pageSize);

            var users = _userService.GetRaUsersWhoCanPromote().ToList();

            var promoteQueueResources = new List<PromoteQueueResource>();

            foreach (var resourcePromoteQueue in resources)
            {
                var resource = new Resource(_resourceService.GetResource(resourcePromoteQueue.ResourceId));

                var promoteQueueResource = new PromoteQueueResource
                {
                    ResourcePromoteQueue = resourcePromoteQueue,
                    Resource = resource,
                    AddedByUserFullName = GetUserFullName(users, resourcePromoteQueue.AddedByUserId),
                    PromotedByUserFullName = GetUserFullName(users,
                        resourcePromoteQueue.PromotedByUserId != null ? resourcePromoteQueue.PromotedByUserId.Value : 0)
                };
                promoteQueueResources.Add(promoteQueueResource);
            }

            return promoteQueueResources;
        }

        private string GetUserFullName(IEnumerable<User> users, int userId)
        {
            var user = users.FirstOrDefault(x => x.Id == userId);
            return user == null ? $"User Id: {userId}" : user.ToFullName();
        }
    }
}