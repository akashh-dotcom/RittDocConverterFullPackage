#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public static class ReportDatesService
    {
        public static void SetDates(BaseReportQuery reportQuery)
        {
            DateTime startDate;
            DateTime endDate;

            var today = DateTime.Now.Date.Date;


            ReportPeriod quarter;
            switch (reportQuery.Period)
            {
                case ReportPeriod.LastTwelveMonths:
                    endDate = today;
                    startDate = today.AddYears(-1);
                    break;
                case ReportPeriod.LastSixMonths:
                    endDate = today;
                    startDate = today.AddMonths(-6);
                    break;
                case ReportPeriod.Last30Days:
                    endDate = today;
                    startDate = today.AddDays(-30);
                    break;
                case ReportPeriod.CurrentMonth:
                    endDate = today;
                    startDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0);
                    break;
                case ReportPeriod.PreviousMonth:
                    endDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0)
                        .AddDays(-1); //new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month), 23, 59, 59);
                    startDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0).AddMonths(-1);
                    break;
                //Quarter 1 : 1/1/2014 - 3/31/2014
                //Quarter 2 : 4/1/2014 - 7/30/2014
                //Quarter 3 : 8/1/2014 - 9/30/2014
                //Quarter 4 : 10/1/2013-12/31/2013
                case ReportPeriod.CurrentQuarter:
                    quarter = GetCurrentQuarter(today);

                    SetDateFromQuarter(quarter, today, out startDate, out endDate);
                    break;
                case ReportPeriod.PreviousQuarter:
                    quarter = GetCurrentQuarter(today);

                    switch (quarter)
                    {
                        case ReportPeriod.Quarter1:
                            today = today.AddYears(-1);
                            quarter = ReportPeriod.Quarter4;
                            break;
                        case ReportPeriod.Quarter2:
                            quarter = ReportPeriod.Quarter1;
                            break;
                        case ReportPeriod.Quarter3:
                            quarter = ReportPeriod.Quarter2;
                            break;
                        case ReportPeriod.Quarter4:
                            quarter = ReportPeriod.Quarter3;
                            break;
                    }

                    SetDateFromQuarter(quarter, today, out startDate, out endDate);
                    break;
                case ReportPeriod.UserSpecified:
                    startDate = reportQuery.PeriodStartDate ?? today.AddYears(-1);
                    endDate = reportQuery.PeriodEndDate ?? today;
                    break;
                case ReportPeriod.CurrentYear:
                    endDate = today;
                    startDate = new DateTime(today.Year, 1, 1, 0, 0, 0);
                    break;
                case ReportPeriod.LastYear:
                    endDate = new DateTime(today.Year, 1, 1, 0, 0, 0).AddDays(-1);
                    startDate = new DateTime(today.Year, 1, 1, 0, 0, 0).AddYears(-1);
                    break;
                case ReportPeriod.Today:
                    endDate = today;
                    startDate = today;
                    break;
                default:
                    endDate = today;
                    startDate = today.AddYears(-1);
                    break;
            }

            var startDate12Am = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 1);
            var endDate1159Pm = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0).AddDays(1)
                .AddMilliseconds(-1);


            reportQuery.PeriodStartDate = startDate12Am;
            reportQuery.PeriodEndDate = endDate1159Pm;

            //_log.DebugFormat("startDate: {0}, endDate: {1}", reportQuery.PeriodStartDate, reportQuery.PeriodEndDate);
        }

        private static ReportPeriod GetCurrentQuarter(DateTime today)
        {
            if (today >= new DateTime(today.Year, 1, 1) && today <= new DateTime(today.Year, 3, 31))
            {
                return ReportPeriod.Quarter1;
            }

            if (today >= new DateTime(today.Year, 4, 1) && today <= new DateTime(today.Year, 6, 30))
            {
                return ReportPeriod.Quarter2;
            }

            if (today >= new DateTime(today.Year, 8, 1) && today <= new DateTime(today.Year, 9, 30))
            {
                return ReportPeriod.Quarter3;
            }

            return ReportPeriod.Quarter4;
        }

        private static void SetDateFromQuarter(ReportPeriod quarter, DateTime today, out DateTime startDate,
            out DateTime endDate)
        {
            //Quarter 1 : 1/1/2014 - 3/31/2014
            //Quarter 2 : 4/1/2014 - 7/30/2014
            //Quarter 3 : 8/1/2014 - 9/30/2014
            //Quarter 4 : 10/1/2013-12/31/2013
            switch (quarter)
            {
                case ReportPeriod.Quarter1:
                    endDate = new DateTime(today.Year, 4, 1, 0, 0, 0).AddSeconds(-1);
                    startDate = new DateTime(today.Year, 1, 1, 0, 0, 0);
                    break;
                case ReportPeriod.Quarter2:
                    endDate = new DateTime(today.Year, 7, 1, 0, 0, 0).AddSeconds(-1);
                    startDate = new DateTime(today.Year, 4, 1, 0, 0, 0);
                    break;
                case ReportPeriod.Quarter3:
                    endDate = new DateTime(today.Year, 10, 1, 0, 0, 0).AddSeconds(-1);
                    startDate = new DateTime(today.Year, 7, 1, 0, 0, 0);
                    break;
                case ReportPeriod.Quarter4:
                    endDate = new DateTime(today.Year + 1, 1, 1, 0, 0, 0).AddSeconds(-1);
                    startDate = new DateTime(today.Year, 10, 1, 0, 0, 0);
                    break;
                default:
                    endDate = today;
                    startDate = today.AddYears(-1);
                    break;
            }
        }
    }
}