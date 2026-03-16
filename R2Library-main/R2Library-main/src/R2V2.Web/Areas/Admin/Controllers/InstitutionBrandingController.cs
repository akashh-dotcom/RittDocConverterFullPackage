#region

using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.InstitutionBranding;
using R2V2.Web.Infrastructure.Settings;
using InstitutionBranding = R2V2.Web.Areas.Admin.Models.InstitutionBranding.InstitutionBranding;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [RequiresInstitutionId]
    public class InstitutionBrandingController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IQueryable<Core.Institution.InstitutionBranding> _institutionBranding;
        private readonly IInstitutionSettings _institutionSettings;
        private readonly ILog<InstitutionBrandingController> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public InstitutionBrandingController(IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IQueryable<Core.Institution.InstitutionBranding> institutionBranding
            , IInstitutionSettings institutionSettings
            , ILog<InstitutionBrandingController> log
            , IUnitOfWorkProvider unitOfWorkProvider
        )
            : base(authenticationContext)
        {
            _adminContext = adminContext;
            _institutionBranding = institutionBranding;
            _institutionSettings = institutionSettings;
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        //
        // GET: /Admin/InstitutionBranding/Detail

        public ActionResult Detail(int institutionId)
        {
            var adminInstitutionBranding = new InstitutionBranding();
            if (institutionId > 0)
            {
                var institution = _adminContext.GetAdminInstitution(institutionId);
                var institutionBranding = _institutionBranding.FirstOrDefault(x => x.Institution.Id == institution.Id);


                if (institutionBranding != null)
                {
                    adminInstitutionBranding = institutionBranding.ToInstitutionBranding(institution);

                    if (!string.IsNullOrWhiteSpace(adminInstitutionBranding.LogoFileName))
                    {
                        adminInstitutionBranding.SetLogoDisplayUrl(_institutionSettings.LogoLocation);
                    }
                }
                else
                {
                    adminInstitutionBranding = new InstitutionBranding(institution);
                }
            }


            return View(adminInstitutionBranding);
        }

        //
        // GET: /Admin/InstitutionBranding/Edit

        [HttpGet]
        public ActionResult Edit(int institutionId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            InstitutionBranding institutionBrandingEdit;
            var institutionBranding = _institutionBranding.FirstOrDefault(x => x.Institution.Id == adminInstitution.Id);

            if (institutionBranding != null)
            {
                institutionBrandingEdit = institutionBranding.ToInstitutionBranding(adminInstitution);
                institutionBrandingEdit.SetLogoDisplayUrl(_institutionSettings.LogoLocation);
            }
            else
            {
                institutionBrandingEdit = new InstitutionBranding(adminInstitution);
            }

            return View(institutionBrandingEdit);
        }

        //
        // POST: /Admin/InstitutionBranding/Edit

        [HttpPost]
        public ActionResult Edit(InstitutionBranding editInstitutionBranding, HttpPostedFileBase file,
            string actionType)
        {
            if (!string.IsNullOrWhiteSpace(actionType))
            {
                return SaveInstitutionBranding(editInstitutionBranding, true);
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);

                    var fileExtenstion = Path.GetExtension(fileName);

                    if (fileExtenstion != null)
                    {
                        switch (fileExtenstion.ToUpper())
                        {
                            case ".BMP":
                                break;
                            case ".GIF":
                                break;
                            case ".JPEG":
                            case ".JPG":
                                break;
                            case ".PNG":
                                break;
                            case ".TIFF":
                                break;
                            default:
                                ModelState.AddModelError("LogoDisplayUrl",
                                    string.Format(
                                        "Your logo file is not a supported image. Please use one of the following: PNG, GIF, JPEG, JPG, BMP, or TIFF."));
                                return View(editInstitutionBranding);
                        }
                    }

                    _log.InfoFormat(">>>>> Institution Branding Logging");
                    _log.InfoFormat("File Name: '{0}'", fileName);
                    var newFileName = $"{editInstitutionBranding.InstitutionId}{fileName}";
                    _log.InfoFormat("newFileName: '{0}'", newFileName);

                    var directoryInfo = new DirectoryInfo(_institutionSettings.LocalLogoLocation);
                    _log.DebugFormat("_institutionSettings.LocalLogoLocation: {0}",
                        _institutionSettings.LocalLogoLocation);
                    _log.DebugFormat("directoryInfo.Exists: {0}", directoryInfo.Exists);

                    if (fileName != null)
                    {
                        var path = Path.Combine($"{_institutionSettings.LocalLogoLocation}", newFileName);
                        _log.DebugFormat("path: {0}", path);
                        if (System.IO.File.Exists(path))
                        {
                            _log.Debug("Path Already Exists Deleting Old");
                            System.IO.File.Delete(path);
                        }

                        file.SaveAs(path);
                        _log.DebugFormat("File Saved, {0}", path);
                        using (var imageFile = Image.FromFile(path))
                        {
                            if (imageFile.Width > 334 || imageFile.Height > 67)
                            {
                                _log.DebugFormat("File's Width: {0} || Height: {1}", imageFile.Width, imageFile.Height);
                                ModelState.AddModelError("LogoDisplayUrl",
                                    string.Format(
                                        "Your logo file is not within the specific size restrictions. Please resize your image and upload again."));
                                imageFile.Dispose();
                                return View(editInstitutionBranding);
                            }
                        }
                        //imageFile.Dispose();
                    }

                    editInstitutionBranding.LogoFileName = newFileName;
                    _log.InfoFormat("<<<<< Institution Branding Logging");
                }

                return SaveInstitutionBranding(editInstitutionBranding, false);
            }

            return View(editInstitutionBranding);
        }

        public RedirectToRouteResult SaveInstitutionBranding(InstitutionBranding editInstitutionBranding,
            bool clearImage)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var institutionBranding = _institutionBranding.FirstOrDefault(x =>
                        x.Institution.Id == editInstitutionBranding.InstitutionId);

                    if (clearImage && institutionBranding != null)
                    {
                        institutionBranding.LogoFileName = null;
                        uow.Update(institutionBranding);
                        uow.Commit();
                        transaction.Commit();
                        if (CurrentUser.InstitutionId == editInstitutionBranding.InstitutionId)
                        {
                            _adminContext.ReloadAdminInstitution(editInstitutionBranding.InstitutionId, CurrentUser.Id);
                        }

                        return RedirectToAction("Edit", "InstitutionBranding",
                            new { institutionId = editInstitutionBranding.InstitutionId });
                    }

                    if (institutionBranding == null)
                    {
                        institutionBranding = new Core.Institution.InstitutionBranding
                        {
                            Institution = new Institution { Id = editInstitutionBranding.InstitutionId }
                        };
                    }

                    institutionBranding.InstitutionDisplayName = editInstitutionBranding.InstitutionDisplayName;
                    institutionBranding.Message = editInstitutionBranding.Message;

                    if (!string.IsNullOrWhiteSpace(editInstitutionBranding.LogoFileName))
                    {
                        institutionBranding.LogoFileName = editInstitutionBranding.LogoFileName;
                    }

                    uow.SaveOrUpdate(institutionBranding);
                    uow.Commit();
                    transaction.Commit();
                    if (CurrentUser.InstitutionId == editInstitutionBranding.InstitutionId)
                    {
                        _adminContext.ReloadAdminInstitution(editInstitutionBranding.InstitutionId, CurrentUser.Id);
                    }

                    return RedirectToAction("Detail", "InstitutionBranding",
                        new { institutionId = editInstitutionBranding.InstitutionId });
                }
            }
        }
    }
}