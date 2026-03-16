#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource.Collection;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    [RequiresInstitutionId]
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.ExpertReviewer })]
    public class CollectionManagementController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly ICollectionService _collectionService;
        private readonly EmailSiteService _emailService;
        private readonly GoogleService _googleService;
        private readonly ILog<CollectionManagementController> _log;
        private readonly IOrderService _orderService;
        private readonly PublisherService _publisherService;
        private readonly RecommendationsService _recommendationsService;
        private readonly IWebSettings _webSettings;

        private bool _sendNewAccountEmail;

        public CollectionManagementController(ILog<CollectionManagementController> log
            , IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IOrderService orderService
            , EmailSiteService emailService
            , IWebSettings webSettings
            , RecommendationsService recommendationsService
            , PublisherService publisherService
            , GoogleService googleService
            , ICollectionService collectionService
        )
            : base(authenticationContext)
        {
            _log = log;
            _adminContext = adminContext;
            _orderService = orderService;
            _emailService = emailService;
            _recommendationsService = recommendationsService;
            _publisherService = publisherService;
            _googleService = googleService;
            _collectionService = collectionService;
            _webSettings = webSettings;
        }

        [RequiresInstitutionId(IgnoreRedirect = true)]
        [HttpGet]
        public ActionResult List(CollectionManagementQuery collectionManagementQuery)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("List", collectionManagementQuery.ToRouteValues());
            }

            if ((CurrentUser.IsRittenhouseAdmin() || CurrentUser.IsSalesAssociate()) &&
                collectionManagementQuery.InstitutionId == 0)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("List", collectionManagementQuery.ToRouteValues());
            }

            var institutionResources = _orderService.GetInstitutionResources(collectionManagementQuery, CurrentUser);
            if (institutionResources.InstitutionId > 0)
            {
                institutionResources.IsPdaEnabled = _webSettings.EnablePatronDrivenAcquisitions;
                SetInstitutionResourcePaging(institutionResources, collectionManagementQuery,
                    institutionResources.TotalCount);

                institutionResources.ToolLinks = GetToolLinks(
                    @Url.Action("Export", institutionResources.CollectionManagementQuery.ToRouteValues(true)),
                    institutionResources.CollectionManagementQuery, _collectionService
                );
                _googleService.LogImpressions(institutionResources.InstitutionResourcesList.ToList(),
                    institutionResources.PageTitle);
            }

            return View(institutionResources);
        }

        [HttpPost]
        [RequiresInstitutionId(IgnoreRedirect = true)]
        public ActionResult List(CollectionManagementQuery collectionManagementQuery, EmailPage emailPage)
        {
            object json;
            try
            {
                if (emailPage.To == null)
                {
                    return RedirectToAction("List", collectionManagementQuery.ToRouteValues());
                }

                if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                    collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
                {
                    collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                    return RedirectToAction("List", collectionManagementQuery.ToRouteValues());
                }

                var institutionResources =
                    _orderService.GetInstitutionResources(collectionManagementQuery, CurrentUser);

                SetInstitutionResourcePaging(institutionResources, collectionManagementQuery,
                    institutionResources.TotalCount);

                var messageBody = RenderRazorViewToString("CollectionManagement", "_List", institutionResources);

                var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

                json = emailStatus
                    ? new JsonResponse { Status = "success", Successful = true }
                    : new JsonResponse { Status = "failure", Successful = false };
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Export(CollectionManagementQuery collectionManagementQuery, string export)
        {
            var institution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var collectionManagementResources = _orderService
                .GetCollectionManagementResources(collectionManagementQuery, CurrentUser.IsExpertReviewer());

            var includePdaLicensing =
                collectionManagementResources.Any(x => x.OriginalSource == LicenseOriginalSource.Pda);

            CollectionManagementExcelExport excelExport;
            if (includePdaLicensing && !collectionManagementQuery.RecommendationsOnly)
            {
                excelExport = new CollectionManagementExcelExport(collectionManagementResources,
                    institution.ProxyPrefix, institution.UrlSuffix,
                    Url.Action("Title", "Resource", new { Area = "" },
                        HttpContext.Request.IsSecureConnection ? "https" : "http"),
                    collectionManagementQuery.IncludePdaResources);
            }
            else if (collectionManagementQuery.RecommendationsOnly)
            {
                IEnumerable<Recommendation> recommendations =
                    _recommendationsService.GetRecommendations(collectionManagementQuery.InstitutionId);

                if (recommendations != null)
                {
                    var recommendationLookup = recommendations.ToLookup(x => x.ResourceId);
                    excelExport = new CollectionManagementExcelExport(collectionManagementResources,
                        institution.ProxyPrefix, institution.UrlSuffix, recommendationLookup,
                        Url.Action("Title", "Resource", new { Area = "" },
                            HttpContext.Request.IsSecureConnection ? "https" : "http"));
                }
                else
                {
                    excelExport = new CollectionManagementExcelExport(collectionManagementResources,
                        institution.ProxyPrefix, institution.UrlSuffix,
                        Url.Action("Title", "Resource", new { Area = "" },
                            HttpContext.Request.IsSecureConnection ? "https" : "http"));
                }
            }
            else
            {
                excelExport = new CollectionManagementExcelExport(collectionManagementResources,
                    institution.ProxyPrefix, institution.UrlSuffix,
                    Url.Action("Title", "Resource", new { Area = "" },
                        HttpContext.Request.IsSecureConnection ? "https" : "http"));
            }

            var fileDownloadName =
                $"R2-CollectionManagement-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        private void SetInstitutionResourcePaging(InstitutionResources institutionResources,
            ICollectionManagementQuery collectionManagementQuery, int resourceCount)
        {
            var pageCount = resourceCount / collectionManagementQuery.PageSize +
                            (resourceCount % collectionManagementQuery.PageSize > 0 ? 1 : 0);

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || collectionManagementQuery.Page <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = collectionManagementQuery.Page - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            institutionResources.PreviousLink = Url.PreviousPageLink(collectionManagementQuery, pageCount);
            institutionResources.NextLink = Url.NextPageLink(collectionManagementQuery, pageCount);

            institutionResources.FirstLink = Url.FirstPageLink(collectionManagementQuery, pageCount);
            institutionResources.LastLink = Url.LastPageLink(collectionManagementQuery, pageCount);

            var pageLinks = new List<PageLink>();
            for (var p = firstPage; p <= lastPage; p++)
            {
                var query = new CollectionManagementQuery(collectionManagementQuery) { Page = p };
                var pageLink = new PageLink
                {
                    Selected = p == collectionManagementQuery.Page,
                    Text = p.ToString(CultureInfo.InvariantCulture),
                    Href = Url.Action("List", "CollectionManagement", query.ToRouteValues())
                };
                pageLinks.Add(pageLink);
            }

            institutionResources.PageLinks = pageLinks;
        }

        [HttpGet]
        [RequiresInstitutionId(IgnoreRedirect = true)]
        public ActionResult Edit(int institutionId, int resourceId, int cartId)
        {
            var institutionResource = _orderService.GetInstitutionResource(institutionId, resourceId, cartId);

            return PartialView("_Edit",
                new CollectionEdit
                    { InstitutionResource = institutionResource, NumberOfLicenses = institutionResource.LicenseCount });
        }

        [HttpPost]
        [RequiresInstitutionId(IgnoreRedirect = true)]
        public ActionResult Edit(CollectionEdit collectionEdit)
        {
            object json;

            try
            {
                var successful = _orderService.UpdateLicenseCount(collectionEdit);
                if (successful)
                {
                    json = new JsonResponse { Status = "success", Successful = true };

                    // need to reload after success
                    _adminContext.ReloadAdminInstitution(collectionEdit.InstitutionId,
                        AuthenticatedInstitution.User.Id);
                }
                else
                {
                    json = new JsonResponse { Status = "failure", Successful = false };
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);

                json = new JsonResponse { Status = "failure", Successful = false };
            }

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Publishers(int institutionId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);
            var publishers = _publisherService.GetPublishers();
            var publisherList = new ActivePublisherList(institution, publishers);

            return View(publisherList);
        }

        public ActionResult ResourceClick(string isbn, string index, string pageTitle)
        {
            int paresdIndex;
            int.TryParse(index, out paresdIndex);

            var institutionResource =
                _orderService.GetInstitutionResource(CurrentUser.InstitutionId.GetValueOrDefault(), isbn, 0);
            _googleService.LogProductClickAndDetail(institutionResource, paresdIndex, pageTitle);
            return null;
        }
    }
}