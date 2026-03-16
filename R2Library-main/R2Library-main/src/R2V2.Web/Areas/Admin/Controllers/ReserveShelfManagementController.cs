#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.ReserveShelfManagement;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Models;
using R2V2.Web.Models.Resource;
using ReserveShelfUrl = R2V2.Web.Areas.Admin.Models.ReserveShelfManagement.ReserveShelfUrl;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.ExpertReviewer })]
    public class ReserveShelfManagementController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly ILog<ReserveShelfManagementController> _log;

        private readonly IOrderService _orderService;
        private readonly ReserveShelfService _reserveShelfService;
        private readonly IResourceAccessService _resourceAccessService;

        private readonly IResourceService _resourceService;
        //
        // GET: /Admin/ReserveShelf/

        public ReserveShelfManagementController(
            IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , ILog<ReserveShelfManagementController> log
            , ReserveShelfService reserveShelfService
            , IResourceService resourceService
            , IResourceAccessService resourceAccessService
            , IOrderService orderService
        ) : base(authenticationContext)
        {
            _adminContext = adminContext;
            _log = log;
            _reserveShelfService = reserveShelfService;
            _resourceService = resourceService;
            _resourceAccessService = resourceAccessService;
            _orderService = orderService;
        }

        private int[] PdaResources { get; set; }

        public ActionResult List(int institutionId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var reserveShelfs = _reserveShelfService.GetInstititionsReserveShelves(institutionId);

            var reserveShelfListManagement =
                new ReserveShelfListManagement(institution, reserveShelfs.ToReserveShelves().ToList());

            return View(reserveShelfListManagement);
        }

        private void BuildPdaResources(IAdminInstitution institution)
        {
            PdaResources = (from license in institution.Licenses
                where license.LicenseType == LicenseType.Pda && license.FirstPurchaseDate == null
                select license.ResourceId).ToArray();
        }

        public ActionResult Detail(int institutionId, int reserveShelfId, string resourceTitleRemoved)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var reserveShelves = _reserveShelfService.GetInstititionsReserveShelves(institutionId);
            BuildPdaResources(institution);

            var editReserveShelf = reserveShelfId == 0
                ? new ReserveShelf()
                : reserveShelves.SingleOrDefault(x => x.Id == reserveShelfId);
            var reserveShelfList = editReserveShelf.ToReserveShelf();
            if (editReserveShelf != null && editReserveShelf.Id > 0)
            {
                var resourceIds = editReserveShelf.ReserveShelfResources.Select(x => x.ResourceId).ToArray();
                var resourceSummaries = new List<ResourceSummary>();

                var r = _resourceService.GetResourcesByIds(resourceIds)
                    .SortBy(editReserveShelf.DefaultSortBy, reserveShelfList.IsAscending);
                foreach (var resource in r)
                {
                    var summary = resource.ToResourceSummary();
                    var license = institution.Licenses.FirstOrDefault(x => x.ResourceId == resource.Id);
                    summary.Url = Url.Action("Title", "Resource", new { resource.Isbn });
                    summary.IsFullTextAvailable = _resourceAccessService.IsFullTextAvailable(license);
                    summary.IsPdaResource = license != null && license.LicenseType == LicenseType.Pda;
                    resourceSummaries.Add(summary);
                }

                reserveShelfList.Resources = resourceSummaries;
            }

            if (!string.IsNullOrWhiteSpace(resourceTitleRemoved))
            {
                reserveShelfList.ResourceTitleRemoved = resourceTitleRemoved;
            }

            var reserveShelfListManagement = new ReserveShelfListManagement(institution, reserveShelfList);

            return View(reserveShelfListManagement);
        }


        public ActionResult Edit(int institutionId, int reserveShelfId, string resourceTitleRemoved)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var reserveShelves = _reserveShelfService.GetInstititionsReserveShelves(institutionId);

            var editReserveShelf = reserveShelfId == 0
                ? new ReserveShelf()
                : reserveShelves.SingleOrDefault(x => x.Id == reserveShelfId);
            var reserveShelfList =
                PopulateReserveShelfList(editReserveShelf, institution); //editReserveShelf.ToReserveShelf();

            if (!string.IsNullOrWhiteSpace(resourceTitleRemoved))
            {
                reserveShelfList.ResourceTitleRemoved = resourceTitleRemoved;
            }

            var reserveShelfListManagement = new ReserveShelfListManagement(institution, reserveShelfList);

            return View(reserveShelfListManagement);
        }

        public ReserveShelfList PopulateReserveShelfList(ReserveShelf editReserveShelf, IAdminInstitution institution)
        {
            var reserveShelfList = editReserveShelf.ToReserveShelf();

            if (editReserveShelf != null && editReserveShelf.Id > 0)
            {
                var reserveShelfResources = editReserveShelf
                    .ReserveShelfResources
                    .Select(x => x.ResourceId)
                    .Select(r => _resourceService.GetResource(r).ToResourceSummary());

                var resources = reserveShelfResources.ToList();
                foreach (var resource in resources)
                {
                    resource.Url = Url.Action("Title", "Resource", new { resource.Isbn });
                }

                foreach (var resourceSummary in resources)
                {
                    resourceSummary.IsFullTextAvailable =
                        _resourceAccessService.IsFullTextAvailable(resourceSummary.Id);
                }

                reserveShelfList.Resources = resources;
                reserveShelfList.SelectedSortBy = editReserveShelf.DefaultSortBy;
            }

            return reserveShelfList;
        }

        [HttpPost]
        public ActionResult Edit(int institutionId, ReserveShelfList editReserveShelf)
        {
            if (editReserveShelf != null)
            {
                var reserveShelfId = _reserveShelfService.SaveUpdateReserveShelfList(editReserveShelf.Id, institutionId,
                    editReserveShelf.Name, editReserveShelf.Description, editReserveShelf.SelectedSortBy,
                    editReserveShelf.IsAscending);
                return RedirectToAction("Detail", new { institutionId, reserveShelfId, resourceTitleRemoved = "" });
            }

            return RedirectToAction("List", new { institutionId });
        }

        public ActionResult EditUrl(int institutionId, int reserveShelfId, int reserveShelfUrlId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var reserveShelves = _reserveShelfService.GetInstititionsReserveShelves(institutionId);

            var editReserveShelf = reserveShelves.SingleOrDefault(x => x.Id == reserveShelfId);

            var editReserveShelfUrl = new ReserveShelfUrl();
            if (reserveShelfUrlId == 0 && reserveShelfId > 0)
            {
                editReserveShelfUrl.ReserveShelfId = reserveShelfId;
            }
            else
            {
                var coreReserveShelfUrl = _reserveShelfService.GetReserveShelfUrl(reserveShelfUrlId, reserveShelfId);
                editReserveShelfUrl = new ReserveShelfUrl(coreReserveShelfUrl.Id, coreReserveShelfUrl.ReserveShelfId,
                    coreReserveShelfUrl.Url, coreReserveShelfUrl.Description);
            }

            var reserveShelfList = PopulateReserveShelfList(editReserveShelf, institution);

            var reserveShelfListManagement =
                new ReserveShelfListManagement(institution, reserveShelfList, editReserveShelfUrl);

            return View("Detail", reserveShelfListManagement);
        }

        [HttpPost]
        public ActionResult EditUrl(ReserveShelfListManagement reserveShelfListManagement, int reserveShelfId,
            int reserveShelfUrlId)
        {
            var reserveShelfUrl = reserveShelfListManagement.EditReserveShelfUrl;
            reserveShelfUrl.ReserveShelfId = reserveShelfId;
            reserveShelfUrl.ReserveShelfUrlId = reserveShelfUrlId;

            if (ModelState.IsValid)
            {
                reserveShelfUrl.ReserveShelfId = reserveShelfId;
                reserveShelfUrl.ReserveShelfUrlId = reserveShelfUrlId;

                //Handles Adding
                if (reserveShelfUrl.ReserveShelfUrlId == 0)
                {
                    _reserveShelfService.AddReserveShelfUrl(reserveShelfUrl.ReserveShelfId, reserveShelfUrl.Url,
                        reserveShelfUrl.Description);
                    return RedirectToAction("Detail",
                        new
                        {
                            reserveShelfListManagement.InstitutionId, reserveShelfUrl.ReserveShelfId,
                            resourceTitleRemoved = ""
                        });
                }

                _reserveShelfService.UpdateReserveShelfUrl(reserveShelfUrl.ReserveShelfId,
                    reserveShelfUrl.ReserveShelfUrlId, reserveShelfUrl.Url, reserveShelfUrl.Description);
                return RedirectToAction("Detail",
                    new
                    {
                        reserveShelfListManagement.InstitutionId, reserveShelfUrl.ReserveShelfId,
                        resourceTitleRemoved = ""
                    });
            }

            return EditUrl(reserveShelfListManagement.InstitutionId, reserveShelfUrl.ReserveShelfId,
                reserveShelfUrl.ReserveShelfUrlId);
        }

        public ActionResult DeleteExternalUrl(int institutionId, int reserveShelfId, int reserveShelfUrlId)
        {
            if (reserveShelfId != 0 && reserveShelfUrlId != 0)
            {
                _reserveShelfService.DeleteReserveShelfUrl(reserveShelfId, reserveShelfUrlId);
            }

            return RedirectToAction("Detail", new { institutionId, reserveShelfId, resourceTitleRemoved = "" });
        }

        public ActionResult Delete(int institutionId, int reserveShelfId)
        {
            if (reserveShelfId != 0)
            {
                _reserveShelfService.DeleteReserveShelfList(reserveShelfId, institutionId);
            }

            return RedirectToAction("List", new { institutionId });
        }

        public ActionResult AddDeleteResources(ReserveShelfQuery reserveShelfQuery, int resourceId,
            bool addInstitutionResource)
        {
            _reserveShelfService.AddDeleteReserveShelfResource(reserveShelfQuery.InstitutionId,
                reserveShelfQuery.ReserveShelfId, resourceId, addInstitutionResource);

            if (addInstitutionResource)
            {
                return RedirectToAction("ManageResources", reserveShelfQuery.ToRouteValues());
            }

            var resource = _resourceService.GetResource(resourceId);

            return RedirectToAction("Detail",
                new
                {
                    reserveShelfQuery.InstitutionId, reserveShelfQuery.ReserveShelfId,
                    resourceTitleRemoved = resource.Title
                });
        }

        public ActionResult ManageResources(ReserveShelfQuery reserveShelfQuery)
        {
            var reserveShelfManagement = _orderService.GetReserveShelfResources(reserveShelfQuery);

            SetReserveShelfResourcePaging(reserveShelfManagement, reserveShelfQuery, reserveShelfManagement.TotalCount);

            reserveShelfManagement.IsPdaEnabled = true; //_webSettings.EnablePatronDrivenAcquisitions; 

            return View(reserveShelfManagement);
        }

        [HttpPost]
        public ActionResult ManageResources(ReserveShelfQuery reserveShelfQuery, EmailPage emailPage)
        {
            object json;

            try
            {
                if (emailPage.To == null)
                {
                    return RedirectToAction("ManageResources", reserveShelfQuery.ToRouteValues());
                }

                json = new JsonResponse { Status = "success", Successful = true };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private void SetReserveShelfResourcePaging(ReserveShelfManagement reserveShelfManagement,
            IReserveShelfQuery reserveShelfQuery, int resourceCount)
        {
            var pageCount = resourceCount / reserveShelfQuery.PageSize +
                            (resourceCount % reserveShelfQuery.PageSize > 0 ? 1 : 0);

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || reserveShelfQuery.Page <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = reserveShelfQuery.Page - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            reserveShelfManagement.PreviousLink = Url.PreviousPageLink(reserveShelfQuery, pageCount);
            reserveShelfManagement.NextLink = Url.NextPageLink(reserveShelfQuery, pageCount);

            reserveShelfManagement.FirstLink = Url.FirstPageLink(reserveShelfQuery, pageCount);
            reserveShelfManagement.LastLink = Url.LastPageLink(reserveShelfQuery, pageCount);

            var pageLinks = new List<PageLink>();
            for (var p = firstPage; p <= lastPage; p++)
            {
                pageLinks.Add(new PageLink
                    { Selected = p == reserveShelfQuery.Page, Text = p.ToString(CultureInfo.InvariantCulture) });
            }

            reserveShelfManagement.PageLinks = pageLinks;
        }
    }
}