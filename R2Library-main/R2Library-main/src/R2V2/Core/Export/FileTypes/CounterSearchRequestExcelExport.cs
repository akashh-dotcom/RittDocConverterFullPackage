#region

using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using R2V2.Core.Admin;
using R2V2.Core.Reports.Counter;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class CounterSearchRequestExcelExport : ExcelBase
    {
        private readonly IAdminInstitution _adminInstitution;
        private readonly CounterSearchesRequest _counterSearchesRequest;
        private readonly string _templateDirectory;

        public CounterSearchRequestExcelExport(string templateDirectory, CounterSearchesRequest counterSearchesRequest,
            IAdminInstitution adminInstitution)
        {
            _templateDirectory = templateDirectory;
            _counterSearchesRequest = counterSearchesRequest;
            _adminInstitution = adminInstitution;
        }

        public new MemoryStream Export()
        {
            var fileName = Path.Combine(_templateDirectory, "CounterBookReport5.xlsx");
            using (var workbook = new XLWorkbook(fileName))
            {
                workbook.Worksheet(1).Cell(2, 1).Value = $"'{_adminInstitution.AccountNumber}";
                workbook.Worksheet(1).Cell(3, 1).Value = _adminInstitution.Name;

                workbook.Worksheet(1).Cell(5, 1).Value =
                    $"{_counterSearchesRequest.ReportRequest.DateRangeStart.ToShortDateString()} to {_counterSearchesRequest.ReportRequest.DateRangeEnd.ToShortDateString()}";
                workbook.Worksheet(1).Cell(7, 1).Value = DateTime.Now.ToShortDateString();
                workbook.Worksheet(1).Cell(7, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                const int rowHeaderNumber = 8;
                const int totalsRowNumber = 9;
                var colNumber = 10;

                var headerRowStyle = workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style;
                var totalRowStyle = workbook.Worksheet(1).Cell(totalsRowNumber, colNumber).Style;
                var resourceRowStyle = workbook.Worksheet(1).Cell(10, 1).Style;

                var titleRowNumber = 10;

                var maxColCount = 0;

                var headerDates = _counterSearchesRequest.CounterSearchRequests.FirstOrDefault();
                if (headerDates != null)
                {
                    foreach (var period in headerDates.ResourcePeriods)
                    {
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Value =
                            $"{period.ShortMonth()}-{period.Year}";
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style = headerRowStyle;
                        colNumber++;
                    }

                    colNumber = 10;
                }

                foreach (var item in _counterSearchesRequest.CounterSearchRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = item.Title;
                    workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = item.Publisher;
                    workbook.Worksheet(1).Cell(titleRowNumber, 3).Value = "R2Library";
                    // workbook.Worksheet(1).Cell(titleRowNumber, 4).Value = string.Format("'{0}", item.Isbn13);

                    workbook.Worksheet(1).Cell(titleRowNumber, 6).Value = $"'{item.Isbn13}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 7).Value = "";
                    workbook.Worksheet(1).Cell(titleRowNumber, 8).Value = "Regular Searches";

                    workbook.Worksheet(1).Cell(titleRowNumber, 9).Value = item.ResourcePeriods.Sum(x => x.HitCount);

                    foreach (var period in item.ResourcePeriods)
                    {
                        workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = period.HitCount;

                        var cell = workbook.Worksheet(1).Cell(9, colNumber);

                        cell.Value = cell.Value.ToString() != ""
                            ? cell.Value.CastTo<int>() + period.HitCount
                            : period.HitCount;
                        cell.Style = totalRowStyle;
                        colNumber++;
                    }

                    if (maxColCount == 0)
                    {
                        maxColCount = colNumber;
                    }

                    titleRowNumber++;
                    colNumber = 10;
                }

                var resourceRow = workbook.Worksheet(1).Row(totalsRowNumber);
                var resourceTotalCells = resourceRow.Cells(9, maxColCount - 1);

                resourceRow.Cell(8).Value = "Regular Searches";
                resourceRow.Cell(9).Value = resourceTotalCells.Where(x => x.Value.ToString() != "")
                    .Sum(x => x.Value.CastTo<int>());

                //Styling
                var stylingRows = workbook.Worksheet(1).Rows(10, titleRowNumber - 1);
                foreach (var row in stylingRows)
                {
                    row.Cells(1, maxColCount - 1).Style = resourceRowStyle;
                }


                workbook.Worksheet(1).Columns().AdjustToContents();
                workbook.Worksheet(1).Rows().AdjustToContents();


                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }
    }
}