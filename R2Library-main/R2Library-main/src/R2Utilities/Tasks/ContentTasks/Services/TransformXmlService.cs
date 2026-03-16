#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.BookInfo;
using R2Utilities.Tasks.ContentTasks.Xsl;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Content;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class TransformXmlService
    {
        private readonly ContentTransformer _contentTransformer;
        private readonly IndexQueueDataService _indexQueueDataService;
        private readonly ILog<TransformXmlService> _log;

        //private readonly IContentSettings _contentSettings;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        private readonly ResourceCoreDataService _resourceCoreDataService;
        private readonly TransformedResourceDataService _transformedResourceFactory;

        private readonly StringBuilder _validationWarningMessages = new StringBuilder();
        public readonly string HtmlRootPath;

        public readonly string XmlRootPath;

        public TransformXmlService(ILog<TransformXmlService> log
            , ContentTransformer contentTransformer
            , IContentSettings contentSettings
            , IR2UtilitiesSettings r2UtilitiesSettings
        )
        {
            _log = log;
            _contentTransformer = contentTransformer;
            //_contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _resourceCoreDataService = new ResourceCoreDataService();
            _transformedResourceFactory = new TransformedResourceDataService();
            HtmlRootPath = $"{contentSettings.NewContentLocation}/html";
            XmlRootPath = contentSettings.ContentLocation;
            _indexQueueDataService = new IndexQueueDataService();
        }

        public string ValidationWarningMessages => _validationWarningMessages.ToString();

        public ResourceTransformData TransformResource(ResourceCore resource)
        {
            try
            {
                if (resource == null)
                {
                    _log.Warn("RESOURCE IS NULL");
                    return null;
                }

                var isValidResource = resource.Id > 0;

                if (!isValidResource)
                {
                    _log.Warn("INVALID RESOURCE");
                }

                _log.Debug(resource.ToDebugString());

                // insert TransformedResource record
                var transformedResource = new TransformedResource
                {
                    ResourceId = resource.Id,
                    Isbn = resource.Isbn,
                    Successfully = false,
                    DateStarted = DateTime.Now,
                    Results = "Processing ..."
                };

                if (isValidResource)
                {
                    _transformedResourceFactory.Insert(transformedResource);
                }

                _log.Info(transformedResource.ToDebugString());

                var rtd = new ResourceTransformData(resource, transformedResource.Id, HtmlRootPath);

                if (isValidResource)
                {
                    TransformResource(rtd);

                    // validate newly transformed HTML file
                    rtd.ValidateNewHtmlFiles();
                    if (rtd.ValidationFailureCount > 0)
                    {
                        _validationWarningMessages.AppendFormat("Id: {0}, ISBN: {1}, ValidationFailureCount: {2}",
                                rtd.Resource.Id, rtd.Isbn, rtd.ValidationFailureCount)
                            .AppendLine();
                    }
                }
                else
                {
                    rtd.StatusMessage = "Invalid Resource - 0 Files Transformed!";
                    rtd.HasWarning = true;
                    rtd.Successful = true;
                }

                _log.DebugFormat("rtd.TransferCount: {0}", rtd.TransferCount);
                if (rtd.TransferCount > 0)
                {
                    var addedToQueue = _indexQueueDataService.AddResourceToQueue(resource.Id, resource.Isbn);
                    _log.DebugFormat("Resource added to index queue: {0}", addedToQueue);
                }

                return rtd;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return null;
            }
        }

        private void TransformResource(ResourceTransformData rtd)
        {
            rtd.TransferCount = 0;
            _log.InfoFormat(">>>>>>>>>> PROCESSING - ResourceId: {0}, ISBN: {1}, Title:{2}", rtd.Resource.Id, rtd.Isbn,
                rtd.Resource.Title);
            _log.InfoFormat(">>>>>>>>>> STATUS - {0}", rtd.Resource.StatusId);
            try
            {
                var filesToTransform = GetFileToTransform(rtd.Isbn);
                _log.InfoFormat("{0} to transform", filesToTransform.Count);
                if (filesToTransform.Count > 0)
                {
                    // get book.xml data and update book.xml with search specific data
                    var contentDirectoryName = $@"{XmlRootPath}\{rtd.Isbn}";
                    var directoryInfo = new DirectoryInfo(contentDirectoryName);
                    var bookSearchInfo = new BookSearchInfo(rtd.Resource, directoryInfo);
                    rtd.HtmlDirectoryInfo.Create();
                    _log.InfoFormat("Processing book.xml, file: {0}",
                        $@"{HtmlRootPath}\{rtd.Isbn}\r2BookSearch.{rtd.Isbn}.xml");


                    var bookSearchResource = bookSearchInfo.ToBookSearchResource(HtmlRootPath, rtd.Isbn);
                    bookSearchResource.SaveR2BookSearchXml();

                    //bookSearchInfo.SaveBookXml(bookXmlFullPath);
                    //bookSearchInfo.SaveR2BookSearchXml(r2BookSearchXmlFullPath);

                    var authorInsertCount = SaveAuthors(bookSearchInfo, rtd.Resource.Id);
                    _log.InfoFormat("Author Insert Count: {0}", authorInsertCount);

                    //ContentTransformer contentTransformer = new ContentTransformer { Isbn = rtd.Isbn };
                    var transformStopwatch = new Stopwatch();
                    transformStopwatch.Start();

                    var totalTransformations = filesToTransform.Count + bookSearchInfo.Glossaries.Count();
                    var transformCount = 0;

                    // sections
                    foreach (var fileInfo in filesToTransform)
                    {
                        var fileParts = fileInfo.Name.Split('.');
                        transformCount++;
                        TransformFile(rtd, fileInfo, bookSearchInfo, fileParts[2], false,
                            $"{transformCount} of {totalTransformations}");
                    }

                    // glossaries
                    foreach (var glossary in bookSearchInfo.Glossaries)
                    {
                        transformCount++;
                        TransformFile(rtd, bookSearchInfo.BookXmlFileInfo, bookSearchInfo, glossary, true,
                            $"{transformCount} of {totalTransformations}");
                    }

                    transformStopwatch.Stop();

                    _log.InfoFormat(
                        "{0} of {1} files transformed successfully in {2:c}, {3} errors, {4} validation failures",
                        rtd.TransferCount, filesToTransform.Count, transformStopwatch.Elapsed, rtd.ErrorCount,
                        rtd.ValidationFailureCount);
                    _log.InfoFormat("Avg Transform Time: {0} ms",
                        transformStopwatch.ElapsedMilliseconds / filesToTransform.Count);

                    rtd.StatusMessage =
                        $"{rtd.TransferCount} of {filesToTransform.Count} files transformed successfully in {transformStopwatch.Elapsed:c}, {rtd.ErrorCount} errors, {rtd.ValidationFailureCount} validation failures, Avg Transform Time: {transformStopwatch.ElapsedMilliseconds / filesToTransform.Count} ms";

                    rtd.Successful = rtd.ErrorCount == 0;
                }
                else
                {
                    rtd.Successful = false;
                    rtd.StatusMessage = "0 files to transform!";
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                rtd.StatusMessage = $"EXCEPTION: {ex.Message}";
                rtd.Successful = false;
            }

            _log.InfoFormat("<<<<<<<<<< SUCCESSFUL - {0}, ISBN: {1}", rtd.Successful, rtd.Isbn);
            rtd.Complete();
        }

        /// <param name="isGlossary"> </param>
        public bool TransformFile(ResourceTransformData rtd, FileInfo fileInfo, BookSearchInfo bookSearchInfo,
            string section, bool isGlossary, string logData)
        {
            try
            {
                //_log.InfoFormat("Transforming '{0}'", fileInfo.Name);
                _contentTransformer.Section = section;
                _contentTransformer.Isbn = rtd.Isbn;

                var contentType = GetContentType(section);

                var transformResult =
                    _contentTransformer.Transform(contentType, ResourceAccess.Allowed, "",
                        false) as HtmlTransformResult;
                if (transformResult != null)
                {
                    rtd.TransferCount++;
                    var htmlFilePath = _contentTransformer.OutputFilename;
                    var modifyHtmlTime = ModifyHtmlFile(transformResult.Result, bookSearchInfo, fileInfo, htmlFilePath);
                    var renameTime = RenameHtmlFile(htmlFilePath, fileInfo.Name, isGlossary ? section : null);
                    _log.InfoFormat("Transformed '{0}' in {1} ms, modified in {2} ms, renamed in {3} ms, {4}",
                        fileInfo.Name, transformResult.TransformTime, modifyHtmlTime, renameTime, logData);
                    return true;
                }

                _log.ErrorFormat("Error transforming file: {0}", fileInfo.Name);
                rtd.Successful = false;
                rtd.StatusMessage = "Error transforming resource";
                rtd.AddError("Error transforming resource", fileInfo.Name);
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                rtd.AddError(ex.Message, fileInfo.Name);
                _log.WarnFormat("ERROR TRANSFORMING {0}, Exception: {1}", fileInfo.Name, ex.Message);
                return false;
            }
        }

        private IList<FileInfo> GetFileToTransform(string isbn)
        {
            var filesToTransform = new List<FileInfo>();
            try
            {
                var contentDirectoryName = $@"{XmlRootPath}\{isbn.Trim()}";

                var directoryInfo = new DirectoryInfo(contentDirectoryName);

                if (!directoryInfo.Exists)
                {
                    _log.WarnFormat("Content directory does not exist, {0}", contentDirectoryName);
                    return filesToTransform;
                }

                var fileInfos = directoryInfo.GetFiles();

                if (fileInfos.Length > 0)
                {
                    foreach (var fileInfo in fileInfos)
                    {
                        if (fileInfo.Name.StartsWith("sect1.") || fileInfo.Name.StartsWith("dedication.") ||
                            fileInfo.Name.StartsWith("appendix.") ||
                            fileInfo.Name.StartsWith("preface."))
                        {
                            filesToTransform.Add(fileInfo);
                        }
                    }
                }

                return filesToTransform;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        private long ModifyHtmlFile(string html, BookSearchInfo bookSearchInfo, FileInfo xmlFileInfo,
            string htmlFullFilePath)
        {
            var modifyHtmlStopwatch = new Stopwatch();
            modifyHtmlStopwatch.Start();

            var filePrefix = xmlFileInfo.Name.Split('.')[0];
            var docSearchInfo = new DocSearchInfo(bookSearchInfo, xmlFileInfo.FullName, filePrefix);

            var fullHtml = new StringBuilder()
                .AppendLine("<html><head>")
                .Append(docSearchInfo.MetaTags)
                .AppendLine("</head>")
                .AppendLine("<body>")
                .AppendLine("<!-- r2v2 content from transform -->")
                .AppendLine(html)
                .AppendLine("</body>")
                .AppendLine("</html>")
                .ToString();

            using (var outfile = new StreamWriter(htmlFullFilePath))
            {
                outfile.Write(fullHtml);
            }

            modifyHtmlStopwatch.Stop();
            //_log.InfoFormat("{0} modified in {1} ms", htmlFullFilePath, modifyHtmlStopwatch.ElapsedMilliseconds);
            return modifyHtmlStopwatch.ElapsedMilliseconds;
        }


        private static ContentType GetContentType(string section)
        {
            var typeCode = section.Substring(0, 2).ToLower();

            switch (typeCode)
            {
                case "ap":
                    return ContentType.Appendix;

                case "dd":
                case "de":
                    return ContentType.Dedication;

                case "pr":
                    return ContentType.Preface;

                case "gl":
                    return ContentType.Glossary;

                case "bi":
                    return ContentType.Bibliography;

                default:
                    return ContentType.Book;
            }
        }

        private long RenameHtmlFile(string htmlFile, string xmlFileName, string section)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var fileInfo = new FileInfo(htmlFile);

            if (!fileInfo.Exists)
            {
                throw new Exception($"File not found! '{htmlFile}'");
            }

            var newFileName = $@"{fileInfo.DirectoryName}\{(!string.IsNullOrWhiteSpace(section)
                ? xmlFileName.Replace(".xml", $".{section}.html")
                : xmlFileName.Replace(".xml", ".html"))}";

            if (newFileName == null)
            {
                throw new Exception($"XML File extension not found! '{htmlFile}'");
            }

            if (newFileName.Length == htmlFile.Length)
            {
                throw new Exception($"replace failed! '{htmlFile}' = '{newFileName}'");
            }

            fileInfo.MoveTo(newFileName);
            //_log.DebugFormat("renamed '{0}' --> '{1}'", htmlFile, newFileName);
            return stopwatch.ElapsedMilliseconds;
        }

        private int SaveAuthors(BookSearchInfo bookSearchInfo, int resourceId)
        {
            var order = 0;

            _resourceCoreDataService.DeleteResourceAuthors(resourceId, _r2UtilitiesSettings.AuthorTableName);

            var primary = bookSearchInfo.PrimaryAuthor;
            if (primary != null)
            {
                if (string.IsNullOrWhiteSpace(primary.LastName))
                {
                    _log.DebugFormat("empty primary author");
                }
                else
                {
                    order++;
                    _log.DebugFormat("Primary author: {0}", bookSearchInfo.PrimaryAuthor.GetFullName());
                    _resourceCoreDataService.InsertAuthor(resourceId, order, bookSearchInfo.PrimaryAuthor,
                        _r2UtilitiesSettings.AuthorTableName);
                    if (order == 1)
                    {
                        var sortAuthor =
                            $"{bookSearchInfo.PrimaryAuthor.LastName}, {bookSearchInfo.PrimaryAuthor.FirstName}";
                        _log.DebugFormat("sortAuthor: {0}", sortAuthor);
                        _resourceCoreDataService.UpdateResourceSortAuthor(resourceId, sortAuthor, "TransformService");
                    }
                }
            }

            foreach (var author in bookSearchInfo.OtherAuthors)
            {
                if (string.IsNullOrWhiteSpace(author.LastName))
                {
                    _log.DebugFormat("empty author");
                    continue;
                }

                if (primary != null && author.LastName == primary.LastName && author.FirstName == primary.FirstName &&
                    author.MiddleInitial == primary.MiddleInitial && author.Lineage == primary.Lineage &&
                    author.Degrees == primary.Degrees)
                {
                    _log.DebugFormat("author same as primary");
                    continue;
                }

                order++;
                _log.DebugFormat("other author: {0}", bookSearchInfo.PrimaryAuthor.GetFullName());
                _resourceCoreDataService.InsertAuthor(resourceId, order, author, _r2UtilitiesSettings.AuthorTableName);
                if (order == 1)
                {
                    var sortAuthor = $"{author.LastName}, {author.FirstName}";
                    _log.DebugFormat("sortAuthor: {0}", sortAuthor);
                    _resourceCoreDataService.UpdateResourceSortAuthor(resourceId, sortAuthor, "TransformService");
                }
            }

            return order;
        }
    }
}