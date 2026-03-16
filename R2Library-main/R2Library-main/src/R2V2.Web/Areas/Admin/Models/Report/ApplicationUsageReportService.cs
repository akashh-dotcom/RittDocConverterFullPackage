#region

using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ApplicationUsageReportService : WebReportServiceBase
    {
        public ApplicationUsageReportService(ILog<ReportServiceBase> log
            , IQueryable<SavedReport> savedReports
            , IReportService reportService
            , IAdminContext adminContext
            , IpAddressRangeService ipAddressRangeService
        )
            : base(log, savedReports, reportService, adminContext, ipAddressRangeService)
        {
        }

        public ApplicationReportCounts RunApplicationUsageReport(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var applicationReportCounts = ReportService.GetApplicationReportCounts(ReportRequest);

            return applicationReportCounts;
        }
    }
}