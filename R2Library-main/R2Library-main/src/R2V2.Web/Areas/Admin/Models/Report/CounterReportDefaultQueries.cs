#region

using R2V2.Core.Admin;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterReportDefaultQueries : AdminBaseModel
    {
        public CounterReportDefaultQueries(IAdminInstitution institution)
            : base(institution)
        {
        }

        public ReportQuery BookRequests => DefaultReportQuery(ReportType.CounterBookRequests);
        public ReportQuery BookAccessDeniedRequests => DefaultReportQuery(ReportType.CounterDeniedRequests);
        public ReportQuery PlatformRequests => DefaultReportQuery(ReportType.CounterPlatformRequests);

        public static ReportQuery NormalizeReportQuery(ReportQuery reportQuery, ReportType reportType,
            IAdminInstitution institution)
        {
            if (!reportQuery.IncludePurchasedTitles && !reportQuery.IncludePdaTitles && !reportQuery.IncludeTrialStats)
            {
                var defaultQuery = new CounterReportDefaultQueries(institution).DefaultReportQuery(reportType);

                reportQuery.IncludePurchasedTitles = defaultQuery.IncludePurchasedTitles;
                reportQuery.IncludePdaTitles = defaultQuery.IncludePdaTitles;
                reportQuery.IncludeTrialStats = defaultQuery.IncludeTrialStats;
            }

            return reportQuery;
        }

        private ReportQuery DefaultReportQuery(ReportType reportType)
        {
            return new ReportQuery
            {
                IncludePurchasedTitles = true,
                IncludePdaTitles = !IsPublisherUser,
                IncludeTrialStats = !IsPublisherUser && reportType != ReportType.CounterDeniedRequests
            };
        }
    }
}