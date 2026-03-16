#region

using R2V2.Core.Reports;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class ApplicationUsageExcelExport : ExcelBase
    {
        public ApplicationUsageExcelExport(ApplicationReportCounts applicationReportCounts)
        {
            SpecifyColumn("User Sessions", "Int32");
            SpecifyColumn("Page Views", "Int32");
            SpecifyColumn("Successful Content Retrievals", "Int32");
            SpecifyColumn("TOC Retrievals", "Int32");
            SpecifyColumn("Concurrency Turnaways", "Int32");
            SpecifyColumn("Access Turnaways", "Int32");
            SpecifyColumn("Active Content Searches", "Int32");
            SpecifyColumn("Archived Content Searches", "Int32");
            SpecifyColumn("Image Only Searches", "Int32");
            SpecifyColumn("Drug Monograph Searches", "Int32");
            SpecifyColumn("PUBMED Searches", "Int32");
            SpecifyColumn("MESH Searches", "Int32");

            PopulateFirstColumn(applicationReportCounts.UserSessionCount);
            PopulateNextColumn(applicationReportCounts.PageViewCount);
            PopulateNextColumn(applicationReportCounts.RestrictedContentRetrievalCount);
            PopulateNextColumn(applicationReportCounts.TocOnlyContentRetrievalCount);
            PopulateNextColumn(applicationReportCounts.ConcurrencyTurnawayCount);
            PopulateNextColumn(applicationReportCounts.AccessTurnawayCount);
            PopulateNextColumn(applicationReportCounts.SearchActiveCount);
            PopulateNextColumn(applicationReportCounts.SearchArchiveCount);
            PopulateNextColumn(applicationReportCounts.SearchImageCount);
            PopulateNextColumn(applicationReportCounts.SearchDrugCount);
            PopulateNextColumn(applicationReportCounts.SearchPubMedCount);
            PopulateLastColumn(applicationReportCounts.SearchMeshCount);
        }
    }
}