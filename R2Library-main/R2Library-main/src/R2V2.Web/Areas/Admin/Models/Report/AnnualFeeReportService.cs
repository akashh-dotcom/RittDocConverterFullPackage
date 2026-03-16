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
    public class AnnualFeeReportService : WebReportServiceBase
    {
        public AnnualFeeReportService(
            ILog<ReportServiceBase> log
            , IQueryable<SavedReport> savedReports
            , IReportService reportService
            , IAdminContext adminContext
            , IpAddressRangeService ipAddressRangeService
        ) : base(log, savedReports, reportService, adminContext, ipAddressRangeService)
        {
        }

        public List<AnnualFeeReportDataItem> GetAnnualFeeReportData(ReportQuery reportQuery)
        {
            Init(reportQuery);

            return ReportService.GetAnnualFeeReportDataItems(ReportRequest);
        }
    }
}