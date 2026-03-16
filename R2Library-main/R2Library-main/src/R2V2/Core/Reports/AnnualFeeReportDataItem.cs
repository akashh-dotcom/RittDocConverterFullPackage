#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public class AnnualFeeReportDataItem
    {
        public int InstitutionId { get; set; }
        public string AccountNumber { get; set; }
        public string InstitutionName { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public DateTime ActiveDate { get; set; }

        public string Consortia { get; set; }

        private DateTime _renewalDate { get; set; }

        public DateTime RenewalDate
        {
            get => _renewalDate.Year == ActiveDate.Year ? _renewalDate.AddYears(1) : _renewalDate;
            set => _renewalDate = value;
        }

        public int UserId { get; set; }
    }
}