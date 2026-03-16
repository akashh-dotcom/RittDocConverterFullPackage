#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Territory;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class PdaCountsReportDetail : ReportModel
    {
        private SelectList _periodList;

        public PdaCountsReportDetail()
        {
        }

        public PdaCountsReportDetail(ReportQuery reportQuery)
            : base(null, reportQuery)
        {
            OverRidePeriodList(PeriodList);
        }

        public List<PdaCountsReportItem> PdaCountsReportItems { get; set; }

        public string DateRangeText { get; set; }

        public int ReportDataCount { get; set; }

        [Display(Name = @"Period:")]
        public SelectList PeriodList =>
            _periodList ??
            (_periodList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = @"last 12 months", Value = $"{ReportPeriod.LastTwelveMonths}" },
                new SelectListItem { Text = @"last 6 months", Value = $"{ReportPeriod.LastSixMonths}" },
                new SelectListItem { Text = @"last 30 days", Value = $"{ReportPeriod.Last30Days}" },
                new SelectListItem { Text = @"previous month", Value = $"{ReportPeriod.PreviousMonth}" },
                new SelectListItem { Text = @"current month", Value = $"{ReportPeriod.CurrentMonth}" },
                new SelectListItem { Text = @"specify a date range", Value = $"{ReportPeriod.UserSpecified}" },
                new SelectListItem { Text = @"current quarter", Value = $"{ReportPeriod.CurrentQuarter}" },
                new SelectListItem { Text = @"previous quarter", Value = $"{ReportPeriod.PreviousQuarter}" }
            }, "Value", "Text", (int)ReportQuery.Period));

        [Display(Name = @"Territory:")] public SelectList TerritoryList { get; set; }

        public void SetReportData(List<PdaCountsReportDataItem> pdaReportCounts, IEnumerable<ITerritory> territories,
            IEnumerable<IInstitution> institutions)
        {
            PdaCountsReportItems = new List<PdaCountsReportItem>();
            foreach (var pdaReportCount in pdaReportCounts)
            {
                PdaCountsReportItems.Add(new PdaCountsReportItem(pdaReportCount));
            }

            ReportDataCount = PdaCountsReportItems.Count;

            var territorySelectList = new List<SelectListItem>
            {
                new SelectListItem { Text = @"All", Value = null }
            };
            territorySelectList.AddRange(territories.Select(x => new SelectListItem { Text = x.Name, Value = x.Code }));
            TerritoryList = new SelectList(territorySelectList, "Value", "Text", ReportQuery.TerritoryCode);
        }
    }
}