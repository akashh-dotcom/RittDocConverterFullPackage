#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ReportResource
    {
        public ReportResource(ResourceReportItem resourceReportItem, string baseImageUrl)
        {
            ResourceId = resourceReportItem.ResourceId;
            ResourceTitle = resourceReportItem.ResourceTitle;
            ResourceIsbn = resourceReportItem.ResourceIsbn;

            ContentRetrievalCount = resourceReportItem.ContentRetrievalCount;
            TocRetrievalCount = resourceReportItem.TocRetrievalCount;
            PrintCount = resourceReportItem.ContentPrintCount;
            EmailCount = resourceReportItem.ContentEmailCount;
            PrintEmailCounts = $"{resourceReportItem.ContentPrintCount}/{resourceReportItem.ContentEmailCount}";

            ConcurrencyTurnawayCount = resourceReportItem.ConcurrencyTurnawayCount;
            AccessTurnawayCount = resourceReportItem.AccessTurnawayCount;

            TotalLicenseCount = resourceReportItem.TotalLicenseCount;
            FirstPurchasedDate = resourceReportItem.FirstPurchasedDate;

            ResourceAveragePrice = resourceReportItem.GetPurchasePrice();

            ResourceTotalPrice = resourceReportItem.ResourceTotalPrice;
            AverageAccessCost = resourceReportItem.AverageAccessCost;

            StatusId = resourceReportItem.ResourceStatusId;

            if (!string.IsNullOrWhiteSpace(baseImageUrl))
            {
                ResourceImageUrl = string.Format("{1}/{0}", resourceReportItem.ResourceImageName, baseImageUrl);
            }

            PdaCreatedDate = resourceReportItem.PdaCreatedDate;
            PdaAddedToCartDate = resourceReportItem.PdaAddedToCartDate;
            OriginalSource = resourceReportItem.OriginalSource;

            TotalPdaAccess = resourceReportItem.TotalPdaAccess;
            SessionCount = resourceReportItem.SessionCount;

            IsFreeResource = resourceReportItem.IsFreeResource;
        }

        public ReportResource(ResourceReportItem resourceReportItem, string baseImageUrl,
            IEnumerable<IResource> resources)
        {
            ResourceId = resourceReportItem.ResourceId;
            ResourceTitle = resourceReportItem.ResourceTitle;
            ResourceIsbn = resourceReportItem.ResourceIsbn;

            ContentRetrievalCount = resourceReportItem.ContentRetrievalCount;
            TocRetrievalCount = resourceReportItem.TocRetrievalCount;
            PrintCount = resourceReportItem.ContentPrintCount;
            EmailCount = resourceReportItem.ContentEmailCount;
            PrintEmailCounts = $"{resourceReportItem.ContentPrintCount}/{resourceReportItem.ContentEmailCount}";

            ConcurrencyTurnawayCount = resourceReportItem.ConcurrencyTurnawayCount;
            AccessTurnawayCount = resourceReportItem.AccessTurnawayCount;

            TotalLicenseCount = resourceReportItem.TotalLicenseCount;
            FirstPurchasedDate = resourceReportItem.FirstPurchasedDate;

            ResourceAveragePrice = resourceReportItem.GetPurchasePrice();

            ResourceTotalPrice = resourceReportItem.ResourceTotalPrice;
            AverageAccessCost = resourceReportItem.AverageAccessCost;

            StatusId = resourceReportItem.ResourceStatusId;

            if (!string.IsNullOrWhiteSpace(baseImageUrl))
            {
                ResourceImageUrl = string.Format("{1}/{0}", resourceReportItem.ResourceImageName, baseImageUrl);
            }

            PdaCreatedDate = resourceReportItem.PdaCreatedDate;
            PdaAddedToCartDate = resourceReportItem.PdaAddedToCartDate;
            OriginalSource = resourceReportItem.OriginalSource;

            TotalPdaAccess = resourceReportItem.TotalPdaAccess;
            SessionCount = resourceReportItem.SessionCount;

            IsFreeResource = resourceReportItem.IsFreeResource;

            Resource = new Resource.Resource(resources.FirstOrDefault(x => x.Id == resourceReportItem.ResourceId));
        }

        public int ResourceId { get; set; }
        public string ResourceTitle { get; set; }
        public string ResourceIsbn { get; set; }
        public string ResourceImageUrl { get; set; }

        [Display(Name = "Successful Content Retrieval: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int ContentRetrievalCount { get; set; }

        [Display(Name = "TOC Retrievals: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int TocRetrievalCount { get; set; }

        [Display(Name = "Print Requests: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int PrintCount { get; set; }

        [Display(Name = "Email Requests: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int EmailCount { get; set; }

        [Display(Name = "Print / Email Requests: ")]
        public string PrintEmailCounts { get; set; }

        [Display(Name = "Content Turnaways: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int ConcurrencyTurnawayCount { get; set; }

        [Display(Name = "Access Turnaways: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int AccessTurnawayCount { get; set; }

        [Display(Name = "Total Licenses Purchased: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int TotalLicenseCount { get; set; }

        [Display(Name = "First License Purchased: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime FirstPurchasedDate { get; set; }

        [Display(Name = "Average Cost Per License: ")]
        [DisplayFormat(DataFormatString = "${0:#,##0.00}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal ResourceAveragePrice { get; set; }

        [Display(Name = "Total Resource Cost: ")]
        [DisplayFormat(DataFormatString = "${0:#,##0.00}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal ResourceTotalPrice { get; set; }

        [Display(Name = "Cost Per Use: ")]
        [DisplayFormat(DataFormatString = "${0:#,##0.00}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal AverageAccessCost { get; set; }

        public int StatusId { get; set; }

        [Display(Name = "Created Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime? PdaCreatedDate { get; set; }

        [Display(Name = "Added To Cart: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime? PdaAddedToCartDate { get; set; }

        [Display(Name = "Order Source: ")] public LicenseOriginalSource OriginalSource { get; set; }

        [Display(Name = "PDA Views: ")] public int TotalPdaAccess { get; set; }

        [Display(Name = "Sessions: ")] public int SessionCount { get; set; }

        public bool IsFreeResource { get; set; }


        public string OriginalSourceString
        {
            get
            {
                switch (OriginalSource)
                {
                    case LicenseOriginalSource.FirmOrder:
                        return "Firm Order";
                    case LicenseOriginalSource.Pda:
                        return "PDA title selection";
                }

                return null;
            }
        }

        public string Status
        {
            get
            {
                switch ((ResourceStatus)StatusId)
                {
                    case ResourceStatus.Active:
                        return "Active";
                    case ResourceStatus.Archived:
                        return "Archived";
                    case ResourceStatus.Forthcoming:
                        return "Pre-Order";
                    case ResourceStatus.Inactive:
                        return "Not Available";
                    default:
                        return "";
                }
            }
        }

        public Resource.Resource Resource { get; set; }
    }
}