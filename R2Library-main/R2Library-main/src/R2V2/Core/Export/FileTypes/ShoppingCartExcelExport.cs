#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class ShoppingCartExcelExport : ExcelBase
    {
        public ShoppingCartExcelExport(IOrder order, string bookUrlPrefix, string bookUrlSuffix, string bookUrl)
        {
            SpecifyColumn("ISBN 10", "String");
            SpecifyColumn("ISBN 13", "String");
            SpecifyColumn("eISBN", "String");
            SpecifyColumn("Title", "String");
            SpecifyColumn("Author(s)", "String");
            SpecifyColumn("Author Affiliation", "String");
            SpecifyColumn("Edition", "String");
            SpecifyColumn("PublicationDate", "String");
            SpecifyColumn("Publisher", "String");
            SpecifyColumn("Practice Areas", "String");
            SpecifyColumn("Disciplines", "String");
            SpecifyColumn("Former Brandon Hill", "String");
            SpecifyColumn("Doody Core Title", "String");
            SpecifyColumn("Essential Doody Core Title", "String");
            SpecifyColumn("NLM #", "String");
            SpecifyColumn("Status", "String");
            SpecifyColumn("Added To Cart", "String");
            SpecifyColumn("Expires", "String");
            SpecifyColumn("Licenses", "String");
            SpecifyColumn("Suggested Retail Price", "Decimal");
            SpecifyColumn("Discount Price", "Decimal");
            SpecifyColumn("Total", "Decimal");
            SpecifyColumn("URL", "String");

            var productRows = new List<DataRow>();

            foreach (var item in order.Items)
            {
                var productOrderItem = item as IProductOrderItem;
                if (productOrderItem != null && productOrderItem.Include)
                {
                    PopulateFirstColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn(productOrderItem.Product.Name);
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn("");
                    PopulateNextColumn(productOrderItem.ListPrice);
                    PopulateNextColumn(productOrderItem.ListPrice);
                    PopulateNextColumn(productOrderItem.ListPrice);
                    PopulateLastColumn("", false);

                    productRows.Add(GetProductRow());
                    continue;
                }

                var resourceOrderItem = item as IResourceOrderItem;
                if (resourceOrderItem == null)
                {
                    continue;
                }

                var resource = resourceOrderItem.CoreResource;

                var resourceStatus = resource.StatusToString();
                var numberOfLicenses = GetLicenseCount(resourceOrderItem);
                if (numberOfLicenses == 0)
                {
                    continue;
                }

                PopulateFirstColumn(resource.Isbn10);
                PopulateNextColumn(resource.Isbn13);
                PopulateNextColumn(resource.EIsbn);
                PopulateNextColumn(resource.Title);
                PopulateNextColumn(resource.Authors);
                PopulateNextColumn(resource.Affiliation);
                PopulateNextColumn(resource.Edition);
                PopulateNextColumn($"{resource.PublicationDate.GetValueOrDefault(DateTime.Now):MM/dd/yyyy}");
                PopulateNextColumn(resource.Publisher.ToName());
                PopulateNextColumn(resource.PracticeAreasToString());
                PopulateNextColumn(resource.SpecialtiesToString());
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.BradonHill, resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.Dct, resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.DctEssential, resource));
                PopulateNextColumn(resource.NlmCall);
                PopulateNextColumn(resourceStatus);
                PopulateNextColumn($"{resourceOrderItem.AddedToCartDate.Date:MM/dd/yyyy}");
                PopulateNextColumn(resourceOrderItem.WasAddedViaPda
                    ? $"{resourceOrderItem.AddedToCartDate.AddDays(30):MM/dd/yyyy}"
                    : "");
                PopulateNextColumn(GetLicenseCount(resourceOrderItem));
                PopulateNextColumn(resourceOrderItem.ListPrice);
                PopulateNextColumn(resourceOrderItem.DiscountPrice);
                PopulateNextColumn(resourceOrderItem.DiscountPrice * numberOfLicenses);
                PopulateLastColumn(BuildBookUrl(resource.Isbn10, bookUrlPrefix, bookUrlSuffix, bookUrl));
            }

            if (productRows.Any())
            {
                PopulateDataRows(productRows);
            }
        }
    }
}