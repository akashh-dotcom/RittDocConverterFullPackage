#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using R2V2.Core.CollectionManagement;
using R2V2.Core.OrderHistory;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;

#endregion

namespace R2V2.Core.Export
{
    public abstract class ExcelBase
    {
        private int _columnNumber;

        protected DataSet ListToDataSet = new DataSet();


        public string MimeType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


        private DataTable DataTable { get; set; }
        private DataRow Row { get; set; }

        public int GetLicenseCount(OrderHistoryItem orderHistoryItem)
        {
            if (orderHistoryItem.Resource.IsFreeResource)
            {
                return orderHistoryItem.NumberOfLicenses == 0 ? 0 : 1;
            }

            return orderHistoryItem.NumberOfLicenses;
        }

        public int GetLicenseCount(IResourceOrderItem resourceOrderItem)
        {
            if (resourceOrderItem.CoreResource.IsFreeResource)
            {
                return resourceOrderItem.NumberOfLicenses == 0 ? 0 : 1;
            }

            return resourceOrderItem.NumberOfLicenses;
        }

        public int GetLicenseCount(ResourceReportItem reportItem)
        {
            if (reportItem.IsFreeResource)
            {
                return reportItem.TotalLicenseCount == 0 ? 0 : reportItem.TotalLicenseCount / 500;
            }

            return reportItem.TotalLicenseCount;
        }

        public int GetLicenseCount(CollectionManagementResource collectionManagementResource)
        {
            if (collectionManagementResource.Resource.IsFreeResource)
            {
                return collectionManagementResource.LicenseCount == 0 ? 0 : 1;
            }

            return collectionManagementResource.LicenseCount;
        }

        public int GetLicenseCount(DiscountResource discountResource)
        {
            if (discountResource.IsFreeResource)
            {
                return discountResource.Licenses == 0 ? 0 : discountResource.Licenses / 500;
            }

            return discountResource.Licenses;
        }

        private static Type GetDefinedTypes(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "string":
                    return Type.GetType("System.String");

                case "datetime":
                    return Type.GetType("System.DateTime");

                case "int32":
                    return Type.GetType("System.Int32");

                case "decimal":
                    return Type.GetType("System.Decimal");

                case "boolean":
                    return Type.GetType("System.Boolean");
            }

            return null;
        }

        protected void AddColumnToDataTable(string columnName, string type, DataTable dataTable)
        {
            dataTable.Columns.Add(columnName, GetDefinedTypes(type));
        }

