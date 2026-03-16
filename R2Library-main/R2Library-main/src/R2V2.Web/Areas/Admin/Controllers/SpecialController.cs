#region

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.R2Utilities;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Special;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class SpecialController : R2AdminBaseController
    {
        private readonly ILog<SpecialController> _log;
        private readonly IResourceService _resourceService;
        private readonly SpecialDiscountResourceService _specialDiscountResourceService;
        private readonly IWebImageSettings _webImageSettings;

        public SpecialController(
            IAuthenticationContext authenticationContext
            , SpecialDiscountResourceService specialDiscountResourceService
            , IWebImageSettings webImageSettings
            , ILog<SpecialController> log
            , IResourceService resourceService
        )
            : base(authenticationContext)
        {
            _specialDiscountResourceService = specialDiscountResourceService;
            _webImageSettings = webImageSettings;
            _log = log;
            _resourceService = resourceService;
        }

        public ActionResult List()
        {
            var specialAdminModels = _specialDiscountResourceService.GetAllAdminSpecials();
            var specialResourceModels =
                _specialDiscountResourceService.GetSpecialResourcesDiscountForSpecialController();

            var model = new SpecialList(specialAdminModels, specialResourceModels);

            return View(model);
        }

        public ActionResult DeleteSpecial(int specialId)
        {
            _specialDiscountResourceService.DeleteSpecial(specialId);

            return RedirectToAction("List");
        }

        public ActionResult DeleteSpecialDiscount(int specialId, int specialDiscountId)
        {
            if (specialDiscountId > 0)
            {
                _specialDiscountResourceService.DeleteSpecialDiscount(specialDiscountId);
            }

            return RedirectToAction("Edit", new { specialId });
        }

        public ActionResult DeleteSpecialDiscountResource(int specialDiscountId, int specialDiscountResourceId)
        {
            if (specialDiscountId > 0 && specialDiscountResourceId > 0)
            {
                var specialId =
                    _specialDiscountResourceService.DeleteSpecialResource(specialDiscountId, specialDiscountResourceId);
                return RedirectToAction("Edit", new { specialId });
            }

            return RedirectToAction("List");
        }


        public ActionResult Edit(int specialId, int specialDiscountId = 0)
        {
            SpecialView model;
            if (specialDiscountId > 0)
            {
                model = _specialDiscountResourceService.GetSpecialViewWithEditDiscount(specialId, specialDiscountId);
            }
            else if (specialDiscountId < 0)
            {
                model = _specialDiscountResourceService.GetSpecialViewWithNewDiscount(specialId);
            }
            else
            {
                model = _specialDiscountResourceService.GetSpecialView(specialId);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SpecialView specialView)
        {
            if (ModelState.IsValid)
            {
                var success = _specialDiscountResourceService.SaveSpecialAndSpecialDiscount(specialView);
                if (success)
                {
                    return RedirectToAction("List");
                }

                ModelState.AddModelError("Special.Name",
                    @"There was an error saving the Special. Please try again later");
            }

            var model = _specialDiscountResourceService.GetSpecialView(specialView.Special.Id);

            return View(model);
        }


        public ActionResult Add()
        {
            return View(new SpecialView());
        }

        [HttpPost]
        public ActionResult Add(string action, SpecialView specialView)
        {
            ModelState.Remove("Special.Id");
            if (ModelState.IsValid)
            {
                var specialId = _specialDiscountResourceService.SaveSpecial(specialView);

                switch (action)
                {
                    case "save":
                        return RedirectToAction("Edit", new { specialId });
                    case "icon":
                        return RedirectToAction("AddIcon", new { specialId });
                    default:
                        return RedirectToAction("AddDiscount", new { specialId });
                }
            }

            return
                View(new SpecialView
                {
                    Special =
                        new SpecialModel
                        {
                            Name = specialView.Special.Name,
                            StartDate = specialView.Special.StartDate,
                            EndDate = specialView.Special.EndDate
                        }
                });
        }


        public ActionResult AddDiscount(int specialId)
        {
            var model = _specialDiscountResourceService.GetSpecialViewWithNewDiscount(specialId);
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult AddDiscount(SpecialView specialView)
        {
            if (ModelState.IsValid)
            {
                var success = _specialDiscountResourceService.SaveSpecialAndSpecialDiscount(specialView);
                if (!specialView.IsDiscountEdit)
                {
                    if (success)
                    {
                        return RedirectToAction("List");
                    }

                    ModelState.AddModelError("Special.Name",
                        @"There was an error saving the Special. Please try again later");
                }
            }

            var model = _specialDiscountResourceService.GetSpecialViewWithEditDiscount(specialView.Special.Id,
                specialView.EditSpecialDiscount.Id);
            return View("Edit", model);
        }

        public ActionResult SaveDiscount(SpecialView specialView)
        {
            if (ModelState.IsValid)
            {
                specialView.EditSpecialDiscount.IconName =
                    _specialDiscountResourceService.GetIconName(specialView.EditSpecialDiscount.SelectIconIndex);
                _specialDiscountResourceService.SaveSpecialDiscount(specialView.EditSpecialDiscount);
            }

            var model = _specialDiscountResourceService.GetSpecialView(specialView.Special.Id);
            return View("Edit", model);
        }

        public ActionResult AddIcon(int specialId)
        {
            return View(new SpecialIcon(specialId));
        }

        [HttpPost]
        public ActionResult AddIcon(SpecialIcon specialIcon, HttpPostedFileBase file)
        {
            string message = null;
            if (ModelState.IsValid)
            {
                message = CopyLocalAndGetFileName(file);
                if (message == "success")
                {
                    return RedirectToAction("Edit", new { specialId = specialIcon.SpecialId });
                }
            }

            ModelState.AddModelError("SpecialId",
                message ?? "There was a problem uploading the icon. Please try again");

            return View(specialIcon);
        }

        public ActionResult BulkAddResources(int specialId, int specialDiscountId)
        {
            return View(new BulkAddResources { SpecialId = specialId, SpecialDiscountId = specialDiscountId });
        }

        public ActionResult BulkAddResourcesVerify(BulkAddResources bulkAddResources)
        {
            var resourcesToAdd = new List<IResource>();
            var lastIsbn = "";
            foreach (var isbn in IsbnUtilities.GetDelimitedIsbns(bulkAddResources.Isbns)
                         .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var resource = _resourceService.GetResource(isbn);
                if (resource == null)
                {
                    bulkAddResources.AddResourceNotFound(isbn);
                }
                else if (resource.NotSaleable ||
                         (resource.StatusId != (int)ResourceStatus.Forthcoming &&
                          resource.StatusId != (int)ResourceStatus.Active))
                {
                    bulkAddResources.AddExcludedResource(resource);
                }
                else
                {
                    resourcesToAdd.Add(resource);
                }
            }

            var excludedResources =
                _specialDiscountResourceService.GetExcludedResourceIdsForSpecial(bulkAddResources.SpecialDiscountId,
                    resourcesToAdd.Select(x => x.Id));

            var currentSpecial =
                _specialDiscountResourceService.GetSpecialViewWithEditDiscount(bulkAddResources.SpecialId,
                    bulkAddResources.SpecialDiscountId);
            var currentResourcesInSpecial = currentSpecial.SpecialDiscountResources.Select(x => x.Resource.Id).ToList();

            foreach (var resource in resourcesToAdd)
            {
                if ((excludedResources != null && excludedResources.Any() && excludedResources.Contains(resource.Id)) ||
                    (currentResourcesInSpecial.Any() && currentResourcesInSpecial.Contains(resource.Id)))
                {
                    bulkAddResources.AddExcludedResource(resource);
                }
                else
                {
                    bulkAddResources.AddResource(resource);
                }
            }

            if (bulkAddResources.Resources != null && bulkAddResources.Resources.Any())
            {
                var sb = new StringBuilder();
                foreach (var resource in bulkAddResources.Resources)
                {
                    sb.AppendFormat("{0},", resource.Id);
                }

                bulkAddResources.ResourceString = sb.ToString(0, sb.Length - 1);
            }


            return View(bulkAddResources);
        }

        [HttpPost]
        public ActionResult BulkAddResources(BulkAddResources bulkAddResources)
        {
            var resourceIds = bulkAddResources.GetResourceIds();
            _specialDiscountResourceService.AddResourcesToSpecial(resourceIds, bulkAddResources.SpecialDiscountId,
                CurrentUser);

            foreach (var resourceId in resourceIds)
            {
                var resource = _resourceService.GetResource(resourceId);
                bulkAddResources.AddResource(resource);
            }

            return View(bulkAddResources);
        }

        private string CopyLocalAndGetFileName(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var extension = GetFileExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    _log.Warn("Invalid file extension.");
                    return "Invalid file type. Only .PNG, .JPG, .JPEG and .GIF are supported.";
                }

                var fileName = Path.GetFileName(file.FileName);

                var directoryInfo = new DirectoryInfo(_webImageSettings.SpecialIconDirectory);

                if (!directoryInfo.Exists)
                {
                    Directory.CreateDirectory(_webImageSettings.SpecialIconDirectory);
                }

                if (fileName != null)
                {
                    var path = Path.Combine($"{_webImageSettings.SpecialIconDirectory}", fileName);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    file.SaveAs(path);
                    var imageFile = Image.FromFile(path);

                    if (imageFile.Size.Height > 28 || imageFile.Size.Width > 28)
                    {
                        imageFile.Dispose();
                        var test = new FileInfo(path);
                        test.Delete();
                        _log.Warn("Icon is too large.");
                        return "The Icon is bigger than the MAX specific size. Please resize the icon and try again..";
                    }

                    imageFile.Dispose();
                    return "success";
                }
            }

            return null;
        }

        private string GetFileExtension(string filename)
        {
            _log.DebugFormat("image file name: {0}", filename);
            var parts = filename.Split('.');

            if (parts.Length == 1)
            {
                return null;
            }

            var extension = parts.Last().ToLower();
            if (extension == "png" || extension == "gif" || extension == "jpg" || extension == "jpeg")
            {
                _log.DebugFormat("extension: {0}", extension);
                return extension;
            }

            return null;
        }
    }
}