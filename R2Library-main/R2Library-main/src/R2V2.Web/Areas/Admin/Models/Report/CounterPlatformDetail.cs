#region

using System.Linq;
using R2V2.Core.Admin;
using R2V2.Core.Reports;
using R2V2.Core.Reports.Counter;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterPlatformDetail : CounterReportModel
    {
        public CounterPlatformDetail(IAdminInstitution institution)
            : base(institution)
        {
            Type = ReportType.CounterPlatformRequests;
            ToolLinks = new ToolLinks { HidePrint = true };
        }

        public CounterPlatformDetail(IAdminInstitution institution,
            CounterTotalSearchesRequest counterTotalSearchesRequests, ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterPlatformRequests;
            CounterTotalSearchesRequests = counterTotalSearchesRequests;
        }

        public CounterPlatformDetail(IAdminInstitution institution,
            CounterPlatformUsageRequest counterPlatformUsageRequests, ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.CounterPlatformRequests;
            CounterPlatformUsageRequests = counterPlatformUsageRequests;

            //This erroneously changes the report query after it has been submitted by the user.    -DRJ
            /*if (!IsPublisherUser)
            {
                ReportQuery.IncludeTrialStats = true;
            }*/
        }

        public CounterTotalSearchesRequest CounterTotalSearchesRequests { get; set; }
        public CounterPlatformUsageRequest CounterPlatformUsageRequests { get; set; }

        public string SearchRequestsTotal()
        {
            return CounterTotalSearchesRequests.SearchRequests.Sum(x => x.HitCount).ToString();
        }

        public string ResourceRequestsTotal()
        {
            return CounterTotalSearchesRequests.ResourceRequests.Sum(x => x.HitCount).ToString();
        }

        public string SectionRequestsTotal()
        {
            return CounterTotalSearchesRequests.SectionRequests.Sum(x => x.HitCount).ToString();
        }

        public string TotalItemRequestsTotal()
        {
            return CounterPlatformUsageRequests.TotalItemRequests.Sum(x => x.HitCount).ToString();
        }

        public string UniqueItemRequestsTotal()
        {
            return CounterPlatformUsageRequests.UniqueItemRequests.Sum(x => x.HitCount).ToString();
        }

        public string UniqueTitleRequestsTotal()
        {
            return CounterPlatformUsageRequests.UniqueTitleRequests.Sum(x => x.HitCount).ToString();
        }

        public bool ContainsReportResults()
        {
            return (CounterTotalSearchesRequests?.ResourceRequests != null &&
                    CounterTotalSearchesRequests.SearchRequests != null &&
                    CounterTotalSearchesRequests.SectionRequests != null)
                   || (CounterPlatformUsageRequests?.TotalItemRequests != null &&
                       CounterPlatformUsageRequests.UniqueItemRequests != null &&
                       CounterPlatformUsageRequests.UniqueTitleRequests != null);
        }
    }
}