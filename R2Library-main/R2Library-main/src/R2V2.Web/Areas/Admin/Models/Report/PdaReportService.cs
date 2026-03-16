#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class PdaReportService : WebReportServiceBase
    {
        public PdaReportService(
            ILog<ReportServiceBase> log
            , IQueryable<SavedReport> savedReports
            , IReportService reportService
            , IAdminContext adminContext
            , IpAddressRangeService ipAddressRangeService
        ) : base(log, savedReports, reportService, adminContext, ipAddressRangeService)
        {
        }

        public List<PdaCountsReportDataItem> GetPdaReportData(ReportQuery reportQuery)
        {
            Init(reportQuery);
            return ReportService.GetPdaReportCounts(ReportRequest);
        }
    }
}