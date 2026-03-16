#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class AnnualFeeReportItem
    {
        public AnnualFeeReportItem()
        {
        }

        public AnnualFeeReportItem(AnnualFeeReportDataItem dataItem)
        {
            InstitutionId = dataItem.InstitutionId;
            AccountNumber = dataItem.AccountNumber;
            InstitutionName = dataItem.InstitutionName;
            ContactName = dataItem.ContactName;
            ContactEmail = dataItem.ContactEmail;
            ActiveDate = dataItem.ActiveDate;
            RenewalDate = dataItem.RenewalDate;
            UserId = dataItem.UserId;
            Consortia = dataItem.Consortia;
        }

        public int InstitutionId { get; set; }

        [Display(Name = "Account Number: ")] public string AccountNumber { get; set; }

        [Display(Name = "Consortia: ")] public string Consortia { get; set; }

        [Display(Name = "Institution Name: ")] public string InstitutionName { get; set; }

        [Display(Name = "Contact Name: ")] public string ContactName { get; set; }

        [Display(Name = "Contact Email: ")] public string ContactEmail { get; set; }

        [Display(Name = "Active Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ActiveDate { get; set; }

        [Display(Name = "Current Renewal Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime RenewalDate { get; set; }

        public int UserId { get; set; }
    }
}