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
    public class CounterSearchDetail : CounterReportModel
    {
        public List<int> ResourceTotalList = new List<int>();

        public CounterSearchDetail(IAdminInstitution institution)
            : base(institution)
        {
            Type = ReportType.CounterSearchRequests;
            ToolLinks = new ToolLinks { HidePrint = true };
        }

        public CounterSearchDetail(IAdminInstitution institution, CounterSearchesRequest counterSearchRequest,
            ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterSearchRequests;
            CounterSearchRequest = counterSearchRequest;
            var counter = 0;
            foreach (var item in counterSearchRequest.CounterSearchRequests)
            {
                for (var i = 0; i < item.ResourcePeriods.Count; i++)
                {
                    if (counter == 0)
                    {
                        ResourceTotalList.Add(item.ResourcePeriods[i].HitCount);
                    }
                    else
                    {
                        ResourceTotalList[i] += item.ResourcePeriods[i].HitCount;
                    }
                }

                counter++;
            }
        }

        public bool IsEmailMode { get; set; }
        public CounterSearchesRequest CounterSearchRequest { get; set; }

        public string ResourceTotal()
        {
            return ResourceTotalList.Sum(x => x).ToString();
        }

        public bool ContainsReportResults()
        {
            return CounterSearchRequest != null && CounterSearchRequest.CounterSearchRequests != null &&
                   CounterSearchRequest.CounterSearchRequests.Any();
        }
    }
}