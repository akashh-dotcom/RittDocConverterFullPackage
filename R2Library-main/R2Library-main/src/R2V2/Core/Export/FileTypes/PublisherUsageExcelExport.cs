#region

using R2V2.Core.Reports;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class PublisherUsageExcelExport : ExcelBase
    {
        public PublisherUsageExcelExport(PublisherReportCounts publisherReportCounts)
        {
            SpecifyColumn("Status", "String");
            SpecifyColumn("Isbn10", "String");
            SpecifyColumn("Isbn13", "String");
            SpecifyColumn("eIsbn", "String");
            SpecifyColumn("Title", "String");
            SpecifyColumn("Publisher", "String");
            SpecifyColumn("Vendor #", "String");
            SpecifyColumn("Authors", "String");
            SpecifyColumn("Author Affiliation", "String");
            SpecifyColumn("R2 Release Date", "String");
            SpecifyColumn("Copyright Year", "String");
            SpecifyColumn("Collections", "String");
            SpecifyColumn("Practice Area", "String");
            SpecifyColumn("Discipline", "String");

            SpecifyColumn("New Title", "Boolean");
            SpecifyColumn("Titles Sold", "Int32");
            SpecifyColumn("Total Sales", "Decimal");

            foreach (var item in publisherReportCounts.Items)
            {
                string resourceStatus;
                switch ((ResourceStatus)item.Resource.StatusId)
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

                PopulateFirstColumn(resourceStatus);
                PopulateNextColumn(item.Resource.Isbn10);
                PopulateNextColumn(item.Resource.Isbn13);
                PopulateNextColumn(item.Resource.EIsbn);
                PopulateNextColumn(item.Resource.Title);
                PopulateNextColumn(string.IsNullOrWhiteSpace(item.Resource.Publisher.DisplayName)
                    ? item.Resource.Publisher.Name
                    : item.Resource.Publisher.DisplayName);
                PopulateNextColumn(item.Resource.Publisher.VendorNumber);
                PopulateNextColumn(item.Resource.Authors);
                PopulateNextColumn(item.Resource.Affiliation);

                var releaseDateString = item.Resource.ReleaseDate.HasValue
                    ? $"{item.Resource.ReleaseDate.Value:MM/dd/yyyy}"
                    : null;
                var copyRightYearString = item.Resource.PublicationDate.HasValue
                    ? $"{item.Resource.PublicationDate.Value.Year}"
                    : null;

                PopulateNextColumn(releaseDateString);
                PopulateNextColumn(copyRightYearString);
                PopulateNextColumn(item.Resource.CollectionsToString());

                PopulateNextColumn(item.Resource.PracticeAreasToString());
                PopulateNextColumn(item.Resource.SpecialtiesToString());

                PopulateNextColumn(item.IsNewTitle);
                PopulateNextColumn(item.Licenses);
                PopulateLastColumn(item.TotalSales);
            }
        }
    }
}