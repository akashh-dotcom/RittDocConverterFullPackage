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
    public class CounterBookAccessDeniedDetail : CounterReportModel
    {
        public List<int> AccessTotalList = new List<int>();
        public List<int> ConcurrencyTotalList = new List<int>();

        public CounterBookAccessDeniedDetail(IAdminInstitution institution)
            : base(institution)
        {
            Type = ReportType.CounterDeniedRequests;
            ToolLinks = new ToolLinks { HidePrint = true };
        }

        public CounterBookAccessDeniedDetail(IAdminInstitution institution,
            CounterBookAccessDeniedRequests counterBookAccessDeniedRequests, ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterDeniedRequests;
            CounterBookAccessDeniedRequests = counterBookAccessDeniedRequests;
            var counter = 0;
            foreach (var resource in counterBookAccessDeniedRequests.CounterBookAccessDeniedResources)
            {
                for (var i = 0; i < resource.AccessTurnawayPeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        AccessTotalList.Add(resource.AccessTurnawayPeriods[i].HitCount);
                    }
                    else
                    {
                        AccessTotalList[i] += resource.AccessTurnawayPeriods[i].HitCount;
                    }
                }

                for (var i = 0; i < resource.ConcurrencyTurnawayPeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        ConcurrencyTotalList.Add(resource.ConcurrencyTurnawayPeriods[i].HitCount);
                    }
                    else
                    {
                        ConcurrencyTotalList[i] += resource.ConcurrencyTurnawayPeriods[i].HitCount;
                    }
                }

                counter++;
            }
        }

        public bool IsEmailMode { get; set; }
        public CounterBookAccessDeniedRequests CounterBookAccessDeniedRequests { get; set; }

        public int AccessTotal()
        {
            return AccessTotalList.Sum(x => x);
        }

        public int ConcurrencyTotal()
        {
            return ConcurrencyTotalList.Sum(x => x);
        }

        public bool ContainsReportResults()
        {
            return CounterBookAccessDeniedRequests != null &&
                   CounterBookAccessDeniedRequests.CounterBookAccessDeniedResources != null &&
                   CounterBookAccessDeniedRequests.CounterBookAccessDeniedResources.Any();
        }
    }
}