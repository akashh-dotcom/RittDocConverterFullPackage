#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Reports
{
    public class SalesReportItems
    {
        public int TitlesSoldCount { get; set; }
        public decimal TitleSales { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<SalesReportItem> Items { get; set; }

        public override string ToString()
        {
            return new StringBuilder("SalesReportItems = [")
                .AppendFormat(", TitlesSoldCount: {0}", TitlesSoldCount)
                .AppendFormat(", TitleSales: {0}", TitleSales)
                .Append("]")
                .ToString();
        }
    }

    public class SalesReportItem
    {
        public virtual int ResourceId { get; set; }
        public virtual int Licenses { get; set; }
        public virtual decimal TotalSales { get; set; }
        public virtual string SortTitle { get; set; }
        public virtual DateTime ReleaseDate { get; set; }
        public virtual DateTime CopyRightDate { get; set; }
        public IResource Resource { get; set; }


        public override string ToString()
        {
            var sb = new StringBuilder("SalesReportItem = [");

            sb.AppendFormat("ResourceId: {0}", ResourceId);
            sb.AppendFormat(", Licenses: {0}", Licenses);
            sb.AppendFormat(", TotalSales: {0}", TotalSales);
            sb.Append("]");

            return sb.ToString();
        }
    }
}