#region

using System;
using System.Text;
using System.Web.Routing;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    [Serializable]
    public class PublisherReportQuery
    {
        public PublisherReportPeriod Period { get; set; } = PublisherReportPeriod.CurrentYear;
        public int PublisherId { get; set; }

        public RouteValueDictionary ToExportValues()
        {
            var routeValueDictionary = new RouteValueDictionary(new { Area = "Admin" })
            {
                { "PublisherId", PublisherId },
                { "Period", Period }
            };
            return routeValueDictionary;
        }

        public string ToDebugString()
        {
            return new StringBuilder()
                .Append("PublisherReportQuery : [")
                .AppendFormat("Period: {0},", Period)
                .AppendFormat("PublisherId: {0},", PublisherId)
                .Append("]")
                .ToString();
        }
    }
}