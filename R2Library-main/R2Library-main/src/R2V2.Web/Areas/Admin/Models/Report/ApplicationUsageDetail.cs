#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Admin;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class ApplicationUsageDetail : ReportModel
    {
        /// <summary>
        /// </summary>
        public ApplicationUsageDetail()
        {
        }


        /// <summary>
        ///     Run Counts before this is created. The institution Is necessary
        /// </summary>
        public ApplicationUsageDetail(IAdminInstitution institution, ReportQuery reportQuery)
            : base(institution, reportQuery)
        {
            Type = ReportType.ApplicationUsageReport;
            IsSaveEnabled = reportQuery.InstitutionId > 0 && reportQuery.ReportId == 0;
            ReportQuery.FilterByIpRanges = reportQuery.FilterByIpRanges;
            DebugInfo = reportQuery.ToDebugString();
        }

        // Application useage counts
        [Display(Name = "User Sessions: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int UserSessionCount { get; set; }

        [Display(Name = "Page Views: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int PageViewCount { get; set; }

        [Display(Name = "Successful Content Retrievals: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int ContentRetrievalCount { get; set; }

        [Display(Name = "TOC Retrievals: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int TocRetrievalCount { get; set; }

        [Display(Name = "Concurrency Turnaways: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int ConcurrencyTurnawayCount { get; set; }

        [Display(Name = "Access Turnaways: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int AccessTurnawayCount { get; set; }

        // search counts
        [Display(Name = "Active Content Searches: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int SearchActiveCount { get; set; }

        [Display(Name = "Archived Content Searches: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int SearchArchivedCount { get; set; }

        [Display(Name = "Image Only Searches: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int SearchImageCount { get; set; }

        [Display(Name = "Drug Monograph Searches: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int SearchDrugCount { get; set; }

        // 3rd party search counts
        [Display(Name = "PUBMED Searches: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int SearchPubMedCount { get; set; }

        [Display(Name = "MESH Searches: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int SearchMeshCount { get; set; }


        [Display(Name = "Total: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int PdaTotalCount { get; set; }

        [Display(Name = "Active: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int PdaActiveCount { get; set; }

        [Display(Name = "Added To Cart : ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int PdaCartCount { get; set; }

        [Display(Name = "Purchased: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int PdaPurchaseCount { get; set; }

        public bool IsFirstRun { get; set; }


        /// <param name="applicationReportCounts"> </param>
        /// <param name="ipAddressRanges"> </param>
        public void SetReportData(ApplicationReportCounts applicationReportCounts,
            IEnumerable<Core.Authentication.IpAddressRange> ipAddressRanges)
        {
            AddIpAddressRanges(ipAddressRanges);

            PageViewCount = applicationReportCounts.PageViewCount;
            UserSessionCount = applicationReportCounts.UserSessionCount;
            ContentRetrievalCount = applicationReportCounts.RestrictedContentRetrievalCount;
            TocRetrievalCount = applicationReportCounts.TocOnlyContentRetrievalCount;
            ConcurrencyTurnawayCount = applicationReportCounts.ConcurrencyTurnawayCount;
            AccessTurnawayCount = applicationReportCounts.AccessTurnawayCount;

            SearchActiveCount = applicationReportCounts.SearchActiveCount;
            SearchArchivedCount = applicationReportCounts.SearchArchiveCount;
            SearchImageCount = applicationReportCounts.SearchImageCount;
            SearchDrugCount = applicationReportCounts.SearchDrugCount;

            SearchPubMedCount = applicationReportCounts.SearchPubMedCount;
            SearchMeshCount = applicationReportCounts.SearchMeshCount;

            PdaTotalCount = applicationReportCounts.PdaTotalCount;
            PdaActiveCount = applicationReportCounts.PdaActiveCount;
            PdaCartCount = applicationReportCounts.PdaCartCount;
            PdaPurchaseCount = applicationReportCounts.PdaPurchasedCount;
        }
    }
}