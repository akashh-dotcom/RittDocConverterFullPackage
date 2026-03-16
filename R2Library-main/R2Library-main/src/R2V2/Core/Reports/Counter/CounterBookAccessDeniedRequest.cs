#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.RequestLogger;

#endregion

namespace R2V2.Core.Reports.Counter
{
    /// <summary>
    ///     Book Access Denied Requests
    /// </summary>
    public class CounterBookAccessDeniedRequests : CounterReportBase
    {
        public CounterBookAccessDeniedRequests(ReportRequest request,
            IEnumerable<CounterBookAccessDeniedResourceDataBase> counterBookAccessDeniedResources)
        {
            ReportRequest = request;

            CounterBookAccessDeniedResources = new List<CounterBookAccessDeniedResource>();

            foreach (var resource in counterBookAccessDeniedResources)
            {
                var deniedResource = CounterBookAccessDeniedResources.FirstOrDefault(x => x.Isbn10 == resource.Isbn10);
                if (deniedResource == null)
                {
                    deniedResource = new CounterBookAccessDeniedResource
                    {
                        Title = resource.Title,
                        Publisher = resource.Publisher,
                        PublisherId = resource.PublisherId,
                        ProprietaryId = resource.ProprietaryId,
                        Isbn10 = resource.Isbn10,
                        Isbn13 = resource.Isbn13,
                        YearOfPublication = resource.YearOfPublication,
                        AccessTurnawayPeriods = new List<RequestPeriod>(),
                        ConcurrencyTurnawayPeriods = new List<RequestPeriod>()
                    };
                    CounterBookAccessDeniedResources.Add(deniedResource);
                }

                switch (resource.TurnawayTypeId)
                {
                    case (int)ContentTurnawayType.Access:
                        deniedResource.AccessTurnawayPeriods.Add(new RequestPeriod
                            { HitCount = resource.HitCount, Month = resource.Month, Year = resource.Year });
                        break;
                    case (int)ContentTurnawayType.Concurrency:
                        deniedResource.ConcurrencyTurnawayPeriods.Add(new RequestPeriod
                            { HitCount = resource.HitCount, Month = resource.Month, Year = resource.Year });
                        break;
                }
            }

            foreach (var resource in CounterBookAccessDeniedResources)
            {
                var startMonth = request.DateRangeStart.Month;
                var startYear = request.DateRangeStart.Year;

                var endMonth = request.DateRangeEnd.Month;
                var endYear = request.DateRangeEnd.Year;

                while (startMonth != endMonth + 1 || startYear != endYear)
                {
                    var foundRequestPeriod =
                        resource.AccessTurnawayPeriods.FirstOrDefault(x =>
                            x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        resource.AccessTurnawayPeriods.Add(new RequestPeriod
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
                        resource.ConcurrencyTurnawayPeriods.FirstOrDefault(x =>
                            x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        resource.ConcurrencyTurnawayPeriods.Add(new RequestPeriod
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

                resource.AccessTurnawayPeriods =
                    resource.AccessTurnawayPeriods.OrderBy(x => x.Month).ThenBy(x => x.Year).ToList();
                resource.ConcurrencyTurnawayPeriods = resource.ConcurrencyTurnawayPeriods.OrderBy(x => x.Month)
                    .ThenBy(x => x.Year).ToList();
            }
        }

        public List<CounterBookAccessDeniedResource> CounterBookAccessDeniedResources { get; set; }
    }

    public class CounterBookAccessDeniedResourceDataBase : CounterResource
    {
        public int HitCount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public int TurnawayTypeId { get; set; }
        public string TurnawayType { get; set; }
    }

    public class CounterBookAccessDeniedResource : CounterResource
    {
        public List<RequestPeriod> AccessTurnawayPeriods { get; set; }
        public List<RequestPeriod> ConcurrencyTurnawayPeriods { get; set; }
    }
}