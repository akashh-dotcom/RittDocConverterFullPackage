#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using R2V2.Core.R2Utilities;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class EBookFilesExcelExport : ExcelBase
    {
        public EBookFilesExcelExport(EBookReport report)
        {
            SpecifyColumn("Folder", "String");
            SpecifyColumn("File Name", "String");
            SpecifyColumn("File Date", "String");
            SpecifyColumn("Path", "String");

            SpecifyColumn("Language", "String");
            SpecifyColumn("Title", "String");
            SpecifyColumn("Isbn10", "String");
            SpecifyColumn("Isbn13", "String");
            SpecifyColumn("Publisher", "String");
            SpecifyColumn("Authors", "String");
            SpecifyColumn("Edition", "string");
            SpecifyColumn("Publication Date", "String");
            SpecifyColumn("Pages", "string");
            SpecifyColumn("Subjects", "String");
            SpecifyColumn("MSRP", "String");
            SpecifyColumn("Synopsis", "String");
            SpecifyColumn("Cover Image", "String");


            var items = new List<EBookFile>();
            report.PublisherFiles.ForEach(x => { items.AddRange(x.Files); });
            items = items.OrderBy(x => x.Folder).ThenByDescending(x => x.Details != null ? x.Details.DatePublished : "")
                .ToList();

            foreach (var item in items)
            {
                PopulateFirstColumn(item.Folder);
                PopulateNextColumn(item.FileName);
                PopulateNextColumn($"{item.CreateTime:yyyy-MM-dd hh:mm:ss}");
                if (item.Details == null)
                {
                    PopulateNextColumn(item.Path);
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
                    PopulateLastColumn("");
                }
                else
                {
                    PopulateNextColumn(item.Path);
                    PopulateNextColumn(item.Details.Language);
                    PopulateNextColumn(item.Details.Title);
                    PopulateNextColumn(item.Details.Isbn10);
                    PopulateNextColumn(item.Details.Isbn13);
                    PopulateNextColumn(item.Details.Publisher);
                    PopulateNextColumn(item.Details.Authors != null ? string.Join(", ", item.Details.Authors) : "");
                    PopulateNextColumn(item.Details.Edition);
                    PopulateNextColumn(item.Details.DatePublished);
                    PopulateNextColumn(item.Details.Pages);
                    PopulateNextColumn(item.Details.Subjects != null ? string.Join(", ", item.Details.Subjects) : "");
                    PopulateNextColumn(item.Details.Msrp.ToString());
                    PopulateNextColumn(item.Details.Synopsis);
                    PopulateLastColumn(item.Details.Image);
                }
            }
        }
    }

    public enum EBookColumns
    {
        ForthComing = 1,
        ProcessTitles = 2,
        Folder = 3,
        Publisher = 4,
        PublicationDate = 5,
        Language = 6,
        Edition = 7,
        Pages = 8,
        MSRP = 9,
        Title = 10,
        Isbn10 = 11,
        Isbn13 = 12,
        Authors = 13,
        Subjects = 14,
        Synopsis = 15,
        Name = 16,
        Extensions = 17,
        FileDate = 18,
        Path = 19,
        CoverImage = 20,
        GoogleBooks = 21,
        Google = 22,
        Amazon = 23
    }

    public class EBookFilesExcelExport2 : ExcelBase
    {
        private string StripHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public MemoryStream CreateExcelWorkbook(EBookReport report)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("eBooks Found");

                // Add column headers
                worksheet.Cell(1, (int)EBookColumns.ForthComing).Value = "Pre-Order?";
                worksheet.Cell(1, (int)EBookColumns.ProcessTitles).Value = "Process?";
                worksheet.Cell(1, (int)EBookColumns.Folder).Value = "Folder";
                worksheet.Cell(1, (int)EBookColumns.Publisher).Value = "Publisher";
                worksheet.Cell(1, (int)EBookColumns.PublicationDate).Value = "Publication Date";
                worksheet.Cell(1, (int)EBookColumns.Language).Value = "Language";
                worksheet.Cell(1, (int)EBookColumns.Edition).Value = "Edition";
                worksheet.Cell(1, (int)EBookColumns.Pages).Value = "Pages";
                worksheet.Cell(1, (int)EBookColumns.MSRP).Value = "MSRP";
                worksheet.Cell(1, (int)EBookColumns.Title).Value = "Title";
                worksheet.Cell(1, (int)EBookColumns.Isbn10).Value = "Isbn10";
                worksheet.Cell(1, (int)EBookColumns.Isbn13).Value = "Isbn13";
                worksheet.Cell(1, (int)EBookColumns.Authors).Value = "Authors";
                worksheet.Cell(1, (int)EBookColumns.Subjects).Value = "Subjects";
                worksheet.Cell(1, (int)EBookColumns.Synopsis).Value = "Synopsis";
                worksheet.Cell(1, (int)EBookColumns.Name).Value = "File Name";
                worksheet.Cell(1, (int)EBookColumns.Extensions).Value = "Extensions";
                worksheet.Cell(1, (int)EBookColumns.FileDate).Value = "File Date";
                worksheet.Cell(1, (int)EBookColumns.Path).Value = "Path";
                worksheet.Cell(1, (int)EBookColumns.CoverImage).Value = "Cover Image"; //URL
                worksheet.Cell(1, (int)EBookColumns.GoogleBooks).Value = "Google Bools"; //URL
                worksheet.Cell(1, (int)EBookColumns.Google).Value = "Google"; //URL
                worksheet.Cell(1, (int)EBookColumns.Amazon).Value = "Amazon"; //URL


                // Optionally, style the headers (bold, background color, etc.)
                var headerRange = worksheet.Range("A1:W1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

                var items = new List<EBookFile>();
                report.PublisherFiles.ForEach(x => { items.AddRange(x.Files); });

                items = items.OrderBy(x => x.Folder).ThenByDescending(x => x.CreateTime).ToList();

                // Loop through the list of objects and populate the rows (starting from row 2)
                var rowNumber = 2; // Start from the second row (below headers)
                foreach (var item in items)
                {
                    worksheet.Cell(rowNumber, (int)EBookColumns.Folder).Value = item.Folder;
                    worksheet.Cell(rowNumber, (int)EBookColumns.Publisher).Value =
                        item.Details != null ? item.Details.Publisher : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.PublicationDate).Value =
                        item.Details != null ? item.Details.DatePublished : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Language).Value =
                        item.Details != null ? item.Details.Language : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Edition).Value =
                        item.Details != null ? item.Details.Edition : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Pages).Value =
                        item.Details != null ? item.Details.Pages : "";
                    if (item.Details != null)
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.MSRP).Value = item.Details.Msrp;
                    }

                    worksheet.Cell(rowNumber, (int)EBookColumns.Title).Value =
                        item.Details != null ? item.Details.Title : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Isbn10)
                        .SetValue(item.Details != null ? item.Details.Isbn10 : "");
                    worksheet.Cell(rowNumber, (int)EBookColumns.Isbn13)
                        .SetValue(item.Details != null ? item.Details.Isbn13 : "");
                    worksheet.Cell(rowNumber, (int)EBookColumns.Authors).Value = item.Details?.Authors != null
                        ? string.Join(", ", item.Details.Authors)
                        : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Subjects).Value = item.Details?.Subjects != null
                        ? string.Join(", ", item.Details.Subjects)
                        : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Synopsis).Value = item.Details?.Synopsis != null
                        ? StripHtmlTags(item.Details.Synopsis)
                        : "";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Name).SetValue(item.Name);
                    worksheet.Cell(rowNumber, (int)EBookColumns.Extensions).Value = item.GetExtensionString();
                    worksheet.Cell(rowNumber, (int)EBookColumns.FileDate).Value =
                        $"{item.CreateTime:yyyy-MM-dd hh:mm:ss}";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Path).Value = item.GetPathString();

                    if (item.Details != null && !string.IsNullOrWhiteSpace(item.Details.Image))
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.CoverImage).Value = "Cover Image";
                        worksheet.Cell(rowNumber, (int)EBookColumns.CoverImage).Hyperlink =
                            new XLHyperlink(item.Details.Image);
                        worksheet.Cell(rowNumber, (int)EBookColumns.CoverImage).Style.Font.FontColor = XLColor.Blue;
                        worksheet.Cell(rowNumber, (int)EBookColumns.CoverImage).Style.Font.Underline =
                            XLFontUnderlineValues.Single;
                    }
                    else
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.CoverImage).Value = "";
                    }

                    if (!string.IsNullOrWhiteSpace(item.GoogleBooksUrl()))
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.GoogleBooks).Value = "Google Books";
                        worksheet.Cell(rowNumber, (int)EBookColumns.GoogleBooks).Hyperlink =
                            new XLHyperlink(item.GoogleBooksUrl());
                        worksheet.Cell(rowNumber, (int)EBookColumns.GoogleBooks).Style.Font.FontColor = XLColor.Blue;
                        worksheet.Cell(rowNumber, (int)EBookColumns.GoogleBooks).Style.Font.Underline =
                            XLFontUnderlineValues.Single;
                    }
                    else
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.GoogleBooks).Value = "";
                    }

                    if (!string.IsNullOrWhiteSpace(item.GoogleSearchUrl()))
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.Google).Value = "Google";
                        worksheet.Cell(rowNumber, (int)EBookColumns.Google).Hyperlink =
                            new XLHyperlink(item.GoogleSearchUrl());
                        worksheet.Cell(rowNumber, (int)EBookColumns.Google).Style.Font.FontColor = XLColor.Blue;
                        worksheet.Cell(rowNumber, (int)EBookColumns.Google).Style.Font.Underline =
                            XLFontUnderlineValues.Single;
                    }
                    else
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.Google).Value = "";
                    }

                    if (!string.IsNullOrWhiteSpace(item.AmazonSearchUrl()))
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.Amazon).Value = "Amazon";
                        worksheet.Cell(rowNumber, (int)EBookColumns.Amazon).Hyperlink =
                            new XLHyperlink(item.AmazonSearchUrl());
                        worksheet.Cell(rowNumber, (int)EBookColumns.Amazon).Style.Font.FontColor = XLColor.Blue;
                        worksheet.Cell(rowNumber, (int)EBookColumns.Amazon).Style.Font.Underline =
                            XLFontUnderlineValues.Single;
                    }
                    else
                    {
                        worksheet.Cell(rowNumber, (int)EBookColumns.Amazon).Value = "";
                    }

                    worksheet.Cell(rowNumber, (int)EBookColumns.Isbn10).Style.NumberFormat.Format = "@";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Isbn10).DataType = XLCellValues.Text;
                    worksheet.Cell(rowNumber, (int)EBookColumns.Isbn13).Style.NumberFormat.Format = "@";
                    worksheet.Cell(rowNumber, (int)EBookColumns.Isbn13).DataType = XLCellValues.Text;
                    worksheet.Cell(rowNumber, (int)EBookColumns.Name).Style.NumberFormat.Format = "@";


                    // Move to the next row
                    rowNumber++;
                }

                // Adjust column widths for better readability
                worksheet.Columns().AdjustToContents();

                worksheet.Column((int)EBookColumns.Synopsis).Width = 100;
                worksheet.Column((int)EBookColumns.Subjects).Width = 100;
                worksheet.Column((int)EBookColumns.Authors).Width = 100;
                worksheet.Column((int)EBookColumns.Name).Width = 16;
                worksheet.Column((int)EBookColumns.MSRP).Style.NumberFormat.Format = "$#,##0.00";

                worksheet.Range(worksheet.FirstCellUsed(), worksheet.LastCellUsed()).CreateTable();


                // Save the workbook to a MemoryStream
                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);

                // Reset the position of the stream to the beginning
                memoryStream.Position = 0;

                return memoryStream; // Return the MemoryStream
            }
        }
    }
}