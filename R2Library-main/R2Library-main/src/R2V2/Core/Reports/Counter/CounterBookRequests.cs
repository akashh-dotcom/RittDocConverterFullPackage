#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Core.Reports.Counter
{
    /// <summary>
    ///     Book Requests
    /// </summary>
    public class CounterBookRequests : CounterReportBase
    {
        public CounterBookRequests(ReportRequest request, IEnumerable<CounterBookRequestDataBase> counterResources)
        {
            ReportRequest = request;
            CounterBookRequestResources = new List<CounterBookRequestResource>();

            foreach (var item in counterResources)
            {
                var counterResourceRequest = CounterBookRequestResources.FirstOrDefault(x => x.Isbn10 == item.Isbn10);
                if (counterResourceRequest == null)
                {
                    counterResourceRequest = new CounterBookRequestResource
                    {
                        Title = item.Title,
                        Publisher = item.Publisher,
                        PublisherId = item.PublisherId,
                        ProprietaryId = item.ProprietaryId,
                        Isbn10 = item.Isbn10,
                        Isbn13 = item.Isbn13,
                        YearOfPublication = item.YearOfPublication,
                        TotalItemResourcePeriods = new List<RequestPeriod>(),
                        UniqueTitleResourcePeriods = new List<RequestPeriod>()
                    };
                    CounterBookRequestResources.Add(counterResourceRequest);
                }

                switch (item.RequestType)
                {
                    case "Item":
                        counterResourceRequest.TotalItemResourcePeriods.Add(new RequestPeriod
                            { HitCount = item.HitCount, Month = item.Month, Year = item.Year });
                        break;
                    case "Title":
                        counterResourceRequest.UniqueTitleResourcePeriods.Add(new RequestPeriod
                            { HitCount = item.UniqueHitCount, Month = item.Month, Year = item.Year });
                        break;
                    default:
                        throw new Exception($"Unknown counter report request type: {item.RequestType}");
                }
            }

            foreach (var item in CounterBookRequestResources)
            {
                var startMonth = request.DateRangeStart.Month;
                var startYear = request.DateRangeStart.Year;

                var endMonth = request.DateRangeEnd.Month;
                var endYear = request.DateRangeEnd.Year;

                while (startMonth != endMonth + 1 || startYear != endYear)
                {
                    var foundRequestPeriod =
                        item.TotalItemResourcePeriods.FirstOrDefault(x => x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        item.TotalItemResourcePeriods.Add(new RequestPeriod
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
                        item.UniqueTitleResourcePeriods.FirstOrDefault(x =>
                            x.Month == startMonth && x.Year == startYear);
                    if (foundRequestPeriod == null)
                    {
                        item.UniqueTitleResourcePeriods.Add(new RequestPeriod
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

                item.TotalItemResourcePeriods =
                    item.TotalItemResourcePeriods.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
                item.UniqueTitleResourcePeriods =
                    item.UniqueTitleResourcePeriods.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
            }
        }

        public List<CounterBookRequestResource> CounterBookRequestResources { get; set; }
    }

    public class CounterBookRequestDataBase : CounterResource
    {
        public int HitCount { get; set; }
        public int UniqueHitCount { get; set; }
        public string RequestType { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class CounterBookRequestResource : CounterResource
    {
        public List<RequestPeriod> TotalItemResourcePeriods { get; set; }

        public List<RequestPeriod> UniqueTitleResourcePeriods { get; set; }
    }
}