#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class CounterReportController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly CounterReportService _counterReportService;
        private readonly EmailSiteService _emailService;
        private readonly IEmailSettings _emailSettings;
        private readonly ILog<CounterReportController> _log;

        public CounterReportController(
            IAuthenticationContext authenticationContext
            , CounterReportService counterReportService
            , IAdminContext adminContext
            , IEmailSettings emailSettings
            , ILog<CounterReportController> log
            , EmailSiteService emailService
        )
            : base(authenticationContext)
        {
            _counterReportService = counterReportService;
            _adminContext = adminContext;
            _emailSettings = emailSettings;
            _log = log;
            _emailService = emailService;
        }

        public ActionResult Index(int institutionId)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                institutionId != CurrentUser.InstitutionId)
            {
                institutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("Index", new { institutionId });
            }

            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            return View(new CounterIndexDetail(adminInstitution));
        }

        public ActionResult BookRequests(ReportQuery reportQuery, EmailPage emailPage)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            RefreshFilterModelState();
            reportQuery =
                CounterReportDefaultQueries.NormalizeReportQuery(reportQuery, ReportType.CounterBookRequests,
                    adminInstitution);

            var counterBookRequests = _counterReportService.GetCounterBookRequests(reportQuery);

            var model = new CounterBookRequestsDetail(adminInstitution, counterBookRequests, reportQuery);

            if (counterBookRequests.CounterBookRequestResources.Any())
            {
                model.ToolLinks = GetToolLinks(true, @Url.Action("ExportBookRequests", reportQuery.ToRouteValues()),
                    null, true);
            }

            if (!string.IsNullOrWhiteSpace(emailPage.To))
            {
                object json;
                try
                {
                    model.IsEmailMode = true;
                    var messageBody = RenderRazorViewToString("CounterReport", "_BookRequests", model);

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

            _log.DebugFormat("model.CounterSuccessfulResourceRequest.CounterResourceRequests.Count: {0}",
                model.CounterBookRequests.CounterBookRequestResources.Count);
            return View(model);
        }

        public ActionResult ExportBookRequests(ReportQuery reportQuery)
        {
            var counterBookRequests = _counterReportService.GetCounterBookRequests(reportQuery);
            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);
            var excelExport = new CounterBookRequestsExcelExport(_emailSettings.TemplatesDirectory, counterBookRequests,
                adminInstitution);

            var fileDownloadName = $"R2-BookRequests-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        // [HttpPost]
        public ActionResult BookAccessDeniedRequests(ReportQuery reportQuery, EmailPage emailPage)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            RefreshFilterModelState();
            reportQuery =
                CounterReportDefaultQueries.NormalizeReportQuery(reportQuery, ReportType.CounterDeniedRequests,
                    adminInstitution);

            var counterbookAccessDeniedRequests = _counterReportService.GetCounterBookAccessDeniedRequests(reportQuery);

            var model = new CounterBookAccessDeniedDetail(adminInstitution, counterbookAccessDeniedRequests,
                reportQuery);

            if (counterbookAccessDeniedRequests.CounterBookAccessDeniedResources.Any())
            {
                model.ToolLinks = GetToolLinks(true,
                    Url.Action("ExportBookAccessDeniedRequests", reportQuery.ToRouteValues()), null, true);
            }

            if (!string.IsNullOrWhiteSpace(emailPage.To))
            {
                object json;
                try
                {
                    model.IsEmailMode = true;
                    var messageBody = RenderRazorViewToString("CounterReport", "_BookAccessDeniedRequests", model);

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

            return View(model);
        }

        public ActionResult ExportBookAccessDeniedRequests(ReportQuery reportQuery)
        {
            var counterBookAccessDeniedRequests = _counterReportService.GetCounterBookAccessDeniedRequests(reportQuery);

            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            var excelExport = new CounterBookAccessDeniedRequestsExcelExport(_emailSettings.TemplatesDirectory,
                counterBookAccessDeniedRequests, adminInstitution);

            var fileDownloadName =
                $"R2-BookAccessDeniedRequests-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        ///[HttpPost]
        public ActionResult SearchRequests(ReportQuery reportQuery, EmailPage emailPage)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);
            var counterSearchResourceRequests = _counterReportService.GetCounterSearchResourceRequests(reportQuery);
            var model = new CounterSearchDetail(adminInstitution, counterSearchResourceRequests, reportQuery)
            {
                ToolLinks = GetToolLinks(true, @Url.Action("ExportSearchRequests", reportQuery.ToRouteValues()), null,
                    true)
            };

            if (!string.IsNullOrWhiteSpace(emailPage.To))
            {
                object json;
                try
                {
                    model.IsEmailMode = true;
                    var messageBody = RenderRazorViewToString("CounterReport", "_SearchRequests", model);

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

            return View(model);
        }

        public ActionResult ExportSearchRequests(ReportQuery reportQuery)
        {
            var counterSearchResourceRequests = _counterReportService.GetCounterSearchResourceRequests(reportQuery);

            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            var excelExport = new CounterSearchRequestExcelExport(_emailSettings.TemplatesDirectory,
                counterSearchResourceRequests, adminInstitution);

            var fileDownloadName = $"R2-SearchRequests-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        //[HttpPost]
        public ActionResult PlatformRequests(ReportQuery reportQuery, EmailPage emailPage)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            RefreshFilterModelState();
            reportQuery = CounterReportDefaultQueries.NormalizeReportQuery(reportQuery,
                ReportType.CounterPlatformRequests, adminInstitution);

            var counterPlatformUsageRequests = _counterReportService.GetCounterPlatformUsageRequests(reportQuery);

            var model = new CounterPlatformDetail(adminInstitution, counterPlatformUsageRequests, reportQuery);

            if (counterPlatformUsageRequests.TotalItemRequests.Any() ||
                counterPlatformUsageRequests.UniqueItemRequests.Any() ||
                counterPlatformUsageRequests.UniqueTitleRequests.Any())
            {
                model.ToolLinks = GetToolLinks(true, Url.Action("ExportPlatformRequests", reportQuery.ToRouteValues()),
                    null, true);
            }

            if (!string.IsNullOrWhiteSpace(emailPage.To))
            {
                object json;
                try
                {
                    var messageBody = RenderRazorViewToString("CounterReport", "_PlatformRequests", model);

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

            return View(model);
        }

        public ActionResult ExportPlatformRequests(ReportQuery reportQuery)
        {
            var counterPlatformUsageRequest = _counterReportService.GetCounterPlatformUsageRequests(reportQuery);

            var adminInstitution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            var excelExport = new CounterPlatformUsageExcelExport(_emailSettings.TemplatesDirectory,
                counterPlatformUsageRequest, adminInstitution);

            var fileDownloadName =
                $"R2-PlatformRequests-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        private void RefreshFilterModelState()
        {
            ModelState.Remove("ReportQuery.IncludePurchasedTitles");
            ModelState.Remove("ReportQuery.IncludePdaTitles");
            ModelState.Remove("ReportQuery.IncludeTrialStats");
        }
    }
}