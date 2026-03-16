#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Cms;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Territory;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Marketing;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Storages;
using Cache = R2V2.Web.Areas.Admin.Models.Cache;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
    public class MarketingController : R2AdminBaseController
    {
        private readonly ApplicationWideStorageService _applicationWideStorageService;
        private readonly AutomatedCartInstitutionEmailBuildService _automatedCartInstitutionEmailBuildService;
        private readonly AutomatedCartService _automatedCartService;
        private readonly CmsService _cmsService;
        private readonly InstitutionService _institutionService;
        private readonly RequestResourceService _requestResourceService;
        private readonly TerritoryService _territoryService;

        public MarketingController(
            IAuthenticationContext authenticationContext
            , TerritoryService territoryService
            , InstitutionService institutionService
            , AutomatedCartService automatedCartService
            , AutomatedCartInstitutionEmailBuildService automatedCartInstitutionEmailBuildService
            , RequestResourceService requestResourceService
            , ApplicationWideStorageService applicationWideStorageService
            , CmsService cmsService
        ) : base(authenticationContext)
        {
            _territoryService = territoryService;
            _institutionService = institutionService;
            _automatedCartService = automatedCartService;
            _automatedCartInstitutionEmailBuildService = automatedCartInstitutionEmailBuildService;
            _requestResourceService = requestResourceService;
            _applicationWideStorageService = applicationWideStorageService;
            _cmsService = cmsService;
        }


        public ActionResult AutomatedCartFilter()
        {
            var territories = _territoryService.GetAllTerritories();
            var institutionTypes = _institutionService.GetInstitutionTypes();
            var model = new AutomatedCartModel(territories, institutionTypes);
            ReportDatesService.SetDates(model.ReportQuery);
            model.HideDateRangeDisplay = true;
            return View(model);
        }

        /// <summary>
        ///     Displays Institutions based off Query
        /// </summary>
        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "AutomatedCartFilter")]
        public ActionResult AutomatedCartFilter(AutomatedCartModel model)
        {
            ValidateAutomatedCartModel(model);

            if (!ModelState.IsValid)
            {
                var territories = _territoryService.GetAllTerritories();
                var institutionTypes = _institutionService.GetInstitutionTypes();
                model.PopulateSelections(territories, institutionTypes);
            }
            else
            {
                var automatedCartInstitutions = _automatedCartService.GetAutomatedCartEventInstitutions(model);

                var territories = _territoryService.GetAllTerritories();
                var institutionTypes = _institutionService.GetInstitutionTypes();
                model = new AutomatedCartModel(territories, institutionTypes, model.ReportQuery,
                    automatedCartInstitutions, model.SelectedInstitutionIds);

                model.ToolLinks = GetToolLinks(false, model.ExcelExportUrl);
            }

            return View(model);
        }

        /// <summary>
        ///     Displays Selected Institutions and allows discount override
        /// </summary>
        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "AutomatedCartSelected")]
        public ActionResult AutomatedCartSelected(AutomatedCartPricingModel model)
        {
            ValidateAutomatedCartPricingModel(model, false);

            //Need to Alert of ModelSTate errors here.
            //If Discount OVerride is invalid it pushes you back a page for now reason.

            if (!ModelState.IsValid)
            {
                var automatedCartInstitutions = _automatedCartService.GetAutomatedCartEventInstitutions(model);

                var territories = _territoryService.GetAllTerritories();
                var institutionTypes = _institutionService.GetInstitutionTypes();
                var cartModel = new AutomatedCartModel(territories, institutionTypes, model.ReportQuery,
                    automatedCartInstitutions, null);

                cartModel.ToolLinks = GetToolLinks(false, cartModel.ExcelExportUrl);

                return View("AutomatedCartFilter", cartModel);
            }

            model = _automatedCartService.GetPricedInstitutionCarts(model, CurrentUser);

            model.ToolLinks = GetToolLinks(false, model.ExcelExportUrl);

            return View(model);
        }

        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "AutomatedCartsFinalize")]
        public ActionResult AutomatedCartsFinalize(AutomatedCartPricingModel model)
        {
            ValidateAutomatedCartPricingModel(model, true);

            if (ModelState.IsValid)
            {
                var automatedCartId = _automatedCartService.SaveAutomatedCart(model);
                if (automatedCartId > 0)
                {
                    var cartModel = _automatedCartService.GetFinalizedAutomatedCart(automatedCartId, CurrentUser);
                    cartModel.ToolLinks = GetToolLinks(false, cartModel.ExcelExportUrl);
                    return View(cartModel);
                }
            }

            model = _automatedCartService.GetPricedInstitutionCarts(model, CurrentUser);
            model.ToolLinks = GetToolLinks(false, model.ExcelExportUrl);

            return View("AutomatedCartSelected", model);
        }

        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "ExportAutomatedCartPricing")]
        public ActionResult ExportAutomatedCartPricing(AutomatedCartPricingModel model)
        {
            var export = _automatedCartService.GetAutomatedCartExcelExport(model);
            var fileDownloadName =
                $"R2-AutomatedCarts-Priced-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(export.Export(), export.MimeType, fileDownloadName);
        }

        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "ExportAutomatedCartFinalized")]
        public ActionResult ExportAutomatedCartFinalized(int automatedCartId, bool displayEmailCounts)
        {
            var export =
                _automatedCartService.GetAutomatedCartExcelExport(automatedCartId, displayEmailCounts, CurrentUser);
            var fileDownloadName =
                $"R2-AutomatedCarts-Priced-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(export.Export(), export.MimeType, fileDownloadName);
        }

        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "ExportAutomatedCart")]
        public ActionResult ExportAutomatedCart(AutomatedCartModel model)
        {
            var export = _automatedCartService.GetAutomatedCartExcelExport(model);
            var fileDownloadName = $"R2-AutomatedCarts-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.xlsx";

            return File(export.Export(), export.MimeType, fileDownloadName);
        }

        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "ExportAutomatedCartEmail")]
        public ActionResult ExportAutomatedCartEmail(AutomatedCartPricingModel model)
        {
            var emailBody =
                _automatedCartInstitutionEmailBuildService.GetAutomatedCartExampleHtml(model.CartName, model.EmailTitle,
                    model.EmailText);

            return Content(emailBody, "text/html");
        }


        public ActionResult AutomatedCartHistoryList()
        {
            var model = _automatedCartService.GetAutomatedCartHistories();

            return View(model);
        }

        public ActionResult AutomatedCartHistory(int automatedCartId)
        {
            var model = _automatedCartService.GetFinalizedAutomatedCart(automatedCartId, CurrentUser);
            model.DisplayEmailCounts = true;
            model.ToolLinks = GetToolLinks(false, model.ExcelExportUrl);
            return View("AutomatedCartsFinalize", model);
        }

        public ActionResult RequestAccess(RequestedResourcesModel model)
        {
            if (Request.HttpMethod != "POST")
            {
                model.ReportQuery.Period = ReportPeriod.Last30Days;
            }

            if (!string.IsNullOrWhiteSpace(model.ReportQuery.AccountNumberBatch))
            {
                var accountNumbers = model.ReportQuery.AccountNumberBatch.Replace(" ", "").Replace("\r\n", "")
                    .Split(',');
                foreach (var accountNumber in accountNumbers)
                {
                    int i;
                    int.TryParse(accountNumber, out i);
                    if (i == 0)
                    {
                        ModelState.AddModelError("ReportQuery.AccountNumberBatch",
                            @"One of the account numbers did not parse properly. Please make sure the all text is numberic and a comma is used to seperate them.");
                        model.ReportQuery.AccountNumberBatch = null;
                        break;
                    }
                }

                //These are now clean
                if (model.ReportQuery.AccountNumberBatch != null)
                {
                    model.ReportQuery.AccountNumberBatch = string.Join(",", accountNumbers);
                }
            }

            model = _requestResourceService.GetRequestedResourcesInstitutions(model.ReportQuery);

            model.ToolLinks = GetToolLinks(false, model.ExcelExportUrl);

            return View(model);
        }

        [HttpPost]
        [MultipleButtonAttribute(Name = "action", Argument = "ExportRequestAccess")]
        public ActionResult ExportRequestAccess(RequestedResourcesModel model)
        {
            var start = model.ReportQuery.PeriodStartDate.GetValueOrDefault(DateTime.Now);
            var end = model.ReportQuery.PeriodEndDate.GetValueOrDefault(DateTime.Now);
            var export = _requestResourceService.GetRequestedResourcesExcelReport(model);
            var startString = $"{start.Year}{start.Month}{start.Day}";
            var endString = $"{end.Year}{end.Month}{end.Day}";

            var fileDownloadName = $"R2-RequestAccess_{startString}-{endString}.xlsx";

            return File(export.Export(), export.MimeType, fileDownloadName);
        }


        private void ValidateAutomatedCartModel(AutomatedCartModel model)
        {
            if (!model.IncludeNewEdition && !model.IncludeReviewed && !model.IncludeTriggeredPda &&
                !model.IncludeTurnaway && !model.IncludeRequested)
            {
                ModelState.AddModelError("IncludeNewEdition",
                    @"You must selected at least one of the Included Titles.");
            }

            if (!string.IsNullOrWhiteSpace(model.ReportQuery.AccountNumberBatch))
            {
                var accountNumbers = model.ReportQuery.AccountNumberBatch.Replace(" ", "").Split(',');
                foreach (var accountNumber in accountNumbers)
                {
                    int i;
                    int.TryParse(accountNumber, out i);
                    if (i == 0)
                    {
                        ModelState.AddModelError("ReportQuery.AccountNumberBatch",
                            @"One of the account numbers did not parse properly. Please make sure the all text is numberic and a comma is used to seperate them.");
                        break;
                    }
                }
            }
        }

        private void ValidateAutomatedCartPricingModel(AutomatedCartPricingModel model, bool isPricedCarts)
        {
            if (!isPricedCarts)
            {
                if (string.IsNullOrWhiteSpace(model.SelectedInstitutionIds))
                {
                    ModelState.AddModelError("SelectedInstitutionIds", @"No Institutions Selected");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.CartName))
                {
                    ModelState.AddModelError("CartName", @"A name of the cart is required.");
                }

                if (string.IsNullOrWhiteSpace(model.EmailText))
                {
                    ModelState.AddModelError("EmailText", @"The text of the email is required.");
                }

                if (string.IsNullOrWhiteSpace(model.EmailTitle))
                {
                    ModelState.AddModelError("EmailTitle", @"The itle of the email is required.");
                }

                if (string.IsNullOrWhiteSpace(model.EmailSubject))
                {
                    ModelState.AddModelError("EmailSubject", @"The Subject of the email is required.");
                }
            }
        }


        public ActionResult Cache()
        {
            var model = new Cache();

            var cache = _applicationWideStorageService.GetEnumerator();
            var sortedCache = new SortedList<string, ApplicationStorageItem>();
            while (cache.MoveNext())
            {
                var applicationStorageItem = cache.Value as ApplicationStorageItem;
                if (applicationStorageItem != null && applicationStorageItem.Key.ToLower().Contains("cms"))
                {
                    sortedCache.Add(applicationStorageItem.Key, applicationStorageItem);
                }
            }

            model.Items = sortedCache.Values;
            model.ToolLinks = GetToolLinks(false);

            return View(model);
        }


        public ActionResult CacheUpdate()
        {
            var cacheToClearKeys = new List<string>();
            var cache = _applicationWideStorageService.GetEnumerator();
            while (cache.MoveNext())
            {
                var applicationStorageItem = cache.Value as ApplicationStorageItem;
                if (applicationStorageItem != null && applicationStorageItem.Key.ToLower().Contains("cms"))
                {
                    cacheToClearKeys.Add(applicationStorageItem.Key);
                }
            }

            _cmsService.ClearCmsCache(cacheToClearKeys);

            return RedirectToAction("Cache");
        }

        public ActionResult CacheUpdateItem(string key)
        {
            _cmsService.ClearCmsCache(new List<string> { key });

            return RedirectToAction("Cache");
        }
    }
}