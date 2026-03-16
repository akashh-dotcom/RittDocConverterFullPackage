#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.WebServiceAuthentication;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class WebServiceAuthenticationController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;

        private readonly WebServiceAuthenticationFactory _webServiceAuthenticationFactory;
        //
        // GET: /Admin/WebServiceAuthentication/

        public WebServiceAuthenticationController(
            IAuthenticationContext authenticationContext
            , WebServiceAuthenticationFactory webServiceAuthenticationFactory
            , IAdminContext adminContext
        )
            : base(authenticationContext)
        {
            _webServiceAuthenticationFactory = webServiceAuthenticationFactory;
            _adminContext = adminContext;
        }


        public ActionResult List(int institutionId)
        {
            var model = GetInstitutionTrustedAuthList(institutionId);

            return View(model);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Add(int institutionId)
        {
            return View(
                new InstitutionWebServiceAuthenticationDetail(_adminContext.GetAdminInstitution(institutionId)));
        }

        [HttpPost]
        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Add(int institutionId,
            InstitutionWebServiceAuthenticationDetail institutionWebServiceAuthenticationDetail, string action)
        {
            if (action == "generatekey")
            {
                var randomKey = GetRandomKey(24);
                institutionWebServiceAuthenticationDetail.WebServiceAuthentication.AuthenticationKey = randomKey;
            }

            var dbWebServiceAuthentication =
                ConvertToDbTrustedAuth(institutionWebServiceAuthenticationDetail.WebServiceAuthentication,
                    institutionId);
            if (dbWebServiceAuthentication.IpNumber == 0)
            {
                ModelState.AddModelError("WebServiceAuthentication",
                    "Please enter an IP address before generating a Web Serivice Authenication Key");
                return View(new InstitutionWebServiceAuthenticationDetail(
                    _adminContext.GetAdminInstitution(institutionId), dbWebServiceAuthentication));
            }

            var saveMessage = _webServiceAuthenticationFactory.SaveWebServiceAuthentication(dbWebServiceAuthentication);
            switch (saveMessage)
            {
                case "success":
                    if (action == "generatekey")
                    {
                        return RedirectToAction("Edit",
                            new { institutionId, institutionTrustedAuthenticationId = dbWebServiceAuthentication.Id });
                    }

                    return RedirectToAction("List", new { institutionId });
                case "fail":
                    ModelState.AddModelError("WebServiceAuthentication", "Error saving new Trusted Authenication");
                    return View(new InstitutionWebServiceAuthenticationDetail(
                        _adminContext.GetAdminInstitution(institutionId), dbWebServiceAuthentication));
                default:
                    ModelState.AddModelError("WebServiceAuthentication",
                        $"This conflicts with another institution : {saveMessage}");

                    return View(new InstitutionWebServiceAuthenticationDetail(
                        _adminContext.GetAdminInstitution(institutionId), dbWebServiceAuthentication));
            }
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Delete(int institutionId, int institutionTrustedAuthenticationId)
        {
            _webServiceAuthenticationFactory.DeleteWebServiceAuthentication(institutionTrustedAuthenticationId);

            return RedirectToAction("List", new { institutionId });
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Edit(int institutionId, int institutionTrustedAuthenticationId)
        {
            var dbInstitutionTrustedAuthentication =
                _webServiceAuthenticationFactory.GetWebServiceAuthentication(institutionTrustedAuthenticationId);

            return View(new InstitutionWebServiceAuthenticationDetail(_adminContext.GetAdminInstitution(institutionId),
                dbInstitutionTrustedAuthentication));
        }

        [HttpPost]
        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.SALESASSOC })]
        public ActionResult Edit(int institutionId,
            InstitutionWebServiceAuthenticationDetail institutionWebServiceAuthenticationDetail, string action)
        {
            if (action == "generatekey")
            {
                var randomKey = GetRandomKey(24);
                institutionWebServiceAuthenticationDetail.WebServiceAuthentication.AuthenticationKey = randomKey;
            }

            var dbWebServiceAuthentication =
                ConvertToDbTrustedAuth(institutionWebServiceAuthenticationDetail.WebServiceAuthentication,
                    institutionId);

            if (dbWebServiceAuthentication.IpNumber == 0)
            {
                ModelState.AddModelError("WebServiceAuthentication",
                    "Please enter an IP address before generating a Web Serivice Authenication Key");
                return View(new InstitutionWebServiceAuthenticationDetail(
                    _adminContext.GetAdminInstitution(institutionId), dbWebServiceAuthentication));
            }

            var saveMessage = _webServiceAuthenticationFactory.SaveWebServiceAuthentication(dbWebServiceAuthentication);
            switch (saveMessage)
            {
                case "success":
                    if (action == "generatekey")
                    {
                        return RedirectToAction("Edit",
                            new { institutionId, institutionTrustedAuthenticationId = dbWebServiceAuthentication.Id });
                    }

                    return RedirectToAction("List", new { institutionId });
                case "fail":
                    ModelState.AddModelError("WebServiceAuthentication",
                        "Error saving new Trusted Serivce Authenication");
                    return View(new InstitutionWebServiceAuthenticationDetail(
                        _adminContext.GetAdminInstitution(institutionId), dbWebServiceAuthentication));
                default:
                    ModelState.AddModelError("WebServiceAuthentication",
                        $"This conflicts with another institution : {saveMessage}");

                    return View(new InstitutionWebServiceAuthenticationDetail(
                        _adminContext.GetAdminInstitution(institutionId), dbWebServiceAuthentication));
            }
        }


        private InstitutionWebServiceAuthenticationList GetInstitutionTrustedAuthList(int institutionId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var dbInstitutionTrustedAuths =
                _webServiceAuthenticationFactory.GetWebServiceAuthentications(institutionId);

            return new InstitutionWebServiceAuthenticationList(adminInstitution, dbInstitutionTrustedAuths);
        }

        private WebServiceAuthentication ConvertToDbTrustedAuth(
            InstitutionWebServiceAuthentication institutionWebServiceAuthentication, int institutionId)
        {
            var dbWebServiceAuthentication = new WebServiceAuthentication();
            if (institutionWebServiceAuthentication.Id > 0)
            {
                dbWebServiceAuthentication =
                    _webServiceAuthenticationFactory.GetWebServiceAuthentication(institutionWebServiceAuthentication
                        .Id);

                dbWebServiceAuthentication.OctetA = institutionWebServiceAuthentication.OctetA;
                dbWebServiceAuthentication.OctetB = institutionWebServiceAuthentication.OctetB;
                dbWebServiceAuthentication.OctetC = institutionWebServiceAuthentication.OctetC;
                dbWebServiceAuthentication.OctetD = institutionWebServiceAuthentication.OctetD;

                dbWebServiceAuthentication.AuthenticationKey = institutionWebServiceAuthentication.AuthenticationKey;

                dbWebServiceAuthentication.PopulateDecimal();

                return dbWebServiceAuthentication;
            }

            dbWebServiceAuthentication.OctetA = institutionWebServiceAuthentication.OctetA;
            dbWebServiceAuthentication.OctetB = institutionWebServiceAuthentication.OctetB;
            dbWebServiceAuthentication.OctetC = institutionWebServiceAuthentication.OctetC;
            dbWebServiceAuthentication.OctetD = institutionWebServiceAuthentication.OctetD;

            dbWebServiceAuthentication.AuthenticationKey = institutionWebServiceAuthentication.AuthenticationKey;
            dbWebServiceAuthentication.InstitutionId = institutionId;
            dbWebServiceAuthentication.PopulateDecimal();

            return dbWebServiceAuthentication;
        }
    }
}