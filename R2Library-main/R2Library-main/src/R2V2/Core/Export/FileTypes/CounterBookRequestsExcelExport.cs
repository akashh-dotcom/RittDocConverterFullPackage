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
    public class CounterBookRequestsExcelExport : ExcelBase
    {
        private readonly IAdminInstitution _adminInstitution;
        private readonly CounterBookRequests _counterBookRequests;
        private readonly string _templateDirectory;

        public CounterBookRequestsExcelExport(string templateDirectory, CounterBookRequests counterBookRequests,
            IAdminInstitution adminInstitution)
        {
            _templateDirectory = templateDirectory;
            _counterBookRequests = counterBookRequests;
            _adminInstitution = adminInstitution;
        }

        public new MemoryStream Export()
        {
            var fileName = Path.Combine(_templateDirectory, "CounterBookRequestsReport.xlsx");
            using (var workbook = new XLWorkbook(fileName))
            {
                workbook.Worksheet(1).Cell(5, 2).Value = $"'{_adminInstitution.AccountNumber}";
                workbook.Worksheet(1).Cell(4, 2).Value = _adminInstitution.Name;

                workbook.Worksheet(1).Cell(7, 2).Value =
                    $"Access_Method=Regular;Begin_Date={_counterBookRequests.ReportRequest.DateRangeStart:yyyy-MM-dd};End_Date={_counterBookRequests.ReportRequest.DateRangeEnd:yyyy-MM-dd}";

                workbook.Worksheet(1).Cell(10, 2).Value =
                    $"Begin_Date={_counterBookRequests.ReportRequest.DateRangeStart:yyyy-MM-dd};End_Date={_counterBookRequests.ReportRequest.DateRangeEnd:yyyy-MM-dd}";

                workbook.Worksheet(1).Cell(11, 2).Value = DateTime.Now.ToString("yyyy-MM-dd");
                workbook.Worksheet(1).Cell(11, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                var rowHeaderNumber = 14;
                var rowNumberTotalItemRequestsSum = 15;
                var rowNumberUniqueTitleRequestsSum = 16;
                var colNumber = 15;

                var headerRowStyle = workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style;
                var totalRowStyle = workbook.Worksheet(1).Cell(rowNumberTotalItemRequestsSum, colNumber).Style;
                var resourceRowStyle = workbook.Worksheet(1).Cell(17, 1).Style;

                var titleRowNumber = 17;

                var totalItemsCellValue = workbook.Worksheet(1).Cell(17, 13).Value;
                var uniqueTitlesCellValue = workbook.Worksheet(1).Cell(18, 13).Value;
                var maxColCount = 0;

                var headerDates = _counterBookRequests.CounterBookRequestResources.FirstOrDefault();
                if (headerDates != null)
                {
                    foreach (var period in headerDates.TotalItemResourcePeriods)
                    {
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Value =
                            $"{period.ShortMonth()}-{period.Year}";
                        workbook.Worksheet(1).Cell(rowHeaderNumber, colNumber).Style = headerRowStyle;
                        colNumber++;
                    }

                    colNumber = 15;
                }

                foreach (var item in _counterBookRequests.CounterBookRequestResources)
                {
                    workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = item.Title;
                    workbook.Worksheet(1).Cell(titleRowNumber, 3).Value = item.Publisher;
                    workbook.Worksheet(1).Cell(titleRowNumber, 4).Value = item.PublisherId;
                    workbook.Worksheet(1).Cell(titleRowNumber, 5).Value = "R2Library";

                    workbook.Worksheet(1).Cell(titleRowNumber, 6).Value = $"'{item.Isbn13}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 7).Value = item.ProprietaryId;
                    workbook.Worksheet(1).Cell(titleRowNumber, 8).Value = $"'{item.Isbn10}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 11).Value =
                        "https://r2library.com/Resource/Title/" + item.Isbn10;
                    workbook.Worksheet(1).Cell(titleRowNumber, 12).Value = item.YearOfPublication;
                    workbook.Worksheet(1).Cell(titleRowNumber, 13).Value = totalItemsCellValue;
                    workbook.Worksheet(1).Cell(titleRowNumber, 14).Value =
                        item.TotalItemResourcePeriods.Sum(x => x.HitCount);

                    foreach (var period in item.TotalItemResourcePeriods)
                    {
                        workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = period.HitCount;

                        var cell = workbook.Worksheet(1).Cell(15, colNumber);

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
                    colNumber = 15;

                    workbook.Worksheet(1).Cell(titleRowNumber, 2).Value = item.Title;
                    workbook.Worksheet(1).Cell(titleRowNumber, 3).Value = item.Publisher;
                    workbook.Worksheet(1).Cell(titleRowNumber, 4).Value = item.PublisherId;
                    workbook.Worksheet(1).Cell(titleRowNumber, 5).Value = "R2Library";

                    workbook.Worksheet(1).Cell(titleRowNumber, 6).Value = $"'{item.Isbn13}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 7).Value = item.ProprietaryId;
                    workbook.Worksheet(1).Cell(titleRowNumber, 8).Value = $"'{item.Isbn10}";
                    workbook.Worksheet(1).Cell(titleRowNumber, 11).Value =
                        "https://r2library.com/Resource/Title/" + item.Isbn10;
                    workbook.Worksheet(1).Cell(titleRowNumber, 12).Value = item.YearOfPublication;
                    workbook.Worksheet(1).Cell(titleRowNumber, 13).Value = uniqueTitlesCellValue;
                    workbook.Worksheet(1).Cell(titleRowNumber, 14).Value =
                        item.UniqueTitleResourcePeriods.Sum(x => x.HitCount);

                    foreach (var period in item.UniqueTitleResourcePeriods)
                    {
                        workbook.Worksheet(1).Cell(titleRowNumber, colNumber).Value = period.HitCount;

                        var cell = workbook.Worksheet(1).Cell(16, colNumber);
                        cell.Value = cell.Value.ToString() != ""
                            ? cell.Value.CastTo<int>() + period.HitCount
                            : period.HitCount;
                        cell.Style = totalRowStyle;
                        colNumber++;
                    }

                    titleRowNumber++;
                    colNumber = 15;
                }

                var resourceTotalItemRequestsRow = workbook.Worksheet(1).Row(15);
                var resourceUniqueTitleRequestsRow = workbook.Worksheet(1).Row(16);
                var resourceTotalItemRequestsTotalCells = resourceTotalItemRequestsRow.Cells(15, maxColCount - 1);
                var resourceUniqueTitleRequestsTotalCells = resourceUniqueTitleRequestsRow.Cells(15, maxColCount - 1);

                resourceTotalItemRequestsRow.Cell(14).Value = resourceTotalItemRequestsTotalCells
                    .Where(x => x.Value.ToString() != "").Sum(x => x.Value.CastTo<int>());
                resourceUniqueTitleRequestsRow.Cell(14).Value = resourceUniqueTitleRequestsTotalCells
                    .Where(x => x.Value.ToString() != "").Sum(x => x.Value.CastTo<int>());

                //Styling
                var stylingRows = workbook.Worksheet(1).Rows(17, titleRowNumber - 1);
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