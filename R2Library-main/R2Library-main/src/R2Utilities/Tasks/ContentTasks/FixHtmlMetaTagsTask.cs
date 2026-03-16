#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Tasks.ContentTasks.BookInfo;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class FixHtmlMetaTagsTask : TaskBase
    {
        private readonly IContentSettings _contentSettings;
        readonly FixHtmlQueueDataService _fixHtmlQueueFactory;
        protected new string TaskName = "FixHtmlMetaTagsTask";

        public FixHtmlMetaTagsTask(IContentSettings contentSettings)
            : base("FixHtmlMetaTagsTask", "-FixHtmlMetaTagsTask", "x22", TaskGroup.Deprecated,
                "Task for updating the HTML meta tags in the HTML content index by dtSearch", false)
        {
            _fixHtmlQueueFactory = new FixHtmlQueueDataService();

            _contentSettings = contentSettings;
        }

        public override void Run()
        {
            try
            {
                var totalRunTime = new Stopwatch();
                totalRunTime.Start();
                var fixedResourceCount = 0;
                var errorResourceCount = 0;
                var totalFixedFileCount = 0;
                var step = new TaskResultStep { Name = "FixMetaTags", StartTime = DateTime.Now };
                TaskResult.AddStep(step);
                UpdateTaskResult();

                var resourceCoreDataService = new ResourceCoreDataService();

                var fixHtmlQueue = _fixHtmlQueueFactory.GetNext();
                while (fixHtmlQueue != null && fixHtmlQueue.ResourceId > 0)
                {
                    fixHtmlQueue.DateStarted = DateTime.Now;
                    var resource = resourceCoreDataService.GetResourceByIsbn(fixHtmlQueue.Isbn, true);
                    fixHtmlQueue.DateFinished = DateTime.Now;

                    if (FixHtmlMetaData(resource, out var fixedFileCount, out var errerMessage))
                    {
                        fixHtmlQueue.Status = "F";
                        fixHtmlQueue.StatusMessage = $"{fixedFileCount} Html Files Fixed";
                        fixedResourceCount++;
                    }
                    else
                    {
                        fixHtmlQueue.Status = "E";
                        fixHtmlQueue.StatusMessage = $"ERROR - {fixedFileCount} Html Files Fixed - {errerMessage}";
                        errorResourceCount++;
                    }

                    totalFixedFileCount += fixedFileCount;

                    _fixHtmlQueueFactory.Update(fixHtmlQueue);

                    //_indexQueueFactory.Insert(fixHtmlQueue.ResourceId, fixHtmlQueue.Isbn, "A", DateTime.Now);

                    Log.InfoFormat(
                        "*** Total Run Time: {0:c}, total resource processed: {1}, fixed: {2}, errors: {3}, total files fixed: {4}",
                        totalRunTime.Elapsed, errorResourceCount + fixedResourceCount, fixedResourceCount,
                        errorResourceCount, totalFixedFileCount);

                    fixHtmlQueue = _fixHtmlQueueFactory.GetNext();
                }

                totalRunTime.Stop();
                Log.InfoFormat(
                    "### Total Run Time: {0:c}, total resource processed: {1}, fixed: {2}, errors: {3}, total files fixed: {4}",
                    totalRunTime.Elapsed, errorResourceCount + fixedResourceCount, fixedResourceCount,
                    errorResourceCount, totalFixedFileCount);

                step.CompletedSuccessfully = true;
                step.Results = $"tasked finished in {totalRunTime.Elapsed:c}";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        /// <param name="fixedFileCount"> </param>
        /// <param name="errorMessage"> </param>
        public bool FixHtmlMetaData(ResourceCore resource, out int fixedFileCount, out string errorMessage)
        {
            fixedFileCount = 0;
            try
            {
                var isbn = resource.Isbn.Trim();

                var infoMsg = new StringBuilder()
                    .AppendLine(">>>>>>>>>>")
                    .AppendFormat("\t\tISBN: {0}, Id: {1}, Status: {2}", isbn, resource.Id, resource.StatusId)
                    .AppendLine()
                    .AppendLine("<<<<<<<<<<");
                Log.Info(infoMsg);

                // verify html directory
                var htmlDirectoryPath = $@"{_contentSettings.NewContentLocation}\{isbn}";
                Log.Info(htmlDirectoryPath);
                var htmlDirectoryInfo = new DirectoryInfo(htmlDirectoryPath);
                if (!htmlDirectoryInfo.Exists)
                {
                    Log.WarnFormat("directory does not exist, {0}", htmlDirectoryInfo.FullName);
                    errorMessage = $"directory does not exist, {htmlDirectoryInfo.FullName}";
                    return false;
                }

                // verify xml directory
                //string xmlDirectoryPath = string.Format(@"{0}\{1}\{2}", _r2UtilitiesSettings.BookLoaderOutputPath,
                //                                        _r2UtilitiesSettings.BookLoaderXmlDirectoryName, isbn);
                var xmlDirectoryPath = $@"{_contentSettings.ContentLocation}\{isbn}";

                Log.Info(xmlDirectoryPath);
                var xmlDirectoryInfo = new DirectoryInfo(xmlDirectoryPath);
                if (!xmlDirectoryInfo.Exists)
                {
                    Log.WarnFormat("directory does not exist, {0}", xmlDirectoryInfo.FullName);
                    errorMessage = $"directory does not exist, {xmlDirectoryInfo.FullName}";
                    return false;
                }

                var bookSearchInfo = new BookSearchInfo(resource, xmlDirectoryInfo);

                var bookSearchResource = bookSearchInfo.ToBookSearchResource(_contentSettings.NewContentLocation, isbn);
                bookSearchResource.SaveR2BookSearchXml();


                //bookSearchInfo.SaveR2BookSearchXml(r2BookSearchXmlFullPath);

                var fileInfos = htmlDirectoryInfo.GetFiles();

                if (fileInfos.Length > 0)
                {
                    foreach (var fileInfo in fileInfos)
                    {
                        if (fileInfo.Extension == ".html")
                        {
                            var html = GetFileText(fileInfo.FullName);
                            var xmlFilePath = fileInfo.FullName.Replace("html", "xml");

                            var filePrefix = fileInfo.Name.Split('.')[0];

                            //ModifyHtmlFile(html, xmlFilePath, fileInfo.FullName, isbn10, isbn13); //, filenameRoot);

                            ReplaceMetaTags(html, bookSearchInfo, xmlFilePath, fileInfo.FullName, filePrefix);

                            fixedFileCount++;
                        }
                    }

                    errorMessage = null;
                    return true;
                }

                Log.WarnFormat("NO files in directory '{0}'", htmlDirectoryPath);
                errorMessage = $"NO files in directory '{htmlDirectoryPath}'";
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        private string GetFileText(string filepath)
        {
            var text = new StringBuilder();

            if (File.Exists(filepath))
            {
                using (var sr = File.OpenText(filepath))
                {
                    string input;
                    while ((input = sr.ReadLine()) != null)
                    {
                        text.AppendLine(input);
                    }
                }
            }

            return text.ToString();
        }

        /// <param name="filePrefix"> </param>
        private void ReplaceMetaTags(string html, BookSearchInfo bookSearchInfo, string xmlFullFilePath,
            string htmlFullFilePath, string filePrefix)
        {
            Log.Debug(htmlFullFilePath);

            var docSearchInfo = new DocSearchInfo(bookSearchInfo, xmlFullFilePath, filePrefix);

            var x = html.IndexOf("<!-- r2v2 meta tags - start -->", 0, StringComparison.Ordinal);
            if (x == -1)
            {
                x = html.IndexOf("<meta name=", 0, StringComparison.Ordinal);
            }
            else
            {
                Log.Debug("debug");
            }

            var y = html.IndexOf("</head>", x, StringComparison.Ordinal);
            string newMetaTags;
            if (y < x)
            {
                // fix needed for bug created when the closing head tag was accidently removed.
                y = html.IndexOf("<body ", x, StringComparison.Ordinal);
                newMetaTags = $"{docSearchInfo.MetaTags}</head>{Environment.NewLine}";
            }
            else
            {
                newMetaTags = docSearchInfo.MetaTags;
            }


            var oldMetaTags = html.Substring(x, y - x);

            html = html.Replace(oldMetaTags, newMetaTags);

            using (var outfile = new StreamWriter(htmlFullFilePath))
            {
                outfile.Write(html);
            }
        }
    }
}