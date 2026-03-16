#region

using R2V2.Core.Reports.Counter;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterReportService : CounterServiceBase
    {
        private readonly CounterFactory _counterService;

        public CounterReportService(
            ILog<CounterServiceBase> log
            , CounterFactory counterService)
            : base(log)
        {
            _counterService = counterService;
        }

        /// <summary>
        ///     Book #2
        /// </summary>
        public CounterSuccessfulResourcesRequest GetCounterSuccessfulSectionRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var counterSuccessfulResourceRequests = _counterService.GetCounterSuccessfulSectionRequests(ReportRequest);

            return counterSuccessfulResourceRequests;
        }

        /// <summary>
        ///     Book #3
        /// </summary>
        public CounterTurnawayResourcesRequest GetCounterTurnawayResourceRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var counterTurnawayResourcesRequests = _counterService.GetCounterTurnawayResourceRequests(ReportRequest);
            return counterTurnawayResourcesRequests;
        }

        /// <summary>
        ///     Book #5
        /// </summary>
        public CounterSearchesRequest GetCounterSearchResourceRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var test = _counterService.GetCounterSearchResourceRequests(ReportRequest);
            return test;
        }

        /// <summary>
        ///     Platform #1
        /// </summary>
        public CounterTotalSearchesRequest GetCounterTotalSearchesRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var test = _counterService.GetCounterTotalSearchesRequests(ReportRequest);
            return test;
        }

        #region Counter 5.0

        /// <summary>
        ///     Book Requests
        /// </summary>
        public CounterBookRequests GetCounterBookRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var counterBookRequests = _counterService.GetCounterBookRequests(ReportRequest);

            return counterBookRequests;
        }

        /// <summary>
        ///     Book #3
        /// </summary>
        public CounterBookAccessDeniedRequests GetCounterBookAccessDeniedRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var counterBookAccessDeniedRequests = _counterService.GetCounterBookAccessDeniedRequests(ReportRequest);
            return counterBookAccessDeniedRequests;
        }

        /// <summary>
        ///     Platform Usage
        /// </summary>
        public CounterPlatformUsageRequest GetCounterPlatformUsageRequests(ReportQuery reportQuery)
        {
            Init(reportQuery);

            var test = _counterService.GetCounterPlatformUsageRequests(ReportRequest);
            return test;
        }

        #endregion Counter 5.0
    }
}