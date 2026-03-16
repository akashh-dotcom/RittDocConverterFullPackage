#region

using System.Collections.Generic;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class ResourceListExcelExport : ExcelBase
    {
        public ResourceListExcelExport(IEnumerable<IResource> resources, string bookUrl, bool isPublisherList)
        {
            IsPublisherList = isPublisherList;
            BuildExcel(resources, null, null, bookUrl);
        }

        public ResourceListExcelExport(IEnumerable<IResource> resources, string bookUrlPrefix, string bookUrlSuffix,
            string bookUrl)
        {
            BuildExcel(resources, bookUrlPrefix, bookUrlSuffix, bookUrl);
        }

        private bool IsPublisherList { get; }

        private void BuildExcel(IEnumerable<IResource> resources, string bookUrlPrefix, string bookUrlSuffix,
            string bookUrl)
        {
            SpecifyColumn("Status", "String");
            SpecifyColumn("ISBN 10", "String");
            SpecifyColumn("ISBN 13", "String");
            SpecifyColumn("eISBN", "String");
            SpecifyColumn("Title", "String");
            SpecifyColumn("Edition", "String");
            SpecifyColumn("Authors", "String");
            SpecifyColumn("Author Affiliation", "String");
            SpecifyColumn("Publisher", "String");
            SpecifyColumn("Publication Date", "String");
            SpecifyColumn("R2 Release Date", "String");
            SpecifyColumn("Archive Date", "String");
            SpecifyColumn("Practice Area", "String");
            SpecifyColumn("Specialties", "String");
            SpecifyColumn("Former Brandon Hill", "String");
            SpecifyColumn("Doody Core Title", "String");
            SpecifyColumn("Essential Doody Core Title", "String");
            SpecifyColumn("Due Date", "String");
            SpecifyColumn("New Edition ISBN", "String");
            SpecifyColumn("New Edition URL", "String");

            if (!IsPublisherList)
            {
                SpecifyColumn("Price", "Decimal");
            }

            foreach (var resource in resources)
            {
                var status = resource.StatusToString();

                PopulateFirstColumn(status);
                PopulateNextColumn(resource.Isbn10);
                PopulateNextColumn(resource.Isbn13);
                PopulateNextColumn(resource.EIsbn);
                PopulateNextColumn(resource.Title);
                PopulateNextColumn(resource.Edition);
                PopulateNextColumn(resource.Authors);
                PopulateNextColumn(resource.Affiliation);
                PopulateNextColumn(resource.Publisher.ToName());
                PopulateNextColumn($"{resource.PublicationDate:yyyy}");
                PopulateNextColumn($"{resource.ReleaseDate:M/d/yyyy}");
                PopulateNextColumn(resource.StatusId == (int)ResourceStatus.Archived
                    ? resource.ArchiveDate.HasValue
                        ? $"{resource.ArchiveDate.Value:M/d/yyyy}"
                        : "Before 11/17/2016"
                    : "");

                PopulateNextColumn(resource.PracticeAreasToString());
                PopulateNextColumn(resource.SpecialtiesToString());
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.BradonHill, resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.Dct, resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.DctEssential, resource));
                PopulateNextColumn(resource.ForthcomingDate);

                var hasNewResource = !string.IsNullOrWhiteSpace(resource.NewEditionResourceIsbn);

                PopulateNextColumn(hasNewResource ? resource.NewEditionResourceIsbn : null);

                if (IsPublisherList)
                {
                    PopulateLastColumn(BuildBookUrl(resource.NewEditionResourceIsbn, bookUrlPrefix, bookUrlSuffix,
                        bookUrl));
                }
                else
                {
                    PopulateNextColumn(hasNewResource
                        ? BuildBookUrl(resource.NewEditionResourceIsbn, bookUrlPrefix, bookUrlSuffix, bookUrl)
                        : null);
                    PopulateLastColumn(resource.ListPrice);
                }
            }
        }
    }
}