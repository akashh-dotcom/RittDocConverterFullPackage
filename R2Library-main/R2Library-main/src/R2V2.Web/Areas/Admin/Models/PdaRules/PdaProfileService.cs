#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Areas.Admin.Models.PdaRules
{
    public class PdaProfileService
    {
        private readonly CollectionManagementSettings _collectionManagementSettings;
        private readonly ICollectionService _collectionService;
        private readonly ILog<PdaProfileService> _log;
        private readonly PdaRuleService _pdaRuleService;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ISpecialtyService _specialtyService;

        public PdaProfileService(
            ILog<PdaProfileService> log
            , PdaRuleService pdaRuleService
            , IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , ICollectionService collectionService
            , CollectionManagementSettings collectionManagementSettings
        )
        {
            _log = log;
            _pdaRuleService = pdaRuleService;
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
            _collectionManagementSettings = collectionManagementSettings;
        }

        public PdaProfileModel GetInstitutionPdaProfile(IAdminInstitution adminInstitution)
        {
            _log.InfoFormat(">> GetInstitutionPdaProfile for Institution: {0}", adminInstitution.Id);
            var model = new PdaProfileModel(adminInstitution);
            try
            {
                var institutionPdaRules = _pdaRuleService.GetInstitutionPdaRules(adminInstitution);
                model.PopulateRules(institutionPdaRules);

                _log.InfoFormat("<< GetInstitutionPdaProfile Rule Count: {0}", institutionPdaRules.Count);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return model;
        }

        public PdaRuleModel GetInstitutionPdaRule(IAdminInstitution adminInstitution, int ruleId)
        {
            _log.InfoFormat(">> GetInstitutionPdaRule for Institution: {0}", adminInstitution.Id);
            var institutionPdaRule = _pdaRuleService.GetInstitutionRule(adminInstitution.Id, ruleId);
            var model = GetModel(adminInstitution, institutionPdaRule);
            _log.InfoFormat("<< GetInstitutionPdaRule Rule: {0}", institutionPdaRule.ToDebugString());
            return model;
        }

        public PdaRuleModel GetNewInstitutionPdaRule(IAdminInstitution adminInstitution)
        {
            _log.InfoFormat(">> GetNewInstitutionPdaRule for Institution: {0}", adminInstitution.Id);
            var model = GetModel(adminInstitution, null);
            _log.Info("<< GetNewInstitutionPdaRule");
            return model;
        }

        private PdaRuleModel GetModel(IAdminInstitution adminInstitution, PdaRule institutionPdaRule)
        {
            var model = new PdaRuleModel(adminInstitution);
            try
            {
                var practiceAreas = _practiceAreaService.GetAllPracticeAreas().ToList();
                var specialties = _specialtyService.GetAllSpecialties().ToList();
                var collections = _collectionService.GetAllCollections().ToList();
                model.PopulateRule(institutionPdaRule, practiceAreas, specialties, collections);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return model;
        }

        public int SavePdaRule(PdaRuleModel pdaRule, int institutionId)
        {
            _log.InfoFormat(">> SavePdaRule for Institution: {0}", institutionId);
            try
            {
                var rule = new PdaRule();
                if (pdaRule.Id > 0)
                {
                    rule = _pdaRuleService.GetInstitutionRule(institutionId, pdaRule.Id);
                }

                rule.ExecuteForFuture = pdaRule.ExecuteForFuture;
                rule.IncludeNewEditionFirm = pdaRule.IncludeNewEditionFirm;
                rule.IncludeNewEditionPda = pdaRule.IncludeNewEditionPda;
                rule.InstitutionId = institutionId;
                rule.MaxPrice = pdaRule.MaxPrice;
                rule.Name = pdaRule.Name;
                rule.RecordStatus = true;

                SetSelectedCollections(rule, pdaRule.CollectionsSelected);
                SetSelectedPracticeAreas(rule, pdaRule.PracticeAreasSelected);
                SetSelectedSpecialties(rule, pdaRule.SpecialtiesSelected);

                var ruleId = _pdaRuleService.SaveInstitutionRule(rule);
                _log.InfoFormat("<< SavePdaRule RuleId: {0}", ruleId);

                return ruleId;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return 0;
        }

        public bool DeletePdaRule(PdaRuleModel pdaRule, int institutionId)
        {
            _log.InfoFormat(">> DeletePdaRule for Institution: {0}", institutionId);
            try
            {
                if (pdaRule.Id > 0)
                {
                    var rule = _pdaRuleService.GetInstitutionRule(institutionId, pdaRule.Id);
                    var success = _pdaRuleService.DeleteInstitutionRule(rule);
                    _log.InfoFormat("<< DeletePdaRule Saved: {0}", success);
                    return success;
                }

                _log.Error("<< DeletePdaRule Rule not Saved:  Rule has not ID and this should not HAPPEN");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        private void SetSelectedPracticeAreas(PdaRule rule, IList<int> practiceAreasSelected)
        {
            _log.InfoFormat(">> SetSelectedPracticeAreas for Institution: {0}", rule.InstitutionId);
            try
            {
                var rulePracticeAreas = rule.PracticeAreas;
                // add
                if (practiceAreasSelected != null)
                {
                    foreach (var practiceAreaId in practiceAreasSelected)
                    {
                        var rulePracticeArea = rulePracticeAreas != null
                            ? rulePracticeAreas.FirstOrDefault(x => x.PracticeAreaId == practiceAreaId)
                            : null;

                        if (rulePracticeArea == null)
                        {
                            rulePracticeArea = new PdaRulePracticeArea
                            {
                                PracticeAreaId = practiceAreaId,
                                //RuleId = rule.Id,
                                RecordStatus = true
                            };
                            if (rule.PracticeAreas == null)
                            {
                                rule.PracticeAreas = new List<PdaRulePracticeArea>();
                            }

                            rule.PracticeAreas.Add(rulePracticeArea);
                        }
                    }
                }

                // delete
                if (rule.PracticeAreas != null)
                {
                    foreach (var rulePracticeArea in rule.PracticeAreas)
                    {
                        if (practiceAreasSelected == null ||
                            !practiceAreasSelected.Contains(rulePracticeArea.PracticeAreaId))
                        {
                            rulePracticeArea.RecordStatus = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("<< SetSelectedPracticeAreas RuleId");
        }

        private void SetSelectedSpecialties(PdaRule rule, IList<int> specialtiesSelected)
        {
            _log.InfoFormat(">> SetSelectedSpecialties for Institution: {0}", rule.InstitutionId);
            try
            {
                var ruleSpecialties = rule.Specialties;
                // add
                if (specialtiesSelected != null)
                {
                    foreach (var specialtyId in specialtiesSelected)
                    {
                        var ruleSpecialty = ruleSpecialties != null
                            ? ruleSpecialties.FirstOrDefault(x => x.SpecialtyId == specialtyId)
                            : null;

                        if (ruleSpecialty == null)
                        {
                            ruleSpecialty = new PdaRuleSpecialty
                            {
                                SpecialtyId = specialtyId,
                                //RuleId = rule.Id,
                                RecordStatus = true
                            };
                            if (rule.Specialties == null)
                            {
                                rule.Specialties = new List<PdaRuleSpecialty>();
                            }

                            rule.Specialties.Add(ruleSpecialty);
                        }
                    }
                }

                // delete
                if (rule.Specialties != null)
                {
                    foreach (var ruleSpecalty in rule.Specialties)
                    {
                        if (specialtiesSelected == null || !specialtiesSelected.Contains(ruleSpecalty.SpecialtyId))
                        {
                            ruleSpecalty.RecordStatus = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("<< SetSelectedSpecialties RuleId");
        }

        private void SetSelectedCollections(PdaRule rule, IList<int> collectionsSelected)
        {
            _log.InfoFormat(">> SetSelectedCollections for Institution: {0}", rule.InstitutionId);
            try
            {
                var ruleCollections = rule.Collections;
                // add
                if (collectionsSelected != null)
                {
                    foreach (var collectionId in collectionsSelected)
                    {
                        var ruleCollection = ruleCollections != null
                            ? ruleCollections.FirstOrDefault(x => x.CollectionId == collectionId)
                            : null;

                        if (ruleCollection == null)
                        {
                            ruleCollection = new PdaRuleCollection
                            {
                                CollectionId = collectionId,
                                //RuleId = rule.Id,
                                RecordStatus = true
                            };
                            if (rule.Collections == null)
                            {
                                rule.Collections = new List<PdaRuleCollection>();
                            }

                            rule.Collections.Add(ruleCollection);
                        }
                    }
                }

                // delete
                if (rule.Collections != null)
                {
                    foreach (var ruleCollection in rule.Collections)
                    {
                        if (collectionsSelected == null || !collectionsSelected.Contains(ruleCollection.CollectionId))
                        {
                            ruleCollection.RecordStatus = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("<< SetSelectedCollections RuleId");
        }

        public void RunRuleNow(IAdminInstitution adminInstitution, IUser user, int ruleId)
        {
            _log.InfoFormat(">> RunRuleNow for Institution: {0} | User: {1} | Rule: {2}", adminInstitution.Id, user.Id,
                ruleId);
            try
            {
                _pdaRuleService.RunRuleNow(adminInstitution, user, ruleId,
                    _collectionManagementSettings.PatronDriveAcquisitionMaxViews);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("<< RunRuleNow");
        }


        public PdaRuleResourcesModel GetRuleResourcesModel(IAdminInstitution adminInstitution, int ruleId,
            string pageTitle)
        {
            _log.InfoFormat(">> GetRuleResourcesModel for Institution: {0} | Rule: {1}", adminInstitution.Id, ruleId);
            var model = new PdaRuleResourcesModel(adminInstitution);
            try
            {
                var rule = _pdaRuleService.GetInstitutionRule(adminInstitution.Id, ruleId);
                _pdaRuleService.PopulateResourceCountsForVerify(rule, adminInstitution);

                var resources = _pdaRuleService.GetBackFillResources(adminInstitution, ruleId);
                model.PopulateRuleAndResources(rule, resources);

                model.PageTitle = pageTitle;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("<< GetRuleResourcesModel");
            return model;
        }

        public bool UpdateRuleToExecuteForFuture(IAdminInstitution adminInstitution, int ruleId)
        {
            _log.InfoFormat(">> UpdateRuleToExecuteForFuture for Institution: {0} | Rule: {1}", adminInstitution.Id,
                ruleId);
            try
            {
                var rule = _pdaRuleService.GetInstitutionRule(adminInstitution.Id, ruleId);
                rule.ExecuteForFuture = true;
                return _pdaRuleService.SaveInstitutionRule(rule) > 0;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            _log.Info("<< UpdateRuleToExecuteForFuture");
            return false;
        }

        public bool DoesRuleNameExist(int ruleId, int institutionId, string ruleName)
        {
            return _pdaRuleService.DoesRuleNameExist(ruleId, institutionId, ruleName);
        }
    }
}