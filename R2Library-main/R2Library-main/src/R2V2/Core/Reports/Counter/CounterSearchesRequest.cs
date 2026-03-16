#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Reports.Counter
{
    public class CounterSearchesRequest : CounterReportBase
    {
        public CounterSearchesRequest(ReportRequest request,
            IEnumerable<CounterSuccessfulResourceDataBase> counterTurnawayResources)
        {
            ReportRequest = request;
            CounterSearchRequests = new List<CounterResourceRequest>();

            foreach (var item in counterTurnawayResources)
            {
                var counterResourceRequest = CounterSearchRequests.FirstOrDefault(x => x.Isbn10 == item.Isbn10);
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
                    CounterSearchRequests.Add(counterResourceRequest);
                }

                counterResourceRequest.ResourcePeriods.Add(new RequestPeriod
                    { HitCount = item.HitCount, Month = item.Month, Year = item.Year });
            }

            foreach (var item in CounterSearchRequests)
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

                //item.ResourcePeriods = item.ResourcePeriods.OrderBy(x => x.Month).OrderBy(x => x.Year).ToList();
                item.ResourcePeriods = item.ResourcePeriods.OrderBy(x => x.Month).ThenBy(x => x.Year).ToList();
            }
        }

        public List<CounterResourceRequest> CounterSearchRequests { get; set; }
    }
}