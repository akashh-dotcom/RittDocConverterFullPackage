#region

using System.Linq;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Storages;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class SalesReportService
    {
        protected const string SalesReportKey = "Report.Sales.Data";

        private readonly IReportService _reportService;
        private readonly IResourceService _resourceService;
        private readonly IUserSessionStorageService _userSessionStorageService;
        private ReportRequest _reportRequest;

        public SalesReportService(IReportService reportService, IResourceService resourceService,
            IUserSessionStorageService userSessionStorageService)
        {
            _reportService = reportService;
            _resourceService = resourceService;
            _userSessionStorageService = userSessionStorageService;
        }

        public SalesReportItems RunSalesReportItems(ReportQuery reportQuery)
        {
            var page = reportQuery.Page;

            var salesReportItems = _userSessionStorageService.Get<SalesReportItems>(SalesReportKey);
            if (salesReportItems == null || page < 2)
            {
                SetReportRequest(reportQuery);
                var resources = _resourceService.GetAllResources().ToList();

                salesReportItems = _reportService.GetSalesReport(_reportRequest, resources);

                _userSessionStorageService.Put(SalesReportKey, salesReportItems);
                salesReportItems.StartDate = _reportRequest.DateRangeStart;
                salesReportItems.EndDate = _reportRequest.DateRangeEnd;
            }

            return salesReportItems;
        }

        public void SetReportRequest(ReportQuery reportQuery)
        {
            _reportRequest = new ReportRequest
            {
                DateRangeEnd = reportQuery.DateRangeEnd.GetValueOrDefault(),
                DateRangeStart = reportQuery.DateRangeStart.GetValueOrDefault(),
                PublisherId = reportQuery.PublisherId,
                Period = reportQuery.Period,
                Type = ReportType.SalesReport,
                InstitutionId = reportQuery.InstitutionId,
                TerritoryCode = reportQuery.TerritoryCode,
                InstitutionTypeId = reportQuery.InstitutionTypeId,
                PracticeAreaId = reportQuery.PracticeAreaId,
                SpecialtyId = reportQuery.SpecialtyId,
                SortBy = reportQuery.SortBy,
                ResourceId = reportQuery.ResourceId,
                Status = (int)reportQuery.ResourceStatus
            };
        }
    }
}