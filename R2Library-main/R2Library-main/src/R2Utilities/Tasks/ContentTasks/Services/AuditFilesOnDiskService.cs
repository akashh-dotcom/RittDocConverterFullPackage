#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using log4net;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Infrastructure.Compression;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class AuditFilesOnDiskService
    {
        private const string RootNodeTocFront = "tocfront";
        private const string RootNodeTocChapter = "tocchap";
        private const string RootNodeTocBack = "tocback";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        private readonly IContentSettings _contentSettings;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly TransformQueueDataService _transformQueueDataService;

        public AuditFilesOnDiskService(IContentSettings contentSettings, IR2UtilitiesSettings r2UtilitiesSettings)
        {
            _contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _transformQueueDataService = new TransformQueueDataService();
            ResourcesWithoutTocXml = new List<ResourceCore>();
            ValidatedResources = new List<ResourceCore>();
            ErrorResources = new List<ResourceCore>();
            ResourcesWithExtraContent = new List<ResourceCore>();
            ResourcesMissingContent = new List<ResourceCore>();
            FilesToDelete = new List<string>();
            MissingFiles = new List<string>();
            BookXmlFilesConfirmed = 0;

            AuditFilesOnDiskResults = new List<AuditFilesOnDiskResult>();
        }

        public List<ResourceCore> ResourcesWithoutTocXml { get; }
        public List<ResourceCore> ValidatedResources { get; }
        public List<ResourceCore> ErrorResources { get; }
        public List<ResourceCore> ResourcesWithExtraContent { get; }
        public List<ResourceCore> ResourcesMissingContent { get; }
        public List<string> FilesToDelete { get; }
        public List<string> MissingFiles { get; }
        public int BookXmlFilesConfirmed { get; private set; }

        public List<AuditFilesOnDiskResult> AuditFilesOnDiskResults { get; }

        public AuditFilesOnDiskResult AuditFilesOnDisk(ResourceCore resourceCore)
        {
            var result = new AuditFilesOnDiskResult { ResourceCore = resourceCore };
            AuditFilesOnDiskResults.Add(result);
            try
            {
                var resourceXmlDirectory = Path.Combine(_contentSettings.ContentLocation, resourceCore.Isbn);
                Log.DebugFormat("resourceXmlDirectory: {0}", resourceXmlDirectory);

                var tocXml = Path.Combine(resourceXmlDirectory, $"toc.{resourceCore.Isbn}.xml");
                Log.DebugFormat("tocXml: {0}", tocXml);

                var fileInfo = new FileInfo(tocXml);
                if (!fileInfo.Exists)
                {
                    ResourcesWithoutTocXml.Add(resourceCore);
                    result.ResourcesWithoutTocXml = true;
                    return result;
                }

                var xmlFiles = new List<string>();

                var tocModifiedDate = fileInfo.LastWriteTime;

                var xmlDoc = new XmlDocument { PreserveWhitespace = false, XmlResolver = null };

                xmlDoc.Load(tocXml);

                // tocfront
                var tocFrontNodes = xmlDoc.GetElementsByTagName(RootNodeTocFront);
                Log.DebugFormat("tocFrontNodes.Count: {0}", tocFrontNodes.Count);
                foreach (XmlNode tocFrontNode in tocFrontNodes)
                {
                    if (tocFrontNode.Attributes != null)
                    {
                        var attribute = tocFrontNode.Attributes["linkend"];
                        if (!string.IsNullOrWhiteSpace(attribute.Value))
                        {
                            var xmlFilename = BuildXmlFilename(resourceCore.Isbn, attribute.Value, RootNodeTocFront);
                            //Log.DebugFormat("xmlFilename: {0}", xmlFilename);
                            xmlFiles.Add(xmlFilename);
                        }
                    }
                }

                // tocchap
                var tocChapNodes = xmlDoc.GetElementsByTagName(RootNodeTocChapter);
                Log.DebugFormat("tocChapNodes.Count: {0}", tocChapNodes.Count);
                foreach (XmlNode tocChapNode in tocChapNodes)
                {
                    foreach (XmlNode childNode in tocChapNode.ChildNodes)
                    {
                        if (childNode.Name.Equals("toclevel1", StringComparison.OrdinalIgnoreCase))
                        {
                            var tocentry = childNode.FirstChild;
                            if (tocentry.Attributes != null)
                            {
                                var attribute = tocentry.Attributes["linkend"];
                                var xmlFilename = BuildXmlFilename(resourceCore.Isbn, attribute.Value,
                                    RootNodeTocChapter);
                                //Log.DebugFormat("xmlFilename: {0}", xmlFilename);
                                xmlFiles.Add(xmlFilename);
                            }
                        }
                    }
                }

                // tocback
                var tocBackNodes = xmlDoc.GetElementsByTagName(RootNodeTocBack);
                Log.DebugFormat("tocBackNodes.Count: {0}", tocBackNodes.Count);
                foreach (XmlNode tocBackNode in tocBackNodes)
                {
                    if (tocBackNode.Attributes != null && tocBackNode.Attributes["linkend"] != null)
                    {
                        var attribute = tocBackNode.Attributes["linkend"];
                        var xmlFilename = BuildXmlFilename(resourceCore.Isbn, attribute.Value, RootNodeTocBack);
                        //Log.DebugFormat("xmlFilename: {0}", xmlFilename);
                        xmlFiles.Add(xmlFilename);
                    }

                    foreach (XmlNode childNode in tocBackNode.ChildNodes)
                    {
                        if (childNode.Name.Equals("tocentry", StringComparison.OrdinalIgnoreCase))
                        {
                            if (childNode.Attributes != null)
                            {
                                var attribute = childNode.Attributes["linkend"];
                                var xmlFilename = BuildXmlFilename(resourceCore.Isbn, attribute.Value, RootNodeTocBack);
                                //Log.DebugFormat("xmlFilename: {0}", xmlFilename);
                                xmlFiles.Add(xmlFilename);
                            }
                        }
                    }
                }

                VerifyFiles(result, resourceXmlDirectory, xmlFiles, tocModifiedDate);
                if (result.ContainsExtraXmlFile)
                {
                    ResourcesWithExtraContent.Add(resourceCore);
                }
                else if (result.MissingXmlFile)
                {
                    ResourcesMissingContent.Add(resourceCore);
                }
                else
                {
                    ValidatedResources.Add(resourceCore);
                }
            }
            catch (Exception ex)
            {
                ErrorResources.Add(resourceCore);
                Log.Error(ex.Message, ex);
                result.ExceptionWhileProcessing = true;
                result.ExceptionMessage = ex.Message;
                result.ExceptionStackTrace = ex.StackTrace;
            }

            return result;
        }

        private void VerifyFiles(AuditFilesOnDiskResult result, string contentDirectoryPath,
            List<string> filesFromTocXml, DateTime tocModifiedDate)
        {
            var xmlFilesOnDisk = Directory.GetFiles(contentDirectoryPath);

            result.FilesReferencedInTocXmlCount = filesFromTocXml.Count;
            result.FilesOnDiskCount = xmlFilesOnDisk.Length;

            var tocXmlHashSet = new HashSet<string>(filesFromTocXml);

            foreach (var xmlFilePath in xmlFilesOnDisk)
            {
                if (xmlFilePath.Contains(@"\toc.") || xmlFilePath.Contains(@"\book."))
                {
                    continue;
                }

                var isXmlFileInToc = tocXmlHashSet.Contains(Path.GetFileName(xmlFilePath));
                if (!isXmlFileInToc)
                {
                    Log.WarnFormat("File not in toc.xml: {0}", xmlFilePath);
                    result.FilesNotInTocXmlCount++;

                    var xmlFileDateModified = new FileInfo(xmlFilePath).LastWriteTime;
                    var dateWindowExceeded = IsDateWindowExceeded(xmlFileDateModified, tocModifiedDate);
                    if (dateWindowExceeded)
                    {
                        Log.WarnFormat("\tFile dates out of sync - Toc DateModified: {0}, Xml DateModified: {1}",
                            tocModifiedDate, xmlFileDateModified);
                        result.FilesDateDiffersFromTocCount++;

                        FilesToDelete.Add(xmlFilePath);
                        result.FilesToDeleteCount++;
                    }
                    else
                    {
                        Log.Info("\tFile date in sync with Toc");
                    }
                }
            }

            var xmlFileNamesOnDiskHashSet = new HashSet<string>(xmlFilesOnDisk.Select(Path.GetFileName));

            foreach (var xmlFileName in filesFromTocXml)
            {
                var isXmlFileOnDisk = xmlFileNamesOnDiskHashSet.Contains(xmlFileName);
                //string xmlFile = xmlFilesOnDisk.FirstOrDefault(x => x.EndsWith(xmlFileName));
                if (!isXmlFileOnDisk)
                {
                    Log.WarnFormat("File not on disk: {0}", xmlFileName);
                    result.FilesNotOnDiskCount++;
                    MissingFiles.Add(xmlFileName);
                }
                else
                {
                    result.FilesConfirmedInTocXmlCount++;
                }
            }

            BookXmlFilesConfirmed += result.FilesConfirmedInTocXmlCount;
            Log.InfoFormat(
                "FilesNotInTocXmlCount: {0}, FilesDateDiffersFromTocCount: {1}, FilesToDeleteCount: {2}, FilesNotOnDiskCount: {3}, FilesConfirmedInTocXmlCount: {4}",
                result.FilesNotInTocXmlCount, result.FilesDateDiffersFromTocCount, result.FilesToDeleteCount,
                result.FilesNotOnDiskCount, result.FilesConfirmedInTocXmlCount);
        }

        private bool IsDateWindowExceeded(DateTime xmlFileDateModified, DateTime tocDateModified)
        {
            DateTime startTime;
            DateTime endTime;

            if (xmlFileDateModified < tocDateModified)
            {
                startTime = xmlFileDateModified;
                endTime = tocDateModified;
            }
            else
            {
                startTime = tocDateModified;
                endTime = xmlFileDateModified;
            }

            var duration = endTime.Subtract(startTime);
            var xmlDateModifiedWindow = new TimeSpan(0, 0, 0, _r2UtilitiesSettings.AuditXmlDateModifiedWindowInSeconds);

            return duration > xmlDateModifiedWindow;
        }


        public int BackupResourcesWithExtraContent()
        {
            var resourcesBackedUp = 0;
            foreach (var resourceCore in ResourcesWithExtraContent)
            {
                try
                {
                    var resourceXmlDirectory = Path.Combine(_contentSettings.ContentLocation, resourceCore.Isbn);
                    var zipFileName = Path.Combine(_r2UtilitiesSettings.AuditFilesOnDiskBackupDirectory,
                        $"{resourceCore.Isbn}.zip");
                    ZipHelper.CompressDirectory(resourceXmlDirectory, zipFileName);

                    _transformQueueDataService.Insert(resourceCore.Id, resourceCore.Isbn, "A");

                    resourcesBackedUp++;
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Error backing up / adding to transform queue - {0}", resourceCore.ToDebugString());
                    Log.Error(ex.Message, ex);
                }
            }

            return resourcesBackedUp;
        }

        public int DeleteResourceFilesNotInBookXml()
        {
            var filesDeleted = 0;
            foreach (var fileToDelete in FilesToDelete)
            {
                try
                {
                    Log.DebugFormat("deleting file: {0}", fileToDelete);
                    File.Delete(fileToDelete);
                    filesDeleted++;
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Error deleting file: {0}", fileToDelete);
                    Log.Error(ex.Message, ex);
                }
            }

            return filesDeleted;
        }

        public int ResourceBackups(string isbns)
        {
            var limitByIsbns = isbns == null ? new string[0] : isbns.Split(',');
            var zipFilesToRestore = new List<FileInfo>();

            var backupZipFiles = Directory.GetFiles(_r2UtilitiesSettings.AuditFilesOnDiskBackupDirectory, "*.zip");

            foreach (var backupZipFile in backupZipFiles)
            {
                Log.DebugFormat("backupZipFile: {0}", backupZipFile);

                var fileInfo = new FileInfo(backupZipFile);

                var isbn = GetIsbnFromZipFileInfo(fileInfo);

                if (limitByIsbns.Length > 0)
                {
                    if (limitByIsbns.Contains(isbn))
                    {
                        zipFilesToRestore.Add(fileInfo);
                    }
                }
                else
                {
                    zipFilesToRestore.Add(fileInfo);
                }
            }

            var resourcesRestored = 0;
            foreach (var zipFileInfo in zipFilesToRestore)
            {
                Log.InfoFormat("Restoring backup {0} of {1} - {2}", resourcesRestored + 1, zipFilesToRestore.Count,
                    zipFileInfo.Name);
                var isbn = GetIsbnFromZipFileInfo(zipFileInfo);

                var resourceXmlDirectory = Path.Combine(_contentSettings.ContentLocation, isbn);

                ZipHelper.ExtractAll(zipFileInfo.FullName, resourceXmlDirectory, true);

                resourcesRestored++;
            }


            return resourcesRestored;
        }

        private string GetIsbnFromZipFileInfo(FileInfo zipFileInfo)
        {
            var isbn = zipFileInfo.Name.Replace(".zip", "");
            return isbn;
        }

        public string BuildXmlFilename(string isbn, string linkend, string rootNodeName)
        {
            if (string.IsNullOrWhiteSpace(linkend) || linkend.Length < 2)
            {
                Log.ErrorFormat("linkend is null, empty, whitespace, less than 2 characters - linkend: '{0}'", linkend);
                return $"book.{isbn}.xml";
            }

            var prefix = linkend.Substring(0, 2);

            switch (prefix)
            {
                case "pr":
                    if (rootNodeName.Equals(RootNodeTocFront))
                    {
                        return $"preface.{isbn}.{linkend}.xml";
                    }

                    if (rootNodeName.Equals(RootNodeTocChapter))
                    {
                        return $"sect1.{isbn}.{linkend}.xml";
                    }

                    Log.ErrorFormat("'pr' PREFIX NOT SUPPORTED for root node name: '{2}' - isbn: {0}, linked: {1}",
                        isbn, linkend, rootNodeName);
                    return $"_missing.{isbn}.{linkend}.xml";

                case "dd":
                    return $"dedication.{isbn}.{linkend}.xml";

                case "ap":
                    if (linkend.Length <= 7)
                    {
                        return $"appendix.{isbn}.{linkend}.xml";
                    }

                    if (linkend.Length > 7)
                    {
                        return $"sect1.{isbn}.{linkend}.xml";
                    }

                    Log.ErrorFormat("'ap' PREFIX NOT SUPPORTED for root node name: '{2}' - isbn: {0}, linked: {1}",
                        isbn, linkend, rootNodeName);
                    return $"_missing.{isbn}.{linkend}.xml";

                case "ch":
                case "pt":
                case "s0":
                case "ci":
                case "p2":
                case "s2":
                    return $"sect1.{isbn}.{linkend}.xml";

                default:
                    if (linkend.StartsWith("bibs", StringComparison.InvariantCultureIgnoreCase) ||
                        linkend.StartsWith("PTE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return $"sect1.{isbn}.{linkend}.xml";
                    }

                    if (linkend.StartsWith("ded", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return $"dedication.{isbn}.{linkend}.xml";
                    }

                    // all of the permutations are listed below to document all the different types, logically they are all not needed.
                    if (linkend.StartsWith("glossary", StringComparison.InvariantCultureIgnoreCase) ||
                        linkend.StartsWith("gl", StringComparison.InvariantCultureIgnoreCase) ||
                        linkend.StartsWith("in", StringComparison.InvariantCultureIgnoreCase) ||
                        linkend.StartsWith("biblio", StringComparison.InvariantCultureIgnoreCase) ||
                        linkend.StartsWith("bibli", StringComparison.InvariantCultureIgnoreCase) ||
                        linkend.StartsWith("bib", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.InfoFormat("Resolve to book.xml - isbn: {0}, linked: {1}", isbn, linkend);
                        return $"book.{isbn}.xml";
                    }

                    Log.ErrorFormat("PREFIX NOT SUPPORTED for root node name: '{2}' - isbn: {0}, linked: {1}", isbn,
                        linkend, rootNodeName);
                    return $"_missing.{isbn}.{linkend}.xml";
            }
        }
    }
}