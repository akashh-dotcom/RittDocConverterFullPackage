#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Reports.Counter
{
    /// <summary>
    ///     Book Requests
    /// </summary>
    public class CounterSuccessfulResourcesRequest : CounterReportBase
    {
        public CounterSuccessfulResourcesRequest(ReportRequest request,
            IEnumerable<CounterSuccessfulResourceDataBase> counterSuccessfulResources)
        {
            ReportRequest = request;
            CounterResourceRequests = new List<CounterResourceRequest>();

            foreach (var item in counterSuccessfulResources)
            {
                var counterResourceRequest = CounterResourceRequests.FirstOrDefault(x => x.Isbn10 == item.Isbn10);
                if (counterResourceRequest == null)
                {
                    counterResourceRequest = new CounterResourceRequest
                    {
                        Title = item.Title,
                        Publisher = item.Publisher,
                        Isbn10 = item.Isbn10,
                        Isbn13 = item.Isbn13,
                        ResourcePeriods = new List<RequestPeriod>()
                    };
                    CounterResourceRequests.Add(counterResourceRequest);
                }

                counterResourceRequest.ResourcePeriods.Add(new RequestPeriod
                    { HitCount = item.HitCount, Month = item.Month, Year = item.Year });
            }

            foreach (var item in CounterResourceRequests)
            {
                var startMonth = request.DateRangeStart.Month;
                var startYear = request.DateRangeStart.Year;

                var endMonth = request.DateRangeEnd.Month;
                var endYear = request.DateRangeEnd.Year;

                while (startMonth != endMonth + 1 || startYear != endYear)
                {
                    var foundRequestPeriod =
                        item.ResourcePeriods.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        item.ResourcePeriods.Add(new RequestPeriod
                            { HitCount = 0, Month = startMonth, Year = startYear });
                    }

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

                item.ResourcePeriods = item.ResourcePeriods.OrderBy(x => x.Month).ThenBy(x => x.Year).ToList();
            }
        }

        public List<CounterResourceRequest> CounterResourceRequests { get; set; }
    }

    public class CounterSuccessfulResourceDataBase : CounterResource
    {
        public int HitCount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class CounterResourceRequest : CounterResource
    {
        public List<RequestPeriod> ResourcePeriods { get; set; }
    }
}