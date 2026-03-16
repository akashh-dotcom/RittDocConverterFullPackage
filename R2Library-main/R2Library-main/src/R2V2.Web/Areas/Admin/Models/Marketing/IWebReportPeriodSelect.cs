#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Reports;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public interface IWebReportPeriodSelect
    {
        [Display(Name = @"Period:")] SelectList PeriodList { get; }

        ReportPeriod Period { get; set; }

        [Display(Name = @"Date Range: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        [DateTenYears("PeriodStartDate")]
        DateTime? PeriodStartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        [DateTenYears("PeriodEndDate")]
        DateTime? PeriodEndDate { get; set; }
    }
}