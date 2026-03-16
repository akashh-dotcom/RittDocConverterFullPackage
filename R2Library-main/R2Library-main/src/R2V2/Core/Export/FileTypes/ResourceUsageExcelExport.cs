#region

using System;
using System.Collections.Generic;
using R2V2.Core.Reports;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class ResourceUsageExcelExport : ExcelBase
    {
        public ResourceUsageExcelExport(IEnumerable<ResourceReportItem> resourceReportItems, bool isPublisher,
            string bookUrlPrefix, string bookUrlSuffix, string bookUrl, bool isRa)
        {
            SpecifyColumn("Status", "String");
            SpecifyColumn("Isbn10", "String");
            SpecifyColumn("Isbn13", "String");
            SpecifyColumn("eIsbn", "String");
            SpecifyColumn("Title", "String");
            SpecifyColumn("Publisher", "String");
            if (isRa)
            {
                SpecifyColumn("Vendor #", "String");
            }

            SpecifyColumn("Authors", "String");
            SpecifyColumn("Author Affiliation", "String");
            SpecifyColumn("R2 Release Date", "String");
            SpecifyColumn("Copyright Year", "String");
            SpecifyColumn("DCT status", "String");
            SpecifyColumn("Practice Area", "String");
            SpecifyColumn("Discipline", "String");

            SpecifyColumn("Successful Content Retrieval", "Int32");
            SpecifyColumn("TOC Retrievals", "Int32");
            SpecifyColumn("Sessions", "Int32");
            if (!isPublisher)
            {
                SpecifyColumn("Print Requests", "Int32");
                SpecifyColumn("Email Requests", "Int32");
            }

            SpecifyColumn("Content Turnaways", "Int32");
            SpecifyColumn("Access Turnaways", "Int32");

            SpecifyColumn("Licenses Purchased", "Int32");

            if (!isPublisher)
            {
                SpecifyColumn("Average Cost Per License", "Decimal");
                SpecifyColumn("Total Resource Cost", "Decimal");
                SpecifyColumn("Cost Per Use", "Decimal");
                SpecifyColumn("First License Purchased", "String");

                SpecifyColumn("Order Source", "String");
                SpecifyColumn("PDA View Count", "String");
                SpecifyColumn("PDA Created Date", "String");
                SpecifyColumn("Added to Cart", "String");
            }

            SpecifyColumn("New Edition ISBN", "String");
            SpecifyColumn("New Edition URL", "String");

            foreach (var item in resourceReportItems)
            {
                string resourceStatus;
                switch ((ResourceStatus)item.ResourceStatusId)
                {
                    case ResourceStatus.Active:
                        resourceStatus = "Active";
                        break;
                    case ResourceStatus.Archived:
                        resourceStatus = "Archived";
                        break;
                    case ResourceStatus.Forthcoming:
                        resourceStatus = "Pre-Order";
                        break;
                    case ResourceStatus.Inactive:
                        resourceStatus = "Not Available";
                        break;
                    default:
                        resourceStatus = "";
                        break;
                }

                var hasNewResource = !string.IsNullOrWhiteSpace(item.NewEditionResourceIsbn);

                PopulateFirstColumn(resourceStatus);
                PopulateNextColumn(item.Isbn10);
                PopulateNextColumn(item.Isbn13);
                PopulateNextColumn(item.EIsbn);
                PopulateNextColumn(item.ResourceTitle);
                PopulateNextColumn(item.Publisher);
                if (isRa)
                {
                    PopulateNextColumn(item.VendorNumber);
                }

                PopulateNextColumn(item.Authors);
                PopulateNextColumn(item.Affiliation);

                var releaseDateString = item.ReleaseDate.HasValue ? $"{item.ReleaseDate.Value:MM/dd/yyyy}" : null;
                var copyRightYearString = item.CopyRightYear.HasValue ? $"{item.CopyRightYear.Value}" : null;

                PopulateNextColumn(releaseDateString);
                PopulateNextColumn(copyRightYearString);
                PopulateNextColumn(item.DctStatus);

                PopulateNextColumn(item.PracticeAreaString);
                PopulateNextColumn(item.SpecialtyString);

                PopulateNextColumn(item.ContentRetrievalCount);
                PopulateNextColumn(item.TocRetrievalCount);
                PopulateNextColumn(item.SessionCount);
                if (isPublisher)
                {
                    PopulateNextColumn(item.ConcurrencyTurnawayCount);
                    PopulateNextColumn(item.AccessTurnawayCount);
                    //Everyone gets 500 licenses, so divide by 500 will give you subscriptions
                    PopulateNextColumn(GetLicenseCount(item));
                    PopulateNextColumn(hasNewResource ? item.NewEditionResourceIsbn : null);
                    PopulateLastColumn(hasNewResource
                        ? BuildBookUrl(item.NewEditionResourceIsbn, bookUrlPrefix, bookUrlSuffix, bookUrl)
                        : null);
                }
                else
                {
                    PopulateNextColumn(item.ContentPrintCount);
                    PopulateNextColumn(item.ContentEmailCount);
                    PopulateNextColumn(item.ConcurrencyTurnawayCount);
                    PopulateNextColumn(item.AccessTurnawayCount);
                    PopulateNextColumn(GetLicenseCount(item));
                    PopulateNextColumn(item.GetPurchasePrice());
                    PopulateNextColumn(item.ResourceTotalPrice);
                    PopulateNextColumn(decimal.Round(item.AverageAccessCost, 2, MidpointRounding.AwayFromZero));
                    PopulateNextColumn(item.FirstPurchasedDate == DateTime.MinValue
                        ? "Not Purchased"
                        : item.FirstPurchasedDate.ToShortDateString());
                    PopulateNextColumn(item.OriginalSourceString);
                    PopulateNextColumn(item.TotalPdaAccess);
                    PopulateNextColumn(item.PdaCreatedDate == null ? "" : $"{item.PdaCreatedDate.Value:M/d/yyyy}");
                    PopulateNextColumn(item.PdaAddedToCartDate == null
                        ? ""
                        : $"{item.PdaAddedToCartDate.Value:M/d/yyyy}");

                    PopulateNextColumn(hasNewResource ? item.NewEditionResourceIsbn : null);
                    PopulateLastColumn(hasNewResource
                        ? BuildBookUrl(item.NewEditionResourceIsbn, bookUrlPrefix, bookUrlSuffix, bookUrl)
                        : null);
                }
            }
        }
    }
}