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
    public class CounterDeniedDetail : CounterReportModel
    {
        public List<int> AccessTotalList = new List<int>();
        public List<int> ConcurrencyTotalList = new List<int>();

        public CounterDeniedDetail(IAdminInstitution institution)
            : base(institution)
        {
            Type = ReportType.CounterDeniedRequests;
            ToolLinks = new ToolLinks { HidePrint = true };
        }

        public CounterDeniedDetail(IAdminInstitution institution,
            CounterTurnawayResourcesRequest counterTurnawayResourcesRequest, ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterDeniedRequests;
            CounterTurnawayResourcesRequest = counterTurnawayResourcesRequest;
            var counter = 0;
            foreach (var turnawayRequest in counterTurnawayResourcesRequest.CounterTurnawayRequests)
            {
                for (var i = 0; i < turnawayRequest.AccessTurnawayPeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        AccessTotalList.Add(turnawayRequest.AccessTurnawayPeriods[i].HitCount);
                    }
                    else
                    {
                        AccessTotalList[i] += turnawayRequest.AccessTurnawayPeriods[i].HitCount;
                    }
                }

                for (var i = 0; i < turnawayRequest.ConcurrencyTurnawayPeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        ConcurrencyTotalList.Add(turnawayRequest.ConcurrencyTurnawayPeriods[i].HitCount);
                    }
                    else
                    {
                        ConcurrencyTotalList[i] += turnawayRequest.ConcurrencyTurnawayPeriods[i].HitCount;
                    }
                }

                counter++;
            }
        }

        public bool IsEmailMode { get; set; }
        public CounterTurnawayResourcesRequest CounterTurnawayResourcesRequest { get; set; }

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
            return CounterTurnawayResourcesRequest != null &&
                   CounterTurnawayResourcesRequest.CounterTurnawayRequests != null &&
                   CounterTurnawayResourcesRequest.CounterTurnawayRequests.Any();
        }
    }
}