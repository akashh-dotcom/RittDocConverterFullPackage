#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using NHibernate.Linq;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[]
        { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC, RoleCode.PUBUSER })]
    public class ReportController : R2AdminBaseController
    {
        private const string ResourceUsageReportKey = "Report.Resource.User.Data";
        private const int PageSize = 50;
        private readonly IAdminContext _adminContext;
        private readonly AnnualFeeReportService _annualFeeReportService;
        private readonly ApplicationUsageReportService _applicationUsageReportService;
        private readonly IContentSettings _contentSettings;
        private readonly DiscountReportService _discountReportService;
        private readonly EmailSiteService _emailService;
        private readonly InstitutionService _institutionService;
        private readonly IQueryable<InstitutionType> _institutionTypes;

        private readonly ILog<ReportController> _log;
        private readonly PdaReportService _pdaReportService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly IPublisherService _publisherService;
        private readonly PublisherUsageReportService _publisherUsageReportService;
        private readonly IResourceService _resourceService;
        private readonly ResourceUsageReportService _resourceUsageReportService;
        private readonly SalesReportService _salesReportService;
        private readonly IQueryable<SavedReport> _savedReports;
        private readonly ISpecialtyService _specialtyService;
        private readonly TerritoryService _territoryService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public ReportController(ILog<ReportController> log
            , IAuthenticationContext authenticationContext
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , IResourceService resourceService
            , IPublisherService publisherService
            , IUserSessionStorageService userSessionStorageService
            , IContentSettings contentSettings
            , EmailSiteService emailService
            , ApplicationUsageReportService applicationUsageReportService
            , ResourceUsageReportService resourceUsageReportService
            , PublisherUsageReportService publisherUsageReportService
            , AnnualFeeReportService annualFeeReportService
            , IQueryable<SavedReport> savedReports
            , IUnitOfWorkProvider unitOfWorkProvider
            , IAdminContext adminContext
            , DiscountReportService discountReportService
            , PdaReportService pdaReportService
            , TerritoryService territoryService
            , InstitutionService institutionService
            , IQueryable<InstitutionType> institutionTypes
            , SalesReportService salesReportService
        )
            : base(authenticationContext)
        {
            _log = log;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _resourceService = resourceService;
            _publisherService = publisherService;
            _userSessionStorageService = userSessionStorageService;
            _contentSettings = contentSettings;
            _emailService = emailService;
            _applicationUsageReportService = applicationUsageReportService;
            _resourceUsageReportService = resourceUsageReportService;
            _publisherUsageReportService = publisherUsageReportService;
            _annualFeeReportService = annualFeeReportService;
            _savedReports = savedReports;
            _unitOfWorkProvider = unitOfWorkProvider;
            _adminContext = adminContext;
            _discountReportService = discountReportService;
            _pdaReportService = pdaReportService;
            _territoryService = territoryService;
            _institutionService = institutionService;
            _institutionTypes = institutionTypes;
            _salesReportService = salesReportService;
        }

        public ActionResult ApplicationUsage(ReportQuery reportQuery, EmailPage emailPage)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                reportQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                reportQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("ApplicationUsage", reportQuery.ToRouteValues());
            }

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("ReportQuery");
            if (emailPage.To != null && tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            var model = RunApplicationUsageReport(reportQuery, tempReportQuery == null);
            if (emailPage.To == null)
            {
                //Set reportQuery to Session

                TempData.AddItem("ReportQuery", reportQuery);
                if (!model.IsFirstRun)
                {
                    model.ToolLinks = GetToolLinks(true,
                        Url.Action("ExportApplicationUsage", reportQuery.ToExportValues()));
                }


                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_ApplicationUsage", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private ApplicationUsageDetail RunApplicationUsageReport(ReportQuery reportQuery, bool firstRun)
        {
            reportQuery.ReportTypeId = (int)ReportType.ApplicationUsageReport;
            _log.Debug(reportQuery.ToDebugString());

            _applicationUsageReportService.Init(reportQuery);
            var applicationReportCounts = new ApplicationReportCounts();
            if (!firstRun)
            {
                applicationReportCounts = _applicationUsageReportService.RunApplicationUsageReport(reportQuery);
            }

            //ApplicationReportCounts applicationReportCounts = !firstRun ? _applicationUsageReportService.RunApplicationUsageReport(reportQuery) : null;
            var model =
                new ApplicationUsageDetail(_applicationUsageReportService.Institution, reportQuery)
                {
                    IsFirstRun = firstRun
                };
            model.SetReportData(applicationReportCounts, _applicationUsageReportService.IpAddressRanges);
            var savedReport = _applicationUsageReportService.SavedReport;
            if (savedReport != null)
            {
                model.ReportQuery.FilterByIpRanges = savedReport.HasIpFilter;
                PopulateModelIpRanges(model, savedReport);
            }
            else
            {
                SetSelectedIpRanges(model);
            }

            return model;
        }

        public ActionResult ExportApplicationUsage(ReportQuery reportQuery)
        {
            _log.Debug(reportQuery.ToDebugString());

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("ReportQuery");
            if (tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            _applicationUsageReportService.Init(reportQuery);
            var applicationReportCounts = _applicationUsageReportService.RunApplicationUsageReport(reportQuery);
            var excelExport = new ApplicationUsageExcelExport(applicationReportCounts);

            var dateRangeString = string.Format("{0:yyyy}{0:MM}{0:dd}--{1:yyyy}{1:MM}{1:dd}",
                reportQuery.DateRangeStart.GetValueOrDefault(), reportQuery.DateRangeEnd.GetValueOrDefault());

            var fileDownloadName = $"R2-ApplicationUsage-{dateRangeString}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        public ActionResult ResourceFilter(string query, string publisherId)
        {
            var resources = _resourceService.GetAllResources();
            if (!string.IsNullOrWhiteSpace(publisherId))
            {
                int.TryParse(publisherId, out var pubId);
                if (pubId > 0)
                {
                    resources = resources.Where(x => x.PublisherId == pubId);
                }
            }

            resources = resources.Where(x => x.Isbn.StartsWith(query, StringComparison.OrdinalIgnoreCase) ||
                                             x.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.SortTitle);

            var resourceObject = resources.Select(x => new
                { label = $"{x.Title} ({x.Isbn} - Edition: {x.Edition})", value = $"{x.Id}" }).ToArray();
            var javaScriptSerializer = new JavaScriptSerializer();
            var jsonResults = javaScriptSerializer.Serialize(resourceObject);
            return Json(jsonResults, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResourceUsage(ReportQuery reportQuery, EmailPage emailPage)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                !CurrentUser.IsPublisherUser() &&
                reportQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                reportQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("ResourceUsage", reportQuery.ToRouteValues());
            }

            _log.Debug(reportQuery.ToDebugString());

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("ResourceUsageReportQuery");
            if (emailPage.To != null && tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            var model = RunResourceUsageReport(reportQuery);
            if (emailPage.To == null)
            {
                //Set reportQuery to Session
                TempData.AddItem("ResourceUsageReportQuery", reportQuery);
                model.ToolLinks = GetToolLinks(true, Url.Action("ExportResourceUsage", reportQuery.ToRouteValues()));
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_ResourceUsage", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private ReportModel RunResourceUsageReport(ReportQuery reportQuery)
        {
            if (reportQuery.DefaultQuery && reportQuery.InstitutionId > 0)
            {
                var institution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

                if (institution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id)
                {
                    reportQuery.IncludePurchasedTitles = true;
                    reportQuery.IncludePdaTitles = true;
                    reportQuery.IncludeTocTitles = true;
                    reportQuery.IncludeTrialStats = true;
                }
                else if (reportQuery.DefaultQuery)
                {
                    var licenses = institution.Licenses.ToList();
                    var hasPdaLicenses = licenses.Any(license => license.OriginalSource == LicenseOriginalSource.Pda);
                    var hasFirmLicenses = licenses.Any(license => license.LicenseType == LicenseType.Purchased);

                    reportQuery.IncludePurchasedTitles = hasFirmLicenses;
                    reportQuery.IncludePdaTitles = hasPdaLicenses;
                    reportQuery.IncludeTocTitles = false;
                    reportQuery.IncludeTrialStats = false;
                }
            }

            _resourceUsageReportService.Init(reportQuery);
            var allResources = _resourceService.GetAllResources().ToList();
            var resources = _resourceService.GetAllResources().Where(x =>
                x.StatusId == (int)ResourceStatus.Active || x.StatusId == (int)ResourceStatus.Archived).ToList();

            _log.Info($"All Resources: {allResources.Count}");
            _log.Info($"Filtered Resources: {resources.Count}");

            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
            var specialties = _specialtyService.GetAllSpecialties().ToList();

            var publishers = _publisherService.GetPublishers()
                .Where(x => x.ConsolidatedPublisher == null && x.ResourceCount > 0);
            var institutionTypes = GetInstitutionTypes(reportQuery);


            var model = new ReportModel(_resourceUsageReportService.Institution, reportQuery);
            model.InitializeLists(practiceAreas, specialties, resources, publishers, institutionTypes);

            var items = _resourceUsageReportService.GetResourceUsageReportData(model);

            PopulateResourceUsageDetailModel(model, items, allResources);
            PopulateIpRangesInModel(model, _resourceUsageReportService.IpAddressRanges);

            var savedReport = _resourceUsageReportService.SavedReport;
            if (savedReport != null)
            {
                model.ReportQuery.PublisherId = savedReport.PublisherId;
                model.ReportQuery.ResourceId = savedReport.ResourceId;
                model.ReportQuery.PracticeAreaId = savedReport.PracticeAreaId;
                model.ReportQuery.SpecialtyId = savedReport.SpecialtyId;
                model.ReportQuery.FilterByIpRanges = savedReport.HasIpFilter;
                model.Name = savedReport.Name;
                model.EmailAddress = savedReport.Email;
                PopulateModelIpRanges(model, savedReport);

                model.ReportQuery.Period = savedReport.Period;
                model.ReportQuery.DateRangeStart = savedReport.PeriodStartDate;
                model.ReportQuery.DateRangeEnd = savedReport.PeriodEndDate;

                model.ReportQuery.IncludePurchasedTitles = savedReport.IncludePurchased;
                model.ReportQuery.IncludePdaTitles = savedReport.IncludePda;
                model.ReportQuery.IncludeTocTitles = savedReport.IncludeToc;
                model.ReportQuery.IncludeTrialStats = savedReport.IncludeTrialStats;
            }
            else
            {
                SetSelectedIpRanges(model);
            }

            if (AuthenticatedInstitution.Publisher != null)
            {
                model.PublisherName = AuthenticatedInstitution.Publisher.Name;
            }

            model.Type = ReportType.ResourceUsageReport;
            model.IsSaveEnabled = reportQuery.InstitutionId > 0 && reportQuery.ReportId == 0;
            model.DebugInfo = model.ToDebugString();

            model.ReportQuery.DateRangeStart = _resourceUsageReportService.ReportRequest.DateRangeStart;
            model.ReportQuery.DateRangeEnd = _resourceUsageReportService.ReportRequest.DateRangeEnd;


            return model;
        }

        public ActionResult ExportResourceUsage(ReportQuery reportQuery)
        {
            var items = _userSessionStorageService.Get<List<ResourceReportItem>>(ResourceUsageReportKey);
            if (items == null || !items.Any())
            {
                return RedirectToAction("ResourceUsage", reportQuery.ToRouteValues());
            }

            var query = reportQuery.ToBaseReportQuery();

            ReportDatesService.SetDates(query);

            var adminInstitution = _adminContext.GetAdminInstitution(query.InstitutionId);

            var isPublisherUser = AuthenticatedInstitution.IsPublisherUser();
            var bookUrl = Url.Action("Title", "Resource", new { Area = "" },
                HttpContext.Request.IsSecureConnection ? "https" : "http");

            var excelExport = new ResourceUsageExcelExport(items, isPublisherUser, adminInstitution.ProxyPrefix,
                adminInstitution.UrlSuffix, bookUrl, CurrentUser.IsRittenhouseAdmin());
            var fileDownloadName =
                $"R2-ResourceUsage-{query.PeriodStartDate.GetValueOrDefault():yyyyMMdd} -- {query.PeriodEndDate.GetValueOrDefault():yyyyMMdd}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        private void PopulateResourceUsageDetailModel(ReportModel model, List<ResourceReportItem> items,
            IEnumerable<IResource> resources)
        {
            if (model.ReportQuery.Page <= 0)
            {
                model.ReportQuery.Page = 1;
            }

            var page = model.ReportQuery.Page;

            var skip = page > 1 ? (page - 1) * PageSize : 0;

            var batch = items.Skip(skip).Take(PageSize);

            var reportResources = batch
                .Select(x => new ReportResource(x, _contentSettings.BookCoverUrl, resources)).ToList();

            var pageResourceLastNumber = page * PageSize;

            model.ResourceCount = items.Count;
            model.PageResourceFirstNumber = (page - 1) * PageSize + 1;
            model.PageResourceLastNumber = pageResourceLastNumber < items.Count ? pageResourceLastNumber : items.Count;
            model.Items = reportResources;

            SetPaging(model, model.ResourceCount);
        }

        private void PopulateIpRangesInModel(ReportModel model, IEnumerable<IpAddressRange> ipAddressRanges)
        {
            model.AddIpAddressRanges(ipAddressRanges);
        }


        private void SetPublisherPaging(PublisherUsageDetail model)
        {
            var itemCount = model.Items.Count;
            var pageCount = itemCount / PageSize +
                            (itemCount % PageSize > 0 ? 1 : 0);

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || model.ReportQuery.Page <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = model.ReportQuery.Page - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            model.PreviousLink = new PageLink
            {
                Active = model.ReportQuery.Page > 1 && pageCount > 1,
                Text = "Previous",
                Href = Url.Action("PublisherUsage", "Report",
                    model.ReportQuery.ToRouteValues(model.ReportQuery.Page - 1))
            };
            model.NextLink = new PageLink
            {
                Active = pageCount > 1 && model.ReportQuery.Page < pageCount,
                Text = "Next",
                Href = Url.Action("PublisherUsage", "Report",
                    model.ReportQuery.ToRouteValues(model.ReportQuery.Page + 1))
            };
            model.FirstLink = new PageLink
            {
                Active = model.ReportQuery.Page == 1,
                Text = "First",
                Href = Url.Action("PublisherUsage", "Report", model.ReportQuery.ToRouteValues(1))
            };

            model.LastLink = new PageLink
            {
                Active = model.ReportQuery.Page == pageCount,
                Text = "Last",
                Href = Url.Action("PublisherUsage", "Report", model.ReportQuery.ToRouteValues(pageCount))
            };

            var pageLinks = new List<PageLink>();
            var currentPage = model.ReportQuery.Page;
            for (var p = firstPage; p <= lastPage; p++)
            {
                model.ReportQuery.Page = p;
                var pageLink = Url.PageLinkPublisherUsage(model.ReportQuery, pageCount, currentPage);
                pageLinks.Add(pageLink);
            }

            model.PageLinks = pageLinks;

            if (model.ReportQuery.Page <= 0)
            {
                model.ReportQuery.Page = 1;
            }

            //int page = model.ReportQuery.Page;
            var skip = currentPage > 1 ? (currentPage - 1) * PageSize : 0;

            var test = model.Items.Skip(skip).Take(PageSize);
            model.Items = test.ToList();

            var pageResourceLastNumber = currentPage * PageSize;

            model.ResourceCount = itemCount;
            model.PageResourceFirstNumber = (currentPage - 1) * PageSize + 1;
            model.PageResourceLastNumber = pageResourceLastNumber < itemCount ? pageResourceLastNumber : itemCount;
        }

        private void SetSalesReportPaging(SalesReportDetail model)
        {
            var itemCount = model.Items.Count;
            var pageCount = itemCount / PageSize +
                            (itemCount % PageSize > 0 ? 1 : 0);

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || model.ReportQuery.Page <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = model.ReportQuery.Page - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            model.PreviousLink = new PageLink
            {
                Active = model.ReportQuery.Page > 1 && pageCount > 1,
                Text = "Previous",
                Href = Url.Action("PublisherUsage", "Report",
                    model.ReportQuery.ToRouteValues(model.ReportQuery.Page - 1))
            };
            model.NextLink = new PageLink
            {
                Active = pageCount > 1 && model.ReportQuery.Page < pageCount,
                Text = "Next",
                Href = Url.Action("PublisherUsage", "Report",
                    model.ReportQuery.ToRouteValues(model.ReportQuery.Page + 1))
            };
            model.FirstLink = new PageLink
            {
                Active = model.ReportQuery.Page == 1,
                Text = "First",
                Href = Url.Action("PublisherUsage", "Report", model.ReportQuery.ToRouteValues(1))
            };

            model.LastLink = new PageLink
            {
                Active = model.ReportQuery.Page == pageCount,
                Text = "Last",
                Href = Url.Action("PublisherUsage", "Report", model.ReportQuery.ToRouteValues(pageCount))
            };

            var pageLinks = new List<PageLink>();
            var currentPage = model.ReportQuery.Page;
            for (var p = firstPage; p <= lastPage; p++)
            {
                model.ReportQuery.Page = p;
                var pageLink = Url.PageLinkPublisherUsage(model.ReportQuery, pageCount, currentPage);
                pageLinks.Add(pageLink);
            }

            model.PageLinks = pageLinks;

            if (model.ReportQuery.Page <= 0)
            {
                model.ReportQuery.Page = 1;
            }

            //int page = model.ReportQuery.Page;
            var skip = currentPage > 1 ? (currentPage - 1) * PageSize : 0;

            var test = model.Items.Skip(skip).Take(PageSize);
            model.Items = test.ToList();

            var pageResourceLastNumber = currentPage * PageSize;

            model.ResourceCount = itemCount;
            model.PageResourceFirstNumber = (currentPage - 1) * PageSize + 1;
            model.PageResourceLastNumber = pageResourceLastNumber < itemCount ? pageResourceLastNumber : itemCount;
        }

        private void SetPaging(ReportModel model, int resourceCount)
        {
            var pageCount = resourceCount / PageSize +
                            (resourceCount % PageSize > 0 ? 1 : 0);

            int lastPage;
            int firstPage;
            if (pageCount <= MaxPages || model.ReportQuery.Page <= 5)
            {
                firstPage = 1;
                lastPage = pageCount < MaxPages ? pageCount : MaxPages;
            }
            else
            {
                firstPage = model.ReportQuery.Page - 4;
                lastPage = firstPage + (MaxPages - 1);
                if (lastPage > pageCount)
                {
                    lastPage = pageCount;
                    firstPage = pageCount - (MaxPages - 1);
                }
            }

            model.PreviousLink = new PageLink
            {
                Active = model.ReportQuery.Page > 1 && pageCount > 1,
                Text = "Previous",
                Href = Url.Action("ResourceUsage", "Report",
                    model.ReportQuery.ToRouteValues(model.ReportQuery.Page - 1))
            };
            model.NextLink = new PageLink
            {
                Active = pageCount > 1 && model.ReportQuery.Page < pageCount,
                Text = "Next",
                Href = Url.Action("ResourceUsage", "Report",
                    model.ReportQuery.ToRouteValues(model.ReportQuery.Page + 1))
            };
            model.FirstLink = new PageLink
            {
                Active = model.ReportQuery.Page == 1,
                Text = "First",
                Href = Url.Action("ResourceUsage", "Report", model.ReportQuery.ToRouteValues(1))
            };

            model.LastLink = new PageLink
            {
                Active = model.ReportQuery.Page == pageCount,
                Text = "Last",
                Href = Url.Action("ResourceUsage", "Report", model.ReportQuery.ToRouteValues(pageCount))
            };

            var pageLinks = new List<PageLink>();
            var currentPage = model.ReportQuery.Page;
            for (var p = firstPage; p <= lastPage; p++)
            {
                model.ReportQuery.Page = p;
                var pageLink = Url.PageLink(model.ReportQuery, pageCount, currentPage);
                pageLinks.Add(pageLink);
            }

            model.PageLinks = pageLinks;
        }

        private void SetSelectedIpRanges(ReportModel model)
        {
            if (model.ReportQuery.EditableIpAddressRange != null)
            {
                model.ReportQuery.EditableIpAddressRange = new ReportIpAddressRange
                {
                    OctetA = model.ReportQuery.EditableIpAddressRange.OctetA,
                    OctetB = model.ReportQuery.EditableIpAddressRange.OctetB,
                    OctetCStart = model.ReportQuery.EditableIpAddressRange.OctetCStart,
                    OctetCEnd = model.ReportQuery.EditableIpAddressRange.OctetCEnd,
                    OctetDStart = model.ReportQuery.EditableIpAddressRange.OctetDStart,
                    OctetDEnd = model.ReportQuery.EditableIpAddressRange.OctetDEnd
                };
            }

            if (model.ReportQuery.SelectedIpAddressRangeIds != null &&
                model.ReportQuery.SelectedIpAddressRangeIds.Count > 0)
            {
                foreach (var range in model.IpAddressRanges)
                {
                    range.Selected = model.ReportQuery.SelectedIpAddressRangeIds.Contains(range.Id);
                    range.Checked = range.Selected ? "checked=\"checked\"" : string.Empty;
                }
            }
        }

        public ActionResult SavedReports(int institutionId)
        {
            var institution = _adminContext.GetAdminInstitution(institutionId);

            var savedReports = _savedReports.Fetch(x => x.Institution).Where(x => x.UserId == CurrentUser.Id);
            SavedReports model;

            if (institutionId > 0)
            {
                savedReports = savedReports.Where(x => x.InstitutionId == institutionId);
                model = new SavedReports(institution);
            }
            else
            {
                model = new SavedReports();
            }

            savedReports = savedReports.OrderByDescending(x => x.CreationDate);

            var publishers = _publisherService.GetPublishers().ToList();
            var resources = _resourceService.GetAllResources().ToList();
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();

            model.AddSavedReports(savedReports.ToList(), Request.RequestContext, institutionId, practiceAreas,
                publishers, resources);

            return View(model);
        }

        public ActionResult SavedReport(ReportQuery reportQuery)
        {
            _log.DebugFormat("SavedReport(Save) - {0}", reportQuery.ToDebugString());

            var institution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);

            var report = new SavedReportDetail(reportQuery, institution);

            _resourceUsageReportService.Init(reportQuery, false);

            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
            var specialties = _specialtyService.GetAllSpecialties().ToList();
            var publishers = _publisherService.GetPublishers().Where(x => x.ConsolidatedPublisher == null);
            var resources = _resourceService.GetAllResources().ToList();
            var institutionTypes = GetInstitutionTypes(reportQuery);


            report.InitializeLists(practiceAreas, specialties, resources, publishers, institutionTypes);
            PopulateIpRangesInModel(report, _resourceUsageReportService.IpAddressRanges);

            var savedReport = _resourceUsageReportService.SavedReport;
            if (savedReport != null)
            {
                report.ReportQuery = new ReportQuery
                {
                    ReportId = savedReport.Id,
                    Description = savedReport.Description,
                    PracticeAreaId = savedReport.PracticeAreaId,
                    SpecialtyId = savedReport.SpecialtyId,
                    PublisherId = savedReport.PublisherId,
                    ResourceId = savedReport.ResourceId,
                    FilterByIpRanges = savedReport.IpFilters != null && savedReport.HasIpFilter,
                    IncludePurchasedTitles = savedReport.IncludePurchased,
                    IncludePdaTitles = savedReport.IncludePda,
                    IncludeTocTitles = savedReport.IncludeToc,
                    IncludeTrialStats = savedReport.IncludeTrialStats,
                    Period = savedReport.Period,
                    DateRangeStart = savedReport.PeriodStartDate,
                    DateRangeEnd = savedReport.PeriodEndDate,
                    ReportTypeId = savedReport.Type,
                    InstitutionId = savedReport.InstitutionId
                };
                report.Type = (ReportType)savedReport.Type;
                report.Name = savedReport.Name;
                report.Frequency = (ReportFrequency)savedReport.Frequency;
                report.EmailAddress = savedReport.Email;
                report.Description = savedReport.Description;
                PopulateModelIpRanges(report, savedReport);
            }
            else
            {
                report.EmailAddress = CurrentUser.Email;
            }

            return View(report);
        }

        [HttpPost]
        public ActionResult CommitSavedReport(SavedReportDetail detail)
        {
            var reportQuery = detail.ReportQuery;
            var institution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);
            var report = new SavedReportDetail(reportQuery, institution);

            _resourceUsageReportService.Init(reportQuery, false);
            if (ModelState.IsValid)
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var savedReport = _resourceUsageReportService.SavedReport;
                        if (savedReport == null)
                        {
                            savedReport = new SavedReport { CreationDate = DateTime.Now, RecordStatus = true };
                        }
                        else
                        {
                            savedReport.LastUpdate = DateTime.Now;
                        }

                        savedReport.Frequency = (int)detail.Frequency;
                        savedReport.Name = detail.Name;
                        savedReport.Email = detail.EmailAddress;
                        savedReport.Description = detail.Description;
                        savedReport.UserId = CurrentUser.Id;
                        savedReport.Type = reportQuery.ReportTypeId;
                        savedReport.HasIpFilter = reportQuery.FilterByIpRanges;
                        savedReport.InstitutionId = reportQuery.InstitutionId;
                        savedReport.ResourceId = reportQuery.ResourceId;
                        savedReport.PublisherId = reportQuery.PublisherId;
                        savedReport.PracticeAreaId = reportQuery.PracticeAreaId;
                        savedReport.SpecialtyId = reportQuery.SpecialtyId;

                        savedReport.Period = reportQuery.Period;
                        savedReport.IncludePurchased = reportQuery.IncludePurchasedTitles;
                        savedReport.IncludePda = reportQuery.IncludePdaTitles;
                        savedReport.IncludeToc = reportQuery.IncludeTocTitles;
                        savedReport.IncludeTrialStats = reportQuery.IncludeTrialStats;

                        if (savedReport.Period == ReportPeriod.UserSpecified)
                        {
                            savedReport.PeriodStartDate = reportQuery.DateRangeStart;
                            savedReport.PeriodEndDate = reportQuery.DateRangeEnd;
                        }

                        PopulateSavedReportIpRanges(savedReport, reportQuery);

                        uow.SaveOrUpdate(savedReport);
                        uow.Commit();
                        transaction.Commit();

                        return RedirectToAction("SavedReports", new { Id = savedReport.InstitutionId });
                    }
                }
            }

            _resourceUsageReportService.Init(reportQuery, false);

            var publishers = _publisherService.GetPublishers().Where(x => x.ConsolidatedPublisher == null);
            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
            var specialties = _specialtyService.GetAllSpecialties().ToList();
            var resources = _resourceService.GetAllResources().ToList();
            var institutionTypes = GetInstitutionTypes(reportQuery);

            report.InitializeLists(practiceAreas, specialties, resources, publishers, institutionTypes);
            PopulateIpRangesInModel(report, _resourceUsageReportService.IpAddressRanges);

            return View("SavedReport", report);
        }


        public ActionResult DeleteSavedReport(int institutionId, int reportId)
        {
            if (reportId > 0)
            {
                try
                {
                    using (var uow = _unitOfWorkProvider.Start())
                    {
                        using (var transaction = uow.BeginTransaction())
                        {
                            var savedReports = _savedReports.Fetch(x => x.Institution)
                                .Where(x => x.UserId == CurrentUser.Id);
                            var savedReport = savedReports.FirstOrDefault(x => x.Id == reportId);
                            if (savedReport != null)
                            {
                                uow.Delete(savedReport);
                                uow.Commit();
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    throw;
                }
            }

            return RedirectToAction("SavedReports", new { Id = institutionId });
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult SalesReport(ReportQuery reportQuery, EmailPage emailPage)
        {
            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempPublisherReportQuery = TempData.GetItem<ReportQuery>("SalesReportQuery");
            if (emailPage.To != null && tempPublisherReportQuery != null)
            {
                reportQuery = tempPublisherReportQuery;
            }

            var model = RunSalesReport(reportQuery);
            if (emailPage.To == null)
            {
                //Set reportQuery to Session
                TempData.AddItem("SalesReportQuery", reportQuery);
                //TODO: Fix This
                model.ToolLinks = GetToolLinks(true, Url.Action("ExportSalesReport", reportQuery.ToExportValues()));
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_SalesReport", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }


        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult PublisherUsage(ReportQuery reportQuery, EmailPage emailPage)
        {
            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempPublisherReportQuery = TempData.GetItem<ReportQuery>("PublisherReportQuery");
            if (emailPage.To != null && tempPublisherReportQuery != null)
            {
                reportQuery = tempPublisherReportQuery;
            }

            var model = RunPublisherUsageReport(reportQuery);
            if (emailPage.To == null)
            {
                //Set reportQuery to Session
                TempData.AddItem("PublisherReportQuery", reportQuery);
                model.ToolLinks = GetToolLinks(true, Url.Action("ExportPublisherUsage", reportQuery.ToExportValues()));
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_PublisherUsage", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);

            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public SalesReportDetail RunSalesReport(ReportQuery reportQuery)
        {
            var model = new SalesReportDetail();

            var baseQ = reportQuery.ToBaseReportQuery();
            ReportDatesService.SetDates(baseQ);
            reportQuery.DateRangeStart = baseQ.PeriodStartDate;
            reportQuery.DateRangeEnd = baseQ.PeriodEndDate;

            var salesReportItems = _salesReportService.RunSalesReportItems(reportQuery);

            model.SetReportData(salesReportItems, reportQuery);
            var resources = _resourceService.GetAllResources().Where(x => x.StatusId != (int)ResourceStatus.Inactive)
                .ToList();

            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
            var specialties = _specialtyService.GetAllSpecialties().ToList();

            var publishers = _publisherService.GetPublishers()
                .Where(x => x.ConsolidatedPublisher == null && x.ResourceCount > 0);
            var institutionTypes = GetInstitutionTypes(reportQuery);
            //InitializeLists
            var territories = _territoryService.GetAllTerritories();
            model.InitializeLists(practiceAreas, specialties, resources, publishers, institutionTypes,
                territories.ToList());

            SetSalesReportPaging(model);
            return model;
        }

        public PublisherUsageDetail RunPublisherUsageReport(ReportQuery publisherReportQuery)
        {
            var model = new PublisherUsageDetail();
            var publishers = _publisherService.GetPublishers().Where(x => x.ConsolidatedPublisher == null).ToList();

            if (publisherReportQuery.PublisherId == 0 && publishers.Any())
            {
                var firstPublisher = publishers.FirstOrDefault();
                if (firstPublisher != null)
                {
                    publisherReportQuery.PublisherId = firstPublisher.Id;
                }
            }

            var baseQ = publisherReportQuery.ToBaseReportQuery();
            ReportDatesService.SetDates(baseQ);
            publisherReportQuery.DateRangeStart = baseQ.PeriodStartDate;
            publisherReportQuery.DateRangeEnd = baseQ.PeriodEndDate;


            var publisherReportCounts = _publisherUsageReportService.RunPublisherReportCounts(publisherReportQuery);

            model.SetReportData(publisherReportCounts, publisherReportQuery);
            model.InitializeLists(publishers, publisherReportQuery.PublisherId);

            SetPublisherPaging(model);
            return model;
        }

        public ActionResult ExportPublisherUsage(ReportQuery publisherReportQuery)
        {
            _log.Debug(publisherReportQuery.ToDebugString());

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempPublisherReportQuery = TempData.GetItem<ReportQuery>("PublisherReportQuery");
            if (tempPublisherReportQuery != null)
            {
                publisherReportQuery = tempPublisherReportQuery;
            }

            var publishers = _publisherService.GetPublishers().Where(x => x.ConsolidatedPublisher == null).ToList();
            var publisherName = "";
            if (publishers.Any())
            {
                var publisher = publishers.FirstOrDefault(x => x.Id == publisherReportQuery.PublisherId);
                if (publisher != null)
                {
                    publisherName = publisher.Name;
                }
            }

            var publisherReportCounts = _publisherUsageReportService.RunPublisherReportCounts(publisherReportQuery);
            var excelExport = new PublisherUsageExcelExport(publisherReportCounts);
            var dateRangeString = string.Format("{0:yyyy}{0:MM}{0:dd}--{1:yyyy}{1:MM}{1:dd}",
                publisherReportCounts.StartDate, publisherReportCounts.EndDate);
            var fileDownloadName = $"R2-PublisherUsage-{publisherName}-{dateRangeString}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        public ActionResult ExportSalesReport(ReportQuery reportQuery)
        {
            _log.Debug(reportQuery.ToDebugString());

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempSalesReportQuery = TempData.GetItem<ReportQuery>("SalesReportQuery");
            if (tempSalesReportQuery != null)
            {
                reportQuery = tempSalesReportQuery;
            }

            var salesReportItems = _salesReportService.RunSalesReportItems(reportQuery);

            //PublisherReportCounts publisherReportCounts = _publisherUsageReportService.RunPublisherReportCounts(reportQuery);
            var excelExport = new SalesReportExcelExport(salesReportItems);
            var dateRangeString = string.Format("{0:yyyy}{0:MM}{0:dd}--{1:yyyy}{1:MM}{1:dd}",
                reportQuery.DateRangeStart.GetValueOrDefault(), reportQuery.DateRangeEnd.GetValueOrDefault());
            var fileDownloadName = $"R2-SalesReport-{dateRangeString}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }


        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult AnnualFeeReport(ReportQuery reportQuery, EmailPage emailPage)
        {
            _log.Debug(reportQuery.ToDebugString());

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("AnnualFeeReportQuery");
            if (emailPage.To != null && tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            var model = RunAnnualFeeReportDetail(reportQuery);
            if (emailPage.To == null)
            {
                //Set reportQuery to Session
                TempData.AddItem("AnnualFeeReportQuery", reportQuery);
                model.ToolLinks = GetToolLinks(true, Url.Action("ExportAnnualFeeReport", reportQuery.ToRouteValues()));
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_AnnualFeeReport", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public AnnualFeeReportDetail RunAnnualFeeReportDetail(ReportQuery reportQuery)
        {
            reportQuery.ReportTypeId = (int)ReportType.AnnualFeeReport;
            var items = _annualFeeReportService.GetAnnualFeeReportData(reportQuery);

            var model = new AnnualFeeReportDetail(reportQuery);
            model.SetReportData(items);

            return model;
        }

        public ActionResult ExportAnnualFeeReport(ReportQuery reportQuery)
        {
            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("AnnualFeeReportQuery");
            if (tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            var items = _annualFeeReportService.GetAnnualFeeReportData(reportQuery);

            var excelExport = new AnnualMaintenanceFeeExcelExport(items);

            var dateRangeString = string.Format("{0:yyyy}{0:MM}{0:dd}--{1:yyyy}{1:MM}{1:dd}",
                reportQuery.DateRangeStart.GetValueOrDefault(), reportQuery.DateRangeEnd.GetValueOrDefault());

            var fileDownloadName = $"R2-AnnualFeeReport-{dateRangeString}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }


        private void PopulateModelIpRanges(ReportModel model, SavedReport savedReport)
        {
            model.ReportQuery.SelectedIpAddressRangeIds = new List<int>();

            foreach (var ipFilter in savedReport.IpFilters)
            {
                // does the ip range exist in the list of institutional IP address ranges?
                var reportIpAddressRange =
                    model.IpAddressRanges.SingleOrDefault(x => x.GetIpAddressStart() == ipFilter.IpStartRange &&
                                                               x.GetIpAddressEnd() == ipFilter.IpEndRange);

                if (reportIpAddressRange != null)
                {
                    reportIpAddressRange.Selected = true;
                    continue;
                }

                if (model.ReportQuery.EditableIpAddressRange == null)
                {
                    model.ReportQuery.EditableIpAddressRange =
                        ReportIpAddressRange.CreateReportIpAddressRange(ipFilter.IpStartRange, ipFilter.IpEndRange);
                }
            }
        }

        private void PopulateSavedReportIpRanges(SavedReport savedReport, ReportQuery reportQuery)
        {
            if (!reportQuery.FilterByIpRanges || reportQuery.SelectedIpAddressRangeIds == null ||
                reportQuery.SelectedIpAddressRangeIds.Count == 0)
            {
                foreach (var ipFilter in savedReport.IpFilters)
                {
                    ipFilter.RecordStatus = false;
                }

                return;
            }

            foreach (var ipFilter in savedReport.IpFilters)
            {
                var range =
                    _resourceUsageReportService.IpAddressRanges.SingleOrDefault(x =>
                        x.GetIpAddressRangeStart() == ipFilter.IpStartRange &&
                        x.GetIpAddressRangeEnd() == ipFilter.IpEndRange);

                if (range != null)
                {
                    if (!reportQuery.SelectedIpAddressRangeIds.Contains(range.Id))
                    {
                        ipFilter.RecordStatus = false;
                        _log.DebugFormat("Delete Saved Report IP Filter, Id: {0}, {1} - {2} --> A", ipFilter.Id,
                            ipFilter.IpStartRange,
                            ipFilter.IpEndRange);
                    }

                    continue;
                }

                if (reportQuery.EditableIpAddressRange.GetIpAddressStart() != ipFilter.IpStartRange ||
                    reportQuery.EditableIpAddressRange.GetIpAddressEnd() == ipFilter.IpEndRange)
                {
                    ipFilter.RecordStatus = false;
                    _log.DebugFormat("Delete Saved Report IP Filter, Id: {0}, {1} - {2} --> B", ipFilter.Id,
                        ipFilter.IpStartRange,
                        ipFilter.IpEndRange);
                }
            }

            // save editable ip range
            var editableIpFilter =
                savedReport.IpFilters.SingleOrDefault(x =>
                    x.IpStartRange == reportQuery.EditableIpAddressRange.GetIpAddressStart() &&
                    x.IpEndRange == reportQuery.EditableIpAddressRange.GetIpAddressEnd());
            if (editableIpFilter == null && reportQuery.EditableIpAddressRange.IsValid())
            {
                var ipAddressStart = reportQuery.EditableIpAddressRange.GetIpAddressStart();
                var ipAddressEnd = reportQuery.EditableIpAddressRange.GetIpAddressEnd();
                savedReport.AddIpFilter(ipAddressStart, ipAddressEnd);
                _log.DebugFormat("Add Saved Report IP Filter, {0} - {1}", ipAddressStart, ipAddressEnd);
            }

            // save list
            foreach (var id in reportQuery.SelectedIpAddressRangeIds)
            {
                var range = _resourceUsageReportService.IpAddressRanges.SingleOrDefault(x => x.Id == id);
                if (range != null)
                {
                    var ipFilter =
                        savedReport.IpFilters.SingleOrDefault(x => x.IpStartRange == range.GetIpAddressRangeStart() &&
                                                                   x.IpEndRange == range.GetIpAddressRangeEnd());
                    if (ipFilter == null)
                    {
                        var ipAddressStart = range.GetIpAddressRangeEnd();
                        var ipAddressEnd = range.GetIpAddressRangeStart();
                        savedReport.AddIpFilter(ipAddressStart, ipAddressEnd);
                        _log.DebugFormat("Add Saved Report IP Filter, {0} - {1}", ipAddressStart, ipAddressEnd);
                    }
                }
                else
                {
                    _log.WarnFormat("IP Address Range Id NOT FOUND! id: {0}", id);
                }
            }
        }

        public ActionResult SpecialReport(EmailPage emailPage, int selectSpecialId = 0)
        {
            var specials = _discountReportService.GetSpecials();
            var tempSpecialReportId = TempData.GetItem<int>("SpecialReportId");
            if (selectSpecialId == 0 && tempSpecialReportId == 0)
            {
                return View(new SpecialReportDetail(specials, selectSpecialId));
            }

            if (selectSpecialId == 0 && tempSpecialReportId > 0)
            {
                selectSpecialId = tempSpecialReportId;
            }

            var discountResources = _discountReportService.GetSpecialsReport(selectSpecialId);
            var model = new SpecialReportDetail(specials, selectSpecialId);
            if (discountResources != null && discountResources.Any())
            {
                var special = specials.FirstOrDefault(x => x.Id == selectSpecialId);
                if (special != null)
                {
                    model.SetDiscountResources(special, discountResources);

                    var rvd = new RouteValueDictionary
                    {
                        { "reportType", "Special" },
                        { "reportId", selectSpecialId }
                    };

                    model.ToolLinks = GetToolLinks(true, Url.Action("ExportDiscountReport", rvd));
                    TempData.AddItem("SpecialReportId", selectSpecialId);
                }
            }

            if (emailPage.To == null)
            {
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_SpecialReport", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PdaPromotionReport(EmailPage emailPage, int selectPdaPromotionId = 0)
        {
            var pdaPromotions = _discountReportService.GetPdaPromotions();
            var tempPdaPromotionId = TempData.GetItem<int>("PdaPromotionId");
            if (selectPdaPromotionId == 0 && tempPdaPromotionId == 0)
            {
                return View(new PdaPromotionReportDetail(pdaPromotions, selectPdaPromotionId));
            }

            if (selectPdaPromotionId == 0 && tempPdaPromotionId > 0)
            {
                selectPdaPromotionId = tempPdaPromotionId;
            }

            var discountResources = _discountReportService.GetPdaPromotionsReport(selectPdaPromotionId);
            var model = new PdaPromotionReportDetail(pdaPromotions, selectPdaPromotionId);
            if (discountResources != null && discountResources.Any())
            {
                var pdaPromotion = pdaPromotions.FirstOrDefault(x => x.Id == selectPdaPromotionId);
                if (pdaPromotion != null)
                {
                    model.SetDiscountResources(pdaPromotion, discountResources);

                    var rvd = new RouteValueDictionary
                    {
                        { "reportType", "PdaPromotion" },
                        { "reportId", selectPdaPromotionId }
                    };

                    model.ToolLinks = GetToolLinks(true, Url.Action("ExportDiscountReport", rvd));
                    TempData.AddItem("PdaPromotionId", selectPdaPromotionId);
                }
            }

            if (emailPage.To == null)
            {
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_PdaPromotionReport", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PromotionReport(EmailPage emailPage, int selectPromotionId = 0)
        {
            var promotions = _discountReportService.GetPromotions();
            var tempPromotionId = TempData.GetItem<int>("PromotionId");
            if (selectPromotionId == 0 && tempPromotionId == 0)
            {
                return View(new PromotionReportDetail(promotions, selectPromotionId));
            }

            if (selectPromotionId == 0 && tempPromotionId > 0)
            {
                selectPromotionId = tempPromotionId;
            }

            var discountResources = _discountReportService.GetPromotionsReport(selectPromotionId);
            var model = new PromotionReportDetail(promotions, selectPromotionId);
            if (discountResources != null && discountResources.Any())
            {
                var promotion = promotions.FirstOrDefault(x => x.Id == selectPromotionId);
                if (promotion != null)
                {
                    model.SetDiscountResources(promotion, discountResources);

                    var rvd = new RouteValueDictionary
                    {
                        { "reportType", "Promotion" },
                        { "reportId", selectPromotionId }
                    };

                    model.ToolLinks = GetToolLinks(true, Url.Action("ExportDiscountReport", rvd));
                    TempData.AddItem("PromotionId", selectPromotionId);
                }
            }

            if (emailPage.To == null)
            {
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_PromotionReport", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportDiscountReport(int reportId, string reportType)
        {
            var discountResources = new List<DiscountResource>();
            var reportName = new StringBuilder();
            reportName.Append("R2-");
            var startDate = new DateTime();
            var endDate = new DateTime();
            switch (reportType.ToLower())
            {
                case "promotion":
                    discountResources = _discountReportService.GetPromotionsReport(reportId);
                    var promotion = _discountReportService.GetPromotion(reportId);
                    reportName.Append("Promotion-");
                    reportName.Append(promotion.Name);
                    startDate = promotion.StartDate;
                    endDate = promotion.EndDate;
                    break;
                case "pdapromotion":
                    discountResources = _discountReportService.GetPdaPromotionsReport(reportId);
                    var pdaPromotion = _discountReportService.GetPdaPromotion(reportId);
                    reportName.Append("PdaPromotion-");
                    reportName.Append(pdaPromotion.Name);
                    startDate = pdaPromotion.StartDate;
                    endDate = pdaPromotion.EndDate;
                    break;
                case "special":
                    discountResources = _discountReportService.GetSpecialsReport(reportId);
                    var special = _discountReportService.GetSpecial(reportId);
                    reportName.Append("Special-");
                    reportName.Append(special.Name);
                    startDate = special.StartDate;
                    endDate = special.EndDate;
                    break;
            }

            var excelExport = new DiscountResourcesExcelExport(discountResources);

            var dateRangeString = string.Format("{0:yyyy}{0:MM}{0:dd}--{1:yyyy}{1:MM}{1:dd}", startDate, endDate);

            var fileDownloadName = $"{reportName}-{dateRangeString}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }


        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult PdaCountsReport(ReportQuery reportQuery, EmailPage emailPage)
        {
            _log.Debug(reportQuery.ToDebugString());

            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("PdaCountsReportQuery");
            if (emailPage.To != null && tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            var model = RunPdaReportDetail(reportQuery);
            if (emailPage.To == null)
            {
                //Set reportQuery to Session
                TempData.AddItem("PdaCountsReportQuery", reportQuery);
                model.ToolLinks = GetToolLinks(true, Url.Action("ExportPdaReport", reportQuery.ToRouteValues()));
                return View(model);
            }

            var messageBody = RenderRazorViewToString("Report", "_PdaCountsReport", model);
            var emailStatus = _emailService.SendEmailMessageToQueue(messageBody, emailPage);
            var json = emailStatus
                ? new JsonResponse { Status = "success", Successful = true }
                : new JsonResponse { Status = "failure", Successful = false };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public PdaCountsReportDetail RunPdaReportDetail(ReportQuery reportQuery)
        {
            var items = _pdaReportService.GetPdaReportData(reportQuery);

            var model = new PdaCountsReportDetail(reportQuery);
            var territories = _territoryService.GetAllTerritories();
            var institutions = _institutionService.GetInstitutions(new InstitutionQuery(), null, true);
            model.SetReportData(items, territories, institutions);

            return model;
        }

        public ActionResult ExportPdaReport(ReportQuery reportQuery)
        {
            //Need to get reportQuery from Session for Emailing and Excel Export
            var tempReportQuery = TempData.GetItem<ReportQuery>("PdaCountsReportQuery");
            if (tempReportQuery != null)
            {
                reportQuery = tempReportQuery;
            }

            var items = _pdaReportService.GetPdaReportData(reportQuery);

            var excelExport = new PdaReportCountsExcelExport(items);

            var dateRangeString = string.Format("{0:yyyy}{0:MM}{0:dd}--{1:yyyy}{1:MM}{1:dd}",
                reportQuery.DateRangeStart.GetValueOrDefault(), reportQuery.DateRangeEnd.GetValueOrDefault());

            var fileDownloadName = $"R2-PdaCountsReport-{dateRangeString}.xlsx";

            return File(excelExport.Export(), excelExport.MimeType, fileDownloadName);
        }

        private List<InstitutionType> GetInstitutionTypes(ReportQuery reportQuery)
        {
            if (CurrentUser.IsRittenhouseAdmin() || CurrentUser.IsSalesAssociate())
            {
                if (reportQuery.InstitutionId == 0)
                {
                    return _institutionTypes.ToList();
                }
            }

            return null;
        }
    }
}