        public MemoryStream Export()
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(ListToDataSet);

                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }

        public int GetStartColumn()
        {
            _columnNumber = 0;
            return _columnNumber;
        }

        public int GetNextColumn()
        {
            _columnNumber++;
            return _columnNumber;
        }

        public DataRow GetProductRow()
        {
            return Row;
        }

        public void SpecifyColumn(string columnName, string type)
        {
            if (DataTable == null)
            {
                DataTable = new DataTable();
                ListToDataSet = new DataSet();
                ListToDataSet.Tables.Add(DataTable);
            }

            DataTable.Columns.Add(columnName, GetDefinedTypes(type));
        }

        public void PopulateDataRows(List<DataRow> productRows)
        {
            foreach (var productRow in productRows)
            {
                DataTable.Rows.Add(productRow);
            }
        }

        public string GetCollectionStatus(CollectionIdentifier identifier, IResource resource)
        {
            return resource.CollectionIdsToArray().Contains((int)identifier) ? "Yes" : "No";
        }

        public string BuildBookUrl(string isbn, string bookUrlPrefix, string bookUrlSuffix, string bookUrl)
        {
            string url = null;
            if (!string.IsNullOrWhiteSpace(isbn))
            {
                bookUrl = $"{bookUrlPrefix}{bookUrl}";
                url = $"{bookUrl}{(bookUrl.Last() == '/' ? "" : "/")}{isbn}";
                url = $"{url}{bookUrlSuffix}";
            }

            return url;
        }

        public void BuildBookUrlAndNewBookUrl(IResource resource, string bookUrlPrefix, string bookUrlSuffix,
            string bookUrl, bool isLast = true)
        {
            PopulateNextColumn(BuildBookUrl(resource.Isbn, bookUrlPrefix, bookUrlSuffix, bookUrl));

            PopulateNextColumn(resource.NewEditionResourceIsbn);
            if (isLast)
            {
                PopulateLastColumn(BuildBookUrl(resource.NewEditionResourceIsbn, bookUrlPrefix, bookUrlSuffix,
                    bookUrl));
            }
            else
            {
                PopulateNextColumn(BuildBookUrl(resource.NewEditionResourceIsbn, bookUrlPrefix, bookUrlSuffix,
                    bookUrl));
            }
        }

        public void SpecifyBaseBookColumns(bool containsTurnawayCounts)
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
            SpecifyColumn("Practice Area", "String");
            SpecifyColumn("Specialties", "String");

            SpecifyColumn("Former Brandon Hill", "String");
            SpecifyColumn("Doody Core Title", "String");
            SpecifyColumn("Essential Doody Core Title", "String");

            SpecifyColumn("Due Date", "String");
            SpecifyColumn("Original Source", "String");

            if (containsTurnawayCounts)
            {
                SpecifyColumn("Concurrent Turnaways", "Int32");
            }
        }

        public void SpecifyBookCostColumns(bool includeLicense)
        {
            if (includeLicense)
            {
                SpecifyColumn("License Count", "Int32");
            }

            SpecifyColumn("List Price", "Decimal");
            SpecifyColumn("Discount Price", "Decimal");
            SpecifyColumn("Total", "Decimal");
            SpecifyColumn("First Purchase Date", "String");
            SpecifyColumn("URL", "String");
            SpecifyColumn("New Edition ISBN", "String");
            SpecifyColumn("New Edition URL", "String");
        }


        #region PopulateFirstColumn

        public void PopulateFirstColumn(string value)
        {
            Row = DataTable.NewRow();
            Row[GetStartColumn()] = value;
        }

        public void PopulateFirstColumn(DateTime value)
        {
            Row = DataTable.NewRow();
            Row[GetStartColumn()] = value;
        }

        public void PopulateFirstColumn(int value)
        {
            Row = DataTable.NewRow();
            Row[GetStartColumn()] = value;
        }

        public void PopulateFirstColumn(decimal value)
        {
            Row = DataTable.NewRow();
            Row[GetStartColumn()] = value;
        }

        public void PopulateFirstColumn(bool value)
        {
            Row = DataTable.NewRow();
            Row[GetStartColumn()] = value;
        }

        #endregion

        #region PopulateNextColumn

        public void PopulateNextColumn(string value)
        {
            Row[GetNextColumn()] = value;
        }

        public void PopulateNextColumn(DateTime value)
        {
            Row[GetNextColumn()] = value;
        }

        public void PopulateNextColumn(int value)
        {
            Row[GetNextColumn()] = value;
        }

        public void PopulateNextColumn(decimal value)
        {
            Row[GetNextColumn()] = value;
        }

        public void PopulateNextColumn(bool value)
        {
            Row[GetNextColumn()] = value;
        }

        #endregion

        #region PopulateLastColumn

        public void PopulateLastColumn(string value, bool addRow = true)
        {
            Row[GetNextColumn()] = value;
            if (addRow)
            {
                DataTable.Rows.Add(Row);
            }
        }

        public void PopulateLastColumn(DateTime value, bool addRow = true)
        {
            Row[GetNextColumn()] = value;
            if (addRow)
            {
                DataTable.Rows.Add(Row);
            }
        }

        public void PopulateLastColumn(int value, bool addRow = true)
        {
            Row[GetNextColumn()] = value;
            if (addRow)
            {
                DataTable.Rows.Add(Row);
            }
        }

        public void PopulateLastColumn(decimal value, bool addRow = true)
        {
            Row[GetNextColumn()] = value;
            if (addRow)
            {
                DataTable.Rows.Add(Row);
            }
        }

        public void PopulateLastColumn(bool value, bool addRow = true)
        {
            Row[GetNextColumn()] = value;
            if (addRow)
            {
                DataTable.Rows.Add(Row);
            }
        }

        #endregion
    }
}