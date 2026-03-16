#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class WebAutomatedCartReportQuery : AutomatedCartReportQuery, IWebReportPeriodSelect
    {
        private SelectList _periodList;

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
    }
}