#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Reports.Counter;
using R2V2.Web.Areas.Admin.Models.Report;
using Sushi.Core;
using ReportRequest = Sushi.Core.ReportRequest;

#endregion

namespace R2V2.Web.Counter
{
    public class UsageReportRepository : IUsageReportRepository
    {
        private readonly CounterReportService _counterReportService;
        private readonly InstitutionService _institutionService;

        public UsageReportRepository(InstitutionService institutionService, CounterReportService counterReportService)
        {
            _institutionService = institutionService;
            _counterReportService = counterReportService;
        }


        public Report[] GetUsageReports(ReportRequest request)
        {
            var institition = _institutionService.GetInstitutionForEdit(request.CustomerReference.ID);

            if (institition == null)
            {
                return null;
            }

            var reportQuery = new ReportQuery
            {
                DateRangeStart = request.ReportDefinition.Filters.UsageDateRange.Begin,
                DateRangeEnd = request.ReportDefinition.Filters.UsageDateRange.End,
                InstitutionId = institition.Id,
                Period = ReportPeriod.UserSpecified
            };


            var reportItems = new List<ReportItem>();

            var report = new Report();

            switch (request.ReportDefinition.Name)
            {
                case "TR_B1":
                    var counterBookRequests = _counterReportService.GetCounterBookRequests(reportQuery);
                    foreach (var reportItem in counterBookRequests.CounterBookRequestResources)
                    {
                        reportItems.AddRange(ProcessCounterBookRequestResource(reportItem));
                    }

                    report = new Report
                    {
                        Created = DateTime.Now, CreatedSpecified = true, ID = "TR_B1", Name = "TR_B1",
                        Title = "Counter Book Requests Report", Version = "5"
                    };
                    break;
                case "TR_B2":
                    var accessDeniedRequests = _counterReportService.GetCounterBookAccessDeniedRequests(reportQuery);
                    foreach (var reportItem in accessDeniedRequests.CounterBookAccessDeniedResources)
                    {
                        reportItems.AddRange(ProcessCounterBookAccessDeniedResource(reportItem));
                    }

                    report = new Report
                    {
                        Created = DateTime.Now, CreatedSpecified = true, ID = "TR_B2", Name = "TR_B2",
                        Title = "Counter Book Access Denied Report", Version = "5"
                    };

                    break;
                case "PR_P1":
                    var platformUsageRequests = _counterReportService.GetCounterPlatformUsageRequests(reportQuery);
                    reportItems = ProcessPlatformUsageRequests(platformUsageRequests, institition.Name);
                    report = new Report
                    {
                        Created = DateTime.Now, CreatedSpecified = true, ID = "PR_P1", Name = "PR_P1",
                        Title = "Platform Usage Report", Version = "5"
                    };
                    break;
            }

            var customer = new ReportCustomer
            {
                InstitutionalIdentifier = new List<Identifier>
                {
                    new Identifier
                        { Type = "Proprietary", Value = institition.AccountNumber }
                }.ToArray(),
                Name = institition.Name,
                ReportItems = reportItems.ToArray()
            };

            var customers = new List<ReportCustomer> { customer }.ToArray();

            report.Customer = customers.ToArray();

            var reports = new List<Report> { report }.ToArray();

            return reports;
        }

        /// <summary>
        ///     Returns 2 report items. 1 for Access Denied and 1 for Concurrency Denied
        /// </summary>
        public List<ReportItem> ProcessCounterBookAccessDeniedResource(CounterBookAccessDeniedResource turnaway)
        {
            var reportItems = new List<ReportItem>();

            var reportItem = new ReportItem
            {
                ItemPlatform = "R2library",
                ItemPublisher = turnaway.Publisher,
                ItemName = turnaway.Title,
                ItemDataType = "Book",
                ItemIdentifier = new[] { new Identifier { Type = "Print_ISBN", Value = turnaway.Isbn13 } }
            };

            var items = new List<Metric>();

            var accessMetrics = turnaway.AccessTurnawayPeriods
                .Select(period => new Metric
                {
                    Category = "Access_denied",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = period.HitCount,
                            MetricType = "No_License"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = period.BeginDate(),
                        End = period.EndDate()
                    }
                }).ToArray();

            var concurrencyMetrics = turnaway.ConcurrencyTurnawayPeriods
                .Select(period => new Metric
                {
                    Category = "Access_denied",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = period.HitCount,
                            MetricType = "Limit_Exceeded"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = period.BeginDate(),
                        End = period.EndDate()
                    }
                }).ToArray();

