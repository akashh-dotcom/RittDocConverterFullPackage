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
    public class CounterPlatformRequestExcelExport : ExcelBase
    {
        private readonly IAdminInstitution _adminInstitution;
        private readonly CounterTotalSearchesRequest _counterTotalSearchesRequest;
        private readonly string _templateDirectory;

        public CounterPlatformRequestExcelExport(string templateDirectory,
            CounterTotalSearchesRequest counterTotalSearchesRequest, IAdminInstitution adminInstitution)
        {
            _templateDirectory = templateDirectory;
            _counterTotalSearchesRequest = counterTotalSearchesRequest;
            _adminInstitution = adminInstitution;
        }

        public new MemoryStream Export()
        {
            var fileName = Path.Combine(_templateDirectory, "CounterPlatformReport1.xlsx");

            using (var workbook = new XLWorkbook(fileName))
            {
                workbook.Worksheet(1).Cell(2, 1).Value = $"'{_adminInstitution.AccountNumber}";
                workbook.Worksheet(1).Cell(3, 1).Value = _adminInstitution.Name;

                workbook.Worksheet(1).Cell(5, 1).Value =
                    $"{_counterTotalSearchesRequest.ReportRequest.DateRangeStart.ToShortDateString()} to {_counterTotalSearchesRequest.ReportRequest.DateRangeEnd.ToShortDateString()}";
                workbook.Worksheet(1).Cell(7, 1).Value = DateTime.Now.ToShortDateString();
                workbook.Worksheet(1).Cell(7, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                const int rowHeaderNumber = 8;
                var colNumber = 5;

                var headerRowStyle = workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style;
                var resourceRowStyle = workbook.Worksheet(1).Cell(9, 1).Style;

                var titleRowNumber = 9;
                var maxColCount = colNumber;

                foreach (var period in _counterTotalSearchesRequest.SectionRequests)
                {
                    workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Value =
                        $"{period.ShortMonth()}-{period.Year}";
                    workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style = headerRowStyle;
                    colNumber++;
                    maxColCount++;
                }

                colNumber = 5;

                workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = "R2Library";
                workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = "";
                workbook.Worksheet(1).Cell(titleRowNumber, 4).Value =
                    _counterTotalSearchesRequest.SearchRequests.Sum(x => x.HitCount);

                foreach (var item in _counterTotalSearchesRequest.SearchRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = item.HitCount;

                    colNumber++;
                }

                colNumber = 5;
                titleRowNumber++;

                workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = "R2Library";
                workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = "";
                workbook.Worksheet(1).Cell(titleRowNumber, 4).Value =
                    _counterTotalSearchesRequest.SectionRequests.Sum(x => x.HitCount);

                foreach (var item in _counterTotalSearchesRequest.SectionRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = item.HitCount;

                    colNumber++;
                }

                colNumber = 5;
                titleRowNumber++;

                workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = "R2Library";
                workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = "";
                workbook.Worksheet(1).Cell(titleRowNumber, 4).Value =
                    _counterTotalSearchesRequest.ResourceRequests.Sum(x => x.HitCount);

                foreach (var item in _counterTotalSearchesRequest.ResourceRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = item.HitCount;

                    colNumber++;
                }

                //Styling
                var stylingRows = workbook.Worksheet(1).Rows(9, 11);
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