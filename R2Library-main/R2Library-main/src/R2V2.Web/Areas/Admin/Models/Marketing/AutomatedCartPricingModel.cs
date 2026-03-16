#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.AutomatedCart;
using R2V2.Core.Reports;
using R2V2.Web.Areas.Admin.Models.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class AutomatedCartPricingModel : AutomatedCartBaseModel
    {
        public AutomatedCartPricingModel(WebAutomatedCartReportQuery reportQuery,
            IEnumerable<AutomatedCartInstitutionPriced> institutions, string[] territoryName, string[] institutionType)
        {
            ReportQuery = reportQuery;
            Institutions = institutions;
            Territories = territoryName;
            InstitutionTypes = institutionType;
        }

        public AutomatedCartPricingModel(WebAutomatedCartReportQuery reportQuery,
            IEnumerable<AutomatedCartInstitutionSummary> institutionSummaries, string[] territoryNames,
            string[] institutionTypes)
        {
            ReportQuery = reportQuery;
            InstitutionSummaries = institutionSummaries;
            Territories = territoryNames;
            InstitutionTypes = institutionTypes;
        }

        public AutomatedCartPricingModel()
        {
        }

        public IEnumerable<AutomatedCartInstitutionPriced> Institutions { get; }

        public IEnumerable<AutomatedCartInstitutionSummary> InstitutionSummaries { get; }
        public bool DisplayEmailCounts { get; set; }

        public string CartName { get; set; }
        [AllowHtml] public string EmailText { get; set; }

        public string EmailTitle { get; set; }

        public string EmailSubject { get; set; }


        [DisplayFormat(DataFormatString = "{0}%", ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true)]
        public decimal? DiscountOverride { get; set; }

        public bool AllowDiscountOverride { get; set; }
        
        [Display(Name = "Territories:")] public string[] Territories { get; }
        public string TerritoriesDisplay => Territories != null ? string.Join(", ", Territories) : null;

        [Display(Name = "Institution Types:")] public string[] InstitutionTypes { get; }

        public string InstitutionTypesDisplay => InstitutionTypes != null ? string.Join(", ", InstitutionTypes) : null;

        public string AccountNumberBatch => ReportQuery.AccountNumberBatch;

        public string SelectedInstitutionIds { get; set; }

        public int AutomatedCartId { get; set; }

        public bool ShowExportToolLink => (Institutions != null && Institutions.Any()) ||
                                          (InstitutionSummaries != null && InstitutionSummaries.Any());

        public string ExcelExportUrl => ShowExportToolLink ? "javascript:void(0)" : null;

        public string GetPeriodDisplay()
        {
            switch (ReportQuery.Period)
            {
                case ReportPeriod.LastTwelveMonths:
                    return "Last 12 Months";
                case ReportPeriod.LastSixMonths:
                    return "Last 6 Months";
                case ReportPeriod.Last30Days:
                    return "Last 30 Days";
                case ReportPeriod.PreviousMonth:
                    return "Previous Month";
                case ReportPeriod.CurrentMonth:
                    return "Current Month";
                case ReportPeriod.UserSpecified:
                    return "Specified Date Range";
                case ReportPeriod.CurrentYear:
                    return $@"{DateTime.Now.Year} Entire Year";
                case ReportPeriod.LastYear:
                    return $@"{DateTime.Now.Year - 1} Entire Year";
                case ReportPeriod.CurrentQuarter:
                    return "Current Quarter";
                case ReportPeriod.PreviousQuarter:
                    return "Previous Quarter";
            }

            return null;
        }

        public string GetToolTip(Address address)
        {
            var sb = new StringBuilder();
            sb.Append($"{address.Address1}&#013;");
            if (!string.IsNullOrWhiteSpace(address.Address2))
            {
                sb.Append($"{address.Address2}&#013;");
            }

            sb.Append($"{address.City}, {address.State} {address.Zip}");
            return sb.ToString();
        }

        public string GetToolTip(Core.Authentication.Address address)
        {
            var sb = new StringBuilder();
            sb.Append($"{address.Address1}&#013;");
            if (!string.IsNullOrWhiteSpace(address.Address2))
            {
                sb.Append($"{address.Address2}&#013;");
            }

            sb.Append($"{address.City}, {address.State} {address.Zip}");
            return sb.ToString();
        }

        public string NumberOfInstitutons()
        {
            return $"{InstitutionSummaries?.Count() ?? 0:#,##0}";
        }

        public string NumberOfEmails()
        {
            return $"{InstitutionSummaries?.Sum(x => x.EmailCount) ?? 0:#,##0}";
        }
    }
}