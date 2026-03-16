#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.Promotion;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class PromotionController : R2AdminBaseController
    {
        private readonly ILog<PromotionController> _log;
        private readonly IQueryable<Product> _products;
        private readonly PromotionsService _promotionsService;

        public PromotionController(ILog<PromotionController> log
            , IAuthenticationContext authenticationContext
            , PromotionsService promotionsService
            , IQueryable<Product> products
        )
            : base(authenticationContext)
        {
            _log = log;
            _promotionsService = promotionsService;
            _products = products;
        }

        /// <summary>
        /// </summary>
        public ActionResult List()
        {
            var model = _promotionsService.GetPromotions();
            return View(model);
        }

        public ActionResult Delete(int promotionId)
        {
            var deleted = _promotionsService.DeletePromotion(promotionId);
            _log.DebugFormat("deleted: {0}", deleted);
            return RedirectToAction("List");
        }

        public ActionResult Add()
        {
            var model = new PromotionModel(_products) { StartDate = DateTime.Now.AddDays(1) };
            model.EndDate = model.StartDate.AddMonths(1);
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(PromotionModel model)
        {
            if (model.MaximumUses == 0)
            {
                ModelState.AddModelError("MaximumUses",
                    @"Maximum Uses per Institution must be > 0. It will be unusable with 0 uses.");
                ModelState.Remove("StartDate");
                ModelState.Remove("EndDate");
            }
            else if (_promotionsService.SavePromotion(model))
            {
                return RedirectToAction("List");
            }

            model.SetPromotionProductions(_products);
            return View(model);
        }

        public ActionResult Edit(int promotionId)
        {
            var model = _promotionsService.GetPromotion(promotionId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PromotionModel model)
        {
            if (_promotionsService.SavePromotion(model))
            {
                return RedirectToAction("List");
            }

            model.SetPromotionProductions(_products);
            return View(model);
        }
    }
}