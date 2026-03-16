#region

using System;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.PdaPromotion;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class PdaPromotionController : R2AdminBaseController
    {
        private readonly ILog<PdaPromotionController> _log;
        private readonly PdaPromotionsService _pdaPromotionsService;

        public PdaPromotionController(
            ILog<PdaPromotionController> log
            , IAuthenticationContext authenticationContext
            , PdaPromotionsService pdaPromotionsService)
            : base(authenticationContext)
        {
            _log = log;
            _pdaPromotionsService = pdaPromotionsService;
        }

        public ActionResult List()
        {
            var model = _pdaPromotionsService.GetPdaPromotions();
            return View(model);
        }

        public ActionResult Delete(int pdaPromotionId)
        {
            var deleted = _pdaPromotionsService.DeletePdaPromotion(pdaPromotionId);
            _log.DebugFormat("deleted: {0}", deleted);
            return RedirectToAction("List");
        }

        public ActionResult Add()
        {
            var model = new PdaPromotionModel { StartDate = DateTime.Now.AddDays(1) };
            model.EndDate = model.StartDate.AddMonths(1);
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(PdaPromotionModel model)
        {
            //TODO: Make sure there is not already a PDA promotion before you save. 
            if (_pdaPromotionsService.SavePdaPromotion(model))
            {
                return RedirectToAction("List");
            }

            ModelState.AddModelError("Name",
                "This PDA Promotion will overlap with another pda promotion. Please change the dates.");
            return View(model);
        }

        public ActionResult Edit(int pdaPromotionId)
        {
            var model = _pdaPromotionsService.GetPdaPromotion(pdaPromotionId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PdaPromotionModel model)
        {
            //TODO: Make sure there is not already a PDA promotion (Ok if it is iteself)before you save. 
            if (_pdaPromotionsService.SavePdaPromotion(model))
            {
                return RedirectToAction("List");
            }

            ModelState.AddModelError("Name",
                "This PDA Promotion will overlap with another pda promotion. Please change the dates.");
            return View(model);
        }
    }
}