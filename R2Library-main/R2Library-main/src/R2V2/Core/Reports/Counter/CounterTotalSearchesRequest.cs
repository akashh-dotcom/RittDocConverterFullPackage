#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Reports.Counter
{
    public class CounterTotalSearchesRequest : CounterReportBase
    {
        public CounterTotalSearchesRequest(List<RequestPeriod> searchRequests, List<RequestPeriod> sectionRequests,
            List<RequestPeriod> resourceRequests, ReportRequest request)
        {
            ReportRequest = request;
            SearchRequests = new List<RequestPeriod>();
            SectionRequests = new List<RequestPeriod>();
            ResourceRequests = new List<RequestPeriod>();

            var startMonth = request.DateRangeStart.Month;
            var startYear = request.DateRangeStart.Year;

            var endMonth = request.DateRangeEnd.Month;
            var endYear = request.DateRangeEnd.Year;

            while (startMonth != endMonth + 1 || startYear != endYear)
            {
                var monthsSearchRequests =
                    searchRequests.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);
                var monthsSectionRequests =
                    sectionRequests.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);
                var monthsResourceRequests =
                    resourceRequests.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);

                SearchRequests.Add(monthsSearchRequests ?? new RequestPeriod(startMonth, startYear));

                SectionRequests.Add(monthsSectionRequests ?? new RequestPeriod(startMonth, startYear));

                ResourceRequests.Add(monthsResourceRequests ?? new RequestPeriod(startMonth, startYear));

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

        public List<RequestPeriod> SearchRequests { get; set; }
        public List<RequestPeriod> SectionRequests { get; set; }
        public List<RequestPeriod> ResourceRequests { get; set; }
    }
}