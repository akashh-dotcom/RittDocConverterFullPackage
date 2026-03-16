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
    public class CounterDeniedRequestExcelExport : ExcelBase
    {
        private readonly IAdminInstitution _adminInstitution;
        private readonly CounterTurnawayResourcesRequest _counterTurnawayResourcesRequest;
        private readonly string _templateDirectory;

        public CounterDeniedRequestExcelExport(string templateDirectory,
            CounterTurnawayResourcesRequest counterTurnawayResourcesRequest, IAdminInstitution adminInstitution)
        {
            _templateDirectory = templateDirectory;
            _counterTurnawayResourcesRequest = counterTurnawayResourcesRequest;
            _adminInstitution = adminInstitution;
        }

        public new MemoryStream Export()
        {
            var fileName = Path.Combine(_templateDirectory, "CounterBookReport3.xlsx");
            using (var workbook = new XLWorkbook(fileName))
            {
                workbook.Worksheet(1).Cell(2, 1).Value = $"'{_adminInstitution.AccountNumber}";
                workbook.Worksheet(1).Cell(3, 1).Value = _adminInstitution.Name;

                workbook.Worksheet(1).Cell(5, 1).Value =
                    $"{_counterTurnawayResourcesRequest.ReportRequest.DateRangeStart.ToShortDateString()} to {_counterTurnawayResourcesRequest.ReportRequest.DateRangeEnd.ToShortDateString()}";
                workbook.Worksheet(1).Cell(7, 1).Value = DateTime.Now.ToShortDateString();
                workbook.Worksheet(1).Cell(7, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                const int rowHeaderNumber = 8;
                var colNumber = 10;

                var headerRowStyle = workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style;
                var totalRowStyle = workbook.Worksheet(1).Cell(9, colNumber).Style;
                var resourceRowStyle = workbook.Worksheet(1).Cell(11, 1).Style;

                var titleRowNumber = 11;

                var concurrencyCellValue = workbook.Worksheet(1).Cell(11, 8).Value;
                var accessCellValue = workbook.Worksheet(1).Cell(12, 8).Value;
                var maxColCount = 0;

                var headerDates = _counterTurnawayResourcesRequest.CounterTurnawayRequests.FirstOrDefault();
                if (headerDates != null)
                {
                    foreach (var period in headerDates.AccessTurnawayPeriods)
                    {
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Value =
                            $"{period.ShortMonth()}-{period.Year}";
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style = headerRowStyle;
                        colNumber++;
                    }

                    colNumber = 10;
                }

                foreach (var item in _counterTurnawayResourcesRequest.CounterTurnawayRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = item.Title;

                    workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = item.Publisher;
                    workbook.Worksheet(1).Cell(titleRowNumber, 3).Value = "R2Library";
                    //workbook.Worksheet(1).Cell(titleRowNumber, 4).Value = string.Format("'{0}", item.Isbn13);

                    workbook.Worksheet(1).Cell(titleRowNumber, 6).Value = $"'{item.Isbn13}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 7).Value = "";

                    workbook.Worksheet(1).Cell(titleRowNumber, 8).Value = concurrencyCellValue;

                    workbook.Worksheet(1).Cell(titleRowNumber, 9).Value =
                        item.ConcurrencyTurnawayPeriods.Sum(x => x.HitCount);

                    foreach (var period in item.ConcurrencyTurnawayPeriods)
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

                    workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = item.Title;
                    workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = item.Publisher;
                    workbook.Worksheet(1).Cell(titleRowNumber, 3).Value = "R2Library";
                    workbook.Worksheet(1).Cell(titleRowNumber, 4).Value = $"'{item.Isbn13}";

                    workbook.Worksheet(1).Cell(titleRowNumber, 6).Value = $"'{item.Isbn10}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 7).Value = "";

                    workbook.Worksheet(1).Cell(titleRowNumber, 8).Value = accessCellValue;

                    workbook.Worksheet(1).Cell(titleRowNumber, 9).Value =
                        item.AccessTurnawayPeriods.Sum(x => x.HitCount);

                    foreach (var period in item.AccessTurnawayPeriods)
                    {
                        workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = period.HitCount;

                        var cell = workbook.Worksheet(1).Cell(10, colNumber);
                        cell.Value = cell.Value.ToString() != ""
                            ? cell.Value.CastTo<int>() + period.HitCount
                            : period.HitCount;
                        cell.Style = totalRowStyle;
                        colNumber++;
                    }

                    titleRowNumber++;
                    colNumber = 10;
                }

                var concurrencyRow = workbook.Worksheet(1).Row(9);
                var concurrencyTotalCells = concurrencyRow.Cells(10, maxColCount - 1);

                concurrencyRow.Cell(9).Value = concurrencyTotalCells.Where(x => x.Value.ToString() != "")
                    .Sum(x => x.Value.CastTo<int>());

                var accessRow = workbook.Worksheet(1).Row(10);
                var accessTotalCells = accessRow.Cells(10, maxColCount - 1);
                accessRow.Cell(9).Value = accessTotalCells.Sum(x => x.Value.CastTo<int>());

                //Styling
                var stylingRows = workbook.Worksheet(1).Rows(11, titleRowNumber - 1);
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

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
    }
}