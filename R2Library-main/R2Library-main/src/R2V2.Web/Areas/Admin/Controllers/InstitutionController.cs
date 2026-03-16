#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core;
using R2V2.Core.Audit;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Institution;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Models;
using Institution = R2V2.Web.Areas.Admin.Models.Institution.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [RequiresInstitutionId]
    public class InstitutionController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly ICartService _cartService;
        private readonly UserService _coreUserService;
        private readonly EmailSiteService _emailService;
        private readonly AuditService _institutionAuditService;
        private readonly InstitutionService _institutionService;
        private readonly IQueryable<InstitutionType> _institutionTypes;
        private readonly IpAddressRangeService _ipAddressRangeService;
        private readonly ILog<InstitutionController> _log;
        private readonly RecentCookieService _recentCookieService;
        private readonly ITerritoryService _territoryService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public InstitutionController(ILog<InstitutionController> log
            , IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , InstitutionService institutionService
            , UserService coreUserService
            , EmailSiteService emailService
            , IUnitOfWorkProvider unitOfWorkProvider
            , ITerritoryService territoryService
            , AuditService institutionAuditService
            , RecentCookieService recentCookieService
            , ICartService cartService
            , IpAddressRangeService ipAddressRangeService
            , IQueryable<InstitutionType> institutionTypes
        )
            : base(authenticationContext)
        {
            _log = log;
            _adminContext = adminContext;
            _institutionService = institutionService;
            _coreUserService = coreUserService;
            _emailService = emailService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _territoryService = territoryService;
            _institutionAuditService = institutionAuditService;
            _recentCookieService = recentCookieService;
            _cartService = cartService;
            _ipAddressRangeService = ipAddressRangeService;
            _institutionTypes = institutionTypes;
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        [RequiresInstitutionId(IgnoreRedirect = true)]
        [HttpGet]
        public ActionResult List(InstitutionQuery institutionQuery)
        {
            var institutionList = GetInstitutionList(institutionQuery);
            if (institutionList.Institutions.Count() == 1 && !string.IsNullOrWhiteSpace(institutionQuery.Query) &&
                institutionQuery.Page == "All")
            {
                return RedirectToAction("Detail", new { id = institutionList.Institutions.First().InstitutionId });
            }

            institutionList.ToolLinks = GetToolLinks(true, Url.Action("Export", institutionQuery.ToRouteValues()));

            return View(institutionList);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        [RequiresInstitutionId(IgnoreRedirect = true)]
        [HttpPost]
        public ActionResult List(InstitutionQuery institutionQuery, EmailPage emailPage)
        {
            if (emailPage.To == null)
            {
                return RedirectToAction("List", institutionQuery.ToRouteValues());
            }


            var institutionList = GetInstitutionList(institutionQuery);

            var messageBody = RenderRazorViewToString("Institution", "_List", institutionList);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Export(InstitutionQuery institutionQuery, string export)
        {
            var recentInstitutionIds = _recentCookieService.GetRecentInstitutionIds(Request);
            var institutions = _institutionService.GetInstitutions(institutionQuery, recentInstitutionIds, true)
                .ToList();

            var institutionExportList = _coreUserService.GetInstitutionExportList(institutions);
            var excelExport = new InstitutionListExcelExport(institutionExportList);

            var fileDownloadName = $"R2-InstitutionList-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";
            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        private InstitutionList GetInstitutionList(IInstitutionQuery institutionQuery)
        {
            _log.DebugFormat("GetInstitutionList() - {0}", institutionQuery.ToDebugInfo());

            var recentInstitutionIds = _recentCookieService.GetRecentInstitutionIds(Request);

            var institutions = _institutionService.GetInstitutions(institutionQuery, recentInstitutionIds).ToList();

            _log.DebugFormat("institutions.Count: {0}", institutions.Count);

            var institutionListItems = new List<InstitutionListItem>();

            foreach (var institution in institutions)
            {
                var institutionListItem = new InstitutionListItem(institution);
                if (institution.Territory != null)
                {
                    institutionListItem.InstitutionTerritory =
                        new InstitutionTerritory(_territoryService.GetTerritory(institution.Territory.Id));
                }

                institutionListItems.Add(institutionListItem);
            }

            _log.DebugFormat("institutionListItems.Count: {0}", institutionListItems.Count);

            var alphaInstitutions = _institutionService.GetInstitutionNames(institutionQuery, recentInstitutionIds);

            _log.DebugFormat("alphaInstitutions.Count: {0}", alphaInstitutions.Count);

            var pageLinks = PaginationHelper.GetAlphaPageLinks(alphaInstitutions, institutionQuery.Page, true, true)
                .ToList();
            _log.DebugFormat("pageLinks.Count: {0}", pageLinks.Count);

            var selectedTerritoryName = "";
            if (institutionQuery.TerritoryId > 0)
            {
                var selectedTerritory = _territoryService.GetTerritory(institutionQuery.TerritoryId);
                if (selectedTerritory != null)
                {
                    selectedTerritoryName = selectedTerritory.Name;
                }
            }


            var selectedInstitutionTypeName = "";
            if (institutionQuery.InstitutionTypeId > 0)
            {
                var selectedInstitutionType =
                    _institutionTypes.FirstOrDefault(x => x.Id == institutionQuery.InstitutionTypeId);
                if (selectedInstitutionType != null)
                {
                    selectedTerritoryName = selectedInstitutionType.Name;
                }
            }


            var totalCount = alphaInstitutions.Count;
            var displayCount = institutions.Count;

            return new InstitutionList
            {
                InstitutionQuery = institutionQuery,
                Institutions = institutionListItems,
                PageLinks = pageLinks,
                TotalCount = totalCount,
                SelectedTerritoryName = selectedTerritoryName,
                SelectedInstitutionTypeName = selectedTerritoryName,
                ResultsFirstItem = displayCount > 0 ? 1 : 0,
                ResultsLastItem = displayCount
            };
        }

        public ActionResult Detail(int institutionId, bool reload = false)
        {
            var adminInstitution = _institutionService.GetInstitutionForAdmin(institutionId);


            if (adminInstitution == null)
            {
                return RedirectToAction("List");
            }

            var adminUser = _coreUserService.GetInstitutionAdministrator(adminInstitution.Id);
            var institutionDetail = new Institution(adminInstitution, adminUser);

            if ((IsRittenhouseAdmin() || IsSalesAssociate()) && institutionId > 0)
            {
                _recentCookieService.SetRecentInstitutionsCookie(institutionId, Response, Request);
            }

            return View(institutionDetail);
        }

        public ActionResult Edit(int institutionId)
        {
            var viewModel = GetInstitutionEdit(institutionId);

            return View(viewModel);
        }

        private InstitutionEditViewModel GetInstitutionEdit(int institutionId)
        {
            var institution = _institutionService.GetInstitutionForEdit(institutionId);
            var adminUser = _coreUserService.GetInstitutionAdministrator(institution.Id);
            var territories = _territoryService.GetAllTerritories();
            var institutionTypes = _institutionTypes.ToList();
            return new InstitutionEditViewModel(institution, adminUser, territories, institutionTypes);
        }

        private AccountStatus GetAccountStatus(int accountStatusId, DateTime? trialEndDate)
        {
            switch (accountStatusId)
            {
                case (int)AccountStatus.Active:
                    return AccountStatus.Active;
                case (int)AccountStatus.Trial:
                case (int)AccountStatus.TrialExpired:
                    return trialEndDate.GetValueOrDefault() > DateTime.Now
                        ? AccountStatus.Trial
                        : AccountStatus.TrialExpired;
                case (int)AccountStatus.Disabled:
                    return AccountStatus.Disabled;
            }

            return AccountStatus.Disabled;
        }

        [HttpPost]
        public ActionResult Edit(InstitutionEditViewModel inputViewModel, string action)
        {
            if (action == "generatekey")
            {
                var randomKey = GetRandomKey(16);
                inputViewModel.TrustedKey = randomKey;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var dbInstitution = _institutionService.GetInstitutionForEdit(inputViewModel.InstitutionId);

                    var dbAccountStatus = GetAccountStatus(dbInstitution.AccountStatusId, dbInstitution.Trial?.EndDate);
                    var modelAccountStatus =
                        GetAccountStatus((int)inputViewModel.AccountStatus, inputViewModel.TrialEndDate);

                    var dbIpPlus = dbInstitution.EnableIpPlus;
                    var modelIpPlus = inputViewModel.EnableIpPlus;

                    if (
                        (dbAccountStatus == AccountStatus.TrialExpired && modelAccountStatus == AccountStatus.Trial) ||
                        (dbAccountStatus == AccountStatus.Disabled && modelAccountStatus != AccountStatus.Disabled)
                    )
                    {
                        var errorMessage =
                            _ipAddressRangeService.CheckIpAddressRanges(dbInstitution,
                                !CurrentUser.IsInstitutionAdmin());
                        if (!string.IsNullOrWhiteSpace(errorMessage))
                        {
                            var errorObject = dbAccountStatus == AccountStatus.Disabled
                                ? "AccountStatusSelectList"
                                : "TrialEndDate";
                            ModelState.AddModelError(errorObject,
                                $@"This Institution's IPs overlap. Please remove the following: {errorMessage}");
                        }
                    }

                    if (!modelIpPlus && dbIpPlus)
                    {
                        var errorMessage =
                            _ipAddressRangeService.CheckIpAddressRanges(dbInstitution,
                                !CurrentUser.IsInstitutionAdmin());
                        if (!string.IsNullOrWhiteSpace(errorMessage))
                        {
                            ModelState.AddModelError("EnableIpPlus",
                                $@"This Institution's IPs overlap.  Please remove the following: {errorMessage}");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        if (dbInstitution.Id == inputViewModel.InstitutionId)
                        {
                            var auditLogData =
                                inputViewModel.UpdateInstitutionForEdit(dbInstitution, AuthenticationContext);
                            if (!string.IsNullOrWhiteSpace(auditLogData))
                            {
                                uow.SaveOrUpdate(dbInstitution);
                                uow.Commit();
                                transaction.Commit();

                                _institutionAuditService.LogInstitutionAudit(dbInstitution.Id,
                                    InstitutionAuditType.InstitutionUpdate, auditLogData);

                                _adminContext.ReloadAdminInstitution(dbInstitution.Id, CurrentUser.Id);
                                if (dbInstitution.Discount == inputViewModel.Discount)
                                {
                                    _cartService.RemoveCartsFromCache();
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                            }

                            return action == "generatekey"
                                ? RedirectToAction("Edit", new { id = inputViewModel.InstitutionId })
                                : RedirectToAction("Detail", new { id = inputViewModel.InstitutionId, reload = true });
                        }
                    }

                    transaction.Rollback();
                }
            }

            var viewModel = GetInstitutionEdit(inputViewModel.InstitutionId);

            return View(viewModel);
        }
    }
}