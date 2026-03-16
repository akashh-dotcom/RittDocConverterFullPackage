#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Alerts;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using AdministratorAlert = R2V2.Web.Areas.Admin.Models.Alerts.AdministratorAlert;
using InstitutionResource = R2V2.Web.Areas.Admin.Models.CollectionManagement.InstitutionResource;

#endregion


namespace R2V2.Web.Areas.Admin.Controllers
{
    public class AlertController : R2AdminBaseController
    {
        private readonly AdministratorAlertService _administratorAlertService;
        private readonly IAdminSettings _adminSettings;
        private readonly IOrderService _orderService;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AlertController(
            IAuthenticationContext authenticationContext
            , IUnitOfWorkProvider unitOfWorkProvider
            , AdministratorAlertService administratorAlertService
            , IAdminSettings adminSettings
            , IResourceService resourceService
            , ICartService cartService
            , IOrderService orderService
        )
            : base(authenticationContext)
        {
            _unitOfWorkProvider = unitOfWorkProvider;
            _administratorAlertService = administratorAlertService;
            _adminSettings = adminSettings;
            _resourceService = resourceService;
            _orderService = orderService;
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult List(string year)
        {
            ClearTempData();
            int alertYear;
            int.TryParse(year, out alertYear);
            if (alertYear < 1990)
            {
                alertYear = DateTime.Now.Year;
            }

            var allAlerts = _administratorAlertService.GetAllAlerts(alertYear);

            var model = new AdministratorAlerts(allAlerts, _adminSettings.AlertImageLocation,
                _resourceService.GetAllResources().ToList());
            SetPaging(alertYear, model);
            return View(model);
        }


        private void SetPaging(int selectedYear, AdministratorAlerts model)
        {
            var alertYears = _administratorAlertService.GetAllAdminAlertYears();
            var pageLinks = new List<PageLink>();
            foreach (var alertYear in alertYears)
            {
                var pageLink = new PageLink
                {
                    Selected = alertYear == selectedYear,
                    Text = alertYear.ToString(),
                    Href = Url.Action("List", "Alert", new { year = alertYear })
                };
                pageLinks.Add(pageLink);
            }

            model.PageLinks = pageLinks;
        }


        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Delete(int alertId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var coreAdministratorAlert = _administratorAlertService.GetAlertToDelete(alertId);

                    if (coreAdministratorAlert != null)
                    {
                        uow.Delete(coreAdministratorAlert);
                        uow.Commit();
                        transaction.Commit();

                        _administratorAlertService.ClearAlertsCache();

                        return RedirectToAction("List");
                    }

                    transaction.Rollback();
                    return RedirectToAction("List");
                }
            }
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Add()
        {
            return View(new AdministratorAlert());
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        [HttpPost]
        public ActionResult Add(AdministratorAlert administratorAlert, HttpPostedFileBase file, string actionType)
        {
            ////////////////////////////////////-------Delete Image if actionType is Int-------////////////////////////////////////
            int fileIdToDelete;
            int.TryParse(actionType, out fileIdToDelete);

            if (fileIdToDelete != 0)
            {
                administratorAlert = DeleteImage(administratorAlert, fileIdToDelete);
                return View(administratorAlert);
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            administratorAlert = ProcessAdministratorAlert(administratorAlert, file, actionType);

            if (administratorAlert == null)
            {
                return RedirectToAction("List");
            }

            if (administratorAlert.AlertId > 0)
            {
                TempData.AddItem("Model", administratorAlert);
                return RedirectToAction("Edit", administratorAlert.AlertId);
            }

            return View(administratorAlert);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Edit(int alertId)
        {
            AdministratorAlert administratorAlert;
            var modelFromTempData = TempData.GetItem<AdministratorAlert>("Model");
            if (modelFromTempData != null)
            {
                administratorAlert = modelFromTempData;
            }
            else
            {
                var coreAdministratorAlert = _administratorAlertService.GetAlertForEdit(alertId);
                IResource resource = null;
                if (coreAdministratorAlert.ResourceId.HasValue)
                {
                    resource = _resourceService.GetAllResources()
                        .FirstOrDefault(x => x.Id == coreAdministratorAlert.ResourceId.Value);
                }

                //
                administratorAlert =
                    new AdministratorAlert(coreAdministratorAlert, _adminSettings.AlertImageLocation, resource);
                if (coreAdministratorAlert.AlertImages != null)
                {
                    var idsAndFileNames = coreAdministratorAlert.AlertImages.Where(x => x.RecordStatus)
                        .ToDictionary(x => x.Id, x => x.ImageFileName);
                    TempData.AddItem("IdsAndFileNames", idsAndFileNames);
                }
            }

            return View(administratorAlert);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        [HttpPost]
        public ActionResult Edit(AdministratorAlert administratorAlert, HttpPostedFileBase file, string actionType)
        {
            ////////////////////////////////////-------Delete Image if actionType is Int-------////////////////////////////////////
            int fileIdToDelete;
            int.TryParse(actionType, out fileIdToDelete);

            if (fileIdToDelete != 0)
            {
                administratorAlert = DeleteImage(administratorAlert, fileIdToDelete);
                return View(administratorAlert);
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            administratorAlert = ProcessAdministratorAlert(administratorAlert, file, actionType);


            if (administratorAlert == null)
            {
                return RedirectToAction("List");
            }

            return View(administratorAlert);
        }

        public ActionResult ResourceFilter(string query)
        {
            var resources = _resourceService.GetAllResources()
                .Where(x => !x.NotSaleable &&
                            (x.StatusId == (int)ResourceStatus.Active ||
                             x.StatusId == (int)ResourceStatus.Forthcoming) &&
                            (x.Isbn.StartsWith(query, StringComparison.OrdinalIgnoreCase) ||
                             x.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(x => x.SortTitle);

            var resourceObject = resources.Select(x => new
                { label = $"{x.Title} ({x.Isbn} - Edition: {x.Edition})", value = $"{x.Id}" }).ToArray();
            var javaScriptSerializer = new JavaScriptSerializer();
            var jsonResults = javaScriptSerializer.Serialize(resourceObject);
            return Json(jsonResults, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddResourceToCart(int resourceId)
        {
            var resource = _resourceService.GetResource(resourceId);
            var collectionAdd = new CollectionAdd
            {
                InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault(),
                InstitutionResource = new InstitutionResource { Id = resourceId },
                NumberOfLicenses = resource.IsFreeResource ? 500 : 1
            };
            _orderService.AddItemToOrder(collectionAdd);

            return RedirectToAction("ShoppingCart", "Cart", new { CurrentUser.InstitutionId });
        }

        public ActionResult HideAlert(int alertId)
        {
            _administratorAlertService.SaveUserAlert(CurrentUser.Id, alertId, CurrentUser.IsPublisherUser());
            var currentUrl = GetRedirect();
            return Redirect(currentUrl);
        }

        private AdministratorAlert DeleteImage(AdministratorAlert administratorAlert, int fileIdToDelete)
        {
            var idsAndFileNames = TempData.GetItem<Dictionary<int, string>>("IdsAndFileNames");
            if (idsAndFileNames != null)
            {
                var idsAndFileNamesToSave = idsAndFileNames.Where(x => x.Key != fileIdToDelete)
                    .ToDictionary(x => x.Key, x => x.Value);
                TempData.AddItem("IdsAndFileNames", idsAndFileNamesToSave);
                RepopulateAlert(administratorAlert, idsAndFileNamesToSave);
            }

            if (fileIdToDelete > 0)
            {
                DeleteImageFromDatabase(fileIdToDelete);
            }

            return administratorAlert;
        }

        /// <summary>
        ///     Used to process the alert for adding or editing. The add and edit process the alert the same way.
        /// </summary>
        private AdministratorAlert ProcessAdministratorAlert(AdministratorAlert administratorAlert,
            HttpPostedFileBase file, string actionType)
        {
            string fileName = null;

            var idsAndFileNames = TempData.GetItem<Dictionary<int, string>>("IdsAndFileNames");

            if (file != null)
            {
                if (administratorAlert.AlertLayout == AlertLayout.TitleText)
                {
                    ModelState.AddModelError("AlertImages", @"This layout does not support images.");
                }

                if ((administratorAlert.AlertLayout == AlertLayout.TitleFullImageText ||
                     administratorAlert.AlertLayout == AlertLayout.TitlePartImageText) &&
                    administratorAlert.AlertImageExists)
                {
                    ModelState.AddModelError("AlertImages",
                        @"This layout only supports 1 image. Either remove the other image or change the layout.");
                }

                if (file.ContentLength > 0 && actionType != null && ModelState.IsValid)
                {
                    fileName = CopyLocalAndGetFileName(administratorAlert, file);

                    int count;
                    if (idsAndFileNames == null)
                    {
                        idsAndFileNames = new Dictionary<int, string>();
                        count = -1;
                    }
                    else
                    {
                        count = idsAndFileNames.Where(x => x.Key < 0).OrderBy(x => x.Key).Select(x => x.Key)
                            .FirstOrDefault() - 1;
                    }

                    idsAndFileNames.Add(count, fileName);
                    administratorAlert.AlertImageExists = true;
                }
            }

            if (administratorAlert.AlertLayout == AlertLayout.ResourceAndText &&
                (string.IsNullOrWhiteSpace(administratorAlert.ResourceFilter) ||
                 !administratorAlert.ResourceId.HasValue))
            {
                ModelState.AddModelError("AlertLayout", @"Please select a Resource or select a different layout.");
            }

            TempData.AddItem("IdsAndFileNames", idsAndFileNames);

            if (file == null && actionType != null && ModelState.IsValid)
            {
                ModelState.AddModelError("AlertImages", @"You must browse to an image before you verify it.");
            }

            if (administratorAlert.StartDate.HasValue || administratorAlert.EndDate.HasValue)
            {
                var startDate = DateTime.Parse("1/1/1753 12:00:00 AM");
                var endDate = DateTime.Parse("12/31/9999 11:59:59 PM");
                if (administratorAlert.StartDate.HasValue)
                {
                    if (administratorAlert.StartDate.Value < startDate || administratorAlert.StartDate.Value > endDate)
                    {
                        ModelState.AddModelError("StartDate",
                            @"Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.");
                    }
                }

                if (administratorAlert.EndDate.HasValue)
                {
                    if (administratorAlert.EndDate.Value < startDate || administratorAlert.EndDate.Value > endDate)
                    {
                        ModelState.AddModelError("EndDate",
                            @"Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM.");
                    }
                }

                if (administratorAlert.StartDate.HasValue && administratorAlert.EndDate.HasValue)
                {
                    if (administratorAlert.EndDate.Value < administratorAlert.StartDate.Value)
                    {
                        ModelState.AddModelError("EndDate", @"Start Date must be before End Date.");
                    }
                }
            }

            if (ModelState.IsValid && fileName == null && actionType == null)
            {
                var alertId = administratorAlert.AlertId;
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var coreAdministratorAlert = _administratorAlertService.GetAlertForEdit(alertId);


                        coreAdministratorAlert.Title = administratorAlert.Title;
                        coreAdministratorAlert.Text = administratorAlert.Text ?? "";
                        coreAdministratorAlert.DisplayOnce = administratorAlert.DisplayOnce;

                        coreAdministratorAlert.Layout = administratorAlert.AlertLayout;

                        coreAdministratorAlert.AlertName = administratorAlert.AlertName;
                        coreAdministratorAlert.StartDate = administratorAlert.StartDate;
                        coreAdministratorAlert.EndDate = administratorAlert.EndDate;
                        coreAdministratorAlert.RoleId = (int)administratorAlert.Role.Code;
                        coreAdministratorAlert.ResourceId = administratorAlert.ResourceId;
                        coreAdministratorAlert.AllowPDA = administratorAlert.DisplayPDA;
                        coreAdministratorAlert.AllowPurchase = administratorAlert.DisplayPurchase;

                        uow.Save(coreAdministratorAlert);

                        if (idsAndFileNames != null)
                        {
                            foreach (var newImage in idsAndFileNames.Where(x => x.Key < 0).Select(idsAndFileName =>
                                         new AlertImage
                                         {
                                             AlertId = coreAdministratorAlert.Id, ImageFileName = idsAndFileName.Value
                                         }))
                            {
                                uow.Save(newImage);
                            }
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                }

                _administratorAlertService.ClearAlertsCache();
                return null;
            }

            RepopulateAlert(administratorAlert, idsAndFileNames);
            return administratorAlert;
        }

        private string CopyLocalAndGetFileName(AdministratorAlert administratorAlert, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);

                var directoryInfo = new DirectoryInfo(_adminSettings.AlertImagePhysicalLocation);

                if (!directoryInfo.Exists)
                {
                    Directory.CreateDirectory(_adminSettings.AlertImagePhysicalLocation);
                }

                if (fileName != null)
                {
                    var path = Path.Combine($"{_adminSettings.AlertImagePhysicalLocation}", fileName);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    file.SaveAs(path);
                    var imageFile = Image.FromFile(path);

                    switch (administratorAlert.AlertLayout)
                    {
                        case AlertLayout.TitleFullImageText:
                            break;
                        case AlertLayout.TitlePartImageText:
                            break;
                        case AlertLayout.Html:
                            break;
                        default:
                            ModelState.AddModelError("ImageFileName",
                                @"You layout selected does not support an image. Please change your layout if you want to include an image.");
                            break;
                    }

                    imageFile.Dispose();
                    return fileName;
                }
            }

            return null;
        }

        /// <summary>
        ///     Repopulates the alert after processing the post.
        /// </summary>
        private void RepopulateAlert(AdministratorAlert administratorAlert, Dictionary<int, string> idsAndFileNames)
        {
            administratorAlert.PopulateImageUrls(idsAndFileNames, _adminSettings.AlertImageLocation);
            administratorAlert.PopulateDropDownLists();
        }

        /// <summary>
        ///     Clears all the Temp Data. This is required to add multiple Images without saving to database till complete.
        /// </summary>
        private void ClearTempData()
        {
            TempData.DeleteItem("Model");
            TempData.DeleteItem("IdsAndFileNames");
        }

        /// <summary>
        ///     Used to Delete an Image from the database.
        /// </summary>
        private void DeleteImageFromDatabase(int alertImageId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var alertImage = _administratorAlertService.GetAlertImage(alertImageId);

                    if (alertImage != null)
                    {
                        uow.Delete(alertImage);
                        uow.Commit();
                        transaction.Commit();

                        _administratorAlertService.ClearAlertsCache();
                        return;
                    }

                    transaction.Rollback();
                }
            }
        }

        private string GetRedirect()
        {
            if (HttpContext.Request.UrlReferrer != null)
            {
                return HttpContext.Request.UrlReferrer.AbsoluteUri;
            }


            var homePage = AuthenticatedInstitution.HomePage;
            switch (homePage)
            {
                case HomePage.Titles:
                case HomePage.Discipline:
                    return Url.Action("Index", "Browse", new { Area = "" });
                case HomePage.AtoZIndex:
                    return Url.Action("Index", "AlphaIndex", new { Area = "" });
                default:
                    return Url.Action("Index", "Home", new { Area = "" });
            }
        }
    }
}