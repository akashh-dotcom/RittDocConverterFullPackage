#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Settings;
using ResourceFile = R2Utilities.DataAccess.ResourceFile;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    /// <summary>
    ///     -VerifyDocIdsTask -maxResources=10000 -isbns= -resourceFileTableName=tResourceTableTemp -logResourceFileSql=true
    ///     -truncateAndReloadResourceFileTable=false
    ///     -generateDtSearchListIndexFile=false -addBadResourcesToTransformQueue=false -removeBadResourcesFromIndex=false
    ///     -removeBadDatabaseDocIds=false -fixDocIdsInDb=true
    ///     -VerifyDocIdsTask -maxResources=10000 -isbns=  -resourceFileTableName=tResourceFileTemp -logResourceFileSql=true
    ///     -truncateAndReloadResourceFileTable=false -generateDtSearchListIndexFile=false
    ///     -addBadResourcesToTransformQueue=false -removeBadResourcesFromIndex=false -removeBadDatabaseDocIds=false
    ///     -fixDocIdsInDb=true -minResourceId=6001 -maxResourceId=7000
    ///     -VerifyDocIdsTask -maxResources=1000 -isbns= -resourceFileTableName=tResourceFile -logResourceFileSql=true
    ///     -truncateAndReloadResourceFileTable=false -generateDtSearchListIndexFile=true
    ///     -addBadResourcesToTransformQueue=false -removeBadResourcesFromIndex=true -removeBadDatabaseDocIds=true
    ///     -fixDocIdsInDb=true -minResourceId=5001 -maxResourceId=5900
    ///     -VerifyDocIdsTask -maxResources=1000 -isbns= -resourceFileTableName=tResourceFile -logResourceFileSql=true
    ///     -truncateAndReloadResourceFileTable=false -generateDtSearchListIndexFile=true
    ///     -addBadResourcesToTransformQueue=false -removeBadResourcesFromIndex=false -removeBadDatabaseDocIds=false
    ///     -fixDocIdsInDb=false -minResourceId=5001 -maxResourceId=5900
    ///     -VerifyDocIdsTask -maxResources=1000 -isbns=0470658665,1107018862,1118448855 -resourceFileTableName=tResourceFile
    ///     -logResourceFileSql=true -truncateAndReloadResourceFileTable=false -generateDtSearchListIndexFile=true
    ///     -addBadResourcesToTransformQueue=true -removeBadResourcesFromIndex=true -removeBadDatabaseDocIds=true
    ///     -fixDocIdsInDb=true -minResourceId=5001 -maxResourceId=5800
    /// </summary>
    public class VerifyDocIdsTask : TaskBase, ITask
    {
        private readonly IContentSettings _contentSettings;
        private readonly DtSearchService _dtSearchService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ResourceCoreDataService _resourceCoreDataService;
        private readonly TransformQueueDataService _transformQueueDataService;
        private bool _addBadResourcesToTransformQueue;
        private bool _fixDocIdsInDb;
        private bool _generateDtSearchListIndexFile; // this should be set to true in production
        private string _isbns;
        private bool _logResourceFileSql;
        private int _maxResourceId;
        private int _maxResources = 20000;
        private int _minResourceId;
        private bool _removeBadDatabaseDocIds;
        private bool _removeBadResourcesFromIndex;
        private ResourceFileDataService _resourceFileDataService;
        private string _resourceFileTableName;
        private int _totalFilesRemovedFromIndexCount;
        private long _totalHtmlDirectorySizeInBytes;
        private int _totalHtmlFileCount;

        private int _totalIndexFileCount;
        private int _totalResourcesRemovedFromIndexCount;
        private long _totalXmlDirectorySizeInBytes;
        private int _totalXmlFileCount;

        private bool _truncateAndReloadResourceFileTable; // this should never be used in production

        protected new string TaskName = "VerifyDocIdsTask";

        public VerifyDocIdsTask(IR2UtilitiesSettings r2UtilitiesSettings, DtSearchService dtSearchService,
            IContentSettings contentSettings)
            : base("VerifyDocIdsTask", "-VerifyDocIdsTask", "24", TaskGroup.DiagnosticsMaintenance,
                "Task to verify the DocIds in the db are the same as DocIds in index", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _dtSearchService = dtSearchService;
            _contentSettings = contentSettings;
            _transformQueueDataService = new TransformQueueDataService();
            _resourceCoreDataService = new ResourceCoreDataService();
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _maxResources = GetArgumentInt32("maxResources", 100000);
            _isbns = GetArgument("isbns");
            _resourceFileTableName = GetArgument("resourceFileTableName") ?? _r2UtilitiesSettings.ResourceFileTableName;
            _logResourceFileSql = GetArgumentBoolean("logResourceFileSql", true);
            _resourceFileDataService = new ResourceFileDataService(_resourceFileTableName, _logResourceFileSql);

            _minResourceId = GetArgumentInt32("minResourceId", 0);
            _maxResourceId = GetArgumentInt32("maxResourceId", 100000);

            Log.InfoFormat(">>> _maxResources: {0}, _isbns: {1}, _resourceFileTableName: {2}", _maxResources, _isbns,
                _resourceFileTableName);
            Log.InfoFormat(">>> _minResourceId: {0}, _maxResourceId: {1}", _minResourceId, _maxResourceId);

            _truncateAndReloadResourceFileTable = GetArgumentBoolean("truncateAndReloadResourceFileTable", false);
            _generateDtSearchListIndexFile = GetArgumentBoolean("generateDtSearchListIndexFile", false);
            _addBadResourcesToTransformQueue = GetArgumentBoolean("addBadResourcesToTransformQueue", false);
            _removeBadResourcesFromIndex = GetArgumentBoolean("removeBadResourcesFromIndex", false);
            _removeBadDatabaseDocIds = GetArgumentBoolean("removeBadDatabaseDocIds", false);
            _fixDocIdsInDb = GetArgumentBoolean("fixDocIdsInDb", false);

            Log.InfoFormat(">>> _truncateAndReloadResourceFileTable: {0}, _generateDtSearchListIndexFile: {1}",
                _truncateAndReloadResourceFileTable, _generateDtSearchListIndexFile);
            Log.InfoFormat(">>> _removeBadResourcesFromIndex: {0}, _removeBadDatabaseDocIds: {1}",
                _removeBadResourcesFromIndex, _removeBadDatabaseDocIds);
            Log.InfoFormat(">>> _addBadResourcesToTransformQueue: {0}, _logResourceFileSql: {1}, _fixDocIdsInDb: {2}",
                _addBadResourcesToTransformQueue, _logResourceFileSql, _fixDocIdsInDb);

            SetSummaryEmailSetting(false, true, 10);
        }


        public override void Run()
        {
            var firstStepResults = new StringBuilder();
            firstStepResults.AppendFormat("_maxResources: {0}, _isbns: {1}, _resourceFileTableName: {2}", _maxResources,
                _isbns, _resourceFileTableName);
            firstStepResults.AppendFormat("_minResourceId: {0}, _maxResourceId: {1}", _minResourceId, _maxResourceId);
            firstStepResults.AppendFormat(
                "_truncateAndReloadResourceFileTable: {0}, _generateDtSearchListIndexFile: {1}",
                _truncateAndReloadResourceFileTable, _generateDtSearchListIndexFile);
            firstStepResults.AppendFormat("_removeBadResourcesFromIndex: {0}, _removeBadDatabaseDocIds: {1}",
                _removeBadResourcesFromIndex, _removeBadDatabaseDocIds);
            firstStepResults.AppendFormat(
                "_addBadResourcesToTransformQueue: {0}, _logResourceFileSql: {1}, _fixDocIdsInDb: {2}",
                _addBadResourcesToTransformQueue, _logResourceFileSql, _fixDocIdsInDb);

            TaskResult.Information =
                "This task will validate the document id within the database (tResourceFile) are the same document ids that are in the dtSearch index.";
            var step = new TaskResultStep
                { Name = "VerifyDocIdsTask", StartTime = DateTime.Now, Results = firstStepResults.ToString() };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var totalRunTime = new Stopwatch();
                totalRunTime.Start();

                //IList<ResourceCore> resourceCores = _resourceCoreDataService.GetResourcesAll(true).ToList();
                IList<ResourceCore> resourceCores;
                if (!string.IsNullOrEmpty(_isbns))
                {
                    var array = _isbns.Split(',');
                    resourceCores = _resourceCoreDataService.GetResourcesByIsbns(array, true);
                }
                else
                {
                    resourceCores =
                        _resourceCoreDataService.GetResources(_minResourceId, _maxResourceId, _maxResources, true);
                }

                if (_generateDtSearchListIndexFile)
                {
                    _dtSearchService.CreateListIndexFile();
                }

                var docIdsList = _dtSearchService.GetResourceDocIdsFromIndexList();

                var indexFileCounts = new Dictionary<string, int>();

                var allResourceDocIds = _resourceFileDataService.GetAllResourceDocIds();

                var invalidDocIdResources = new List<InvalidDocIds>();

                var removeFromIndex = new List<ResourceCore>();

                var removeDatabaseDocIds = new List<ResourceCore>();

                var resourceContentStatuses = new List<ResourceContentStatus>();

                if (_truncateAndReloadResourceFileTable)
                {
                    _resourceFileDataService.TruncateTable();
                    _resourceFileDataService.DeleteAll();
                }

                var resourceCount = 0;
                var databaseFileCount = 0;
                var totalResourceCount = resourceCores.Count;

                var activeStatusCount = 0;
                var archivedStatusCount = 0;
                var forthcomingStatusCount = 0;
                var inactiveStatusCount = 0;

                var softDeletedCount = 0;

                var results = new StringBuilder();

                var shouldResourceBeIndexedCount = 0;
                foreach (var resourceCore in resourceCores)
                {
                    if (resourceCount >= _maxResources)
                    {
                        Log.WarnFormat("MAX RESOURCES reached! _maxResources = {0}", _maxResources);
                        break;
                    }

                    resourceCount++;
                    var resourceInfo =
                        $"Id: {resourceCore.Id}, ISBN: {resourceCore.Isbn} - status: {resourceCore.StatusId}, record status: {resourceCore.RecordStatus}";
                    Log.InfoFormat(">>> {0} of {1} - {2}", resourceCount, resourceCores.Count, resourceInfo);

                    results.AppendFormat(resourceInfo);

                    // this info is only needed for status
                    switch (resourceCore.StatusId)
                    {
                        case 6:
                            activeStatusCount++;
                            break;
                        case 7:
                            archivedStatusCount++;
                            break;
                        case 8:
                            forthcomingStatusCount++;
                            break;
                        case 72:
                            inactiveStatusCount++;
                            break;
                    }

                    if (resourceCore.RecordStatus == 0)
                    {
                        softDeletedCount++;
                    }

                    var resourceContentStatus = new ResourceContentStatus(resourceCore, _contentSettings);
                    var resourceDocIds = allResourceDocIds.SingleOrDefault(x => x.Id == resourceCore.Id);
                    var docIds = new DocIds();

                    if (resourceDocIds != null)
                    {
                        databaseFileCount += resourceDocIds.MaxDocId - resourceDocIds.MinDocId + 1;
                    }

                    var isIsbnInIndex = docIdsList.ContainsKey(resourceCore.Isbn);
                    var areResouceDocIdsInDatabase = resourceDocIds != null;
                    var docIdsMatch = false;

                    if (isIsbnInIndex)
                    {
                        docIds = docIdsList[resourceCore.Isbn];
                        indexFileCounts[resourceCore.Isbn] = docIds.Filenames.Count;

                        resourceContentStatus.ValidateResourceInIndex(docIds);

                        if (areResouceDocIdsInDatabase)
                        {
                            docIdsMatch = docIds.MinimumDocId == resourceDocIds.MinDocId &&
                                          docIds.MaximumDocId == resourceDocIds.MaxDocId;
                        }
                    }

                    if (ShouldResourceBeIndexed(resourceCore))
                    {
                        shouldResourceBeIndexedCount++;

                        //int invalidReasonId = -1;
                        var invalidReasonId = InvalidReasonId.NotDefined;

                        var warning = "";

                        if (!isIsbnInIndex)
                        {
                            warning = ResourceMessage("RESOURCE NOT IN INDEX", resourceCore);
                            invalidReasonId = InvalidReasonId.ResourceNotInIndex;
                        }
                        else if (docIds.Filenames.Any(x => x.IsInvalidPath))
                        {
                            warning = ResourceMessage("INDEX CONTAINS RESOURCE WITH INVALID PATH", resourceCore);
                            invalidReasonId = InvalidReasonId.IndexContainsResourceWithInvalidPath;
                        }
                        else if (!areResouceDocIdsInDatabase)
                        {
                            warning = ResourceMessage("RESOURCE DOC IDS NOT IN DATABASE", resourceCore);
                            invalidReasonId = InvalidReasonId.ResourceDocIdsNotInDatabase;
                        }
                        else if (!docIdsMatch)
                        {
                            warning =
                                $"RESOURCE DOC IDS DIFFER - ISBN: {resourceCore.Isbn}, Id: {resourceCore.Id} --> {docIds.MinimumDocId} != {resourceDocIds.MinDocId} and/or {docIds.MaximumDocId} != {resourceDocIds.MaxDocId}";
                            invalidReasonId = InvalidReasonId.ResourceDocIdsDiffer;
                        }
                        else if (IsXmlMissing(resourceContentStatus.Status))
                        {
                            warning = ResourceMessage("XML FILES MISSING FOR RESOURCE", resourceCore);
                            invalidReasonId = InvalidReasonId.XmlFilesMissingForResource;
                        }
                        else if (IsHtmlMissing(resourceContentStatus.Status))
                        {
                            warning = ResourceMessage("HTML FILES MISSING FOR RESOURCE", resourceCore);
                            invalidReasonId = InvalidReasonId.HtmlFilesMissingForResource;
                        }
                        else if (IsHtmlNotIndexed(resourceContentStatus.Status))
                        {
                            warning = ResourceMessage("HTML FILES NOT IN INDEX", resourceCore);
                            invalidReasonId = InvalidReasonId.HtmlFilesNotInIndex;
                        }
                        else if (DoesIndexContainMissingFiles(resourceContentStatus.Status))
                        {
                            warning = ResourceMessage("INDEX CONTAINS MISSING FILES", resourceCore);
                            invalidReasonId = InvalidReasonId.IndexContainsMissingFiles;
                        }

                        if (invalidReasonId != InvalidReasonId.NotDefined)
                        {
                            Log.Warn(warning);
                            var invalidDocIds = new InvalidDocIds(resourceCore, invalidReasonId, warning, docIds,
                                resourceDocIds);
                            invalidDocIdResources.Add(invalidDocIds);
                        }
                        else
                        {
                            Log.InfoFormat("Resource doc ids match - ISBN: {0}, Id: {1}", resourceCore.Isbn,
                                resourceCore.Id);
                        }
                    }
                    else
                    {
                        var isIndexRemove = false;

                        if (isIsbnInIndex)
                        {
                            if (ShouldIsbnBeIndexed(resourceCores, resourceCore.Isbn))
                            {
                                Log.Info(ResourceMessage(
                                    "Do not remove ISBN from index, another valid resource exists for this ISBN",
                                    resourceCore));
                            }
                            else
                            {
                                isIndexRemove = true;

                                removeFromIndex.Add(resourceCore);

                                Log.Warn(!docIdsMatch
                                    ? ResourceMessage("Doc ids differ", resourceCore, true)
                                    : ResourceMessage("Doc ids ok", resourceCore, true));
                            }
                        }
                        else
                        {
                            Log.Info(ResourceMessage("Resource is not in index and does not need to be", resourceCore));
                        }

                        if (areResouceDocIdsInDatabase)
                        {
                            if (!isIndexRemove)
                            {
                                //Not removing from ISBN index but still need to remove resource docids from database
                                removeDatabaseDocIds.Add(resourceCore);
                                Log.Warn(ResourceMessage("Bad resource doc ids exist in database", resourceCore));
                            }
                        }
                        else
                        {
                            Log.Info(ResourceMessage("Resource is not in the database and does not need to be",
                                resourceCore));
                        }
                    }

                    resourceContentStatuses.Add(resourceContentStatus);
                    Log.InfoFormat("resourceContentStatus.Isbn: {0}, resourceContentStatus.Status: {1}",
                        resourceContentStatus.Isbn, resourceContentStatus.Status);
                    foreach (var statusMessage in resourceContentStatus.StatusMessages)
                    {
                        Log.InfoFormat(" --> error: {0}", statusMessage);
                    }
                }

                long indexFileCount = indexFileCounts.Sum(p => p.Value);

                totalRunTime.Stop();
                Log.InfoFormat(
                    "### Total Run Time: {0:c}, total resource processed: {1:#,###}, total files verified: {2:#,###}/{3:#,###}",
                    totalRunTime.Elapsed, resourceCount, databaseFileCount, indexFileCount);

                //step.Results = string.Format("tasked finished in {0:c}", totalRunTime.Elapsed);
                firstStepResults.Insert(0, $"tasked finished in {totalRunTime.Elapsed:c}, ");
                step.Results = firstStepResults.ToString();
                step.CompletedSuccessfully = true;
                step.EndTime = DateTime.Now;

                Log.WarnFormat("Invalid resource count: {0}", invalidDocIdResources.Count);
                Log.WarnFormat("shouldResourceBeIndexedCount: {0}", shouldResourceBeIndexedCount);

                Log.WarnFormat("RESOURCE NOT IN INDEX:                     {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.ResourceNotInIndex));
                Log.WarnFormat("RESOURCE DOC IDS NOT IN DATABASE:          {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.ResourceDocIdsNotInDatabase));
                Log.WarnFormat("RESOURCE DOC IDS DIFFER:                   {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.ResourceDocIdsDiffer));
                Log.WarnFormat("XML FILES MISSING FOR RESOURCE:            {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.XmlFilesMissingForResource));
                Log.WarnFormat("HTML FILES MISSING FOR RESOURCE:           {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.HtmlFilesMissingForResource));
                Log.WarnFormat("HTML FILES NOT IN INDEX:                   {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.HtmlFilesNotInIndex));
                Log.WarnFormat("INDEX CONTAINS RESOURCE WITH INVALID PATH: {0}",
                    invalidDocIdResources.Count(x =>
                        x.InvalidReasonId == InvalidReasonId.IndexContainsResourceWithInvalidPath));
                Log.WarnFormat("INDEX CONTAINS MISSING FILES:              {0}",
                    invalidDocIdResources.Count(x => x.InvalidReasonId == InvalidReasonId.IndexContainsMissingFiles));

                // count the number of files in the index
                foreach (var docIds in docIdsList.Values)
                {
                    _totalIndexFileCount += docIds.MaximumDocId - docIds.MinimumDocId + 1;
                }

                if (invalidDocIdResources.Count > 0)
                {
                    AddMissingDocIdsToDatabase(invalidDocIdResources);

                    AddBadResourcesToTransformQueue(invalidDocIdResources);
                }

                RemoveDocIdsFromDatabase(removeDatabaseDocIds, resourceContentStatuses);

                RemoveResourcesFromIndex(removeFromIndex, docIdsList, invalidDocIdResources);

                step = new TaskResultStep { Name = "Summary", StartTime = DateTime.Now };
                TaskResult.AddStep(step);

                var summaryMessage = new StringBuilder();
                summaryMessage.AppendFormat("{0} resource in db", totalResourceCount);
                summaryMessage.AppendFormat(",\r\n {0} active resources", activeStatusCount);
                summaryMessage.AppendFormat(",\r\n {0} archived resources", archivedStatusCount);
                summaryMessage.AppendFormat(",\r\n {0} Pre-Order resources", forthcomingStatusCount);
                summaryMessage.AppendFormat(",\r\n {0} inactive resources", inactiveStatusCount);
                summaryMessage.AppendFormat(",\r\n {0} soft deleted resources", softDeletedCount);
                summaryMessage.AppendFormat(",\r\n {0} total resources in db with doc ids", allResourceDocIds.Count);
                summaryMessage.AppendFormat(",\r\n {0} total resources in index", docIdsList.Count);
                summaryMessage.AppendFormat(",\r\n {0} resources with invalid doc ids", invalidDocIdResources.Count);
                summaryMessage.AppendFormat(",\r\n {0} resources to remove from index", removeFromIndex.Count);
                summaryMessage.AppendFormat(",\r\n {0} resources removed from index",
                    _totalResourcesRemovedFromIndexCount);
                summaryMessage.AppendFormat(",\r\n {0} files removed from index", _totalFilesRemovedFromIndexCount);
                summaryMessage.AppendFormat(",\r\n {0:#,###} total files in index", _totalIndexFileCount);
                summaryMessage.AppendFormat(",\r\n {0:#,###} XML files", _totalXmlFileCount);
                summaryMessage.AppendFormat(",\r\n {0:#,###} HTML files", _totalHtmlFileCount);
                summaryMessage.AppendFormat(",\r\n {0:#,###} XML files in bytes", _totalXmlDirectorySizeInBytes);
                summaryMessage.AppendFormat(",\r\n {0:#,###} HTML files in bytes", _totalHtmlDirectorySizeInBytes);

                summaryMessage.AppendFormat(",\r\n -maxResources={0}", _maxResources);
                summaryMessage.AppendFormat(",\r\n -isbns={0}", _isbns);
                summaryMessage.AppendFormat(",\r\n -resourceFileTableName={0}", _resourceFileTableName);
                summaryMessage.AppendFormat(",\r\n -minResourceId={0}", _minResourceId);
                summaryMessage.AppendFormat(",\r\n -maxResourceId={0}", _maxResourceId);
                summaryMessage.AppendFormat(",\r\n -truncateAndReloadResourceFileTable={0}",
                    _truncateAndReloadResourceFileTable);
                summaryMessage.AppendFormat(",\r\n -generateDtSearchListIndexFile={0}", _generateDtSearchListIndexFile);
                summaryMessage.AppendFormat(",\r\n -removeBadResourcesFromIndex={0}", _removeBadResourcesFromIndex);
                summaryMessage.AppendFormat(",\r\n -removeBadDatabaseDocIds={0}", _removeBadDatabaseDocIds);
                summaryMessage.AppendFormat(",\r\n -addBadResourcesToTransformQueue={0}",
                    _addBadResourcesToTransformQueue);
                summaryMessage.AppendFormat(",\r\n -logResourceFileSql={0}", _logResourceFileSql);
                summaryMessage.AppendFormat(",\r\n -fixDocIdsInDb={0}", _fixDocIdsInDb);

                step.Results = summaryMessage.ToString();
                step.CompletedSuccessfully = true;
                step.EndTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                firstStepResults.Insert(0, $"EXCEPTION: {ex.Message}\r\n\r\n");
                //step.Results = ex.Message;
                step.Results = firstStepResults.ToString();
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        private void AddBadResourcesToTransformQueue(List<InvalidDocIds> invalidDocIdResources)
        {
            TaskResultStep step;

            if (_addBadResourcesToTransformQueue)
            {
                foreach (var invalidDoc in invalidDocIdResources)
                {
                    step = new TaskResultStep
                        { Name = $"AddResourceToTransformQueue-{invalidDoc.Resource.Isbn}", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    Log.WarnFormat(
                        "(Add to queue) - Invalid resources doc ids - Id: {0}, ISBN: {1} - Reason: {2} - {3}",
                        invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, invalidDoc.InvalidReasonId,
                        invalidDoc.InvalidReason);
                    AddInvalidResourcesToTransformQueue(invalidDoc);

                    step.Results =
                        $"Invalid Reason: {invalidDoc.InvalidReasonId}, {invalidDoc.InvalidReason} -- Successfully added to transform queue.";
                    step.CompletedSuccessfully = true;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
            else
            {
                foreach (var invalidDoc in invalidDocIdResources)
                {
                    Log.WarnFormat("(No action) - Invalid resources doc ids - Id: {0}, ISBN: {1} - Reason: {2} - {3}",
                        invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, invalidDoc.InvalidReasonId,
                        invalidDoc.InvalidReason);

                    step = new TaskResultStep
                        { Name = $"ReIndexNeeded-{invalidDoc.Resource.Isbn}", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    step.Results =
                        $"(No action) - Invalid resources doc ids - Id: {invalidDoc.Resource.Id}, ISBN: {invalidDoc.Resource.Isbn} - Reason: {invalidDoc.InvalidReasonId} - {invalidDoc.InvalidReason}";
                    step.CompletedSuccessfully = false;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
        }

        private void RemoveDocIdsFromDatabase(List<ResourceCore> removeDatabaseDocIds,
            List<ResourceContentStatus> resourceContentStatuses)
        {
            TaskResultStep step;

            if (removeDatabaseDocIds.Count > 0)
            {
                Log.WarnFormat("Bad database docids resource count: {0}", removeDatabaseDocIds.Count);
            }

            if (_removeBadDatabaseDocIds)
            {
                foreach (var resourceCore in removeDatabaseDocIds)
                {
                    step = new TaskResultStep
                    {
                        Name =
                            $"RemoveBadDatabaseDocIds-{resourceCore.Isbn}, Id:{resourceCore.Id}",
                        StartTime = DateTime.Now
                    };
                    TaskResult.AddStep(step);

                    Log.WarnFormat("Deleting bad database doc ids - Id: {0}, ISBN: {1} ",
                        resourceCore.Id, resourceCore.Isbn);
                    _resourceFileDataService.DeleteByResourceId(resourceCore.Id);

                    step.Results = "Successfully deleted bad doc ids";
                    step.CompletedSuccessfully = true;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
            else
            {
                foreach (var resourceCore in removeDatabaseDocIds)
                {
                    Log.WarnFormat("(No action) - Bad database doc ids - Id: {0}, ISBN: {1} ",
                        resourceCore.Id, resourceCore.Isbn);

                    step = new TaskResultStep
                    {
                        Name =
                            $"BadDatabaseDocIdRemovalNeeded-{resourceCore.Isbn}, Id:{resourceCore.Id}",
                        StartTime = DateTime.Now
                    };
                    TaskResult.AddStep(step);

                    step.Results =
                        $"(No action) - Bad database doc ids - Id: {resourceCore.Id}, ISBN: {resourceCore.Isbn} ";
                    step.CompletedSuccessfully = false;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }

            foreach (var resourceContentStatus in resourceContentStatuses)
            {
                _totalXmlFileCount += resourceContentStatus.XmlFileCount;
                _totalHtmlFileCount += resourceContentStatus.HtmlFileCount;
                _totalXmlDirectorySizeInBytes += resourceContentStatus.XmlDirectorySizeInBytes;
                _totalHtmlDirectorySizeInBytes += resourceContentStatus.HtmlDirectorySizeInBytes;

                if (resourceContentStatus.IsSoftDeleted)
                {
                    continue;
                }

                if (resourceContentStatus.ResourceStatus != ResourceStatus.Active &&
                    resourceContentStatus.ResourceStatus != ResourceStatus.Archived)
                {
                    continue;
                }

                if (resourceContentStatus.Status != ResourceContentStatusType.Ok &&
                    resourceContentStatus.Status != ResourceContentStatusType.XmlAndHtmlOk
                    //&& (resourceContentStatus.Status != ResourceContentStatusType.XmlDirectoryDoesNotExist)
                   )
                {
                    step = new TaskResultStep
                    {
                        Name = $"{resourceContentStatus.Status}-{resourceContentStatus.Isbn}", StartTime = DateTime.Now
                    };
                    TaskResult.AddStep(step);

                    var msg = new StringBuilder();
                    foreach (var statusMessage in resourceContentStatus.StatusMessages)
                    {
                        msg.AppendFormat("{0}{1}", msg.Length == 0 ? "" : ",\r\n ", statusMessage);
                    }

                    step.Results = msg.ToString();
                    step.CompletedSuccessfully = false;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
        }

        private void RemoveResourcesFromIndex(List<ResourceCore> removeFromIndex,
            IDictionary<string, DocIds> docIdsList, List<InvalidDocIds> invalidDocIdResources)
        {
            TaskResultStep step;

            IList<InvalidDocIds> invalidPathDocIds =
                invalidDocIdResources.Where(x =>
                        x.InvalidReasonId == InvalidReasonId.IndexContainsResourceWithInvalidPath
                        || x.InvalidReasonId == InvalidReasonId.ResourceDocIdsNotInDatabase
                        || x.InvalidReasonId == InvalidReasonId.ResourceDocIdsDiffer
                    )
                    .ToArray();

            // remove resources from index
            if (_removeBadResourcesFromIndex)
            {
                foreach (var resourceCore in removeFromIndex)
                {
                    step = new TaskResultStep { Name = "RemoveFromIndex", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    Log.InfoFormat("Remove from index - ISBN: {0}, id: {1}, StatusId: {2}", resourceCore.Isbn,
                        resourceCore.Id,
                        resourceCore.StatusId);

                    var deleteDocIds = docIdsList[resourceCore.Isbn];
                    var indexRemovedCount = _dtSearchService.RemoveDocumentIds(deleteDocIds, resourceCore.Id);
                    var removeSuccessful = indexRemovedCount >= 0;
                    string removeResults;

                    if (removeSuccessful)
                    {
                        _totalResourcesRemovedFromIndexCount++;
                        _totalFilesRemovedFromIndexCount += indexRemovedCount;

                        removeResults =
                            $"Successfully removed ISBN {deleteDocIds.Isbn} from dtSearch index ({indexRemovedCount} docids)";
                        Log.InfoFormat(removeResults);
                    }
                    else
                    {
                        removeResults =
                            $"Failed to remove ISBN {deleteDocIds.Isbn} from index, see log for error detail.";
                        Log.ErrorFormat(removeResults);
                    }

                    step.Results = removeResults;
                    step.CompletedSuccessfully = removeSuccessful;
                    step.EndTime = DateTime.Now;

                    UpdateTaskResult();
                }


                // removed invalid docs from index
                foreach (var invalidDoc in invalidPathDocIds)
                {
                    var deleteDocIds =
                        invalidDoc.IndexDocIds.GetInvalidDocsInIndex(_contentSettings.NewContentLocation);
                    if (deleteDocIds.Filenames.Count > 0)
                    {
                        step = new TaskResultStep { Name = "RemoveFromIndex-InvalidPath", StartTime = DateTime.Now };
                        TaskResult.AddStep(step);

                        Log.InfoFormat("Remove from index (Invalid Path) - ISBN: {0}, id: {1}, StatusId: {2}",
                            invalidDoc.Resource.Isbn, invalidDoc.Resource.Id,
                            invalidDoc.Resource.StatusId);

                        var indexRemovedCount =
                            _dtSearchService.RemoveDocumentIds(deleteDocIds, invalidDoc.Resource.Id);
                        var removeSuccessful = indexRemovedCount >= 0;
                        string removeResults;

                        if (removeSuccessful)
                        {
                            _totalResourcesRemovedFromIndexCount++;
                            _totalFilesRemovedFromIndexCount += indexRemovedCount;

                            removeResults =
                                $"Successfully removed ISBN {deleteDocIds.Isbn} from dtSearch index ({indexRemovedCount} docids)";
                            Log.InfoFormat(removeResults);
                        }
                        else
                        {
                            removeResults =
                                $"Failed to remove ISBN {deleteDocIds.Isbn} from index, see log for error detail.";
                            Log.ErrorFormat(removeResults);
                        }

                        step.Results = removeResults;
                        step.CompletedSuccessfully = removeSuccessful;
                        step.EndTime = DateTime.Now;

                        UpdateTaskResult();
                    }
                    else
                    {
                        Log.InfoFormat("RemoveFromIndex-InvalidPath-{0} => no resource file inserts required",
                            invalidDoc.Resource.Isbn);
                    }
                }
            }
            else
            {
                foreach (var resourceCore in removeFromIndex)
                {
                    Log.WarnFormat(
                        "(No action) - Remove from index - Id: {0}, ISBN: {1}, StatusId: {2}, RecordStatus: {3}",
                        resourceCore.Id, resourceCore.Isbn, resourceCore.StatusId, resourceCore.RecordStatus);

                    step = new TaskResultStep
                        { Name = $"IndexRemovalNeeded-{resourceCore.Isbn}", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    step.Results =
                        $"(No action) - Remove from index - Id: {resourceCore.Id}, ISBN: {resourceCore.Isbn}, StatusId: {resourceCore.StatusId}, RecordStatus: {resourceCore.RecordStatus}";
                    step.CompletedSuccessfully = false;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }

                foreach (var invalidDoc in invalidPathDocIds)
                {
                    Log.WarnFormat(
                        "(No action) - Remove from index - Id: {0}, ISBN: {1}, StatusId: {2}, RecordStatus: {3}",
                        invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, invalidDoc.Resource.StatusId,
                        invalidDoc.Resource.RecordStatus);

                    step = new TaskResultStep
                        { Name = $"IndexRemovalNeeded-{invalidDoc.Resource.Isbn}", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    step.Results =
                        $"(No action) - Remove from index - Id: {invalidDoc.Resource.Id}, ISBN: {invalidDoc.Resource.Isbn}, StatusId: {invalidDoc.Resource.StatusId}, RecordStatus: {invalidDoc.Resource.RecordStatus}";
                    step.CompletedSuccessfully = false;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
        }

        private void AddMissingDocIdsToDatabase(List<InvalidDocIds> invalidDocIdResources)
        {
            TaskResultStep step;

            if (_fixDocIdsInDb)
            {
                foreach (var invalidDoc in invalidDocIdResources)
                {
                    //ResourceDocIdsNotInDatabase = 1,
                    //ResourceDocIdsDiffer = 2,
                    if (invalidDoc.InvalidReasonId != InvalidReasonId.ResourceDocIdsNotInDatabase &&
                        invalidDoc.InvalidReasonId != InvalidReasonId.ResourceDocIdsDiffer &&
                        invalidDoc.InvalidReasonId != InvalidReasonId.ResourceDocIdsNotInDatabase &&
                        invalidDoc.InvalidReasonId != InvalidReasonId.ResourceDocIdsDiffer)
                    {
                        continue;
                    }

                    step = new TaskResultStep
                        { Name = $"FixDocIdsInDb-{invalidDoc.Resource.Isbn}", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    Log.WarnFormat("(Action) - Fix doc ids - Id: {0}, ISBN: {1} - Reason: {2} - {3}",
                        invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, invalidDoc.InvalidReasonId,
                        invalidDoc.InvalidReason);

                    var countByDocIds = invalidDoc.IndexDocIds.MaximumDocId - invalidDoc.IndexDocIds.MinimumDocId + 1;
                    if (invalidDoc.IndexDocIds.Filenames.Count != countByDocIds)
                    {
                        Log.ErrorFormat("invalidDoc.IndexDocIds.Filenames.Count != countByDocIds, {0} != {1}",
                            invalidDoc.IndexDocIds.Filenames.Count, countByDocIds);
                        step.Results =
                            $"Invalid Reason: {invalidDoc.InvalidReasonId}, {invalidDoc.InvalidReason} -- invalidDoc.IndexDocIds.Filenames.Count != countByDocIds, {invalidDoc.IndexDocIds.Filenames.Count} != {countByDocIds}";
                        step.CompletedSuccessfully = false;

                        foreach (var docIdFilename in invalidDoc.IndexDocIds.Filenames)
                        {
                            Log.DebugFormat("-->> Id: {0}, IsInvalidPath: {1}, Name: {2}", docIdFilename.Id,
                                docIdFilename.IsInvalidPath, docIdFilename.Name);
                        }
                    }
                    else
                    {
                        var rowsDeleted = _resourceFileDataService.DeleteByResourceId(invalidDoc.Resource.Id);

                        var resourceFiles = new List<ResourceFile>();
                        foreach (var docIdFilename in invalidDoc.IndexDocIds.Filenames)
                        {
                            resourceFiles.Add(new ResourceFile(docIdFilename.Id, docIdFilename.Name,
                                invalidDoc.Resource.Id));
                        }

                        var rowsInserted = _resourceFileDataService.InsertBatch(resourceFiles,
                            _r2UtilitiesSettings.ResourceFileInsertBatchSize);

                        step.Results =
                            $"Invalid Reason: {invalidDoc.InvalidReasonId}, {invalidDoc.InvalidReason} -- Deleted {rowsDeleted}, added {rowsInserted} rows to resource file table.";
                        step.CompletedSuccessfully = true;
                    }

                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
            else
            {
                foreach (var invalidDoc in invalidDocIdResources)
                {
                    if (invalidDoc.InvalidReasonId != InvalidReasonId.ResourceDocIdsNotInDatabase &&
                        invalidDoc.InvalidReasonId != InvalidReasonId.ResourceDocIdsDiffer)
                    {
                        continue;
                    }

                    Log.WarnFormat("(No action) - Fix doc ids - Id: {0}, ISBN: {1} - Reason: {2} - {3}",
                        invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, invalidDoc.InvalidReasonId,
                        invalidDoc.InvalidReason);

                    step = new TaskResultStep
                        { Name = $"FixDocIdsNeeded-{invalidDoc.Resource.Isbn}", StartTime = DateTime.Now };
                    TaskResult.AddStep(step);

                    step.Results =
                        $"(No action) - Fix doc ids - Id: {invalidDoc.Resource.Id}, ISBN: {invalidDoc.Resource.Isbn} - Reason: {invalidDoc.InvalidReasonId} - {invalidDoc.InvalidReason}";
                    step.CompletedSuccessfully = false;
                    step.EndTime = DateTime.Now;
                    UpdateTaskResult();
                }
            }
        }

        private static string ResourceMessage(string message, ResourceCore resourceCore, bool removeFromIndex = false)
        {
            var removeMessage = removeFromIndex ? "Resource needs to be removed from dtSearch index, " : "";

            return
                $"{removeMessage}{message} - ISBN: {resourceCore.Isbn}, Id: {resourceCore.Id}, StatusId: {resourceCore.StatusId}, RecordStatus: {resourceCore.RecordStatus}";
        }

        private static bool IsXmlMissing(ResourceContentStatusType resourceContentStatusType)
        {
            return resourceContentStatusType == ResourceContentStatusType.XmlDirectoryDoesNotExist
                   || resourceContentStatusType == ResourceContentStatusType.XmlDirectoryIsEmpty
                   || resourceContentStatusType == ResourceContentStatusType.MissingXmlFiles;
        }

        private static bool IsHtmlMissing(ResourceContentStatusType resourceContentStatusType)
        {
            return resourceContentStatusType == ResourceContentStatusType.HtmlDirectoryDoesNotExist
                   || resourceContentStatusType == ResourceContentStatusType.HtmlDirectoryIsEmpty
                   || resourceContentStatusType == ResourceContentStatusType.MissingHtmlFiles
                   || resourceContentStatusType == ResourceContentStatusType.MissingHtmlGlossaryFiles;
        }

        private static bool IsHtmlNotIndexed(ResourceContentStatusType resourceContentStatusType)
        {
            return resourceContentStatusType == ResourceContentStatusType.HtmlFilesNotInIndex;
        }

        private static bool DoesIndexContainMissingFiles(ResourceContentStatusType resourceContentStatusType)
        {
            return resourceContentStatusType == ResourceContentStatusType.IndexContainsMissingFiles;
        }

        private static bool ShouldResourceBeIndexed(ResourceCore resourceCore)
        {
            return (resourceCore.StatusId == 6 || resourceCore.StatusId == 7) && resourceCore.RecordStatus == 1;
        }

        private static bool ShouldIsbnBeIndexed(IEnumerable<ResourceCore> resourceCores, string isbn)
        {
            return resourceCores.Any(r => r.Isbn == isbn && ShouldResourceBeIndexed(r));
        }

        private void AddInvalidResourcesToTransformQueue(InvalidDocIds invalidDoc)
        {
            if (_transformQueueDataService.GetCount(invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, "A") == 0)
            {
                _transformQueueDataService.Insert(invalidDoc.Resource.Id, invalidDoc.Resource.Isbn, "A");
            }
        }
    }
}