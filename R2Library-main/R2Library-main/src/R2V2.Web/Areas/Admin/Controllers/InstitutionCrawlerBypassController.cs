#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.InstitutionCrawlerBypass;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class InstitutionCrawlerBypassController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly InstitutionCrawlerBypassFactory _institutionCrawlerBypassFactory;

        public InstitutionCrawlerBypassController(
            IAuthenticationContext authenticationContext
            , InstitutionCrawlerBypassFactory institutionCrawlerBypassFactory
            , IAdminContext adminContext
        ) : base(authenticationContext)
        {
            _institutionCrawlerBypassFactory = institutionCrawlerBypassFactory;
            _adminContext = adminContext;
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Index(int institutionId)
        {
            var model = GetInstitutionTrustedAuthList(institutionId);

            return View(model);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Add(int institutionId)
        {
            return View(new InstitutionCrawlerBypassDetail(_adminContext.GetAdminInstitution(institutionId)));
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        [HttpPost]
        public ActionResult Add(int institutionId, InstitutionCrawlerBypassDetail institutionCrawlerBypassDetail)
        {
            var institutionCrawlerBypass =
                ConvertToDbInstitutionCrawlerBypass(institutionCrawlerBypassDetail.InstitutionCrawlerBypass,
                    institutionId);

            var saveMessage = _institutionCrawlerBypassFactory.SaveInstitutionCrawlerBypass(institutionCrawlerBypass);
            switch (saveMessage)
            {
                case "success":
                    return RedirectToAction("Index", new { institutionId });
                case "fail":
                    ModelState.AddModelError("InstitutionCrawlerBypass", "Error saving new Crawler Bypass");
                    return
                        View(new InstitutionCrawlerBypassDetail(_adminContext.GetAdminInstitution(institutionId),
                            institutionCrawlerBypass));
                default:
                    ModelState.AddModelError("InstitutionCrawlerBypass",
                        $"This conflicts with another institution : {saveMessage}");
                    return View(new InstitutionCrawlerBypassDetail(_adminContext.GetAdminInstitution(institutionId),
                        institutionCrawlerBypass));
            }
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Edit(int institutionId, int institutionCrawlerBypassId)
        {
            var dbInstitutionCrawlerBypass =
                _institutionCrawlerBypassFactory.GetInstitutionCrawlerBypass(institutionCrawlerBypassId);

            return View(new InstitutionCrawlerBypassDetail(_adminContext.GetAdminInstitution(institutionId),
                dbInstitutionCrawlerBypass));
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        [HttpPost]
        public ActionResult Edit(int institutionId, InstitutionCrawlerBypassDetail institutionCrawlerBypassDetail)
        {
            var dbInstitutionCrawlerBypass =
                ConvertToDbInstitutionCrawlerBypass(institutionCrawlerBypassDetail.InstitutionCrawlerBypass,
                    institutionId);

            var saveMessage = _institutionCrawlerBypassFactory.SaveInstitutionCrawlerBypass(dbInstitutionCrawlerBypass);
            switch (saveMessage)
            {
                case "success":
                    return RedirectToAction("Index", new { institutionId });
                case "fail":
                    ModelState.AddModelError("InstitutionCrawlerBypass", "Error saving new Crawler Bypass");
                    return View(new InstitutionCrawlerBypassDetail(_adminContext.GetAdminInstitution(institutionId),
                        dbInstitutionCrawlerBypass));
                default:
                    ModelState.AddModelError("InstitutionCrawlerBypass",
                        $"This conflicts with another institution : {saveMessage}");

                    return View(new InstitutionCrawlerBypassDetail(_adminContext.GetAdminInstitution(institutionId),
                        dbInstitutionCrawlerBypass));
            }
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Delete(int institutionId, int institutionCrawlerBypassId)
        {
            _institutionCrawlerBypassFactory.DeletInstitutionCrawlerBypass(institutionCrawlerBypassId);

            return RedirectToAction("Index", new { institutionId });
        }


        private InstitutionCrawlerBypassList GetInstitutionTrustedAuthList(int institutionId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var dbInstitutionCrawlerBypass =
                _institutionCrawlerBypassFactory.GetInstitutionCrawlerBypasses(institutionId);

            return new InstitutionCrawlerBypassList(adminInstitution, dbInstitutionCrawlerBypass);
        }

        private InstitutionCrawlerBypass ConvertToDbInstitutionCrawlerBypass(
            InstitutionCrawlerBypassModel institutionCrawlerBypassModel, int institutionId)
        {
            var dbInstitutionCrawlerBypass = new InstitutionCrawlerBypass();
            if (institutionCrawlerBypassModel.Id > 0)
            {
                dbInstitutionCrawlerBypass =
                    _institutionCrawlerBypassFactory.GetInstitutionCrawlerBypass(institutionCrawlerBypassModel.Id);

                dbInstitutionCrawlerBypass.OctetA = institutionCrawlerBypassModel.OctetA;
                dbInstitutionCrawlerBypass.OctetB = institutionCrawlerBypassModel.OctetB;
                dbInstitutionCrawlerBypass.OctetC = institutionCrawlerBypassModel.OctetC;
                dbInstitutionCrawlerBypass.OctetD = institutionCrawlerBypassModel.OctetD;

                dbInstitutionCrawlerBypass.PopulateDecimal();

                dbInstitutionCrawlerBypass.UserAgent = institutionCrawlerBypassModel.UserAgent;
                return dbInstitutionCrawlerBypass;
            }

            dbInstitutionCrawlerBypass.OctetA = institutionCrawlerBypassModel.OctetA;
            dbInstitutionCrawlerBypass.OctetB = institutionCrawlerBypassModel.OctetB;
            dbInstitutionCrawlerBypass.OctetC = institutionCrawlerBypassModel.OctetC;
            dbInstitutionCrawlerBypass.OctetD = institutionCrawlerBypassModel.OctetD;

            dbInstitutionCrawlerBypass.PopulateDecimal();

            dbInstitutionCrawlerBypass.UserAgent = institutionCrawlerBypassModel.UserAgent;
            dbInstitutionCrawlerBypass.InstitutionId = institutionId;

            return dbInstitutionCrawlerBypass;
        }
    }
}