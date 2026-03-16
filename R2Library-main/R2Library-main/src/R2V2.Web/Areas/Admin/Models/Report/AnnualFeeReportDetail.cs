#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class AnnualFeeReportDetail : ReportModel
    {
        private SelectList _periodList;

        public AnnualFeeReportDetail()
        {
        }

        public AnnualFeeReportDetail(ReportQuery reportQuery)
            : base(null, reportQuery)
        {
            OverRidePeriodList(PeriodList);
            if (reportQuery.Period == ReportPeriod.LastTwelveMonths)
            {
                DateRangeText = "All Institutions";
            }
        }

        public List<AnnualFeeReportItem> AnnualFeeReportItems { get; set; }

        public string DateRangeText { get; set; }

        public int ReportDataCount { get; set; }

        [Display(Name = "Period:")]
        public SelectList PeriodList =>
            _periodList ??
            (_periodList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "all", Value = $"{ReportPeriod.LastTwelveMonths}" },
                new SelectListItem
                {
                    Text = "last 6 months", Value =
                        $"{ReportPeriod.LastSixMonths}"
                },
                new SelectListItem
                {
                    Text = "last 30 days", Value = $"{ReportPeriod.Last30Days}"
                },
                new SelectListItem
                {
                    Text = "previous month", Value =
                        $"{ReportPeriod.PreviousMonth}"
                },
                new SelectListItem
                {
                    Text = "current month", Value =
                        $"{ReportPeriod.CurrentMonth}"
                },
                new SelectListItem
                {
                    Text = "specify a date range", Value =
                        $"{ReportPeriod.UserSpecified}"
                },
                new SelectListItem
                {
                    Text = "current quarter", Value =
                        $"{ReportPeriod.CurrentQuarter}"
                },
                new SelectListItem
                {
                    Text = "previous quarter", Value =
                        $"{ReportPeriod.PreviousQuarter}"
                }
            }, "Value", "Text", (int)ReportQuery.Period));

        public void SetReportData(List<AnnualFeeReportDataItem> annualFeeReportDataItems)
        {
            AnnualFeeReportItems = new List<AnnualFeeReportItem>();
            foreach (var annualFeeReportDataItem in annualFeeReportDataItems)
            {
                AnnualFeeReportItems.Add(new AnnualFeeReportItem(annualFeeReportDataItem));
            }

            ReportDataCount = AnnualFeeReportItems.Count;
        }
    }
}