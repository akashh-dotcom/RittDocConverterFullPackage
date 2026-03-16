#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using Newtonsoft.Json;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Email.EmailBuilders;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Export.FileTypes;
using R2V2.Core.R2Utilities;

#endregion

namespace R2Utilities.Tasks.ReportTasks
{
    public class FindEbooksTask : EmailTaskBase
    {
        private readonly FindEBookEmailBuildService _emailBuildService;
        private readonly EmailTaskService _emailTaskService; //Can get resources from here.
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private DateTime _filesAfterDate;

        public FindEbooksTask(
            IR2UtilitiesSettings r2UtilitiesSettings
            , EmailTaskService emailTaskService
            , FindEBookEmailBuildService emailBuildService
        )
            : base("FindEbooksTask", "-FindEbooksTask", "56", TaskGroup.CustomerEmails,
                "Sends an email with all new eBooks found on the file system", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _emailTaskService = emailTaskService;
            _emailBuildService = emailBuildService;
        }

        public override void Run()
        {
            var step = new TaskResultStep { Name = "FindEbooksTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _filesAfterDate = DateTime.Now.AddDays(-_r2UtilitiesSettings.FindEbookDaysAgo);

                var fileExtensionArray = _r2UtilitiesSettings.FindEbookFileExtensions.Split(',');
                var extensions = new List<string>();
                extensions.AddRange(fileExtensionArray);

                var excludedFolderArray = _r2UtilitiesSettings.FindEbookFileExcludedFolders.Split(',');
                var excludedFolders = new List<string>();
                excludedFolders.AddRange(excludedFolderArray);

                // Get all files with the specified extensions
                var report = GetFilesWithExtensions3(extensions, excludedFolders);

                BuildAndSendEmail(report);

                step.CompletedSuccessfully = true;
                step.Results = "Done";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        private void BuildAndSendEmail(EBookReport report)
        {
            _emailBuildService.InitEmailTemplates();
            var emailArray = _r2UtilitiesSettings.FindEbookRecipients.Split(';');
            var emailMessage = _emailBuildService.BuildEmail(report, emailArray);

            var excelExport2 = new EBookFilesExcelExport2();
            var excelStream = excelExport2.CreateExcelWorkbook(report);
            var contentType = new ContentType
            {
                Name = $"R2_eBook_Report{DateTime.Now.ToShortDateString()}.xlsx"
            };

            var attachment = new Attachment(excelStream, contentType)
                { ContentType = { MediaType = excelExport2.MimeType } };
            emailMessage.ExcelAttachment = attachment;
            EmailDeliveryService.SendCustomerTaskEmail(emailMessage, _r2UtilitiesSettings.DefaultFromAddress,
                _r2UtilitiesSettings.DefaultFromAddressName);
        }

        private EBookReport GetFilesWithExtensions3(List<string> extensions, List<string> excludedFolders)
        {
            var resources = _emailTaskService.GetResources();
            var report = new EBookReport
            {
                StartDate = _filesAfterDate, EndDate = DateTime.Now, PublisherFiles = new List<EBookPublisher>()
            };

            // Get all files in the directory and subdirectories
            try
            {
                var files = Directory.GetFiles(_r2UtilitiesSettings.FindEbookRootFileLocation, "*.*",
                        SearchOption.AllDirectories)
                    .Where(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) &&
                                   !excludedFolders.Any(excluded =>
                                       file.StartsWith(
                                           Path.Combine(_r2UtilitiesSettings.FindEbookRootFileLocation, excluded),
                                           StringComparison.OrdinalIgnoreCase)));

                foreach (var file in files)
                {
                    var addPublisher = false;
                    var fi = new FileInfo(file);
                    if (fi.CreationTime > _filesAfterDate)
                    {
                        var path = fi.DirectoryName.Replace(_r2UtilitiesSettings.FindEbookRootFileLocation, "")
                            .Substring(1);
                        var publisher = path.Split('\\').FirstOrDefault();
                        var foundPublisher = report.PublisherFiles.Find(x => x.Publisher == publisher);
                        var name = fi.Name.Split('.').FirstOrDefault();
                        if (foundPublisher == null)
                        {
                            foundPublisher = new EBookPublisher
                                { Publisher = publisher, Files = new List<EBookFile>() };
                            addPublisher = true;
                        }

                        var isIsbn = false;
                        if (IsValidISBN(name))
                        {
                            isIsbn = true;
                            if (name.Length == 10)
                            {
                                var resource = resources.Find(x =>
                                    x.Isbn10 != null &&
                                    x.Isbn10.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                                if (resource != null)
                                {
                                    continue;
                                }
                            }

                            if (name.Length == 13)
                            {
                                var resource = resources.Find(x =>
                                    (x.Isbn13 != null &&
                                     x.Isbn13.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                                    ||
                                    (x.EIsbn != null &&
                                     x.EIsbn.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                                );
                                if (resource != null)
                                {
                                    continue;
                                }
                            }
                        }

                        var isNewFile = false;
                        var eBookFile = foundPublisher.Files.Find(x => x.Name == name);
                        if (eBookFile == null)
                        {
                            isNewFile = true;
                            eBookFile = new EBookFile
                            {
                                FileName = fi.Name,
                                Path = path,
                                CreateTime = fi.CreationTime,
                                Folder = publisher,
                                Name = name,
                                Extensions = new List<string> { fi.Extension },
                                Paths = new List<string> { path },
                                NameAsIsbn = isIsbn
                            };
                        }
                        else
                        {
                            eBookFile.Extensions.Add(fi.Extension);
                            eBookFile.Paths.Add(path);
                        }
                        //isIsbn

                        if (isIsbn)
                        {
                            eBookFile.Details = GetEBookDetails(name);
                            Thread.Sleep(350);
                        }

                        if (isNewFile)
                        {
                            foundPublisher.Files.Add(eBookFile);
                        }

                        if (addPublisher)
                        {
                            report.PublisherFiles.Add(foundPublisher);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error accessing directory: {ex.Message}");
            }

            report.PublisherCount = report.PublisherFiles.Count;
            report.TitleCount = report.PublisherFiles.Sum(x => x.Files.Count);
            report.PublisherFiles.ForEach(x =>
            {
                x.FileCount = x.Files.Count;
                x.Files = x.Files.OrderByDescending(y => y.CreateTime).ToList();
            });

            return report;
        }

        private EBookDetails GetEBookDetails(string isbn)
        {
            // The URL of the API endpoint
            //string url = $"https://api2.isbndb.com/book/{isbn}"
            var url = $"{_r2UtilitiesSettings.FindEbookUrl}{isbn}";
            //api.premium.isbndb.com
            // The Bearer token for authentication
            var token = _r2UtilitiesSettings.FindEbookIsbnDbKey;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<EBookDetailsRoot>(jsonResponse);
                    return details.Book;
                }

                Log.Warn($"Error: {response.StatusCode}");
            }

            return null;
        }

        static bool IsValidISBN(string isbn)
        {
            // Remove hyphens and spaces
            var cleanedISBN = isbn.Replace("-", "").Replace(" ", "");

            // Check if it's a valid ISBN-10 or ISBN-13
            return IsValidISBN10(cleanedISBN) || IsValidISBN13(cleanedISBN);
        }

        static bool IsValidISBN10(string isbn)
        {
            // ISBN-10 must be exactly 10 characters
            if (isbn.Length != 10) return false;

            var sum = 0;

            // Loop through the first 9 characters
            for (var i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn[i])) return false; // Ensure all are digits
                sum += (i + 1) * (isbn[i] - '0');
            }

            // Handle the last character, which can be a digit or 'X'
            var lastChar = isbn[9];
            if (lastChar == 'X')
            {
                sum += 10 * 10; // 'X' represents 10
            }
            else if (char.IsDigit(lastChar))
            {
                sum += 10 * (lastChar - '0');
            }
            else
            {
                return false;
            }

            // The sum must be divisible by 11
            return sum % 11 == 0;
        }

        static bool IsValidISBN13(string isbn)
        {
            // ISBN-13 must be exactly 13 characters and all digits
            if (isbn.Length != 13 || !long.TryParse(isbn, out _)) return false;

            var sum = 0;

            // Loop through all 13 digits, applying the alternating weight of 1 and 3
            for (var i = 0; i < 13; i++)
            {
                var digit = isbn[i] - '0';

                // Multiply by 1 if even index, 3 if odd index
                sum += i % 2 == 0 ? digit : digit * 3;
            }

            // The sum must be divisible by 10
            return sum % 10 == 0;
        }
    }
}