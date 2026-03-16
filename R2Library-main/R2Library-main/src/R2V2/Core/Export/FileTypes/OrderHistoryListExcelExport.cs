#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class OrderHistoryListExcelExport : ExcelBase
    {
        public OrderHistoryListExcelExport(IEnumerable<IOrder> orders)
        {
            SpecifyColumn("PO #", "String");
            SpecifyColumn("PO Comment", "String");
            SpecifyColumn("# of Titles", "String");
            SpecifyColumn("# of licenses", "String");
            SpecifyColumn("Purchase Date", "String");

            foreach (var cart in orders)
            {
                PopulateFirstColumn(cart.PurchaseOrderNumber);
                PopulateNextColumn(cart.PurchaseOrderComment);
                PopulateNextColumn(cart.Items.Count());

                var licenseCount = cart.Items.Sum(x => x.NumberOfLicenses);
                PopulateNextColumn(licenseCount);

                PopulateLastColumn(cart.PurchaseDate.GetValueOrDefault(DateTime.Now));
            }
        }

        public OrderHistoryListExcelExport(IEnumerable<OrderHistory.OrderHistory> orders)
        {
            SpecifyColumn("PO #", "String");
            SpecifyColumn("PO Comment", "String");
            SpecifyColumn("# of Titles", "String");
            SpecifyColumn("# of licenses", "String");
            SpecifyColumn("Purchase Date", "String");

            foreach (var order in orders)
            {
                PopulateFirstColumn(order.PurchaseOrderNumber);
                PopulateNextColumn(order.PurchaseOrderComment);
                PopulateNextColumn(order.NumberofTitles);
                PopulateNextColumn(order.NumberofLicenses);
                PopulateLastColumn(order.PurchaseDate);
            }
        }
    }
}