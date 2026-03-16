#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class SavedReportListItem
    {
        public int Id { get; set; }

        [Display(Name = @"Name: ")] public string Name { get; set; }

        [Display(Name = @"Type: ")] public string Type { get; set; }

        [Display(Name = @"Email Schedule: ")] public ReportFrequency Frequency { get; set; }

        [Display(Name = @"Date Created: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }

        [Display(Name = @"Last Updated: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }

        [Display(Name = @"Email: ")] public string Email { get; set; }

        [Display(Name = @"Institution: ")] public string InstitutionName { get; set; }
        public string InstitutionAccountNumber { get; set; }
        public int InstitutionId { get; set; }

        [Display(Name = @"Description: ")] public string Description { get; set; }

        public string ExecuteLink { get; set; }
        public string DetailLink { get; set; }
        public string DeleteLink { get; set; }
        public string AdminDeleteLink { get; set; }

        [Display(Name = @"Purchased")] public bool IncludePurchased { get; set; }

        [Display(Name = @"PDA")] public bool IncludePda { get; set; }

        [Display(Name = @"TOC")] public bool IncludeToc { get; set; }
        [Display(Name = @"Trial")] public bool IncludeTrialStats { get; set; }

        [Display(Name = @"Period: ")] public ReportPeriod Period { get; set; }

        [Display(Name = @"Type: ")] public ReportType ReportType { get; set; }

        [Display(Name = @"Period Range: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime? PeriodStartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime? PeriodEndDate { get; set; }

        [Display(Name = @"Resource: ")] public string ResourceDisplay { get; set; }

        [Display(Name = @"Publisher: ")] public string PublisherDisplay { get; set; }

        [Display(Name = @"Practice Area: ")] public string PracticeAreaDisplay { get; set; }

        //new SelectListItem {Text = "last 12 months", Value = string.Format("{0}", ReportPeriod.LastTwelveMonths)},
        //new SelectListItem {Text = "last 6 months", Value = string.Format("{0}", ReportPeriod.LastSixMonths)},
        //new SelectListItem {Text = "last 30 days", Value = string.Format("{0}", ReportPeriod.Last30Days)},
        //new SelectListItem {Text = "previous month", Value = string.Format("{0}", ReportPeriod.PreviousMonth)},
        //new SelectListItem {Text = "current month", Value = string.Format("{0}", ReportPeriod.CurrentMonth)},
        //new SelectListItem {Text = "specify a date range", Value = string.Format("{0}", ReportPeriod.UserSpecified)}
        public string PeriodDisplay()
        {
            switch (Period)
            {
                case ReportPeriod.Last30Days:
                    return "Last 30 days";
                case ReportPeriod.LastSixMonths:
                    return "Last 6 months";
                case ReportPeriod.LastTwelveMonths:
                    return "Last 12 months";
                case ReportPeriod.UserSpecified:
                    return "Specified a date range";
                case ReportPeriod.CurrentMonth:
                    return "Current month";
                case ReportPeriod.CurrentYear:
                    return $"{DateTime.Now.Year} entire year";
                case ReportPeriod.LastYear:
                    return $"{DateTime.Now.Year - 1} entire year";
                case ReportPeriod.CurrentQuarter:
                    return "Current Quarter";
                case ReportPeriod.PreviousQuarter:
                    return "Previous Quarter";
                default:
                    return "Previous month";
            }
        }
    }
}