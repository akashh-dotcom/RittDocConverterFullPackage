#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Institution;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.PdaRules;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers.CollectionManagement
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.INSTADMIN, RoleCode.SALESASSOC })]
    public class PdaController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly ICollectionService _collectionService;
        private readonly ILog<PdaController> _log;
        private readonly IOrderService _orderService;
        private readonly PatronDrivenAcquisitionService _patronDrivenAcquisitionService;
        private readonly PdaService _pdaService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly PdaProfileService _profileService;
        private readonly ISpecialtyService _specialtyService;

        public PdaController(ILog<PdaController> log
            , IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IOrderService orderService
            , PatronDrivenAcquisitionService patronDrivenAcquisitionService
            , PdaService pdaService
            , PdaProfileService profileService
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , ICollectionService collectionService
        )
            : base(authenticationContext)
        {
            _log = log;
            _adminContext = adminContext;
            _orderService = orderService;
            _patronDrivenAcquisitionService = patronDrivenAcquisitionService;
            _pdaService = pdaService;
            _profileService = profileService;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
        }

        public ActionResult PdaAdd(CollectionManagementQuery collectionManagementQuery, string addPdaToCollection)
        {
            var tempCollectionManagementQuery =
                TempData.GetItem<CollectionManagementQuery>("CollectionManagementQuery");
            if (tempCollectionManagementQuery != null)
            {
                collectionManagementQuery = tempCollectionManagementQuery;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
            }

            // ReSharper disable once PossibleNullReferenceException
            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var institutionResource = _orderService.GetInstitutionResource(collectionManagementQuery.InstitutionId,
                collectionManagementQuery.ResourceId, collectionManagementQuery.CartId);

            var model = new CollectionAdd
            {
                InstitutionResource = institutionResource,
                NumberOfLicenses = 1,
                ResourceQuery = collectionManagementQuery
            };
            if (_pdaService.ShowPdaTrialConvert(adminInstitution, collectionManagementQuery))
            {
                return View("PdaTrialConvertModal", model);
            }

            if (_pdaService.ShowEula(adminInstitution, collectionManagementQuery))
            {
                return View("../Checkout/EulaModal", model);
            }

            if (_pdaService.ShowPdaEula(adminInstitution, collectionManagementQuery))
            {
                return View("../Checkout/PdaEulaModal", model);
            }

            // todo: replace magic string
            if (!string.IsNullOrWhiteSpace(addPdaToCollection) && addPdaToCollection.ToLower() == "yes")
            {
                _pdaService.ConvertAndSignEulasIfNeeded(adminInstitution, collectionManagementQuery);

                _patronDrivenAcquisitionService.AddPartonDrivenAcquisition(collectionManagementQuery.ResourceId,
                    adminInstitution.Id, AuthenticatedInstitution.User.Id);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId,
                    AuthenticatedInstitution.User.Id);

                model.KeepShoppingLink = string.Format("{0}#{1}",
                    Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues()),
                    collectionManagementQuery.ResourceId);
                model.ViewMyPdaCollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, IncludePdaResources = true });

                return View("PdaAdded", model);
            }

            // todo: replace magic string
            return View("PdaAdd", model);
        }

        public ActionResult PdaTrialConvertModal(CollectionManagementQuery collectionManagementQuery)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
            }

            collectionManagementQuery.TrialConvert = true;

            var pdaRuleModel = TempData.GetItem<PdaRuleModel>("PdaRule");
            if (pdaRuleModel != null)
            {
                return RedirectToAction("SaveRule",
                    new { collectionManagementQuery.InstitutionId, TrialConvert = true });
            }

            if (!string.IsNullOrWhiteSpace(collectionManagementQuery.Resources))
            {
                //Needed because collectionManagementQuery can become large and cause a 404.15 error
                TempData.AddItem("CollectionManagementQuery", collectionManagementQuery);
                return RedirectToAction("BulkAddPda");
            }

            return RedirectToAction("PdaAdd", collectionManagementQuery);
        }

        public ActionResult PdaRemove(CollectionManagementQuery collectionManagementQuery, string removeConfirmed)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
            }

            var model = new CollectionAdd
            {
                //InstitutionResource = institutionResource,
                NumberOfLicenses = 1,
                ResourceQuery = collectionManagementQuery
            };

            if (collectionManagementQuery.ResourceId == 0)
            {
                return View("PdaRemove", model);
            }

            model.InstitutionResource = _orderService.GetInstitutionResource(collectionManagementQuery.InstitutionId,
                collectionManagementQuery.ResourceId, collectionManagementQuery.CartId);

            if (!string.IsNullOrWhiteSpace(removeConfirmed) && removeConfirmed.ToLower() == "yes")
            {
                var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

                var deleted =
                    _patronDrivenAcquisitionService.DeletePartonDrivenAcquisition(collectionManagementQuery.ResourceId,
                        adminInstitution.Id, CurrentUser);

                _log.DebugFormat("PdaRemove() - ResourceId: {0}, InstitutionId: {1}, deleted: {3}",
                    collectionManagementQuery.ResourceId, adminInstitution.Id, deleted);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId,
                    AuthenticatedInstitution.User.Id);

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}#{collectionManagementQuery.ResourceId}";
                model.ViewMyPdaCollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, IncludePdaResources = true });

                return View("PdaRemoved", model);
            }

            return View("PdaRemove", model);
        }

        public ActionResult BulkAddPda(CollectionManagementQuery collectionManagementQuery, string bulkAddPda)
        {
            var tempCollectionManagementQuery =
                TempData.GetItem<CollectionManagementQuery>("CollectionManagementQuery");
            if (tempCollectionManagementQuery != null)
            {
                collectionManagementQuery = tempCollectionManagementQuery;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
            }

            // ReSharper disable once PossibleNullReferenceException
            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var model = new BulkAddToCart(adminInstitution) { ResourceQuery = collectionManagementQuery };

            if (adminInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id &&
                !collectionManagementQuery.TrialConvert)
            {
                return View("PdaTrialConvertModal", new CollectionAdd { ResourceQuery = collectionManagementQuery });
            }

            if (!adminInstitution.IsEulaSigned && !collectionManagementQuery.EulaSigned)
            {
                return View("../Checkout/EulaModal", new CollectionAdd { ResourceQuery = collectionManagementQuery });
            }

            if (!adminInstitution.IsPdaEulaSigned && !collectionManagementQuery.PdaEulaSigned)
            {
                return View("../Checkout/PdaEulaModal",
                    new CollectionAdd { ResourceQuery = collectionManagementQuery });
            }


            var resourceIds = collectionManagementQuery.GetResourceIds().ToList();
            if (resourceIds.Any())
            {
                foreach (var institutionResource in resourceIds.Select(resourceId =>
                             _orderService.GetInstitutionResource(collectionManagementQuery.InstitutionId, resourceId,
                                 collectionManagementQuery.CartId)))
                {
                    if (institutionResource.IsPdaEligible && institutionResource.IsForSale)
                    {
                        model.AddResource(institutionResource);
                    }
                    else
                    {
                        model.AddExcludedResource(institutionResource);
                    }
                }
            }
            else
            {
                List<string> isbnsNotFound;
                var institutionResources =
                    _orderService.GetInstitutionResources(collectionManagementQuery, out isbnsNotFound);

                model.IsbnsNotFound = string.Join(", ", isbnsNotFound);
                foreach (var institutionResource in institutionResources)
                {
                    if (institutionResource.IsPdaEligible && institutionResource.PdaCreatedDate == null &&
                        institutionResource.IsForSale)
                    {
                        model.AddResource(institutionResource);
                    }
                    else
                    {
                        model.AddExcludedResource(institutionResource);
                    }
                }
            }

            if (bulkAddPda == "yes")
            {
                if (adminInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id &&
                    collectionManagementQuery.TrialConvert)
                {
                    _pdaService.ConvertTrial(collectionManagementQuery.InstitutionId);
                }

                if (!adminInstitution.IsEulaSigned && collectionManagementQuery.EulaSigned)
                {
                    _pdaService.SignEula(collectionManagementQuery.InstitutionId);
                }

                if (!adminInstitution.IsPdaEulaSigned && collectionManagementQuery.PdaEulaSigned)
                {
                    _pdaService.SignPdaEula(collectionManagementQuery.InstitutionId);
                }


                IList<BulkAddResource> bulkAddResources = model.Resources.Select(resource => new BulkAddResource
                {
                    ResourceId = resource.Id, InstitutionId = collectionManagementQuery.InstitutionId,
                    NumberOfLicenses = 1, OriginalSource = LicenseOriginalSource.Pda
                }).ToList();

                _patronDrivenAcquisitionService.AddBuildPdaLicenses(bulkAddResources,
                    collectionManagementQuery.InstitutionId, AuthenticatedInstitution.User.Id);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId,
                    AuthenticatedInstitution.User.Id);

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}";
                model.CollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, IncludePdaResources = true });

                return View("BulkAddPdaConfirm", model);
            }

            return View(model);
        }


        public ActionResult BulkDeletePda(CollectionManagementQuery collectionManagementQuery, string bulkDeletePda)
        {
            var tempCollectionManagementQuery =
                TempData.GetItem<CollectionManagementQuery>("CollectionManagementQuery");
            if (tempCollectionManagementQuery != null)
            {
                collectionManagementQuery = tempCollectionManagementQuery;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
            }


            // ReSharper disable once PossibleNullReferenceException
            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var model = new BulkDeletePda(adminInstitution, collectionManagementQuery);

            var resourceIds = collectionManagementQuery.GetResourceIds().ToList();
            if (resourceIds.Any())
            {
                foreach (var item in resourceIds.Select(resourceId =>
                             _orderService.GetInstitutionResource(collectionManagementQuery.InstitutionId, resourceId,
                                 collectionManagementQuery.CartId)))
                {
                    if (item.IsActivePdaResource)
                    {
                        model.AddResource(item);
                    }
                    else
                    {
                        model.AddExcludedResource(item);
                    }
                }
            }

            if (bulkDeletePda == "yes")
            {
                var modeltest = model.Resources.Select(x => x.Id).ToArray();

                _patronDrivenAcquisitionService.BulkDeletePdaLicenses(modeltest,
                    collectionManagementQuery.InstitutionId, AuthenticatedInstitution.User);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId,
                    AuthenticatedInstitution.User.Id);

                model.KeepShoppingLink =
                    $"{Url.Action("List", "CollectionManagement", collectionManagementQuery.ToRouteValues())}";
                model.CollectionLink = Url.Action("List", "CollectionManagement",
                    new { collectionManagementQuery.InstitutionId, IncludePdaResources = true });

                return View("BulkDeletePdaConfirm", model);
            }


            return View(model);
        }

        public ActionResult PdaProfile(int institutionId)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                institutionId != CurrentUser.InstitutionId)
            {
                institutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("PdaProfile", new { institutionId });
            }

            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var profile = _profileService.GetInstitutionPdaProfile(adminInstitution);

            TempData.Remove("PdaRule");
            TempData.Remove("ExecuteFuture");

            return View(profile);
        }

        public ActionResult AddRule(int institutionId)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                institutionId != CurrentUser.InstitutionId)
            {
                institutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("AddRule", new { institutionId });
            }

            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);

            var rule = _profileService.GetNewInstitutionPdaRule(adminInstitution);
            return View("Rule", rule);
        }

        public ActionResult EditRule(int institutionId, int ruleId)
        {
            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                institutionId != CurrentUser.InstitutionId)
            {
                institutionId = CurrentUser.InstitutionId.GetValueOrDefault();
                return RedirectToAction("EditRule", new { institutionId, ruleId });
            }

            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);

            var rule = _profileService.GetInstitutionPdaRule(adminInstitution, ruleId);
            return View("Rule", rule);
        }

        public ActionResult VerifyRule(int ruleId, CollectionManagementQuery collectionManagementQuery)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            var modal = _profileService.GetRuleResourcesModel(adminInstitution, ruleId, "Verify Task");

            if (!modal.ResourceString.Any())
            {
                modal.HideForm = true;
            }

            TempData.Remove("PdaRule");
            TempData.Remove("ExecuteFuture");

            return View(modal);
        }

        [HttpPost]
        public ActionResult RunRuleNow(int ruleId, CollectionManagementQuery collectionManagementQuery)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);

            if (ruleId > 0)
            {
                _profileService.RunRuleNow(adminInstitution, CurrentUser, ruleId);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId, CurrentUser.Id);
            }
            else
            {
                return RedirectToAction("PdaProfile", new { collectionManagementQuery.InstitutionId });
            }

            var modal = _profileService.GetRuleResourcesModel(adminInstitution, ruleId, "Task Verified");
            modal.HideForm = true;
            return View("VerifyRule", modal);
        }

        public ActionResult SaveRule(PdaRuleModel pdaRule, CollectionManagementQuery collectionManagementQuery)
        {
            ModelState.Remove("CollectionsSelectListItems");
            ModelState.Remove("SpecialtiesSelectListItems");
            ModelState.Remove("PracticeAreaSelectListItems");

            //Only used if EULAS need to be signed.

            var tempPdaRule = TempData.GetItem<PdaRuleModel>("PdaRule");
            if (tempPdaRule != null)
            {
                pdaRule = tempPdaRule;
                ModelState.Remove("Name");
            }

            if (!CurrentUser.IsRittenhouseAdmin() && !CurrentUser.IsSalesAssociate() &&
                collectionManagementQuery.InstitutionId != CurrentUser.InstitutionId)
            {
                collectionManagementQuery.InstitutionId = CurrentUser.InstitutionId.GetValueOrDefault();
            }

            var ruleNameExists = true;
            if (pdaRule != null)
            {
                ruleNameExists = _profileService.DoesRuleNameExist(pdaRule.Id, collectionManagementQuery.InstitutionId,
                    pdaRule.Name);
            }

            if (ModelState.IsValid && !ruleNameExists)
            {
                var adminInstitution = _adminContext.GetAdminInstitution(collectionManagementQuery.InstitutionId);
                collectionManagementQuery.IsPdaProfile = true;

                if (pdaRule.IncludeNewEditionFirm || pdaRule.IncludeNewEditionPda)
                {
                    pdaRule.SpecialtiesSelected = null;
                    pdaRule.CollectionsSelected = null;
                    pdaRule.PracticeAreasSelected = null;
                    pdaRule.MaxPrice = null;
                }


                var executeFuture = pdaRule.ExecuteForFuture;

                var pdaRuleModel = TempData.GetItem<PdaRuleModel>("PdaRule");

                if (pdaRuleModel == null)
                {
                    executeFuture = pdaRule.ExecuteForFuture;

                    pdaRule.ExecuteForFuture = false;

                    pdaRule.Id = _profileService.SavePdaRule(pdaRule, collectionManagementQuery.InstitutionId);
                }

                //No need to check EULAS is profile is not for future or execute now.
                if (!executeFuture && !pdaRule.ExecuteNow)
                {
                    return RedirectToAction("PdaProfile", new { collectionManagementQuery.InstitutionId });
                }

                //Returns a view for EULAS id needed.
                var check = CheckEulas(adminInstitution, collectionManagementQuery);

                if (check != null)
                {
                    pdaRule.ExecuteForFuture = executeFuture;
                    //Need to add rule to temp data so I can keep track of ExecuteFuture and ExecuteNow
                    TempData.AddItem("PdaRule", pdaRule);
                    return check;
                }

                //Sign EULAS if needed and then reload the institution.
                _pdaService.ConvertAndSignEulasIfNeeded(adminInstitution, collectionManagementQuery);
                _adminContext.ReloadAdminInstitution(collectionManagementQuery.InstitutionId, CurrentUser.Id);

                if (pdaRule.Id > 0)
                {
                    if (executeFuture)
                    {
                        _profileService.UpdateRuleToExecuteForFuture(adminInstitution, pdaRule.Id);
                    }

                    if (pdaRule.ExecuteNow)
                    {
                        return RedirectToAction("VerifyRule",
                            new { collectionManagementQuery.InstitutionId, ruleId = pdaRule.Id });
                    }

                    return RedirectToAction("PdaProfile", new { collectionManagementQuery.InstitutionId });
                }

                ModelState.AddModelError("Name", @"Error saving the task.");
            }

            if (pdaRule == null)
            {
                return RedirectToAction("PdaProfile", new { collectionManagementQuery.InstitutionId });
            }

            if (ruleNameExists)
            {
                ModelState.AddModelError("Name",
                    $"The task name \"{pdaRule.Name}\" already exists. Task names must be unique.");
            }

            var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
            var specialties = _specialtyService.GetAllSpecialties().ToList();
            var collections = _collectionService.GetAllCollections().ToList();

            pdaRule.RepopulateSelectListItems(practiceAreas, specialties, collections);

            return View("Rule", pdaRule);
        }

        public ActionResult DeleteRule(int institutionId, int ruleId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            var rule = _profileService.GetInstitutionPdaRule(adminInstitution, ruleId);
            if (rule != null)
            {
                _profileService.DeletePdaRule(rule, institutionId);
            }

            return RedirectToAction("PdaProfile", new { institutionId });
        }

        private ViewResult CheckEulas(IAdminInstitution adminInstitution,
            CollectionManagementQuery collectionManagementQuery)
        {
            if (adminInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id &&
                !collectionManagementQuery.TrialConvert)
            {
                return View("../ExpressCheckout/PdaTrialConvertModal",
                    new CollectionAdd(adminInstitution, collectionManagementQuery));
            }

            if (!adminInstitution.IsEulaSigned && !collectionManagementQuery.EulaSigned)
            {
                return View("../ExpressCheckout/EulaModal",
                    new CollectionAdd(adminInstitution, collectionManagementQuery));
            }

            if (!adminInstitution.IsPdaEulaSigned && !collectionManagementQuery.PdaEulaSigned)
            {
                return View("../ExpressCheckout/PdaEulaModal",
                    new CollectionAdd(adminInstitution, collectionManagementQuery));
            }

            return null;
        }
    }
}