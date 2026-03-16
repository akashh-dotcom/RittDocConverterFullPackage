#region

using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Cms;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.AccessAndDiscoverability;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class AccessAndDiscoverabilityController : R2AdminBaseController
    {
        private readonly CmsService _cmsService;

        public AccessAndDiscoverabilityController(
            IAuthenticationContext authenticationContext
            , CmsService cmsService
        ) : base(authenticationContext)
        {
            _cmsService = cmsService;
        }

        public ActionResult Index(int institutionId, string codeName, bool isExternalLink = false)
        {
            if (isExternalLink)
            {
                var url = _cmsService.GetAccessAndDiscoverabilityText(codeName);
                return Redirect(url);
            }

            var html = _cmsService.GetAccessAndDiscoverabilityText(codeName);
            var model = new AdminCmsItem
            {
                InstitutionId = institutionId,
                Html = html
            };
            return View(model);
        }

        public ActionResult Integration(int institutionId)
        {
            return View(new Integration { InstitutionId = institutionId });
        }

        public ActionResult Access(int institutionId)
        {
            return View(new Access { InstitutionId = institutionId });
        }
    }
}