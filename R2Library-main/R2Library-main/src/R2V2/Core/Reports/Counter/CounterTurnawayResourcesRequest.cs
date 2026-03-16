#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.RequestLogger;

#endregion

namespace R2V2.Core.Reports.Counter
{
    /// <summary>
    ///     Book #3
    /// </summary>
    public class CounterTurnawayResourcesRequest : CounterReportBase
    {
        public CounterTurnawayResourcesRequest(ReportRequest request,
            IEnumerable<CounterTurnawayResourceDataBase> counterTurnawayResources)
        {
            ReportRequest = request;

            CounterTurnawayRequests = new List<CounterTurnawayRequest>();

            foreach (var resource in counterTurnawayResources)
            {
                var turnawayRequest = CounterTurnawayRequests.FirstOrDefault(x => x.Isbn10 == resource.Isbn10);
                if (turnawayRequest == null)
                {
                    turnawayRequest = new CounterTurnawayRequest
                    {
                        Title = resource.Title,
                        Publisher = resource.Publisher,
                        Isbn10 = resource.Isbn10,
                        Isbn13 = resource.Isbn13,
                        AccessTurnawayPeriods = new List<RequestPeriod>(),
                        ConcurrencyTurnawayPeriods = new List<RequestPeriod>()
                    };
                    CounterTurnawayRequests.Add(turnawayRequest);
                }

                switch (resource.TurnawayTypeId)
                {
                    case (int)ContentTurnawayType.Access:
                        turnawayRequest.AccessTurnawayPeriods.Add(new RequestPeriod
                            { HitCount = resource.HitCount, Month = resource.Month, Year = resource.Year });
                        break;
                    case (int)ContentTurnawayType.Concurrency:
                        turnawayRequest.ConcurrencyTurnawayPeriods.Add(new RequestPeriod
                            { HitCount = resource.HitCount, Month = resource.Month, Year = resource.Year });
                        break;
                }
            }

            foreach (var turnawayRequest in CounterTurnawayRequests)
            {
                var startMonth = request.DateRangeStart.Month;
                var startYear = request.DateRangeStart.Year;

                var endMonth = request.DateRangeEnd.Month;
                var endYear = request.DateRangeEnd.Year;

                while (startMonth != endMonth + 1 || startYear != endYear)
                {
                    var foundRequestPeriod =
                        turnawayRequest.AccessTurnawayPeriods.FirstOrDefault(x =>
                            x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        turnawayRequest.AccessTurnawayPeriods.Add(new RequestPeriod
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

                startMonth = request.DateRangeStart.Month;
                startYear = request.DateRangeStart.Year;

                endMonth = request.DateRangeEnd.Month;
                endYear = request.DateRangeEnd.Year;

                while (startMonth != endMonth + 1 || startYear != endYear)
                {
                    var foundRequestPeriod =
                        turnawayRequest.ConcurrencyTurnawayPeriods.FirstOrDefault(x =>
                            x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        turnawayRequest.ConcurrencyTurnawayPeriods.Add(new RequestPeriod
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

                turnawayRequest.AccessTurnawayPeriods = turnawayRequest.AccessTurnawayPeriods.OrderBy(x => x.Month)
                    .ThenBy(x => x.Year).ToList();
                turnawayRequest.ConcurrencyTurnawayPeriods = turnawayRequest.ConcurrencyTurnawayPeriods
                    .OrderBy(x => x.Month).ThenBy(x => x.Year).ToList();
            }
        }

        public List<CounterTurnawayRequest> CounterTurnawayRequests { get; set; }
    }

    public class CounterTurnawayResourceDataBase : CounterResource
    {
        public int HitCount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public int TurnawayTypeId { get; set; }
        public string TurnawayType { get; set; }
    }

    public class CounterTurnawayRequest : CounterResource
    {
        public List<RequestPeriod> AccessTurnawayPeriods { get; set; }
        public List<RequestPeriod> ConcurrencyTurnawayPeriods { get; set; }
    }
}