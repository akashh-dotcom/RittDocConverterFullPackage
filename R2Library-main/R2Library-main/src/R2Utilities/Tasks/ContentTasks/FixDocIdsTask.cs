#region

using System;
using System.Diagnostics;
using System.Linq;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class FixDocIdsTask : TaskBase, ITask
    {
        private readonly FixDocIdsQueueDataService _fixDocIdsQueueFactory;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly SearchService _searchService;
        private string _isbn;
        private bool _logResourceFileSql;

        private int _maxResources = 20000;
        private ResourceFileDataService _resourceFileDataService;
        private string _resourceFileTableName;

        protected new string TaskName = "FixDocIdsTask";

        public FixDocIdsTask(SearchService searchService, IR2UtilitiesSettings r2UtilitiesSettings)
            : base("FixDocIdsTask", "-FixDocIdsTask", "23", TaskGroup.DiagnosticsMaintenance,
                "Task to fix DocIds in RIT001.tResourceFile table", false)
        {
            _searchService = searchService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _fixDocIdsQueueFactory = new FixDocIdsQueueDataService();
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _maxResources = GetArgumentInt32("maxResources", 100000);
            _isbn = GetArgument("isbn");
            _resourceFileTableName = GetArgument("resourceFileTableName") ?? _r2UtilitiesSettings.ResourceFileTableName;
            _logResourceFileSql = GetArgumentBoolean("logResourceFileSql", true);
            _resourceFileDataService = new ResourceFileDataService(_resourceFileTableName, _logResourceFileSql);

            Log.InfoFormat(">>> _maxResources: {0}, _isbn: {1}, _resourceFileTableName: {2}, _logResourceFileSql: {3}",
                _maxResources, _isbn, _resourceFileTableName, _logResourceFileSql);
        }

        public override void Run()
        {
            try
            {
                var totalRunTime = new Stopwatch();
                totalRunTime.Start();
                var step = new TaskResultStep { Name = "FixDocIdsTask", StartTime = DateTime.Now };
                TaskResult.AddStep(step);
                UpdateTaskResult();

                //_resourceFileFactory.TruncateTable();
                //_resourceFileFactory.DeleteAll();

                var resourceCount = 0;
                var totalFileCount = 0;

                var fixDocIdsQueue = _fixDocIdsQueueFactory.GetNext();
                while (fixDocIdsQueue != null && fixDocIdsQueue.ResourceId > 0)
                {
                    fixDocIdsQueue.DateStarted = DateTime.Now;
                    Log.InfoFormat(">>>>>>>>>> ResourceId: {0}, ISBN: {1}", fixDocIdsQueue.ResourceId,
                        fixDocIdsQueue.Isbn);
                    resourceCount++;
                    var fileCount = LoadDocumentIds(fixDocIdsQueue);
                    Log.InfoFormat("Document Id Fixed: {0}", fileCount);
                    totalFileCount += fileCount;
                    fixDocIdsQueue.DateFinished = DateTime.Now;

                    fixDocIdsQueue.Status = "F";
                    fixDocIdsQueue.StatusMessage = $"{fileCount} Doc Ids Fixed";

                    _fixDocIdsQueueFactory.Update(fixDocIdsQueue);

                    Log.InfoFormat("*** Total Run Time: {0:c}, total resource processed: {1}, total files fixed: {2}",
                        totalRunTime.Elapsed, resourceCount, totalFileCount);

                    fixDocIdsQueue = _fixDocIdsQueueFactory.GetNext();
                }

                totalRunTime.Stop();
                Log.InfoFormat("### Total Run Time: {0:c}, total resource processed: {1}, total files fixed: {2}",
                    totalRunTime.Elapsed, resourceCount, totalFileCount);

                step.CompletedSuccessfully = true;
                step.Results = $"tasked finished in {totalRunTime.Elapsed:c}";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public int LoadDocumentIds(FixDocIdsQueue fixDocIdsQueue)
        {
            var searchResults = _searchService.PerformSearchByIsbn(fixDocIdsQueue.Isbn.Trim());
            var resourceFiles = searchResults.Select(searchResultItem => new ResourceFile(searchResultItem.DocumnetId,
                searchResultItem.DisplayName, fixDocIdsQueue.ResourceId)).ToList();
            _resourceFileDataService.InsertBatch(resourceFiles, _r2UtilitiesSettings.ResourceFileInsertBatchSize);
            return resourceFiles.Count;
        }
    }
}