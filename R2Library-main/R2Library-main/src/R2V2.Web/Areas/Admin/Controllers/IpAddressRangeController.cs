#region

using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.IpAddressRange;
using R2V2.Web.Areas.Admin.Services;
using WebIpRange = R2V2.Web.Areas.Admin.Models.IpAddressRange.WebIpRange;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class IpAddressRangeController : R2AdminBaseController
    {
        private readonly IpAddressRangeService _ipAddressRangeService;

        public IpAddressRangeController(
            IAuthenticationContext authenticationContext
            , IpAddressRangeService ipAddressRangeService
        )
            : base(authenticationContext)
        {
            _ipAddressRangeService = ipAddressRangeService;
        }

        public ActionResult List(int institutionId)
        {
            var institutionIpRange = _ipAddressRangeService.GetInstitutionIpRangeWithConflicts(institutionId);

            return View(institutionIpRange);
        }

        public ActionResult Add(int institutionId)
        {
            var institutionIpRange = _ipAddressRangeService.GetInstitutionIpRange(institutionId);

            institutionIpRange.EditIpAddressRange = new WebIpRange();


            return View(institutionIpRange);
        }

        [HttpPost]
        public ActionResult Add(InstitutionIpRanges institutionIpRange)
        {
            if (ModelState.IsValid)
            {
                var errorMessage = _ipAddressRangeService.SaveIpAddressRange(institutionIpRange.ToIpAddressRange(),
                    IsRittenhouseAdmin(), institutionIpRange.InstitutionId);

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    return RedirectToAction("List", new { institutionId = institutionIpRange.InstitutionId });
                }

                ModelState.AddModelError("EditIpAddressRange", errorMessage);
            }

            var model = _ipAddressRangeService.GetInstitutionIpRange(institutionIpRange.InstitutionId);
            model.EditIpAddressRange = institutionIpRange.EditIpAddressRange;

            return View(model);
        }

        public ActionResult Edit(int institutionId, int id)
        {
            var model = _ipAddressRangeService.GetInstitutionIpRangeForEdit(institutionId, id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(InstitutionIpRanges institutionIpRange)
        {
            if (ModelState.IsValid)
            {
                var errorMessage = _ipAddressRangeService.SaveIpAddressRange(institutionIpRange.ToIpAddressRange(),
                    IsRittenhouseAdmin(), institutionIpRange.InstitutionId);

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    return RedirectToAction("List", new { institutionId = institutionIpRange.InstitutionId });
                }

                ModelState.AddModelError("EditIpAddressRange", errorMessage);
            }

            var model = _ipAddressRangeService.GetInstitutionIpRangeForEdit(institutionIpRange.InstitutionId,
                institutionIpRange.EditIpAddressRange.Id);

            return View(model);
        }

        public ActionResult Delete(int institutionId, int id)
        {
            _ipAddressRangeService.Delete(institutionId, id);

            return RedirectToAction("List", new { institutionId });
        }

        public ActionResult BulkDelete(int institutionId, string ipRangeIdsToDelete)
        {
            var model = _ipAddressRangeService.GetBulkRemoveIpRanges(institutionId, ipRangeIdsToDelete);
            return View(model);
        }

        [HttpPost]
        public ActionResult BulkDelete(BulkRemoveIpRanges bulkRemoveIpRanges)
        {
            bulkRemoveIpRanges.FormattedIpAddress = _ipAddressRangeService.BulkDelete(bulkRemoveIpRanges.InstitutionId,
                bulkRemoveIpRanges.IpRangeIdsToDelete);
            bulkRemoveIpRanges.IpRangeIdsToDelete = null;

            return View(bulkRemoveIpRanges);
        }

        public ActionResult Consolidate(int institutionId, bool getConflicts)
        {
            var model = _ipAddressRangeService.GetConsolidatedIpRanges(institutionId, getConflicts);

            return View(model);
        }

        public ActionResult ConsolidateRange(int institutionId, int startIpAddressRangeId, int endIpAddressRangeId,
            bool getConflicts)
        {
            var errorMessage = _ipAddressRangeService.ConsolidateIpAddressRanages(institutionId, startIpAddressRangeId,
                endIpAddressRangeId, CurrentUser.IsRittenhouseAdmin());

            var model = _ipAddressRangeService.GetConsolidatedIpRanges(institutionId, getConflicts);
            model.ErrorMessage = errorMessage;

            if (model.ConsolidateIpAddressRanges.Any())
            {
                return View("Consolidate", model);
            }

            return RedirectToAction("List", new { institutionId });
        }
    }
}