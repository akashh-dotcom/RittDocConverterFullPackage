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
    public class SalesReportDetail : ReportModel
    {
        private SelectList _sortByList;

        [Display(Name = @"Resource Status:")]
        public SelectList ResourceStatusList => new SelectList(new List<SelectListItem>
        {
            new SelectListItem { Text = @"All", Value = $"{ResourceStatus.All}" },
            new SelectListItem { Text = @"Active", Value = $"{ResourceStatus.Active}" },
            new SelectListItem { Text = @"Pre-Order", Value = $"{ResourceStatus.Forthcoming}" },
            new SelectListItem { Text = @"Archived", Value = $"{ResourceStatus.Archived}" },
            new SelectListItem { Text = @"Inactive", Value = $"{ResourceStatus.Inactive}" }
        }, "Value", "Text", (int)ReportQuery.ResourceStatus);

        public IEnumerable<PageLink> PageLinks { get; set; }

        public PageLink NextLink { get; set; }
        public PageLink PreviousLink { get; set; }

        public PageLink FirstLink { get; set; }
        public PageLink LastLink { get; set; }
        public List<SalesReportDetailItem> Items { get; set; }


        [Display(Name = "Titles Sold: ")]
        [DisplayFormat(DataFormatString = "{0:#,##0}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public int TitlesSoldCount { get; set; }

        [Display(Name = "Total Sales: ")]
        [DisplayFormat(DataFormatString = "{0:C}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal TotalSales { get; set; }

        [Display(Name = @"Sort By:")]
        public SelectList SortByList => _sortByList ??
                                        (_sortByList = new SelectList(new List<SelectListItem>
                                        {
                                            new SelectListItem { Text = @"Title", Value = $"{ReportSortBy.Title}" },
                                            new SelectListItem
                                                { Text = @"R2 Release Date", Value = $"{ReportSortBy.ReleaseDate}" },
                                            new SelectListItem
                                                { Text = @"Copyright Year", Value = $"{ReportSortBy.Copyright}" },
                                            new SelectListItem
                                                { Text = @"Licenses Sold", Value = $"{ReportSortBy.LicensesSold}" }
                                            //
                                        }, "Value", "Text", (int)ReportQuery.SortBy));

        public void SetReportData(SalesReportItems salesReportItems, ReportQuery reportQuery)
        {
            TitlesSoldCount = salesReportItems.TitlesSoldCount;
            TotalSales = salesReportItems.TitleSales;
            ReportQuery = reportQuery;
            //Period = reportQuery.Period;
            //PublisherId = publisherReportQuery.PublisherId;

            Items = salesReportItems.Items.Select(x => x.ToSalesReportDetailItem()).ToList();
        }
    }

    public static class SalesReportDetailExtensions
    {
        public static SalesReportDetailItem ToSalesReportDetailItem(this SalesReportItem item)
        {
            return new SalesReportDetailItem
            {
                Licenses = item.Licenses,
                Resource = item.Resource,
                ResourceId = item.ResourceId,
                TotalSales = item.TotalSales
            };
        }
    }

    public class SalesReportDetailItem
    {
        public int ResourceId { get; set; }
        [Display(Name = "Licenses Sold: ")] public int Licenses { get; set; }

        [Display(Name = "Total Sales: ")]
        [DisplayFormat(DataFormatString = "{0:C}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public decimal TotalSales { get; set; }


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