#region

using System;
using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Reports
{
    public interface IReportService
    {
        ApplicationReportCounts GetApplicationReportCounts(ReportRequest reportRequest);
        List<ResourceReportItem> GetResourceReportItems(ReportRequest reportRequest, List<IResource> resources);
        void SaveSavedReport(DateTime lastUpdated, int savedReportId);
        List<SavedReport> GetSavedReports(ReportFrequency frequency);
        List<TurnawayResource> GetTurnawayResources2(string reportDatabaseName, string r2DatabaseName);
        PublisherReportCounts GetPublisherReportCounts(ReportRequest reportRequest, List<IResource> resources);
        List<AnnualFeeReportDataItem> GetAnnualFeeReportDataItems(ReportRequest reportRequest);

        //Dictionary<int, decimal> GetAveragePricesForOwnedResources(int institutionId);

        List<PdaCountsReportDataItem> GetPdaReportCounts(ReportRequest reportRequest);

        bool LogReportRequest(ReportRequest reportRequest);

        List<ResourceRequestItem> GetResourceRequestItems(ResourceAccessReportRequest reportRequest);

        SalesReportItems GetSalesReport(ReportRequest reportRequest, List<IResource> resources);
    }
}