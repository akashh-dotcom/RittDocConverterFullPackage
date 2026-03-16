#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Cms;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.SystemInformation;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class SystemInformationController : R2AdminBaseController
    {
        private readonly CmsService _cmsService;

        public SystemInformationController(
            IAuthenticationContext authenticationContext
            , CmsService cmsService
        ) : base(authenticationContext)
        {
            _cmsService = cmsService;
        }

        public ActionResult Index(string codeName)
        {
            var html = _cmsService.GetSystemInformationText(codeName);
            var model = new AdminCmsItem
            {
                InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault(),
                Html = html
            };
            return View(model);
        }

        public ActionResult Outreach(int institutionId)
        {
            return View(new Outreach { InstitutionId = institutionId });
        }

        public ActionResult NewsletterSignup(int institutionId)
        {
            return View(new NewsletterSignup { InstitutionId = institutionId });
        }

        public ActionResult Documentation(int institutionId)
        {
            return View(new Documentation { InstitutionId = institutionId });
        }
    }
}