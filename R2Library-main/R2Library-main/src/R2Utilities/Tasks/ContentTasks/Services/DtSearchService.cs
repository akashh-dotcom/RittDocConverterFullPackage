#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using dtSearch.Engine;
using NHibernate;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Threads;
using ResourceFile = R2Utilities.DataAccess.ResourceFile;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class DtSearchService : R2UtilitiesBase, IIndexStatusHandler
    {
        private const string IndexStatusCodeError = "E";
        private const string IndexStatusCodeProcessing = "P";
        private const string IndexStatusCodeIndexed = "I";

        private readonly IContentSettings _contentSettings;
        private readonly string _htmlRootPath;
        private readonly string _indexListFile;
        private readonly string _indexPath;
        private readonly IndexQueueDataService _indexQueueDataService;
        private readonly ILog<DtSearchService> _log;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly string _removalFilePath;
        private readonly ResourceFileDataService _resourceFileDataService;
        private readonly SearchService _searchService;

        private int _indexedFileCount;
        private List<IndexQueue> _indexQueues;
        private ResourceToIndex _resourceToIndex;

        private List<ResourceToIndex> _resourceToIndexList;

        public DtSearchService(IContentSettings contentSettings, IR2UtilitiesSettings r2UtilitiesSettings,
            SearchService searchService, ILog<DtSearchService> log)
        {
            _contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _searchService = searchService;
            _log = log;
            _indexQueueDataService = new IndexQueueDataService();

            _indexPath = _contentSettings.DtSearchIndexLocation;
            _htmlRootPath = _contentSettings.NewContentLocation;

            _resourceFileDataService = new ResourceFileDataService(r2UtilitiesSettings.ResourceFileTableName, true);

            _indexListFile = Path.Combine(_contentSettings.DtSearchIndexLocation, "docids.txt");
            _removalFilePath = Path.Combine(_contentSettings.DtSearchIndexLocation, "remove_docids.txt");
        }

        void IIndexStatusHandler.OnProgressUpdate(IndexProgressInfo info)
        {
            if (info.UpdateType != MessageCode.dtsnIndexFileDone)
            {
                return;
            }

            var resourceFile = new ResourceFile(info.File.DocId, info.File.DisplayName);
            _indexedFileCount++;

            if (_resourceToIndex == null || _resourceToIndex.IndexQueue.Isbn != resourceFile.Isbn)
            {
                Log.InfoFormat(
                    "Indexing ISBN: {0}, First file: {1}, DocId: {2}, Indexed {3} files in {4:#,###}s, {5}s remaining, {6}% complete",
                    resourceFile.Isbn, resourceFile.FilenameFull, resourceFile.DocumentId,
                    _indexedFileCount, info.ElapsedSeconds, info.EstRemainingSeconds, info.PercentDone);
                IList<ResourceToIndex> list = _resourceToIndexList.FindAll(x => x.IndexQueue.Isbn == resourceFile.Isbn);
                if (list.Any())
                {
                    if (list.Count > 1)
                    {
                        Log.WarnFormat("ISBN found multiple times: {0}, use first", list.Count());
                    }

                    _resourceToIndex = list.First();
                    _resourceToIndex.AddResourceFile(resourceFile);
                }
                else
                {
                    Log.WarnFormat("Can't find resource by ISBN {0}, {1}, {2}", resourceFile.FilenameFull,
                        resourceFile.Isbn, resourceFile.DocumentId);
                }
            }
            else
            {
                _resourceToIndex.AddResourceFile(resourceFile);
            }
        }

        /// <summary>
        /// </summary>
        AbortValue IIndexStatusHandler.CheckForAbort()
        {
            return AbortValue.Continue;
        }


        public void CreateDtSearchIndex()
        {
            try
            {
                var options = new Options();
                var indexDirectory = _indexPath;
                var directoryInfo = new DirectoryInfo(indexDirectory);

                if (!directoryInfo.Exists)
                {
                    options.MaxStoredFieldSize = 1024;
                    options.Save();
                    //options.FieldFlags

                    directoryInfo.Create();
                }

                var files = directoryInfo.GetFiles();
                if (!files.Any())
                {
                    // only create index if files don't exist
                    var indexJob = CreateIndexJob(true, true, false);
                    indexJob.ActionCreate = true;
                    indexJob.Execute();
                    Log.InfoFormat("DTSEARCH INDEX CREATED - {0}", indexDirectory);
                }

                var indexInfo = GetIndexStatus();
                Log.Info(indexInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        private IndexJob CreateIndexJob(bool actionCreate, bool actionAdd, bool actionCompress)
        {
            // http://support.dtsearch.com/webhelp/dtsearchnetapi2/frames.html?frmname=topic&frmfile=idx.html
            var indexJob = new IndexJob
            {
                IndexPath = _indexPath,
                CreateRelativePaths = true,
                ActionAdd = actionAdd,
                ActionCreate = actionCreate,
                ActionCompress = actionCompress,
                IndexingFlags =
                    IndexingFlags
                        .dtsIndexKeepExistingDocIds // When compressing an index, do not remap document ids, so document ids will be unmodified in the index once compression is done.
                    | IndexingFlags
                        .dtsIndexCacheText // Compress and store the text of documents in the index, for use in generating Search Reports and highlighting hits.
                    | IndexingFlags
                        .dtsIndexCreateRelativePaths // Use relative rather than absolute paths in storing document locations.
            };

            // dtSearch documentation - http://support.dtsearch.com/webhelp/dtSearchNetApi2/frames.html?frmname=topic&frmfile=dtSearch__Engine__IndexJob__EnumerableFields.html
            // All enumerable fields are also automatically designated as stored fields (see StoredFields).
            indexJob.EnumerableFields.AddRange(_r2UtilitiesSettings.IndexEnumerableFields);
            indexJob.StoredFields.AddRange(_r2UtilitiesSettings.IndexStoredFields);
            return indexJob;
        }

        public bool DoesDirectoryHaveFileToIndex(string isbn, out int directoryFileCount, out long directorySize)
        {
            directoryFileCount = 0;
            directorySize = 0;
            try
            {
                var contentDirectoryName = $@"{_htmlRootPath}\{isbn.Trim()}";

                var directoryInfo = new DirectoryInfo(contentDirectoryName);

                if (!directoryInfo.Exists)
                {
                    Log.WarnFormat("Content directory does not exist, {0}", contentDirectoryName);
                    return false;
                }

                var fileInfos = directoryInfo.GetFiles();

                var containsBookXml = false;
                var containsTocXml = false;

                long folderSize = 0;
                if (fileInfos.Length > 0)
                {
                    directoryFileCount += fileInfos.Length;

                    foreach (var fileInfo in fileInfos)
                    {
                        directorySize += fileInfo.Length;
                        folderSize += fileInfo.Length;

                        if (fileInfo.Name.StartsWith("book."))
                        {
                            containsBookXml = true;
                        }
                        else if (fileInfo.Name.StartsWith("toc."))
                        {
                            containsTocXml = true;
                        }
                    }
                }

                Log.DebugFormat(
                    "contentDirectoryName: {0}, file count: {1}, folder size: {2:0,000}, containsBookXml: {3}, containsTocXml: {4}",
                    contentDirectoryName, fileInfos.Length, folderSize, containsBookXml, containsTocXml);
                return fileInfos.Length > 0 && containsBookXml && containsTocXml;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        /// <param name="compressIndex"> </param>
        public bool AddDirectoriesToDtSearchIndex(IEnumerable<IndexQueue> indexQueues, bool compressIndex,
            ref int indexedResourceCount, ref int indexedDocumentCount)
        {
            _indexQueues = new List<IndexQueue>(indexQueues);
            _resourceToIndexList = new List<ResourceToIndex>();
            try
            {
                var batchFileCount = 0;
                var batchDocumentCount = 0;

                var indexJob = CreateIndexJob(false, true, compressIndex);
                foreach (var indexQueue in _indexQueues)
                {
                    var resourceToIndex = new ResourceToIndex(indexQueue);
                    _resourceToIndexList.Add(resourceToIndex);

                    var directoryPath = $@"{_htmlRootPath}\html\{indexQueue.Isbn.Trim()}";
                    var directoryInfo = new DirectoryInfo(directoryPath);
                    if (!directoryInfo.Exists)
                    {
                        Log.WarnFormat("DIRECTORY DOES NOT EXIST! path: {0}", directoryInfo);
                        indexQueue.IndexStatus = IndexStatusCodeError;
                        indexQueue.StatusMessage = $"DIRECTORY DOES NOT EXIST! path: {directoryInfo}";
                        continue;
                    }

                    var files = directoryInfo.GetFiles();
                    if (files.Length == 0)
                    {
                        Log.WarnFormat("DIRECTORY IS EMPTY! path: {0}", directoryInfo);
                        indexQueue.IndexStatus = IndexStatusCodeError;
                        indexQueue.StatusMessage = $"DIRECTORY IS EMPTY! path: {directoryInfo}";
                        continue;
                    }

                    Log.DebugFormat("{0} files in directory '{1}'", files.Length, directoryPath);

                    indexQueue.IndexStatus = IndexStatusCodeProcessing;

                    // touch files to force re-index
                    var lastModofiedDate = DateTime.Now;
                    foreach (var fileInfo in files)
                    {
                        var fullName = fileInfo.FullName;
                        Attempt.Execute(() => File.SetLastWriteTime(fullName, lastModofiedDate), 3, 3000);
                    }

                    var folderName = $"{directoryPath}<+>";
                    indexJob.FoldersToIndex.Add(folderName);
                    indexedResourceCount++;
                    indexedDocumentCount += files.Length;

                    batchFileCount++;
                    batchDocumentCount += files.Length;
                }

                _indexedFileCount = 0;
                indexJob.StatusHandler = this;

                Log.InfoFormat("Indexing {0} resources, {1:#,###} files", batchFileCount, batchDocumentCount);
                var jobStatus = indexJob.Execute();
                Log.InfoFormat("jobStatus: {0}", jobStatus);
                Log.InfoFormat("Indexing {0} total resources, {1:#,###} total files", indexedResourceCount,
                    indexedDocumentCount);
                return jobStatus;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public int SaveResourceFiles(TaskResult taskResult, DateTime batchStartTime)
        {
            var insertTimer = new Stopwatch();
            var resourceCount = 0;

            foreach (var resourceToIndex in _resourceToIndexList)
            {
                var indexQueue = resourceToIndex.IndexQueue;
                var docIdsStep = new TaskResultStep
                {
                    Name =
                        $"Updating doc ids for ISBN: {indexQueue.Isbn}, resource id: {indexQueue.ResourceId}, index queue id: {indexQueue.Id}",
                    StartTime = DateTime.Now
                };
                taskResult.AddStep(docIdsStep);

                resourceCount++;
                Log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                Log.InfoFormat("resourceCount: {0}, Id: {1}, ISBN: {2}", resourceCount, indexQueue.Id, indexQueue.Isbn);

                if (indexQueue.IndexStatus == IndexStatusCodeProcessing)
                {
                    insertTimer.Restart();
                    var successful = SaveDocumentIds(resourceToIndex);
                    insertTimer.Stop();

                    if (successful)
                    {
                        indexQueue.IndexStatus = IndexStatusCodeIndexed; // A=added, I=indexed
                        indexQueue.StatusMessage = "resource indexed successfully.";
                    }
                    else
                    {
                        indexQueue.IndexStatus = IndexStatusCodeError; // A=added, I=indexed, E = error
                        indexQueue.StatusMessage = "Updating tResourceFile failed.";
                    }
                }

                indexQueue.DateStarted = batchStartTime;
                indexQueue.DateFinished = DateTime.Now;
                _indexQueueDataService.Update(indexQueue);

                docIdsStep.CompletedSuccessfully = indexQueue.IndexStatus != IndexStatusCodeError;
                docIdsStep.EndTime = DateTime.Now;
                docIdsStep.Results = indexQueue.StatusMessage;

                var insertElapsed = insertTimer.ElapsedMilliseconds;

                var fileCount = indexQueue.LastDocumentId - indexQueue.FirstDocumentId + 1;

                var avgInsertTimePerFile = (double)insertElapsed / fileCount;
                Log.DebugFormat("insertElapsed: {0:0,000} ms, avgInsertTimePerFile: {1:0.000} ms", insertElapsed,
                    avgInsertTimePerFile);
                Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            }

            return resourceCount;
        }

        public bool CompressIndex()
        {
            try
            {
                var indexJob = CreateIndexJob(false, false, true);
                var jobStatus = indexJob.Execute();
                Log.InfoFormat("jobStatus: {0}", jobStatus);
                return jobStatus;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public void LoadDocumentIds(Resource resource, ISession session)
        {
            var searchResults = _searchService.PerformSearchByIsbn(resource.Isbn.Trim());
            var resourceFiles = new List<ResourceFile>();
            foreach (var searchResultItem in searchResults)
            {
                Log.DebugFormat("DocumnetId: {0}, Filename: {1}", searchResultItem.DocumnetId,
                    searchResultItem.DisplayName);
                var resourceFile =
                    new ResourceFile(searchResultItem.DocumnetId, searchResultItem.DisplayName, resource.Id);
                resourceFiles.Add(resourceFile);
            }

            _resourceFileDataService.InsertBatch(resourceFiles, _r2UtilitiesSettings.HtmlIndexerMaxIndexBatches);
        }

        public bool LoadDocumentIds(IndexQueue indexQueue)
        {
            IList<ISearchResultItem> searchResults = null;
            try
            {
                searchResults = _searchService.PerformSearchByIsbn(indexQueue.Isbn.Trim());
                var resourceFiles = new List<ResourceFile>();
                indexQueue.FirstDocumentId = 0;
                indexQueue.LastDocumentId = 0;
                foreach (var searchResultItem in searchResults)
                {
                    var resourceFile = new ResourceFile(searchResultItem.DocumnetId, searchResultItem.DisplayName,
                        indexQueue.ResourceId);

                    resourceFiles.Add(resourceFile);
                    if (resourceFile.DocumentId < indexQueue.FirstDocumentId || indexQueue.FirstDocumentId == 0)
                    {
                        indexQueue.FirstDocumentId = resourceFile.DocumentId;
                    }

                    if (resourceFile.DocumentId > indexQueue.LastDocumentId || indexQueue.LastDocumentId == 0)
                    {
                        indexQueue.LastDocumentId = resourceFile.DocumentId;
                    }
                }

                _resourceFileDataService.DeleteByResourceId(indexQueue.ResourceId);
                _resourceFileDataService.InsertBatch(resourceFiles, _r2UtilitiesSettings.ResourceFileInsertBatchSize);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                if (searchResults != null)
                {
                    foreach (var searchResultItem in searchResults)
                    {
                        Log.DebugFormat("DocumnetId: {0}, Filename: {1}", searchResultItem.DocumnetId,
                            searchResultItem.DisplayName);
                    }
                }

                return false;
            }
        }

        private bool SaveDocumentIds(ResourceToIndex resourceToIndex)
        {
            try
            {
                _resourceFileDataService.DeleteByResourceId(resourceToIndex.IndexQueue.ResourceId);
                _resourceFileDataService.InsertBatch(resourceToIndex.ResourceFiles,
                    _r2UtilitiesSettings.ResourceFileInsertBatchSize);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                foreach (var resourceFile in resourceToIndex.ResourceFiles)
                {
                    Log.WarnFormat("DocumentId: {0}, FilenameFull: {1}", resourceFile.DocumentId,
                        resourceFile.FilenameFull);
                }

                return false;
            }
        }

        public string GetIndexStatus()
        {
            var indexInfo = IndexJob.GetIndexInfo(_indexPath);

            var info = new StringBuilder()
                .AppendLine(">>>>>>>>>> INDEX STATUS:");

            info.AppendFormat("CompressedDate  : {0}", indexInfo.CompressedDate).AppendLine();
            info.AppendFormat("CreatedDate     : {0}", indexInfo.CreatedDate).AppendLine();
            info.AppendFormat("DocCount        : {0}", indexInfo.DocCount).AppendLine();
            info.AppendFormat("Fragmentation   : {0}", indexInfo.Fragmentation).AppendLine();
            info.AppendFormat("IndexSize       : {0}", indexInfo.IndexSize).AppendLine();
            info.AppendFormat("LastDocId       : {0}", indexInfo.LastDocId).AppendLine();
            info.AppendFormat("ObsoleteCount   : {0}", indexInfo.ObsoleteCount).AppendLine();
            info.AppendFormat("PercentFull     : {0}", indexInfo.PercentFull).AppendLine();
            info.AppendFormat("StartingDocId   : {0}", indexInfo.StartingDocId).AppendLine();
            info.AppendFormat("StructureVersion: {0}", indexInfo.StructureVersion).AppendLine();
            info.AppendFormat("UpdatedDate     : {0}", indexInfo.UpdatedDate).AppendLine();
            info.AppendFormat("WordCount       : {0}", indexInfo.WordCount).AppendLine();

            var alwaysAdd = (int)indexInfo.Flags & (int)IndexingFlags.dtsAlwaysAdd;
            var checkDiskSpace = (int)indexInfo.Flags & (int)IndexingFlags.dtsCheckDiskSpace;
            var indexCacheOriginalFile = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCacheOriginalFile;
            var indexCacheText = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCacheText;
            var indexCacheTextWithoutFields = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCacheTextWithoutFields;
            var indexCreateAccentSensitive = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCreateAccentSensitive;
            var indexCreateCaseSensitive = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCreateCaseSensitive;
            var indexCreateRelativePaths = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCreateRelativePaths;
            var indexCreateVersion6 = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexCreateVersion6;
            var indexKeepExistingDocIds = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexKeepExistingDocIds;
            var indexIndexResumeUpdate = (int)indexInfo.Flags & (int)IndexingFlags.dtsIndexResumeUpdate;

            var indexFlags = new StringBuilder();
            indexFlags.AppendFormat("alwaysAdd: {0}", alwaysAdd);
            indexFlags.AppendFormat(", checkDiskSpace: {0}", checkDiskSpace);
            indexFlags.AppendFormat(", indexCacheOriginalFile: {0}", indexCacheOriginalFile);
            indexFlags.AppendFormat(", indexCacheText: {0}", indexCacheText);
            indexFlags.AppendFormat(", indexCacheTextWithoutFields: {0}", indexCacheTextWithoutFields);
            indexFlags.AppendFormat(", indexCreateAccentSensitive: {0}", indexCreateAccentSensitive);
            indexFlags.AppendFormat(", indexCreateCaseSensitive: {0}", indexCreateCaseSensitive);
            indexFlags.AppendFormat(", indexCreateRelativePaths: {0}", indexCreateRelativePaths);
            indexFlags.AppendFormat(", indexCreateVersion6: {0}", indexCreateVersion6);
            indexFlags.AppendFormat(", indexKeepExistingDocIds: {0}", indexKeepExistingDocIds);
            indexFlags.AppendFormat(", indexIndexResumeUpdate: {0}", indexIndexResumeUpdate);

            info.AppendFormat("Flags           : {0} - [{1}]", indexInfo.Flags, indexFlags).AppendLine();

            info.AppendLine("<<<<<<<<<< INDEX STATUS");

            _log.Info(info.ToString());
            return info.ToString();
        }

        public int GetIndexFragmentationStatus()
        {
            var indexInfo = IndexJob.GetIndexInfo(_indexPath);

            var fragmentationPercentage = (int)indexInfo.Fragmentation;

            Log.DebugFormat("fragmentationPercentage: {0}", fragmentationPercentage);

            return fragmentationPercentage;
        }

        public void CreateListIndexFile()
        {
            var listIndexJob = new ListIndexJob
            {
                IndexPath = _indexPath,
                ListIndexFlags = ListIndexFlags.dtsListIndexFiles | ListIndexFlags.dtsListIndexIncludeDocId,
                //ListIndexFlags = ListIndexFlags.dtsListIndexWords,
                OutputToString = false,
                OutputFile = _indexListFile
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            listIndexJob.Execute();
            stopwatch.Stop();
            Log.DebugFormat("listIndexJob.Execute() took {0:#,###} ms", stopwatch.ElapsedMilliseconds);
        }

        public IDictionary<string, DocIds> GetResourceDocIdsFromIndexList()
        {
            var resourcesDocIds = new Dictionary<string, DocIds>();

            var counter = 0;
            var validPathCount = 0;
            var invalidPathCount = 0;

            // Read the file and display it line by line.
            using (var file = new StreamReader(_indexListFile))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    counter++;

                    var parts = line.Trim().Split(' ');

                    var docId = int.Parse(parts[0]);

                    var filePathParts = parts[1].Split('\\');
                    var isbn = filePathParts[filePathParts.Length - 2];
                    //Log.DebugFormat("docId: {0}, isbn: {1}", docId, isbn);

                    DocIds docIds;

                    var filename = filePathParts.Last();

                    var docIdFilename = new DocIdFilename
                    {
                        Id = docId,
                        Name = filename,
                        IsInvalidPath = !parts[1].StartsWith(_contentSettings.NewContentLocation,
                            StringComparison.CurrentCultureIgnoreCase)
                    };

                    if (docIdFilename.IsInvalidPath)
                    {
                        invalidPathCount++;
                        Log.WarnFormat("Invalid Path: {0}", parts[1]);
                    }
                    else
                    {
                        validPathCount++;
                    }

                    if (resourcesDocIds.ContainsKey(isbn))
                    {
                        docIds = resourcesDocIds[isbn];
                        if (docIds.MaximumDocId < docId)
                        {
                            docIds.MaximumDocId = docId;
                        }
                        else if (docIds.MinimumDocId > docId)
                        {
                            docIds.MinimumDocId = docId;
                        }
                    }
                    else
                    {
                        Log.DebugFormat("line: {0} --> {1}", counter, line);
                        docIds = new DocIds { Isbn = isbn, MaximumDocId = docId, MinimumDocId = docId };
                        resourcesDocIds.Add(isbn, docIds);
                    }

                    docIds.Filenames.Add(docIdFilename);
                }

                file.Close();
            }

            Log.InfoFormat("file: {0}, line count: {1}, valid path count: {2}, invalid path count: {3}", _indexListFile,
                counter, validPathCount, invalidPathCount);
            return resourcesDocIds;
        }

        public int RemoveDocumentIds(DocIds docIds, int resourceId)
        {
            try
            {
                GenerateRemovalFile(docIds);
                var ids = docIds.Filenames.Select(x => x.Id).ToArray();

                var jobStatus = RemoveDocumentIds();

                if (jobStatus)
                {
                    //_resourceFileDataService.DeleteByResourceId(resourceId);
                    _resourceFileDataService.DeleteBatch(ids, resourceId);
                    //jobStatus = (rowsDeleted == docIds.Filenames.Count);
                }

                return jobStatus ? docIds.Filenames.Count : -1;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public bool RemoveDocumentIds(string docIdsToRemove)
        {
            try
            {
                File.WriteAllText(_removalFilePath, docIdsToRemove);

                var jobStatus = RemoveDocumentIds();

                return jobStatus;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }


        private void GenerateRemovalFile(DocIds docIds)
        {
            using (var streamWriter = new StreamWriter(_removalFilePath, false))
            {
                foreach (var filename in docIds.Filenames)
                {
                    streamWriter.WriteLine(">{0}", filename.Id);
                }
            }
        }

        private bool RemoveDocumentIds(bool deleteFile = true)
        {
            var indexJob = CreateIndexJob(false, false, false);
            indexJob.ActionRemoveListed = true;

            indexJob.ToRemoveListName = _removalFilePath;

            var jobStatus = indexJob.Execute();

            if (!jobStatus)
            {
                Log.Error("RemoveDocumentIds indexjob failed! Errors:");
                for (var i = 0; i < indexJob.Errors.Count; i++)
                {
                    Log.ErrorFormat("{0}", indexJob.Errors.Message(i));
                }
            }
            else if (deleteFile)
            {
                var newFilename = Path.Combine(_contentSettings.DtSearchIndexLocation,
                    $"remove_docids_{DateTime.Now:yyyyMMdd-HHmmss}.txt");
                File.Move(_removalFilePath, newFilename);
            }

            return jobStatus;
        }

        public void CleanupDocIds(TaskResult taskResult, DateTime batchStartTime)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var indexedResources = _resourceToIndexList.Where(x => x.IndexQueue.IndexStatus == IndexStatusCodeIndexed)
                .ToList();

            var cleanupStep = new TaskResultStep
            {
                Name = $"Cleanup Doc Ids for {indexedResources.Count} resources",
                StartTime = DateTime.Now
            };
            taskResult.AddStep(cleanupStep);

            try
            {
                var badDocIdCount = 0;
                var docIdsToRemove = new StringBuilder();

                if (indexedResources.Any())
                {
                    Log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    Log.InfoFormat("Creating List Index File: {0}", _indexListFile);

                    CreateListIndexFile();

                    Log.InfoFormat("Reading file: {0}", _indexListFile);
                    var docIdsList = GetResourceDocIdsFromIndexList();

                    Log.Info("Finding bad doc ids ...");
                    foreach (var resourceToIndex in indexedResources)
                    {
                        var docIds = docIdsList[resourceToIndex.IndexQueue.Isbn];
                        foreach (var filename in docIds.Filenames)
                        {
                            var resourceFile =
                                resourceToIndex.ResourceFiles.FirstOrDefault(x => x.FilenameFull.Equals(filename.Name));
                            if (resourceFile != null)
                            {
                                continue;
                            }

                            Log.DebugFormat(
                                "Indexed file not in tResourceFile, Doc Id: {0}, Filename: {1}, IsInvalidPath: {2}",
                                filename.Id, filename.Name, filename.IsInvalidPath);
                            docIdsToRemove.AppendFormat(">{0}", filename.Id).AppendLine();
                            badDocIdCount++;
                        }
                    }
                }

                if (badDocIdCount > 0)
                {
                    Log.InfoFormat("Removing {0} doc ids from index ...", badDocIdCount);
                    RemoveDocumentIds(docIdsToRemove.ToString());
                    stopwatch.Stop();
                    Log.InfoFormat("Removed {0} doc ids from index in {1:#,###} ms", badDocIdCount,
                        stopwatch.ElapsedMilliseconds);
                    cleanupStep.Results =
                        $"Removed {badDocIdCount} doc ids from index in {stopwatch.ElapsedMilliseconds:#,###} ms";
                }
                else
                {
                    Log.Info("No bad doc ids found!");
                    cleanupStep.Results = "No bad doc ids found!";
                }

                cleanupStep.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                cleanupStep.CompletedSuccessfully = false;
                cleanupStep.Results = $"EXCEPTION: {ex.Message}";
                Log.ErrorFormat(ex.Message, ex);
            }
            finally
            {
                stopwatch.Stop();
                cleanupStep.EndTime = DateTime.Now;
            }
        }
    }
}