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
    public class CounterPlatformUsageExcelExport : ExcelBase
    {
        private readonly IAdminInstitution _adminInstitution;
        private readonly CounterPlatformUsageRequest _counterPlatformUsageRequest;
        private readonly string _templateDirectory;

        public CounterPlatformUsageExcelExport(string templateDirectory,
            CounterPlatformUsageRequest counterPlatformUsageRequest, IAdminInstitution adminInstitution)
        {
            _templateDirectory = templateDirectory;
            _counterPlatformUsageRequest = counterPlatformUsageRequest;
            _adminInstitution = adminInstitution;
        }

        public new MemoryStream Export()
        {
            var fileName = Path.Combine(_templateDirectory, "CounterPlatformUsageReport.xlsx");

            using (var workbook = new XLWorkbook(fileName))
            {
                workbook.Worksheet(1).Cell(5, 2).Value = $"'{_adminInstitution.AccountNumber}";
                workbook.Worksheet(1).Cell(4, 2).Value = _adminInstitution.Name;

                workbook.Worksheet(1).Cell(7, 2).Value =
                    $"Access_Method=Regular;Begin_Date={_counterPlatformUsageRequest.ReportRequest.DateRangeStart:yyyy-MM-dd};End_Date={_counterPlatformUsageRequest.ReportRequest.DateRangeEnd:yyyy-MM-dd}";

                workbook.Worksheet(1).Cell(10, 2).Value =
                    $"Begin_Date={_counterPlatformUsageRequest.ReportRequest.DateRangeStart:yyyy-MM-dd};End_Date={_counterPlatformUsageRequest.ReportRequest.DateRangeEnd:yyyy-MM-dd}";

                workbook.Worksheet(1).Cell(11, 2).Value = DateTime.Now.ToString("yyyy-MM-dd");
                workbook.Worksheet(1).Cell(11, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                const int rowHeaderNumber = 14;
                var colNumber = 4;

                var headerRowStyle = workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style;
                var resourceRowStyle = workbook.Worksheet(1).Cell(15, 1).Style;

                var titleRowNumber = 15;
                var maxColCount = colNumber;

                foreach (var period in _counterPlatformUsageRequest.TotalItemRequests)
                {
                    workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Value =
                        $"{period.ShortMonth()}-{period.Year}";
                    workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style = headerRowStyle;
                    colNumber++;
                    maxColCount++;
                }

                colNumber = 4;

                workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = "R2Library";
                workbook.Worksheet(1).Cell(titleRowNumber, 3).Value =
                    _counterPlatformUsageRequest.TotalItemRequests.Sum(x => x.HitCount);

                foreach (var item in _counterPlatformUsageRequest.TotalItemRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = item.HitCount;

                    colNumber++;
                }

                colNumber = 4;
                titleRowNumber++;

                workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = "R2Library";
                workbook.Worksheet(1).Cell(titleRowNumber, 3).Value =
                    _counterPlatformUsageRequest.UniqueItemRequests.Sum(x => x.HitCount);

                foreach (var item in _counterPlatformUsageRequest.UniqueItemRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = item.HitCount;

                    colNumber++;
                }

                colNumber = 4;
                titleRowNumber++;

                workbook.Worksheet(1).Cell(titleRowNumber, 1).Value = "R2Library";
                workbook.Worksheet(1).Cell(titleRowNumber, 3).Value =
                    _counterPlatformUsageRequest.UniqueTitleRequests.Sum(x => x.HitCount);

                foreach (var item in _counterPlatformUsageRequest.UniqueTitleRequests)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = item.HitCount;

                    colNumber++;
                }

                //Styling
                var stylingRows = workbook.Worksheet(1).Rows(15, 17);
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