#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Reports
{
    public class PublisherReportCounts
    {
        public int NewTitlesCount { get; set; }
        public int TitlesSoldCount { get; set; }
        public decimal TitleSales { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<PublisherReportCount> Items { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder("PublisherReportCounts = [");

            sb.AppendFormat("NewTitlesCount: {0}", NewTitlesCount);
            sb.AppendFormat(", TitlesSoldCount: {0}", TitlesSoldCount);
            sb.AppendFormat(", TitleSales: {0}", TitleSales);
            sb.Append("]");

            return sb.ToString();
        }
    }

    public class PublisherReportCount
    {
        public virtual int ResourceId { get; set; }
        public virtual int Licenses { get; set; }
        public virtual decimal TotalSales { get; set; }

        public virtual bool IsNewTitle { get; set; }

        public IResource Resource { get; set; }


        public override string ToString()
        {
            var sb = new StringBuilder("PublisherReportCounts = [");

            sb.AppendFormat("ResourceId: {0}", ResourceId);
            sb.AppendFormat(", Licenses: {0}", Licenses);
            sb.AppendFormat(", TotalSales: {0}", TotalSales);
            sb.AppendFormat(", IsNewTitle: {0}", IsNewTitle);
            sb.Append("]");

            return sb.ToString();
        }
    }
}