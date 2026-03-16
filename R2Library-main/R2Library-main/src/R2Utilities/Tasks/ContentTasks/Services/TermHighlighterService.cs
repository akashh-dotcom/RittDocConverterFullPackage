#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.DataAccess.Terms;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class TermHighlighterService : R2UtilitiesBase
    {
        /// <param name="hitHighlighter"> </param>
        public TermHighlighterService(IR2UtilitiesSettings r2UtilitiesSettings
            , HitHighlighter hitHighlighter
        )
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _hitHighlighter = hitHighlighter;
            _resourceCoreDataService = new ResourceCoreDataService();
        }

        public void Init(ITermHighlightSettings termHighlightSettings, ITermDataService termDataService,
            string taskName)
        {
            _termHighlightSettings = termHighlightSettings;
            _termHighlightQueueDataService =
                new TermHighlightQueueDataService(_termHighlightSettings.TermHighlightType);
            _hitHighlighter.Init(_termHighlightSettings, termDataService, taskName);
        }

        public bool ProcessNextBatch(TaskResult taskResult)
        {
            TermHighlightTimeSpan = new TimeSpan();
            ResourceFileLoadTimeSpan = new TimeSpan();

            var step = new TaskResultStep { Name = "TermHighlightBatch", StartTime = DateTime.Now };
            taskResult.AddStep(step);

            var stepResults = new StringBuilder();

            try
            {
                var termHighlightQueueSize = _termHighlightQueueDataService.GetTermHighlightQueueSize();
                Log.DebugFormat("termHighlightQueueSize: {0}", termHighlightQueueSize);

                var resourceCount = 0;
                var maxBatchSize = _termHighlightSettings.BatchSize;

                Log.InfoFormat(">>>>>>>>>> HIGHLIGHTING UP TO {0} RESOURCES <<<<<<<<<<", maxBatchSize);

                var batchResourceCount = 0;
                var batchFileCount = 0;
                var timestamp = DateTime.Now;

                TermHighlightQueue queue;
                do
                {
                    if (resourceCount >= maxBatchSize)
                    {
                        Log.InfoFormat("MAX BATCH SIZE REACHED: {0}", maxBatchSize);
                        break;
                    }

                    queue = _termHighlightQueueDataService.GetNext(_r2UtilitiesSettings.OrderBatchDescending);
                    if (queue == null) continue;

                    resourceCount++;

                    Log.InfoFormat("Processing {0} out of a possible {1} resources", resourceCount, maxBatchSize);

                    stepResults.AppendFormat("\tISBN: {0}", string.Join(",", queue.Isbn)).AppendLine();

                    var termHighlightTimer = new Stopwatch();
                    termHighlightTimer.Start();

                    var termHighlightWasSuccessful = HighlightTerms(queue, timestamp, out var highlightedFileCount);
                    termHighlightTimer.Stop();

                    if (termHighlightWasSuccessful)
                    {
                        HighlightedResourceCount++;
                        HighlightedFileCount += highlightedFileCount;

                        batchResourceCount++;
                        batchFileCount += highlightedFileCount;
                    }

                    //long termHighlightElapsed = termHighlightTimer.ElapsedMilliseconds;
                    TermHighlightTimeSpan = termHighlightTimer.Elapsed;
                    //Log.InfoFormat("termHighlightElapsed: {0:0,000} ms, termHighlightWasSuccessful: {1}", termHighlightElapsed, termHighlightWasSuccessful);

                    //double totalAvgTermHighlightTimePerFile = (double)termHighlightElapsed / fileCount;
                    //Log.DebugFormat("termHighlightElapsed: {0:0,000} ms, totalAvgTermHighlightTimePerFile: {1:0.000} ms", termHighlightElapsed, totalAvgTermHighlightTimePerFile);

                    UpdateQueue(queue, taskResult, highlightedFileCount);

                    step.CompletedSuccessfully = true;
                } while (queue != null);

                if (resourceCount == 0)
                {
                    Log.Info("No more resources to highlight");
                    step.CompletedSuccessfully = true;
                    return false;
                }

                Log.InfoFormat("Highlighting {0} resources, {1:#,###} files", batchResourceCount, batchFileCount);
                Log.InfoFormat("Highlighting {0} total resources, {1:#,###} total files", HighlightedResourceCount,
                    HighlightedFileCount);

                return resourceCount > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                step.Results = stepResults.ToString();
            }
        }

        bool HighlightTerms(TermHighlightQueue termHighlightQueue, DateTime timestamp, out int highlightedFileCount)
        {
            try
            {
                highlightedFileCount = 0;

                termHighlightQueue.TermHighlightStatus = TermHighlightStatusCodeProcessing;
                termHighlightQueue.DateStarted = DateTime.Now; // SJS - set the start time to the current time
                _termHighlightQueueDataService.Update(termHighlightQueue);

                var resourceToHighlight =
                    new ResourceToHighlight(_termHighlightSettings, termHighlightQueue, timestamp);

                if (_termHighlightSettings.TermHighlightType == TermHighlightType.IndexTerms)
                {
                }

                resourceToHighlight.ResourceCore =
                    _resourceCoreDataService.GetResourceByIsbn(termHighlightQueue.Isbn, true);

                var resourceLocation = resourceToHighlight.ResourceLocation;
                var outputLocation = resourceToHighlight.OutputLocation;
                var backupLocation = resourceToHighlight.BackupLocation;

                var directoryInfo = new DirectoryInfo(resourceLocation);
                if (!directoryInfo.Exists)
                {
                    Log.WarnFormat("DIRECTORY DOES NOT EXIST! path: {0}", directoryInfo);
                    termHighlightQueue.TermHighlightStatus = TermHighlightStatusCodeError;
                    termHighlightQueue.StatusMessage = $"DIRECTORY DOES NOT EXIST! path: {directoryInfo}";
                    return false;
                }

                var files = directoryInfo.GetFiles();
                if (files.Length == 0)
                {
                    Log.WarnFormat("DIRECTORY IS EMPTY! path: {0}", directoryInfo);
                    termHighlightQueue.TermHighlightStatus = TermHighlightStatusCodeError;
                    termHighlightQueue.StatusMessage = $"DIRECTORY IS EMPTY! path: {directoryInfo}";
                    return false;
                }

                Log.DebugFormat("{0} files in directory '{1}'", files.Length, resourceLocation);

                Directory.CreateDirectory(backupLocation);
                Directory.CreateDirectory(outputLocation);

                bool isSuccessful;
                var termHighlightTimer = new Stopwatch();

                try
                {
                    termHighlightTimer.Start();

                    _hitHighlighter.HighlightResource(resourceToHighlight);
                    isSuccessful = true;

                    termHighlightQueue.TermHighlightStatus = TermHighlightStatusCodeHighlighted;

                    termHighlightTimer.Stop();
                }
                catch (Exception ex)
                {
                    isSuccessful = false;

                    Log.Error(ex.Message, ex);
                    termHighlightQueue.TermHighlightStatus = TermHighlightStatusCodeError;
                    //termHighlightQueue.StatusMessage = "resource failed";
                    //_termHighlightQueueDataService.Update(termHighlightQueue);
                }

                highlightedFileCount = _hitHighlighter.ResourceToHighlight.HighlightedFileCount;
                var totalFileCount = _hitHighlighter.ResourceToHighlight.TotalFileCount;

                /*if (!isSuccessful)
                {
                    Log.Info("\n***** Resource Failed!!! *****\n");
                    Log.Info("Continuing to next resource...");
                    return false;
                }*/

                //termHighlightQueue.StatusMessage = "resource highlighted successfully.";

                var errorCount = isSuccessful ? 0 : 1;

                var avgHighlightTime = highlightedFileCount == 0
                    ? termHighlightTimer.ElapsedMilliseconds
                    : termHighlightTimer.ElapsedMilliseconds / highlightedFileCount;

                Log.InfoFormat("{0} of {1} files highlighted successfully in {2:c}, {3} error(s)",
                    highlightedFileCount, totalFileCount, termHighlightTimer.Elapsed, errorCount);
                Log.InfoFormat("Avg Highlight Time: {0} ms", avgHighlightTime);

                termHighlightQueue.StatusMessage =
                    $"{highlightedFileCount} of {totalFileCount} files highlighted successfully in {termHighlightTimer.Elapsed:c}, {errorCount} error(s), Avg Highlight Time: {avgHighlightTime} ms";

                if (_termHighlightSettings.TermHighlightType == TermHighlightType.Tabers &&
                    _termHighlightSettings.UpdateResourceStatus)
                {
                    Log.Info("Updating Resource Tabers Status");
                    _resourceCoreDataService.SetResourceTabersStatus(termHighlightQueue.ResourceId, true,
                        "TermHighlighter");
                }

                return isSuccessful;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        void UpdateQueue(TermHighlightQueue termHighlightQueue, TaskResult taskResult, int highlightedFileCount)
        {
            var insertTimer = new Stopwatch();

            var updateQueueStep = new TaskResultStep
            {
                Name =
                    $"Updating queue for ISBN: {termHighlightQueue.Isbn}, resource id: {termHighlightQueue.ResourceId}, term highlight queue id: {termHighlightQueue.Id}",
                StartTime = DateTime.Now
            };
            taskResult.AddStep(updateQueueStep);

            Log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Log.InfoFormat("Queue Id: {0}, ISBN: {1}", termHighlightQueue.Id, termHighlightQueue.Isbn);

            //termHighlightQueue.DateStarted = startTime;	// SJS - the start time needs to be the start time of the resource, not the batch
            termHighlightQueue.DateFinished = DateTime.Now;
            _termHighlightQueueDataService.Update(termHighlightQueue);

            updateQueueStep.CompletedSuccessfully =
                termHighlightQueue.TermHighlightStatus != TermHighlightStatusCodeError;
            updateQueueStep.EndTime = DateTime.Now;
            updateQueueStep.Results = termHighlightQueue.StatusMessage;

            var insertElapsed = insertTimer.ElapsedMilliseconds;

            //int fileCount = (termHighlightQueue.LastDocumentId - termHighlightQueue.FirstDocumentId) + 1;

            var avgInsertTimePerFile = (double)insertElapsed / highlightedFileCount;
            Log.DebugFormat("insertElapsed: {0:0,000} ms, avgInsertTimePerFile: {1:0.000} ms", insertElapsed,
                avgInsertTimePerFile);
            Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
        }

        #region Fields

        private const string TermHighlightStatusCodeError = "E";
        private const string TermHighlightStatusCodeProcessing = "P";
        private const string TermHighlightStatusCodeHighlighted = "H";

        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private ITermHighlightSettings _termHighlightSettings;
        private readonly HitHighlighter _hitHighlighter;
        private readonly ResourceCoreDataService _resourceCoreDataService;
        private TermHighlightQueueDataService _termHighlightQueueDataService;

        public int HighlightedResourceCount { get; private set; }
        public int HighlightedFileCount { get; private set; }

        public TimeSpan TermHighlightTimeSpan { get; set; }
        public TimeSpan ResourceFileLoadTimeSpan { get; set; }

        #endregion
    }
}