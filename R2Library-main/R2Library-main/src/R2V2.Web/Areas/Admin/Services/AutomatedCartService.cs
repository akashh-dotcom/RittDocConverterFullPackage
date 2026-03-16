#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models.Marketing;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class AutomatedCartService
    {
        private readonly AutomatedCartFactory _automatedCartFactory;
        private readonly AutomatedCartQueueService _automatedCartQueueService;
        private readonly InstitutionService _institutionService;
        private readonly ILog<AutomatedCartService> _log;
        private readonly IResourceService _resourceService;
        private readonly TerritoryService _territoryService;

        public AutomatedCartService(
            ILog<AutomatedCartService> log
            , IResourceService resourceService
            , AutomatedCartFactory automatedCartFactory
            , TerritoryService territoryService
            , AutomatedCartQueueService automatedCartQueueService
            , InstitutionService institutionService
        )
        {
            _log = log;
            _resourceService = resourceService;
            _automatedCartFactory = automatedCartFactory;
            _territoryService = territoryService;
            _automatedCartQueueService = automatedCartQueueService;
            _institutionService = institutionService;
        }

        public IEnumerable<AutomatedCartInstitution> GetAutomatedCartEventInstitutions(AutomatedCartPricingModel model)
        {
            var newModel = new AutomatedCartModel
            {
                ReportQuery = model.ReportQuery,
                DateRangeStart = model.DateRangeStart,
                DateRangeEnd = model.DateRangeEnd
            };

            return GetAutomatedCartEventInstitutions(newModel);
        }

        public IEnumerable<AutomatedCartInstitution> GetAutomatedCartEventInstitutions(AutomatedCartModel model)
        {
            ReportDatesService.SetDates(model.ReportQuery);
            var timer = new Stopwatch();
            timer.Start();
            _log.Debug(">>>>> Start GetAutomatedCartEventInstitutions");

            var cartEvents = _automatedCartFactory.GetAutomatedCartEvents(model.ReportQuery, null);
            var institutionIds = cartEvents.Select(y => y.InstitutionId).Distinct().ToArray();

            var institutions = _institutionService.GetInstitutions(institutionIds);

            var automatedCartInstitutions = new List<AutomatedCartInstitution>();

            var institutionsTimer = new Stopwatch();
            institutionsTimer.Start();

            foreach (var institution in institutions)
            {
                var institutionCartEvents = cartEvents.Where(x => x.InstitutionId == institution.Id).ToList();

                var automatedCartInstiution = new AutomatedCartInstitution(institution)
                {
                    Turnaway = institutionCartEvents.Any(x => x.Turnaway > 0),
                    NewEdition = institutionCartEvents.Any(x => x.NewEdition > 0),
                    Reviewed = institutionCartEvents.Any(x => x.Reviewed > 0),
                    TriggeredPda = institutionCartEvents.Any(x => x.TriggeredPda > 0),
                    Requested = institutionCartEvents.Any(x => x.Requested > 0)
                };


                automatedCartInstitutions.Add(automatedCartInstiution);
            }

            _log.Debug($"++++++ It took {institutionsTimer.ElapsedMilliseconds}ms to process the institutions.");

            _log.Debug($"<<<<< End GetAutomatedCartEventInstitutions -- run time: {timer.ElapsedMilliseconds}ms");
            return automatedCartInstitutions.OrderBy(x => x.InstitutionName);
        }


        public AutomatedCartPricingModel GetPricedInstitutionCarts(AutomatedCartPricingModel model, IUser user)
        {
            ReportDatesService.SetDates(model.ReportQuery);

            var reportQuery = model.ReportQuery;

            var institutionIds = ParseInstitutionIds(model.SelectedInstitutionIds);
            institutionIds = institutionIds.Distinct().ToList();

            var timer = new Stopwatch();
            timer.Start();
            _log.Debug(">>>>> Start GetPricedCarts");

            var cartEvents = _automatedCartFactory.GetAutomatedCartEvents(reportQuery, institutionIds.ToArray());

            var resources = _resourceService.GetAllResources().ToList();

            var institutions = _institutionService.GetInstitutions(institutionIds.ToArray());

            var pricedInstitutions = new List<AutomatedCartInstitutionPriced>();

            var institutionsTimer = new Stopwatch();
            institutionsTimer.Start();

            foreach (var institution in institutions)
            {
                var highestDiscountPercentage = institution.Discount > model.DiscountOverride.GetValueOrDefault(0)
                    ? institution.Discount
                    : model.DiscountOverride.GetValueOrDefault(0);
                var institutionCartEvents = cartEvents.Where(x => x.InstitutionId == institution.Id).ToList();
                var pricedResources =
                    GetAutomatedCartPricedResources(institutionCartEvents, resources, highestDiscountPercentage);

                pricedInstitutions.Add(new AutomatedCartInstitutionPriced(institution, pricedResources));
            }

            _log.Debug($"++++++ It took {institutionsTimer.ElapsedMilliseconds}ms to process the institutions.");

            _log.Debug($"<<<<< End GetPricedCarts -- run time: {timer.ElapsedMilliseconds}ms");

            var selectedTerritoryCodeList = GetSelectedTerritoryCodeList(reportQuery.TerritoryCodes);

            var selectedInstitutionTypeList = GetSelectedInstitutionTypeList(reportQuery.InstitutionTypeIds);

            model = new AutomatedCartPricingModel(reportQuery, pricedInstitutions.OrderBy(x => x.InstitutionName),
                selectedTerritoryCodeList.ToArray(), selectedInstitutionTypeList.ToArray())
            {
                SelectedInstitutionIds = institutionIds.Any() ? string.Join(",", institutionIds) : null,
                AllowDiscountOverride = user != null && user.IsRittenhouseAdmin()
            };

            return model;
        }

        private void PopulateSummaryEstimates(DbAutomatedCart automatedCart,
            List<AutomatedCartInstitutionSummary> automatedCartInstitutionSummaries,
            WebAutomatedCartReportQuery reportQuery)
        {
            var institutionIds = automatedCartInstitutionSummaries.Select(x => x.InstitutionId).ToArray();
            var cartEvents = _automatedCartFactory.GetAutomatedCartEvents(reportQuery, institutionIds);
            var resources = _resourceService.GetAllResources().ToList();
            var institutions = _institutionService.GetInstitutions(institutionIds).ToList();
            if (!institutions.Any())
            {
                _log.Error($"No Institutions where found for automatedCartId: {automatedCart.Id}");
                return;
            }

            foreach (var item in automatedCartInstitutionSummaries)
            {
                //Only populate estimates if items are not populated from database. 
                if (item.ListPrice > 0)
                {
                    continue;
                }

                var institution = institutions.FirstOrDefault(x => x.Id == item.InstitutionId);
                if (institution == null)
                {
                    _log.Error($"No institution found for InstitutionId: {item.InstitutionId}");
                    return;
                }

                var events = cartEvents.Where(x => x.InstitutionId == item.InstitutionId).ToList();
                if (!events.Any())
                {
                    _log.Error($"No events found for InstitutionId: {institution.Id}");
                    return;
                }

                var eventResourceIds = events.Select(x => x.ResourceId).Distinct();
                var eventResources = resources.Where(x => eventResourceIds.Contains(x.Id)).ToList();
                if (!eventResources.Any())
                {
                    _log.Error(
                        $"No eventResources found for InstitutionId: {institution.Id} automatedCartId: {automatedCart.Id}");
                    return;
                }

                var highestDiscountPercentage = institution.Discount > automatedCart.Discount
                    ? institution.Discount
                    : automatedCart.Discount;

                item.ListPrice = eventResources.Sum(x => x.ListPrice);
                item.DiscountPrice =
                    eventResources.Sum(x => x.ListPrice - highestDiscountPercentage / 100 * x.ListPrice);
                if (automatedCart.NewEdition)
                {
                    item.NewEditionCount = events.Sum(x => x.NewEdition);
                }

                if (automatedCart.TriggeredPda)
                {
                    item.PdaCount = events.Sum(x => x.TriggeredPda);
                }

                if (automatedCart.Reviewed)
                {
                    item.ReviewedCount = events.Sum(x => x.Reviewed);
                }

                if (automatedCart.Turnaway)
                {
                    item.TurnawayCount = events.Sum(x => x.Turnaway);
                }

                if (automatedCart.Requested)
                {
                    item.RequestedCount = events.Sum(x => x.Requested);
                }

                item.TitleCount = eventResources.Count;
                item.EmailCount = 0;
            }
        }

        public AutomatedCartPricingModel GetFinalizedAutomatedCart(int automatedCartId, IUser user)
        {
            var summaries = _automatedCartFactory.GetAutomatedCartInstitutionSummaries(automatedCartId);
            var automatedCart = _automatedCartFactory.GetAutomatedCart(automatedCartId);

            var query = new WebAutomatedCartReportQuery
            {
                Period = automatedCart.Period,
                AccountNumberBatch = automatedCart.AccountNumbers,
                IncludeNewEdition = automatedCart.NewEdition,
                IncludeReviewed = automatedCart.Reviewed,
                IncludeTriggeredPda = automatedCart.TriggeredPda,
                IncludeTurnaway = automatedCart.Turnaway,
                IncludeRequested = automatedCart.Requested,
                PeriodStartDate = automatedCart.StartDate,
                PeriodEndDate = automatedCart.EndDate
            };
            //Only populates estimates if the item has not been processed yet. 
            PopulateSummaryEstimates(automatedCart, summaries, query);

            var institutionTypeNameArray = GetSelectedInstitutionTypeNameList(automatedCart.InstitutionTypeIds);
            var territoryCodeArray = GetSelectedTerritoryCodeList(automatedCart.TerritoryIds);

            var model = new AutomatedCartPricingModel(query, summaries, territoryCodeArray, institutionTypeNameArray)
            {
                DiscountOverride = automatedCart.Discount,
                CartName = automatedCart.CartName,
                EmailSubject = automatedCart.EmailSubject,
                EmailTitle = automatedCart.EmailTitle,
                EmailText = automatedCart.EmailText,
                AutomatedCartId = automatedCart.Id,
                AllowDiscountOverride = user != null && user.IsRittenhouseAdmin()
            };


            return model;
        }


        private List<AutomatedCartPricedResources> GetAutomatedCartPricedResources(
            List<DbAutomatedCartEvent> institutionCartEvents, List<IResource> resources,
            decimal highestDiscountPercentage)
        {
            var institutionCartEventsResources = institutionCartEvents.Select(x => x.ResourceId).Distinct();

            var pricedResources = new List<AutomatedCartPricedResources>();
            foreach (var institutionCartEventsResource in institutionCartEventsResources)
            {
                var pricedResource = new AutomatedCartPricedResources();
                var resource = resources.FirstOrDefault(x => x.Id == institutionCartEventsResource);
                if (resource != null)
                {
                    pricedResource.ResourceId = resource.Id;
                    pricedResource.ListPrice = resource.ListPrice;
                    pricedResource.DiscountPrice =
                        resource.ListPrice - highestDiscountPercentage / 100 * resource.ListPrice;
                    pricedResource.NewEditionCount = institutionCartEvents.Where(y => y.ResourceId == resource.Id)
                        .Sum(x => x.NewEdition);
                    pricedResource.TriggeredPdaCount = institutionCartEvents.Where(y => y.ResourceId == resource.Id)
                        .Sum(x => x.TriggeredPda);
                    pricedResource.ReviewedCount = institutionCartEvents.Where(y => y.ResourceId == resource.Id)
                        .Sum(x => x.Reviewed);
                    pricedResource.TurnawayCount = institutionCartEvents.Where(y => y.ResourceId == resource.Id)
                        .Sum(x => x.Turnaway);
                    pricedResource.RequestedCount = institutionCartEvents.Where(y => y.ResourceId == resource.Id)
                        .Sum(x => x.Requested);
                    pricedResources.Add(pricedResource);
                }
                else
                {
                    _log.Warn(
                        $"AutomatedCartService.GetPricedCarts - Resource Not Found: {institutionCartEventsResource}");
                }
            }

            return pricedResources;
        }


        public int SaveAutomatedCart(AutomatedCartPricingModel model)
        {
            ReportDatesService.SetDates(model.ReportQuery);

            var reportQuery = model.ReportQuery;

            var institutionIds = ParseInstitutionIds(model.SelectedInstitutionIds);

            var territoryIdArray = GetTerritoryIds(reportQuery.TerritoryCodes);

            var automatedCart = new DbAutomatedCart
            {
                Period = reportQuery.Period,
                StartDate = reportQuery.PeriodStartDate.GetValueOrDefault(),
                EndDate = reportQuery.PeriodEndDate.GetValueOrDefault(),
                NewEdition = reportQuery.IncludeNewEdition,
                TriggeredPda = reportQuery.IncludeTriggeredPda,
                Reviewed = reportQuery.IncludeReviewed,
                Turnaway = reportQuery.IncludeTurnaway,
                Requested = reportQuery.IncludeRequested,
                Discount = model.DiscountOverride.GetValueOrDefault(0),
                AccountNumbers = reportQuery.AccountNumberBatch,
                CartName = model.CartName,
                EmailSubject = model.EmailSubject,
                EmailTitle = model.EmailTitle,
                EmailText = model.EmailText,
                TerritoryIds = territoryIdArray != null ? string.Join(",", territoryIdArray) : null,
                InstitutionTypeIds = reportQuery.InstitutionTypeIds != null
                    ? string.Join(",", reportQuery.InstitutionTypeIds)
                    : null
            };

            _automatedCartFactory.SaveAutomatedCart(automatedCart, institutionIds.ToArray());

            //Write to Queue
            if (automatedCart.Id > 0)
            {
                var automatedCartMessage = new AutomatedCartMessage(automatedCart, institutionIds.ToArray(),
                    territoryIdArray, reportQuery.InstitutionTypeIds);
                _automatedCartQueueService.WriteDataToMessageQueue(automatedCartMessage);
            }

            return automatedCart.Id;
        }

        public AutomatedCartExcelExport GetAutomatedCartExcelExport(AutomatedCartPricingModel model)
        {
            ReportDatesService.SetDates(model.ReportQuery);

            var reportQuery = model.ReportQuery;

            var selectedInstitutionIds = model.SelectedInstitutionIds.Replace(" ", "");

            var institutionIdStrings = selectedInstitutionIds.Split(',');

            var institutionIds = new List<int>();
            foreach (var s in institutionIdStrings)
            {
                int i;
                int.TryParse(s, out i);
                institutionIds.Add(i);
            }


            var timer = new Stopwatch();
            timer.Start();
            _log.Debug(">>>>> Start GetPricedCarts");

            var cartEvents = _automatedCartFactory.GetAutomatedCartEvents(reportQuery, institutionIds.ToArray());

            var resources = _resourceService.GetAllResources().ToList();

            var institutions = _institutionService.GetInstitutions(institutionIds.ToArray()).ToList();
            var automatedCartPricedReports = new List<AutomatedCartPricedReport>();
            foreach (var institution in institutions)
            {
                var highestDiscountPercentage = institution.Discount > model.DiscountOverride.GetValueOrDefault(0)
                    ? institution.Discount
                    : model.DiscountOverride.GetValueOrDefault(0);
                var institutionCartEvents = cartEvents.Where(x => x.InstitutionId == institution.Id).ToList();
                var pricedResources =
                    GetAutomatedCartPricedResources(institutionCartEvents, resources, highestDiscountPercentage);
                var automatedCartPricedReport = new AutomatedCartPricedReport
                {
                    Institution = institution,
                    NewEditionCount = pricedResources.Sum(x => x.NewEditionCount),
                    TriggeredPdaCount = pricedResources.Sum(x => x.TriggeredPdaCount),
                    ReviewedCount = pricedResources.Sum(x => x.ReviewedCount),
                    TurnawayCount = pricedResources.Sum(x => x.TurnawayCount),
                    RequestedCount = pricedResources.Sum(x => x.RequestedCount),
                    ResourceCount = pricedResources.Count,
                    ListPrice = pricedResources.Sum(x => x.ListPrice),
                    DiscountPrice = pricedResources.Sum(x => x.DiscountPrice)
                };
                automatedCartPricedReports.Add(automatedCartPricedReport);
            }

            return new AutomatedCartExcelExport(automatedCartPricedReports.OrderBy(x => x.Institution.Name));
        }

        public AutomatedCartExcelExport GetAutomatedCartExcelExport(int automatedCartId, bool displayEmailCounts,
            IUser user)
        {
            var automatedCartPricingModel = GetFinalizedAutomatedCart(automatedCartId, user);

            return new AutomatedCartExcelExport(
                automatedCartPricingModel.InstitutionSummaries.OrderBy(x => x.InstitutionName), displayEmailCounts);
        }

        public AutomatedCartExcelExport GetAutomatedCartExcelExport(AutomatedCartModel model)
        {
            ReportDatesService.SetDates(model.ReportQuery);

            var cartEvents = _automatedCartFactory.GetAutomatedCartEvents(model.ReportQuery, null);
            var institutionIds = cartEvents.Select(y => y.InstitutionId).Distinct().ToList();

            var institutions = _institutionService.GetInstitutions(institutionIds.ToArray()).ToList();
            var automatedCartReports = new List<AutomatedCartReport>();
            foreach (var institution in institutions)
            {
                var institutionCartEvents = cartEvents.Where(x => x.InstitutionId == institution.Id).ToList();

                var automatedCartReport = new AutomatedCartReport
                {
                    Institution = institution,
                    Turnaway = institutionCartEvents.Any(x => x.Turnaway > 0),
                    NewEdition = institutionCartEvents.Any(x => x.NewEdition > 0),
                    Reviewed = institutionCartEvents.Any(x => x.Reviewed > 0),
                    TriggeredPda = institutionCartEvents.Any(x => x.TriggeredPda > 0),
                    Requested = institutionCartEvents.Any(x => x.Requested > 0)
                };

                automatedCartReports.Add(automatedCartReport);
            }

            return new AutomatedCartExcelExport(automatedCartReports.OrderBy(x => x.Institution.Name));
        }

        public AutomatedCartHistoryModel GetAutomatedCartHistories()
        {
            var histories = _automatedCartFactory.GetAutomatedCartHistories();
            var model = new AutomatedCartHistoryModel { AutomatedCartHistories = histories };
            return model;
        }

        private List<string> GetSelectedTerritoryCodeList(string[] territoryCodes)
        {
            var selectedTerritoryCodeList = new List<string> { "All" };
            if (territoryCodes != null && territoryCodes.Any())
            {
                var selectedTerritories = _territoryService.GetTerritories(territoryCodes);
                var selectedTerritoryCodes = selectedTerritories.Select(y => y.Name).ToList();
                if (selectedTerritoryCodes.Any())
                {
                    selectedTerritoryCodeList = selectedTerritoryCodes;
                }
            }

            return selectedTerritoryCodeList;
        }

        private string[] GetSelectedTerritoryCodeList(string territoryCodeString)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(territoryCodeString))
                {
                    var territoryCodes = territoryCodeString.Split(',').ToArray();
                    var selectedTerritories = _territoryService.GetTerritories(territoryCodes);
                    return selectedTerritories.Select(y => y.Name).ToArray();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        private int[] GetTerritoryIds(string[] territoryCodes)
        {
            try
            {
                if (territoryCodes != null && territoryCodes.Any())
                {
                    var territoryIdArray = _territoryService.GetTerritories(territoryCodes);
                    if (territoryIdArray.Any())
                    {
                        return territoryIdArray.Select(x => x.Id).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return null;
        }


        private List<string> GetSelectedInstitutionTypeList(int[] institutionTypeIds)
        {
            var selectedInstitutionTypeList = new List<string> { "All" };

            if (institutionTypeIds != null && institutionTypeIds.Any())
            {
                var selectedInstitutionTypes = _institutionService.GetInstitutionTypes(institutionTypeIds);
                if (selectedInstitutionTypes.Any())
                {
                    selectedInstitutionTypeList = selectedInstitutionTypes.Select(x => x.Name).ToList();
                }
            }

            return selectedInstitutionTypeList;
        }

        private string[] GetSelectedInstitutionTypeNameList(string institutionTypeIdString)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(institutionTypeIdString))
                {
                    var institutionTypeIds = institutionTypeIdString.Split(',').Select(int.Parse).ToArray();

                    var selectedInstitutionTypes = _institutionService.GetInstitutionTypes(institutionTypeIds);
                    if (selectedInstitutionTypes.Any())
                    {
                        return selectedInstitutionTypes.Select(x => x.Name).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        private List<int> ParseInstitutionIds(string institutionIdString)
        {
            var institutionIds = new List<int>();
            try
            {
                var selectedInstitutionIds = institutionIdString.Replace(" ", "");
                var institutionIdStrings = selectedInstitutionIds.Split(',');

                foreach (var s in institutionIdStrings)
                {
                    int i;
                    int.TryParse(s, out i);
                    institutionIds.Add(i);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return institutionIds;
        }
    }
}