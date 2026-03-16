#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class OrderHistoryExcelExport : ExcelBase
    {
        public OrderHistoryExcelExport(OrderHistory.OrderHistory orderHistory, string bookUrlPrefix,
            string bookUrlSuffix, string bookUrl)
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
            SpecifyColumn("Licenses", "String");
            SpecifyColumn("Price", "Decimal");
            SpecifyColumn("Total", "Decimal");
            SpecifyColumn("URL", "String");

            var productRows = new List<DataRow>();

            foreach (var item in orderHistory.OrderHistoryProducts)
            {
                PopulateFirstColumn("");
                PopulateNextColumn("");
                PopulateNextColumn("");
                PopulateNextColumn(item.Product.Name);
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
                PopulateNextColumn(item.ListPrice);
                PopulateNextColumn(item.ListPrice);
                PopulateLastColumn("", false);

                productRows.Add(GetProductRow());
            }

            foreach (var item in orderHistory.OrderHistoryResources)
            {
                var resourceStatus = item.Resource.StatusToString();

                PopulateFirstColumn(item.Resource.Isbn10);
                PopulateNextColumn(item.Resource.Isbn13);
                PopulateNextColumn(item.Resource.EIsbn);
                PopulateNextColumn(item.Resource.Title);
                PopulateNextColumn(item.Resource.Authors);
                PopulateNextColumn(item.Resource.Affiliation);
                PopulateNextColumn(item.Resource.Edition);
                PopulateNextColumn(item.Resource.PublicationDate.GetValueOrDefault(DateTime.Now));
                PopulateNextColumn(item.Resource.Publisher.ToName());
                PopulateNextColumn(item.Resource.PracticeAreasToString());
                PopulateNextColumn(item.Resource.SpecialtiesToString());
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.BradonHill, item.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.Dct, item.Resource));
                PopulateNextColumn(GetCollectionStatus(CollectionIdentifier.DctEssential, item.Resource));
                PopulateNextColumn(item.Resource.NlmCall);
                PopulateNextColumn(resourceStatus);
                PopulateNextColumn(GetLicenseCount(item));
                PopulateNextColumn(item.DiscountPrice);
                PopulateNextColumn(item.DiscountPrice * item.NumberOfLicenses);
                PopulateLastColumn(BuildBookUrl(item.Resource.Isbn10, bookUrlPrefix, bookUrlSuffix, bookUrl));
            }


            if (productRows.Any())
            {
                PopulateDataRows(productRows);
            }
        }
    }
}