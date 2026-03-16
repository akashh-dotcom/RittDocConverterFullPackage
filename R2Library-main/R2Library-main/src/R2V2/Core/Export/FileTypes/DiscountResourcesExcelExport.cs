#region

using System.Collections.Generic;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class DiscountResourcesExcelExport : ExcelBase
    {
        public DiscountResourcesExcelExport(IEnumerable<DiscountResource> discountResources)
        {
            SpecifyColumn("Account #", "String");
            SpecifyColumn("Institution", "String");
            SpecifyColumn("Isbn 10", "String");
            SpecifyColumn("Isbn 13", "String");
            SpecifyColumn("Title", "String");
            SpecifyColumn("Publisher", "String");
            SpecifyColumn("Order Number", "String");
            SpecifyColumn("Purchase Date", "DateTime");
            SpecifyColumn("Licenses", "int32");
            SpecifyColumn("List Price", "Decimal");
            SpecifyColumn("Discount Price", "Decimal");
            SpecifyColumn("Discount Percent", "Int32");
            SpecifyColumn("Total", "Decimal");

            foreach (var discountResource in discountResources)
            {
                PopulateFirstColumn(discountResource.AccountNumber);
                PopulateNextColumn(discountResource.InstitutionName);
                PopulateNextColumn(discountResource.Isbn10);
                PopulateNextColumn(discountResource.Isbn13);
                PopulateNextColumn(discountResource.Title);
                PopulateNextColumn(discountResource.Publisher);
                PopulateNextColumn(discountResource.OrderNumber);
                PopulateNextColumn(discountResource.PurchaseDate);
                PopulateNextColumn(GetLicenseCount(discountResource));
                PopulateNextColumn(discountResource.ListPrice);
                PopulateNextColumn(discountResource.DiscountPrice);
                PopulateNextColumn(discountResource.DiscountPercentage);
                PopulateLastColumn(discountResource.Total);
            }
        }
    }
}