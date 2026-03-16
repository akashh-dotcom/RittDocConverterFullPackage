#region

using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.InstitutionReferrer;
using InstitutionReferrer = R2V2.Core.Institution.InstitutionReferrer;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class InstitutionReferrerController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IQueryable<InstitutionReferrer> _institutionReferrer;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public InstitutionReferrerController(
            IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IQueryable<InstitutionReferrer> institutionReferrer
            , IUnitOfWorkProvider unitOfWorkProvider
        )
            : base(authenticationContext)
        {
            _adminContext = adminContext;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutionReferrer = institutionReferrer;
        }

        public ActionResult List(int institutionId)
        {
            return View(BuildReferrerList(institutionId));
        }

        public ActionResult Add(int institutionId)
        {
            var referrerList = BuildReferrerList(institutionId);

            referrerList.EditReferrer = new Models.InstitutionReferrer.InstitutionReferrer
                { InstitutionId = institutionId, ValidReferer = "http://" };

            return View(referrerList);
        }

        [HttpPost]
        public ActionResult Add(InstitutionReferrerList institutionReferrerList)
        {
            if (ModelState.IsValid)
            {
                if (IsValidUrl(institutionReferrerList) || institutionReferrerList.EditReferrer.ValidReferer ==
                    institutionReferrerList.EditReferrer.VerifiedReferrer)
                {
                    return Save(institutionReferrerList);
                }
            }

            var referrerList = BuildReferrerList(institutionReferrerList.InstitutionId);
            referrerList.EditReferrer = new Models.InstitutionReferrer.InstitutionReferrer
                { InstitutionId = institutionReferrerList.InstitutionId };

            if (institutionReferrerList.EditReferrer.WasVerified)
            {
                referrerList.EditReferrer.VerifiedReferrer = institutionReferrerList.EditReferrer.ValidReferer;
            }

            return View(referrerList);
        }

        public ActionResult Edit(int institutionId, int id)
        {
            var referrerList = BuildReferrerList(institutionId);
            referrerList.EditReferrer = referrerList.InstitutionReferrers.FirstOrDefault(x => x.ValidReferrerId == id);

            return View(referrerList);
        }

        [HttpPost]
        public ActionResult Edit(InstitutionReferrerList institutionReferrerList)
        {
            if (ModelState.IsValid)
            {
                if (IsValidUrl(institutionReferrerList) || institutionReferrerList.EditReferrer.ValidReferer ==
                    institutionReferrerList.EditReferrer.VerifiedReferrer)
                {
                    return Save(institutionReferrerList);
                }
            }

            var referrerList = BuildReferrerList(institutionReferrerList.InstitutionId);
            referrerList.EditReferrer = referrerList.InstitutionReferrers.FirstOrDefault(x =>
                x.ValidReferrerId == institutionReferrerList.EditReferrer.ValidReferrerId);

            if (institutionReferrerList.EditReferrer.WasVerified)
            {
                referrerList.EditReferrer.VerifiedReferrer = institutionReferrerList.EditReferrer.ValidReferer;
            }

            return View(referrerList);
        }

        private bool IsValidUrl(InstitutionReferrerList institutionReferrerList)
        {
            //Check if valid URL
            Uri uri;
            if (!Uri.TryCreate(institutionReferrerList.EditReferrer.ValidReferer, UriKind.Absolute, out uri))
            {
                ModelState.AddModelError("EditReferrer.ValidReferer",
                    string.Format(
                        "This string you entered is not a Valid URL. Please make corrections and try again."));
                return false;
            }

            if (ModelState.IsValid)
            {
                //Check if URL is already in system
                var overlappingReferrers =
                    _institutionReferrer.Where(x =>
                        (institutionReferrerList.EditReferrer.ValidReferer.Contains(x.ValidReferer) ||
                         x.ValidReferer.Contains(institutionReferrerList.EditReferrer.ValidReferer))
                        && x.Id != institutionReferrerList.EditReferrer.ValidReferrerId
                    );
                if (overlappingReferrers.Any())
                {
                    var adminInstitution = _adminContext.GetAdminInstitution(institutionReferrerList.InstitutionId);
                    var errorMessage = new StringBuilder()
                        .Append("<ul>")
                        .AppendFormat("<li>This Url Referrer belongs to another Institution. Account Number : {0}</li>",
                            overlappingReferrers.FirstOrDefault().Institution.AccountNumber).AppendLine()
                        .AppendFormat(
                            "<li>If this is valid, make sure the Institution includes DB={0} in the querystring.</li>",
                            adminInstitution.AccountNumber).AppendLine()
                        .Append("<li>It is the only way we can differentiate between the instititons.</li>")
                        .Append("<ul>")
                        .ToString();

                    ModelState.AddModelError("EditReferrer.ValidReferer",
                        $"This Url Referrer belongs to another Institution. Account Number : {overlappingReferrers.FirstOrDefault().Institution.AccountNumber}");
                    ModelState.AddModelError("EditReferrer.ValidReferer",
                        $"If this is valid, make sure the Institution includes DB={adminInstitution.AccountNumber} in the querystring.");
                    ModelState.AddModelError("EditReferrer.ValidReferer",
                        "It is the only way we can differentiate between the instititons.");


                    institutionReferrerList.EditReferrer.WasVerified = true;
                    return false;
                }
            }

            return true;
        }

        public ActionResult Delete(int institutionId, int id)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var institutionReferrer = _institutionReferrer.FirstOrDefault(x => x.Id == id);
                    if (institutionReferrer != null)
                    {
                        uow.Delete(institutionReferrer);
                        uow.Commit();
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
            }

            return RedirectToAction("List", new { institutionId });
        }

        private ActionResult Save(InstitutionReferrerList institutionReferrerList)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var referrerToSave = new InstitutionReferrer();
                    if (institutionReferrerList.EditReferrer.ValidReferrerId > 0)
                    {
                        referrerToSave =
                            _institutionReferrer.FirstOrDefault(x =>
                                x.Id == institutionReferrerList.EditReferrer.ValidReferrerId) ??
                            new InstitutionReferrer();
                    }

                    referrerToSave.InstitutionId = institutionReferrerList.InstitutionId;
                    referrerToSave.ValidReferer = institutionReferrerList.EditReferrer.ValidReferer;


                    uow.SaveOrUpdate(referrerToSave);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            return RedirectToAction("List", new { institutionReferrerList.InstitutionId });
        }


        private InstitutionReferrerList BuildReferrerList(int institutionId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var institutionReferrersList = _institutionReferrer.Where(x => x.InstitutionId == institutionId).ToList();
            var institutionReferrers = institutionReferrersList.ToInstitutionReferrers();

            return new InstitutionReferrerList(adminInstitution) { InstitutionReferrers = institutionReferrers };
        }
    }
}