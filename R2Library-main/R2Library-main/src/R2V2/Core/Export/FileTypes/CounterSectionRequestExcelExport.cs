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
    public class CounterSectionRequestExcelExport : ExcelBase
    {
        private readonly IAdminInstitution _adminInstitution;
        private readonly CounterSuccessfulResourcesRequest _counterSuccessfulResourceRequests;
        private readonly string _templateDirectory;

        public CounterSectionRequestExcelExport(string templateDirectory,
            CounterSuccessfulResourcesRequest counterSuccessfulResourceRequests, IAdminInstitution adminInstitution)
        {
            _templateDirectory = templateDirectory;
            _counterSuccessfulResourceRequests = counterSuccessfulResourceRequests;
            _adminInstitution = adminInstitution;
        }

        public new MemoryStream Export()
        {
            var fileName = Path.Combine(_templateDirectory, "CounterBookReport2.xlsx");
            using (var workbook = new XLWorkbook(fileName))
            {
                workbook.Worksheet(1).Cell(2, 1).Value = $"'{_adminInstitution.AccountNumber}";
                workbook.Worksheet(1).Cell(3, 1).Value = _adminInstitution.Name;

                workbook.Worksheet(1).Cell(5, 1).Value =
                    $"{_counterSuccessfulResourceRequests.ReportRequest.DateRangeStart.ToShortDateString()} to {_counterSuccessfulResourceRequests.ReportRequest.DateRangeEnd.ToShortDateString()}";
                workbook.Worksheet(1).Cell(7, 1).Value = DateTime.Now.ToShortDateString();
                workbook.Worksheet(1).Cell(7, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                var rowHeaderNumber = 8;
                var rowNumber = 9;
                var colNumber = 9;

                var headerRowStyle = workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style;
                var totalRowStyle = workbook.Worksheet(1).Cell(rowNumber, colNumber).Style;
                var resourceRowStyle = workbook.Worksheet(1).Cell(10, 1).Style;

                var titleRowNumber = 10;

                var maxColCount = 0;

                var headerDates = _counterSuccessfulResourceRequests.CounterResourceRequests.FirstOrDefault();
                if (headerDates != null)
                {
                    foreach (var period in headerDates.ResourcePeriods)
                    {
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Value =
                            $"{period.ShortMonth()}-{period.Year}";
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style = headerRowStyle;
                        colNumber++;
                    }

                    colNumber = 9;
                }

                foreach (var item in _counterSuccessfulResourceRequests.CounterResourceRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = item.Title;
                    workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = item.Publisher;
                    workbook.Worksheet(1).Cell(titleRowNumber, 3).Value = "R2Library";
                    //workbook.Worksheet(1).Cell(titleRowNumber, 4).Value = string.Format("'{0}", item.Isbn13);

                    workbook.Worksheet(1).Cell(titleRowNumber, 6).Value = $"'{item.Isbn13}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 7).Value = "";

                    workbook.Worksheet(1).Cell(titleRowNumber, 8).Value = item.ResourcePeriods.Sum(x => x.HitCount);

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
                    colNumber = 9;
                }

                var resourceRow = workbook.Worksheet(1).Row(9);
                var resourceTotalCells = resourceRow.Cells(9, maxColCount - 1);

                resourceRow.Cell(8).Value = resourceTotalCells.Where(x => x.Value.ToString() != "")
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