            items.AddRange(accessMetrics);
            items.AddRange(concurrencyMetrics);

            reportItem.ItemPerformance = items.OrderBy(x => x.Period.Begin).ThenBy(x => x.Category).ToArray();
            reportItems.Add(reportItem);

            return reportItems;
        }

        public List<ReportItem> ProcessCounterBookRequestResource(CounterBookRequestResource resource)
        {
            var reportItems = new List<ReportItem>();

            var reportItem = new ReportItem
            {
                ItemPlatform = "R2library",
                ItemPublisher = resource.Publisher,
                ItemName = resource.Title,
                ItemDataType = "Book",
                ItemIdentifier = new[] { new Identifier { Type = "Print_ISBN", Value = resource.Isbn13 } }
            };

            var items = new List<Metric>();

            var totalItemMetrics = resource.TotalItemResourcePeriods
                .Select(period => new Metric
                {
                    Category = "Total Item Requests",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = period.HitCount,
                            MetricType = "Total_Item_Requests"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = period.BeginDate(),
                        End = period.EndDate()
                    }
                }).ToArray();

            var uniqueTitleMetrics = resource.UniqueTitleResourcePeriods
                .Select(period => new Metric
                {
                    Category = "Unique Title Requests",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = period.HitCount,
                            MetricType = "Unique_Title_Requests"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = period.BeginDate(),
                        End = period.EndDate()
                    }
                }).ToArray();

            items.AddRange(totalItemMetrics);
            items.AddRange(uniqueTitleMetrics);

            reportItem.ItemPerformance = items.OrderBy(x => x.Period.Begin).ThenBy(x => x.Category).ToArray();
            reportItems.Add(reportItem);

            return reportItems;
        }


        public List<ReportItem> ProcessCounterSearchRequest(CounterResourceRequest resource)
        {
            var reportItems = new List<ReportItem>();

            var reportItem = new ReportItem
            {
                ItemPlatform = "R2library",
                ItemPublisher = resource.Publisher,
                ItemName = resource.Title,
                ItemDataType = "Book",
                ItemIdentifier = new[] { new Identifier { Type = "Print_ISBN", Value = resource.Isbn13 } }
            };

            var resourceMetrics = resource.ResourcePeriods
                .Select(period => new Metric
                {
                    Category = "Searches",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = period.HitCount,
                            MetricType = "search_reg"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = period.BeginDate(),
                        End = period.EndDate()
                    }
                }).ToArray();
            reportItem.ItemPerformance = resourceMetrics;
            reportItems.Add(reportItem);
            return reportItems;
        }

        public List<ReportItem> ProcessPlatformUsageRequests(CounterPlatformUsageRequest platformUsageRequests,
            string institutionName)
        {
            var reportItems = new List<ReportItem>();
            var reportItem = new ReportItem
            {
                ItemPlatform = "R2library",
                ItemDataType = "Platform",
                ItemIdentifier = new[] { new Identifier { Type = "Proprietary", Value = institutionName } }
            };

            var items = new List<Metric>();

            for (var i = 0; i < platformUsageRequests.TotalItemRequests.Count; i++)
            {
                var totalItemMetrics = new Metric
                {
                    Category = "Total Item Requests",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = platformUsageRequests.TotalItemRequests[i].HitCount,
                            MetricType = "Total_Item_Requests"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = platformUsageRequests.TotalItemRequests[i].BeginDate(),
                        End = platformUsageRequests.TotalItemRequests[i].EndDate()
                    }
                };

                var uniqueMetrics = new Metric
                {
                    Category = "Unique Requests",
                    Instance = new List<PerformanceCounter>
                    {
                        new PerformanceCounter
                        {
                            Count = platformUsageRequests.UniqueItemRequests[i].HitCount,
                            MetricType = "Unique_Item_Requests"
                        },
                        new PerformanceCounter
                        {
                            Count = platformUsageRequests.UniqueTitleRequests[i].HitCount,
                            MetricType = "Unique_Title_Requests"
                        }
                    }.ToArray(),
                    Period = new DateRange
                    {
                        Begin = platformUsageRequests.UniqueItemRequests[i].BeginDate(),
                        End = platformUsageRequests.UniqueItemRequests[i].EndDate()
                    }
                };

                items.Add(totalItemMetrics);
                items.Add(uniqueMetrics);
            }

            reportItem.ItemPerformance = items.ToArray();
            reportItems.Add(reportItem);

            return reportItems;
        }
    }
}