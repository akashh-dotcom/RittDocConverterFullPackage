#region

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RequestResourceService
    {
        private readonly InstitutionService _institutionService;
        private readonly ILog<RequestResourceService> _log;
        private readonly IReportService _reportService;
        private readonly IResourceService _resourceService;
        private readonly ITerritoryService _territoryService;

        public RequestResourceService(
            ILog<RequestResourceService> log
            , IReportService reportService
            , InstitutionService institutionService
            , IResourceService resourceService
            , ITerritoryService territoryService
        )
        {
            _log = log;
            _reportService = reportService;
            _institutionService = institutionService;
            _resourceService = resourceService;
            _territoryService = territoryService;
        }

        public RequestedResourcesExcelExport GetRequestedResourcesExcelReport(RequestedResourcesModel model)
        {
            var requestedItems = GetRequestedResourceItems(model.ReportQuery);

            var institutionIds = requestedItems.Select(x => x.InstitutionId).Distinct().ToList();
            var resourceIds = requestedItems.Select(x => x.ResourceId).Distinct().ToList();
            var institutions = _institutionService.GetInstitutions(institutionIds.ToArray()).ToList();
            var resources = _resourceService.GetResourcesByIds(resourceIds.ToArray()).ToList();

            return new RequestedResourcesExcelExport(requestedItems, resources, institutions);
        }

        private List<ResourceRequestItem> GetRequestedResourceItems(RequestedResourcesQuery query)
        {
            ReportDatesService.SetDates(query);

            var request = new ResourceAccessReportRequest();
            request.StartDate = query.PeriodStartDate.GetValueOrDefault();
            request.EndDate = query.PeriodEndDate.GetValueOrDefault();
            request.AccountNumbers = query.GetAccountNumberArray();

            if (query.TerritoryCodes != null && query.TerritoryCodes.Any())
            {
                if (query.TerritoryCodes.Length == 1 && query.TerritoryCodes.FirstOrDefault() == "All")
                {
                    //Do Nothing
                }
                else
                {
                    var territories = _territoryService.GetAllTerritories();
                    var selectedTerritoryIds = territories.Where(x => query.TerritoryCodes.Contains(x.Code)).ToList();
                    if (selectedTerritoryIds.Any())
                    {
                        request.TerritoryIds = selectedTerritoryIds.Select(x => x.Id).ToArray();
                    }
                }
            }

            if (query.InstitutionTypeIds != null && query.InstitutionTypeIds.Length == 1 &&
                query.InstitutionTypeIds.FirstOrDefault() == 0)
            {
                //Do Nothing
            }
            else
            {
                request.InstitutionTypeIds = query.InstitutionTypeIds;
            }

            return _reportService.GetResourceRequestItems(request);
        }

        public RequestedResourcesModel GetRequestedResourcesInstitutions(RequestedResourcesQuery query)
        {
            var requestResourcesInstitutions = new List<RequestedResourcesInstitution>();

            var missingResources = new List<string>();
            var missingInstitutions = new List<string>();

            var requestedItems = GetRequestedResourceItems(query);

            var institutionIds = requestedItems.Select(x => x.InstitutionId).Distinct().ToList();
            var resourceIds = requestedItems.Select(x => x.ResourceId).Distinct().ToList();
            var institutions = _institutionService.GetInstitutions(institutionIds.ToArray()).ToList();
            var resources = _resourceService.GetResourcesByIds(resourceIds.ToArray()).ToList();

            var lastItem = new RequestedResourcesInstitution();
            foreach (var item in requestedItems)
            {
                if (item.InstitutionId == lastItem.Institution?.InstitutionId)
                {
                    RequestedResource requestedResourceFound = null;
                    try
                    {
                        requestedResourceFound =
                            lastItem.RequestedResources?.FirstOrDefault(x => x.Resource.Id == item.ResourceId);
                        //Filters out duplicates that are created because the book was purchasd mulitple times.
                        if (requestedResourceFound != null)
                        {
                            if (item.AutomatedCartId.HasValue)
                            {
                                if (!requestedResourceFound.AutomatedCartIdAndNames.ContainsKey(item.AutomatedCartId
                                        .Value))
                                {
                                    requestedResourceFound.AutomatedCartIdAndNames.Add(item.AutomatedCartId.Value,
                                        item.AutomatedCartName);
                                }
                            }

                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        var logingString =
                            $"ResourceRequestItem: {item.ToDebug()} || requestedResourceFound: {requestedResourceFound?.ToDebug()}";
                        _log.Warn($"{ex.Message} {logingString}", ex);
                    }


                    var resource = resources.FirstOrDefault(x => x.Id == item.ResourceId);
                    if (resource == null)
                    {
                        missingResources.Add($"Resource Id Not Found: {item.ResourceId}");
                        continue;
                    }
                }
                else
                {
                    var institution = institutions.FirstOrDefault(x => x.Id == item.InstitutionId);
                    if (institution == null)
                    {
                        missingInstitutions.Add($"Institution Id Not Found: {item.InstitutionId}");
                        continue;
                    }

                    //Create new Last Item
                    lastItem = new RequestedResourcesInstitution(
                        institutions.FirstOrDefault(x => x.Id == item.InstitutionId));
                }


                var requestResource = new RequestedResource
                {
                    Resource = resources.FirstOrDefault(x => x.Id == item.ResourceId),
                    AutomatedCartId = item.AutomatedCartId,
                    CartId = item.CartId,
                    OrderHistoryId = item.OrderHistoryId,
                    PurchaseDate = item.PurchaseDate,
                    AddedDate = item.AddedDate,
                    LastRequestDate = item.LastRequestDate,
                    RequestedCount = item.RequestCount,
                    PurchasePrice = item.PurchasePrice.GetValueOrDefault(0),
                    AutomatedCartIdAndNames = new Dictionary<int, string>()
                };

                if (item.AutomatedCartId.HasValue)
                {
                    requestResource.AutomatedCartIdAndNames.Add(item.AutomatedCartId.Value, item.AutomatedCartName);
                }


                lastItem.RequestedResources?.Add(requestResource);

                if (lastItem.RequestedResources?.Count == 1)
                {
                    requestResourcesInstitutions.Add(lastItem);
                }
            }

            if (missingResources.Any())
            {
                _log.Error(
                    $"GetRequestedResourcesInstitutions -> MissingResources: [{string.Join("][", missingResources)}]");
            }

            if (missingInstitutions.Any())
            {
                _log.Error(
                    $"GetRequestedResourcesInstitutions -> MissingInstitutions: [{string.Join("][", missingInstitutions)}]");
            }

            var territories = _territoryService.GetAllTerritories();
            var institutionTypes = _institutionService.GetInstitutionTypes();
            var model = new RequestedResourcesModel(requestResourcesInstitutions, query, territories, institutionTypes);

            return model;
            //return requestResourcesInstitutions;
        }
    }
}