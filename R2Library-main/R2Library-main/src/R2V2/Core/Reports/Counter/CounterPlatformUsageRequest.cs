#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Reports.Counter
{
    public class CounterPlatformUsageRequest : CounterReportBase
    {
        public CounterPlatformUsageRequest(List<RequestPeriod> totalItemRequests,
            List<RequestPeriod> uniqueItemRequests, List<RequestPeriod> uniqueTitleRequests, ReportRequest request)
        {
            ReportRequest = request;
            TotalItemRequests = new List<RequestPeriod>();
            UniqueItemRequests = new List<RequestPeriod>();
            UniqueTitleRequests = new List<RequestPeriod>();

            var startMonth = request.DateRangeStart.Month;
            var startYear = request.DateRangeStart.Year;

            var endMonth = request.DateRangeEnd.Month;
            var endYear = request.DateRangeEnd.Year;

            while (startMonth != endMonth + 1 || startYear != endYear)
            {
                var monthsTotalItemRequests =
                    totalItemRequests.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);
                var monthsUniqueItemRequests =
                    uniqueItemRequests.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);
                var monthsUniqueTitleRequests =
                    uniqueTitleRequests.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);

                TotalItemRequests.Add(monthsTotalItemRequests ?? new RequestPeriod(startMonth, startYear));

                UniqueItemRequests.Add(monthsUniqueItemRequests ?? new RequestPeriod(startMonth, startYear));

                UniqueTitleRequests.Add(monthsUniqueTitleRequests ?? new RequestPeriod(startMonth, startYear));

                if (startMonth == 12 && startYear != endYear)
                {
                    startMonth = 1;
                    startYear++;
                }
                else
                {
                    startMonth++;
                }
            }
        }

        public List<RequestPeriod> TotalItemRequests { get; set; }
        public List<RequestPeriod> UniqueItemRequests { get; set; }
        public List<RequestPeriod> UniqueTitleRequests { get; set; }
    }
}