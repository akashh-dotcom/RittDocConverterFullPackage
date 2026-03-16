#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Reports;
using R2V2.Core.Reports.Counter;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterBookRequestsDetail : CounterReportModel
    {
        public List<int> ResourceTotalItemRequestsList = new List<int>();
        public List<int> ResourceUniqueTitleRequestsList = new List<int>();

        public CounterBookRequestsDetail(IAdminInstitution institution)
            : base(institution)
        {
            ToolLinks = new ToolLinks { HidePrint = true };
            Type = ReportType.CounterBookRequests;
        }

        public CounterBookRequestsDetail(IAdminInstitution institution, CounterBookRequests counterBookRequests,
            ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterBookRequests;
            CounterBookRequests = counterBookRequests;

            //This erroneously changes the report query after it has been submitted by the user.    -DRJ
            /*if (!IsPublisherUser)
            {
                ReportQuery.IncludeTrialStats = true;
            }*/

            var counter = 0;
            foreach (var resource in counterBookRequests.CounterBookRequestResources)
            {
                for (var i = 0; i < resource.TotalItemResourcePeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        ResourceTotalItemRequestsList.Add(resource.TotalItemResourcePeriods[i].HitCount);
                    }
                    else
                    {
                        if (i >= 0 && i < ResourceTotalItemRequestsList.Count &&
                            i < resource.TotalItemResourcePeriods.Count &&
                            resource.TotalItemResourcePeriods.ElementAtOrDefault(i)?.HitCount > 0)
                        {
                            ResourceTotalItemRequestsList[i] += resource.TotalItemResourcePeriods[i].HitCount;
                        }
                    }
                }

                for (var i = 0; i < resource.UniqueTitleResourcePeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        ResourceUniqueTitleRequestsList.Add(resource.UniqueTitleResourcePeriods[i].HitCount);
                    }
                    else
                    {
                        if (i >= 0 && i < ResourceUniqueTitleRequestsList.Count &&
                            i < resource.UniqueTitleResourcePeriods.Count &&
                            resource.UniqueTitleResourcePeriods.ElementAtOrDefault(i)?.HitCount > 0)
                        {
                            ResourceUniqueTitleRequestsList[i] += resource.UniqueTitleResourcePeriods[i].HitCount;
                        }
                    }
                }

                counter++;
            }
        }

        public bool IsEmailMode { get; set; }
        public CounterBookRequests CounterBookRequests { get; set; }

        public int ResourceTotalItemRequests()
        {
            return ResourceTotalItemRequestsList.Sum(x => x);
        }

        public int ResourceUniqueTitleRequests()
        {
            return ResourceUniqueTitleRequestsList.Sum(x => x);
        }

        /// <summary>
        /// </summary>
        public bool ContainsReportResults()
        {
            return CounterBookRequests != null && CounterBookRequests.CounterBookRequestResources != null &&
                   CounterBookRequests.CounterBookRequestResources.Any();
        }
    }
}