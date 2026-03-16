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
    public class CounterSectionDetail : CounterReportModel
    {
        public List<int> ResourceTotalList = new List<int>();

        public CounterSectionDetail(IAdminInstitution institution)
            : base(institution)
        {
            ToolLinks = new ToolLinks { HidePrint = true };
            Type = ReportType.CounterSectionRequests;
        }

        public CounterSectionDetail(IAdminInstitution institution,
            CounterSuccessfulResourcesRequest counterSuccessfulResourceRequest, ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterSectionRequests;
            CounterSuccessfulResourceRequest = counterSuccessfulResourceRequest;
            var counter = 0;
            foreach (var item in counterSuccessfulResourceRequest.CounterResourceRequests)
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
        public CounterSuccessfulResourcesRequest CounterSuccessfulResourceRequest { get; set; }

        public int ResourceTotal()
        {
            return ResourceTotalList.Sum(x => x);
        }

        public bool ContainsReportResults()
        {
            return CounterSuccessfulResourceRequest != null &&
                   CounterSuccessfulResourceRequest.CounterResourceRequests != null &&
                   CounterSuccessfulResourceRequest.CounterResourceRequests.Any();
        }
    }
}