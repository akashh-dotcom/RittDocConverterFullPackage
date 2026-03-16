#region

using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class PdaHistoryExcelExport : ExcelBase
    {
        public PdaHistoryExcelExport(PdaHistoryReport pdaHistoryReport, string bookUrlPrefix, string bookUrlSuffix,
            string bookUrl)
        {
            SpecifyBaseBookColumns(false);

            SpecifyColumn("PDA Date Created", "String");
            SpecifyColumn("PDA Date Added to Cart", "String");
            SpecifyColumn("PDA Date Removed from Cart", "String");
            SpecifyColumn("PDA Removed from Cart By", "String");
            SpecifyColumn("PDA Date Saved to Cart", "String");
            SpecifyColumn("# of PDA Accesses", "Int32");
            SpecifyColumn("No Longer For Sale", "String");
            SpecifyColumn("First Purchase Date", "String");

            SpecifyColumn("Successful Content Retrieval", "Int32");
            SpecifyColumn("TOC Retrievals", "Int32");
            SpecifyColumn("Sessions", "Int32");
            SpecifyColumn("Print Requests", "Int32");
            SpecifyColumn("Email Requests", "Int32");
            SpecifyColumn("Access Turnaways", "Int32");
            SpecifyColumn("URL", "String");
            SpecifyColumn("New Edition ISBN", "String");
            SpecifyColumn("New Edition URL", "String");

            if (pdaHistoryReport == null || pdaHistoryReport.PdaHistoryCounts == null)
            {
                return;
            }

            foreach (var item in pdaHistoryReport.PdaHistoryCounts)
            {
                PopulateFirstColumn(item.CollectionManagementResource.LicenseType == LicenseType.Pda
                    ? "PDA"
                    : item.CollectionManagementResource.Resource.StatusToString());
                PopulateNextColumn(item.CollectionManagementResource.Resource.Isbn10);
                PopulateNextColumn(item.CollectionManagementResource.Resource.Isbn13);
                PopulateNextColumn(item.CollectionManagementResource.Resource.EIsbn);
                PopulateNextColumn(item.CollectionManagementResource.Resource.Title);
                PopulateNextColumn(item.CollectionManagementResource.Resource.Edition);
                PopulateNextColumn(item.CollectionManagementResource.Resource.Authors);
                PopulateNextColumn(item.CollectionManagementResource.Resource.Affiliation);
                PopulateNextColumn(item.CollectionManagementResource.Resource.Publisher.Name);
                PopulateNextColumn($"{item.CollectionManagementResource.Resource.PublicationDate:yyyy}");
                PopulateNextColumn($"{item.CollectionManagementResource.Resource.ReleaseDate:M/d/yyyy}");
                PopulateNextColumn(item.CollectionManagementResource.Resource.PracticeAreasToString());
                PopulateNextColumn(item.CollectionManagementResource.Resource.SpecialtiesToString());
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.BradonHill,
                    item.CollectionManagementResource.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.Dct,
                    item.CollectionManagementResource.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.DctEssential,
                    item.CollectionManagementResource.Resource));
                PopulateNextColumn(item.CollectionManagementResource.Resource.ForthcomingDate);
                PopulateNextColumn(item.CollectionManagementResource.OriginalSourceString);

                var pdaAddedDate = item.CollectionManagementResource.PdaAddedDate != null
                    ? $"{item.CollectionManagementResource.PdaAddedDate.Value:M/d/yyyy}"
                    : null;
                var pdaAddedToCartDate = item.CollectionManagementResource.PdaAddedToCartDate != null
                    ? $"{item.CollectionManagementResource.PdaAddedToCartDate.Value:M/d/yyyy}"
                    : null;
                var pdaCartDeletedDate = item.CollectionManagementResource.PdaCartDeletedDate != null
                    ? $"{item.CollectionManagementResource.PdaCartDeletedDate.Value:M/d/yyyy}"
                    : null;
                var resourceNotSaleableDate = item.CollectionManagementResource.ResourceNotSaleableDate != null
                    ? $"{item.CollectionManagementResource.ResourceNotSaleableDate:M/d/yyyy}"
                    : string.Empty;
                var firstPurchaseDate = item.CollectionManagementResource.FirstPurchaseDate != null
                    ? $"{item.CollectionManagementResource.FirstPurchaseDate:M/d/yyyy}"
                    : string.Empty;

                PopulateNextColumn(pdaAddedDate);
                PopulateNextColumn(pdaAddedToCartDate);
                PopulateNextColumn(pdaCartDeletedDate);
                PopulateNextColumn(item.CollectionManagementResource.PdaCartDeletedByName);
                PopulateNextColumn(item.CollectionManagementResource.DateOrNameCartWasSaved);
                PopulateNextColumn(item.CollectionManagementResource.PdaViewCount);
                PopulateNextColumn(resourceNotSaleableDate);
                PopulateNextColumn(firstPurchaseDate);
                PopulateNextColumn(item.ContentRetrievalCount);
                PopulateNextColumn(item.TocRetrievalCount);
                PopulateNextColumn(item.SessionCount);
                PopulateNextColumn(item.PrintCount);
                PopulateNextColumn(item.EmailCount);
                PopulateNextColumn(item.AccessTurnawayCount);

                BuildBookUrlAndNewBookUrl(item.CollectionManagementResource.Resource, bookUrlPrefix, bookUrlSuffix,
                    bookUrl);
            }
        }
    }
}