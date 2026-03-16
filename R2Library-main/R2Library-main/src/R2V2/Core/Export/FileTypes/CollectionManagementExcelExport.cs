#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class CollectionManagementExcelExport : ExcelBase
    {
        public CollectionManagementExcelExport(IEnumerable<CollectionManagementResource> collectionManagementResources,
            string bookUrlPrefix, string bookUrlSuffix, string bookUrl)
        {
            var resources = collectionManagementResources == null ? null : collectionManagementResources.ToList();
            var containsTurnaways = resources != null && resources.Any(x => x.ConcurrentTurnawayCount > 0);
            SpecifyBaseBookColumns(containsTurnaways);
            SpecifyBookCostColumns(true);

            if (resources == null)
            {
                return;
            }

            foreach (var resource in resources)
            {
                var status = resource.Resource.StatusToString();

                var brandonHill =
                    resource.Resource.CollectionIdsToArray().Contains((int)CollectionIdentifier.BradonHill)
                        ? "Yes"
                        : "No";
                var dct = resource.Resource.CollectionIdsToArray().Contains((int)CollectionIdentifier.Dct)
                    ? "Yes"
                    : "No";
                var dctEssential =
                    resource.Resource.CollectionIdsToArray().Contains((int)CollectionIdentifier.DctEssential)
                        ? "Yes"
                        : "No";

                PopulateFirstColumn(status);
                PopulateNextColumn(resource.Resource.Isbn10);
                PopulateNextColumn(resource.Resource.Isbn13);
                PopulateNextColumn(resource.Resource.EIsbn);
                PopulateNextColumn(resource.Resource.Title);
                PopulateNextColumn(resource.Resource.Edition);
                PopulateNextColumn(resource.Resource.Authors);
                PopulateNextColumn(resource.Resource.Affiliation);
                PopulateNextColumn(resource.Resource.Publisher.Name);
                PopulateNextColumn($"{resource.Resource.PublicationDate:yyyy}");
                PopulateNextColumn($"{resource.Resource.ReleaseDate:M/d/yyyy}");
                PopulateNextColumn(resource.Resource.PracticeAreasToString());
                PopulateNextColumn(resource.Resource.SpecialtiesToString());
                PopulateNextColumn(brandonHill);
                PopulateNextColumn(dct);
                PopulateNextColumn(dctEssential);
                PopulateNextColumn(resource.Resource.ForthcomingDate);
                PopulateNextColumn(resource.OriginalSourceString);
                if (containsTurnaways)
                {
                    PopulateNextColumn(resource.ConcurrentTurnawayCount);
                }

                PopulateNextColumn(GetLicenseCount(resource));
                PopulateNextColumn(resource.Resource.ListPrice);
                PopulateNextColumn(resource.DiscountPrice);
                PopulateNextColumn(resource.LicenseCount * resource.DiscountPrice);
                PopulateNextColumn($"{resource.FirstPurchaseDate:M/d/yyyy}");
                BuildBookUrlAndNewBookUrl(resource.Resource, bookUrlPrefix, bookUrlSuffix, bookUrl);
            }
        }

        public CollectionManagementExcelExport(IEnumerable<CollectionManagementResource> collectionManagementResources,
            string bookUrlPrefix, string bookUrlSuffix, string bookUrl, bool pdaTitlesOnly)
        {
            var resources = collectionManagementResources == null ? null : collectionManagementResources.ToList();
            var containsTurnaways = resources != null && resources.Any(x => x.ConcurrentTurnawayCount > 0);
            SpecifyBaseBookColumns(containsTurnaways);

            SpecifyColumn("PDA Date Created", "String");
            SpecifyColumn("PDA Date Added to Cart", "String");
            SpecifyColumn("PDA Date Removed from Cart", "String");
            //SpecifyColumn("PDA removed from Cart by", "String");
            SpecifyColumn("# of PDA Accesses", "Int32");
            SpecifyColumn("No Longer For Sale", "String");

            SpecifyBookCostColumns(!pdaTitlesOnly);

            if (resources == null)
            {
                return;
            }

            foreach (var resource in resources)
            {
                PopulateFirstColumn(
                    resource.LicenseType == LicenseType.Pda ? "PDA" : resource.Resource.StatusToString());
                PopulateNextColumn(resource.Resource.Isbn10);
                PopulateNextColumn(resource.Resource.Isbn13);
                PopulateNextColumn(resource.Resource.EIsbn);
                PopulateNextColumn(resource.Resource.Title);
                PopulateNextColumn(resource.Resource.Edition);
                PopulateNextColumn(resource.Resource.Authors);
                PopulateNextColumn(resource.Resource.Affiliation);
                PopulateNextColumn(resource.Resource.Publisher.Name);
                PopulateNextColumn($"{resource.Resource.PublicationDate:yyyy}");
                PopulateNextColumn($"{resource.Resource.ReleaseDate:M/d/yyyy}");
                PopulateNextColumn(resource.Resource.PracticeAreasToString());
                PopulateNextColumn(resource.Resource.SpecialtiesToString());
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.BradonHill, resource.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.Dct, resource.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.DctEssential, resource.Resource));
                PopulateNextColumn(resource.Resource.ForthcomingDate);
                PopulateNextColumn(resource.OriginalSourceString);
                if (containsTurnaways)
                {
                    PopulateNextColumn(resource.ConcurrentTurnawayCount);
                }

                PopulateNextColumn(resource.PdaAddedDate != null ? $"{resource.PdaAddedDate.Value:M/d/yyyy}" : null);
                PopulateNextColumn(resource.PdaAddedToCartDate != null
                    ? $"{resource.PdaAddedToCartDate.Value:M/d/yyyy}"
                    : null);
                PopulateNextColumn(resource.PdaCartDeletedDate != null
                    ? $"{resource.PdaCartDeletedDate.Value:M/d/yyyy}"
                    : null);
                //PopulateNextColumn(resource.PdaCartDeletedByName);
                PopulateNextColumn(resource.PdaViewCount);
                PopulateNextColumn(resource.ResourceNotSaleableDate != null
                    ? $"{resource.ResourceNotSaleableDate:M/d/yyyy}"
                    : string.Empty);

                if (pdaTitlesOnly)
                {
                    PopulateNextColumn(resource.Resource.ListPrice);
                    PopulateNextColumn(resource.DiscountPrice);
                    PopulateNextColumn(resource.LicenseCount * resource.DiscountPrice);
                    PopulateNextColumn($"{resource.FirstPurchaseDate:M/d/yyyy}");
                    BuildBookUrlAndNewBookUrl(resource.Resource, bookUrlPrefix, bookUrlSuffix, bookUrl);
                }
                else
                {
                    PopulateNextColumn(GetLicenseCount(resource));
                    PopulateNextColumn(resource.Resource.ListPrice);
                    PopulateNextColumn(resource.DiscountPrice);
                    PopulateNextColumn(resource.LicenseCount * resource.DiscountPrice);
                    PopulateNextColumn($"{resource.FirstPurchaseDate:M/d/yyyy}");
                    BuildBookUrlAndNewBookUrl(resource.Resource, bookUrlPrefix, bookUrlSuffix, bookUrl);
                }
            }
        }

        public CollectionManagementExcelExport(IEnumerable<CollectionManagementResource> collectionManagementResources,
            string bookUrlPrefix, string bookUrlSuffix, ILookup<int, Recommendation> recommendationsLookup,
            string bookUrl)
        {
            var resources = collectionManagementResources == null ? null : collectionManagementResources.ToList();
            var containsTurnaways = resources != null && resources.Any(x => x.ConcurrentTurnawayCount > 0);

            SpecifyBaseBookColumns(containsTurnaways);
            SpecifyBookCostColumns(false);

            SpecifyColumn("Expert Reviewer Name", "String");
            SpecifyColumn("Expert Reviewer Department", "String");
            SpecifyColumn("Recommended Date", "String");
            SpecifyColumn("Expert Reviewer Note", "String");

            if (resources == null)
            {
                return;
            }

            foreach (var resource in resources)
            {
                var recommendation = recommendationsLookup[resource.Resource.Id].FirstOrDefault();
                var status = resource.Resource.StatusToString();

                PopulateFirstColumn(status);
                PopulateNextColumn(resource.Resource.Isbn10);
                PopulateNextColumn(resource.Resource.Isbn13);
                PopulateNextColumn(resource.Resource.EIsbn);
                PopulateNextColumn(resource.Resource.Title);
                PopulateNextColumn(resource.Resource.Edition);
                PopulateNextColumn(resource.Resource.Authors);
                PopulateNextColumn(resource.Resource.Affiliation);
                PopulateNextColumn(resource.Resource.Publisher.Name);
                PopulateNextColumn($"{resource.Resource.PublicationDate:yyyy}");
                PopulateNextColumn($"{resource.Resource.ReleaseDate:M/d/yyyy}");
                PopulateNextColumn(resource.Resource.PracticeAreasToString());
                PopulateNextColumn(resource.Resource.SpecialtiesToString());
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.BradonHill, resource.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.Dct, resource.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.DctEssential, resource.Resource));
                PopulateNextColumn(resource.Resource.ForthcomingDate);
                PopulateNextColumn(resource.OriginalSourceString);
                if (containsTurnaways)
                {
                    PopulateNextColumn(resource.ConcurrentTurnawayCount);
                }

                PopulateNextColumn(resource.Resource.ListPrice);
                PopulateNextColumn(resource.DiscountPrice);
                PopulateNextColumn(resource.LicenseCount * resource.DiscountPrice);

                PopulateNextColumn($"{resource.FirstPurchaseDate:M/d/yyyy}");
                BuildBookUrlAndNewBookUrl(resource.Resource, bookUrlPrefix, bookUrlSuffix, bookUrl,
                    recommendation == null);

                if (recommendation != null)
                {
                    PopulateNextColumn(
                        $"{recommendation.RecommendedByUser.LastName}, {recommendation.RecommendedByUser.FirstName} [{recommendation.RecommendedByUser.UserName}]");
                    PopulateNextColumn(recommendation.RecommendedByUser.Department != null
                        ? recommendation.RecommendedByUser.Department.Name
                        : "");
                    PopulateNextColumn(recommendation.CreationDate);
                    PopulateLastColumn(recommendation.Notes);
                }
            }
        }
    }

    public class ExportBookUrl
    {
    }
}