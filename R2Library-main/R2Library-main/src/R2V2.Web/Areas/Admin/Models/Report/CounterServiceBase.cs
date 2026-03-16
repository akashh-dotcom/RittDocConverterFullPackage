#region

using System;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Counter;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterServiceBase
    {
        private readonly ILog<CounterServiceBase> _log;

        public CounterServiceBase(ILog<CounterServiceBase> log)
        {
            _log = log;
        }

        public ReportRequest
            ReportRequest { get; set; } // ReportRequest is data that is based into the Core logic to run the report

        public IInstitution Institution { get; set; }

        public void Init(ReportQuery reportQuery)
        {
            var test = new CounterService();
            Institution = test.InstitutionService.GetInstitutionForEdit(reportQuery.InstitutionId);
            ReportRequest = GetReportRequest(reportQuery);
        }

        /// <summary>
        ///     Create a ReportRequest object using the data in the ReportQuery object populated from the request
        /// </summary>
        protected ReportRequest GetReportRequest(ReportQuery reportQuery)
        {
            DateTime startDate;
            DateTime endDate;

            var today = DateTime.Now.Date.Date;
            switch (reportQuery.Period)
            {
                case ReportPeriod.LastTwelveMonths:
                    endDate = today.AddDays(1).AddMilliseconds(-1);
                    startDate = FirstDayOfMonthFromDateTime(today.AddYears(-1));
                    break;
                case ReportPeriod.LastSixMonths:
                    endDate = today.AddDays(1).AddMilliseconds(-1);
                    startDate = FirstDayOfMonthFromDateTime(today.AddMonths(-6));
                    break;
                case ReportPeriod.Last30Days:
                    endDate = today.AddDays(1).AddMilliseconds(-1);
                    startDate = today.AddDays(-30);
                    break;
                case ReportPeriod.CurrentMonth:
                    endDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month), 0, 0,
                        0);
                    startDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0);
                    break;
                case ReportPeriod.PreviousMonth:
                    var previousMonth = today.AddMonths(-1);
                    endDate = new DateTime(previousMonth.Year, previousMonth.Month,
                        DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month), 0, 0, 0);
                    startDate = new DateTime(previousMonth.Year, previousMonth.Month, 1, 0, 0, 0);
                    break;
                case ReportPeriod.UserSpecified:
                    try
                    {
                        startDate = reportQuery.DateRangeStart == null
                            ? today.AddYears(-1)
                            : (DateTime)reportQuery.DateRangeStart;
                        endDate = reportQuery.DateRangeEnd == null
                            ? today.AddDays(1).AddMilliseconds(-1)
                            : (DateTime)reportQuery.DateRangeEnd;
                    }
                    catch (Exception ex)
                    {
                        endDate = today.AddDays(1).AddMilliseconds(-1);
                        startDate = FirstDayOfMonthFromDateTime(today.AddYears(-1));
                        _log.WarnFormat("Invalid date, reportQuery.DateRangeStart: {0}, reportQuery.DateRangeEnd: {1}",
                            reportQuery.DateRangeStart, reportQuery.DateRangeEnd);
                        _log.Error(ex.Message, ex);
                    }

                    break;
                case ReportPeriod.CurrentYear:
                    endDate = DateTime.Now;
                    startDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, 1);
                    break;
                case ReportPeriod.LastYear:
                    endDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, 0).AddMilliseconds(-1);
                    startDate = new DateTime(DateTime.Now.Year - 1, 1, 1, 0, 0, 0, 1);
                    break;
                default:
                    endDate = today.AddDays(1).AddMilliseconds(-1);
                    startDate = FirstDayOfMonthFromDateTime(today.AddYears(-1));
                    break;
            }


            _log.DebugFormat("startDate: {0}, endDate: {1}", startDate, endDate);

            var reportRequest = new ReportRequest
            {
                InstitutionId = reportQuery.InstitutionId,
                DateRangeStart = startDate,
                DateRangeEnd = endDate,
                IncludePdaTitles = reportQuery.IncludePdaTitles,
                IncludePurchasedTitles = reportQuery.IncludePurchasedTitles,
                IncludeTrialStats = reportQuery.IncludeTrialStats,
                Period = reportQuery.Period,
                Type = (ReportType)reportQuery.ReportTypeId
            };
            return reportRequest;
        }

        public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }
    }
}