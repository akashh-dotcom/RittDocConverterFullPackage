#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class RequestedResourcesQuery : BaseReportQuery, IWebReportPeriodSelect
    {
        private SelectList _periodList;
        public string[] TerritoryCodes { get; set; }
        public int[] InstitutionTypeIds { get; set; }

        public string AccountNumberBatch { get; set; }

        public SelectList PeriodList => _periodList ??
                                        (_periodList = new SelectList(new List<SelectListItem>
                                        {
                                            new SelectListItem
                                            {
                                                Text = @"last 12 months", Value = $"{ReportPeriod.LastTwelveMonths}"
                                            },
                                            new SelectListItem
                                                { Text = @"last 6 months", Value = $"{ReportPeriod.LastSixMonths}" },
                                            new SelectListItem
                                                { Text = @"last 30 days", Value = $"{ReportPeriod.Last30Days}" },
                                            new SelectListItem
                                                { Text = @"previous month", Value = $"{ReportPeriod.PreviousMonth}" },
                                            new SelectListItem
                                                { Text = @"current month", Value = $"{ReportPeriod.CurrentMonth}" },
                                            new SelectListItem
                                            {
                                                Text = @"specify a date range", Value = $"{ReportPeriod.UserSpecified}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = $@"{DateTime.Now.Year} entire year",
                                                Value = $"{ReportPeriod.CurrentYear}"
                                            },
                                            new SelectListItem
                                            {
                                                Text = $@"{DateTime.Now.Year - 1} entire year",
                                                Value = $"{ReportPeriod.LastYear}"
                                            },
                                            new SelectListItem
                                                { Text = @"current quarter", Value = $"{ReportPeriod.CurrentQuarter}" },
                                            new SelectListItem
                                            {
                                                Text = @"previous quarter", Value = $"{ReportPeriod.PreviousQuarter}"
                                            }
                                        }, "Value", "Text", (int)Period));

        public string[] GetAccountNumberArray()
        {
            if (string.IsNullOrWhiteSpace(AccountNumberBatch))
            {
                return null;
            }

            var cleanNumberBatch = AccountNumberBatch.Replace(" ", "");
            var accountNumberArray = cleanNumberBatch.Split(',');
            return accountNumberArray;
        }

        //[Display(Name = @"Date Range: ")]
        //[DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "", ApplyFormatInEditMode = true)]
        //[DateTenYears("PeriodStartDate")]
        //public DateTime? PeriodStartDate { get; set; }

        //[DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "", ApplyFormatInEditMode = true)]
        //[DateTenYears("PeriodEndDate")]
        //public DateTime? PeriodEndDate { get; set; }
    }
}