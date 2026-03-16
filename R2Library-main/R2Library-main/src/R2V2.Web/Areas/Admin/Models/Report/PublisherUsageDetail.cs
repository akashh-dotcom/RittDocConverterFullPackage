#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class PublisherUsageDetail : ReportModel
    {
        private SelectList _periodList;


        public PublisherUsageDetail()
        {
        }

        [Display(Name = "New Titles: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int NewTitlesCount { get; set; }

        [Display(Name = "Titles Sold: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int TitlesSoldCount { get; set; }

        [Display(Name = "Total Sales: ")]
        [DisplayFormat(DataFormatString = "{0:C}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal TotalSales { get; set; }

        public int PublisherId { get; set; }

        public List<PublisherUsageDetailItem> Items { get; set; }

        public ReportPeriod Period { get; set; }

        public IEnumerable<PageLink> PageLinks { get; set; }

        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }

        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }


        public void SetReportData(PublisherReportCounts publisherReportCounts, ReportQuery publisherReportQuery)
        {
            NewTitlesCount = publisherReportCounts.NewTitlesCount;
            TitlesSoldCount = publisherReportCounts.TitlesSoldCount;
            TotalSales = publisherReportCounts.TitleSales;
            ReportQuery = publisherReportQuery;
            Period = publisherReportQuery.Period;
            PublisherId = publisherReportQuery.PublisherId;

            Items = publisherReportCounts.Items.Select(x => x.ToPublisherUsageDetailItem()).ToList();
        }
    }

    public static class PublisherUsageDetailExtensions
    {
        public static PublisherUsageDetailItem ToPublisherUsageDetailItem(this PublisherReportCount item)
        {
            return new PublisherUsageDetailItem
            {
                IsNewTitle = item.IsNewTitle,
                Licenses = item.Licenses,
                Resource = item.Resource,
                ResourceId = item.ResourceId,
                TotalSales = item.TotalSales
            };
        }
    }

    public class PublisherUsageDetailItem
    {
        public int ResourceId { get; set; }
        [Display(Name = "Licenses Sold: ")] public int Licenses { get; set; }

        [Display(Name = "Total Sales: ")]
        [DisplayFormat(DataFormatString = "{0:C}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal TotalSales { get; set; }

        [Display(Name = "Is New Title?: ")] public bool IsNewTitle { get; set; }

        public IResource Resource { get; set; }

        public string Status
        {
            get
            {
                switch ((ResourceStatus)Resource.StatusId)
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
    }
}