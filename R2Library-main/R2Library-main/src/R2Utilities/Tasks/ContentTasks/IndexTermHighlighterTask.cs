#region

using System;
using System.Diagnostics;
using R2Utilities.DataAccess.Terms;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class IndexTermHighlighterTask : TaskBase
    {
        private readonly IndexTermHighlightSettings _indexTermHighlightSettings;
        private readonly TermHighlighterService _termHighlighterService;

        public IndexTermHighlighterTask(TermHighlighterService termHighlighterService
            , IndexTermHighlightSettings indexTermHighlightSettings
            , IndexTermDataService indexTermDataService /*This needs to be of type IndexTermDataService, not simply ITermDataService*/
        )
            : base("IndexTermHighlighterTask", "-IndexTermHighlighterTask", "05", TaskGroup.ContentLoading,
                "Task to update the term highlighting", true)
        {
            _indexTermHighlightSettings = indexTermHighlightSettings;

            _termHighlighterService = termHighlighterService;
            _termHighlighterService.Init(_indexTermHighlightSettings, indexTermDataService, "IndexTermHighlighterTask");
        }

        public override void Run()
        {
            var runtimeTimer = new Stopwatch();
            runtimeTimer.Start();

            try
            {
                var totalHighlightedResourceCount = 0;
                var totalHighlightedDocumentCount = 0;
                var totalHighlightTimeSpan = new TimeSpan();
                var totalResourceFileLoadTimeSpan = new TimeSpan();

                var batchNumber = 0;
                while (_termHighlighterService.ProcessNextBatch(TaskResult))
                {
                    batchNumber++;
                    Log.InfoFormat("batchNumber: {0} - COMPLETE", batchNumber);

                    totalHighlightedResourceCount += _termHighlighterService.HighlightedResourceCount;
                    totalHighlightedDocumentCount += _termHighlighterService.HighlightedFileCount;
                    totalHighlightTimeSpan = totalHighlightTimeSpan.Add(_termHighlighterService.TermHighlightTimeSpan);
                    totalResourceFileLoadTimeSpan =
                        totalResourceFileLoadTimeSpan.Add(_termHighlighterService.ResourceFileLoadTimeSpan);

                    var resourceHighlightAvg = _termHighlighterService.HighlightedResourceCount != 0
                        ? _termHighlighterService.TermHighlightTimeSpan.TotalMilliseconds /
                          _termHighlighterService.HighlightedResourceCount
                        : 0;
                    var resourceFileInsertAvg = _termHighlighterService.HighlightedFileCount != 0
                        ? _termHighlighterService.ResourceFileLoadTimeSpan.TotalMilliseconds /
                          _termHighlighterService.HighlightedFileCount
                        : 0;

                    var totalResourceHighlightAvg = totalHighlightedResourceCount != 0
                        ? totalHighlightTimeSpan.TotalMilliseconds / totalHighlightedResourceCount
                        : 0;
                    var totalResourceFileInsertAvg = totalHighlightedDocumentCount != 0
                        ? totalResourceFileLoadTimeSpan.TotalMilliseconds / totalHighlightedDocumentCount
                        : 0;

                    Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");
                    Log.InfoFormat("HighlightedResourceCount: {0}", _termHighlighterService.HighlightedResourceCount);
                    Log.InfoFormat("HighlightedDocumentCount: {0}", _termHighlighterService.HighlightedFileCount);
                    Log.InfoFormat("IndexTimeSpan: {0:c}", _termHighlighterService.TermHighlightTimeSpan);
                    Log.InfoFormat("ResourceFileLoadTimeSpan: {0:c}", _termHighlighterService.ResourceFileLoadTimeSpan);
                    Log.InfoFormat("Resource Highlight Average: {0} ms", resourceHighlightAvg);
                    Log.InfoFormat("ResourceFile Insert Average: {0} ms", resourceFileInsertAvg);
                    Log.Info("+    +    +    +    +    +    +    +    +    +    +");
                    Log.InfoFormat("totalHighlightedResourceCount: {0}", totalHighlightedResourceCount);
                    Log.InfoFormat("totalHighlightedDocumentCount: {0}", totalHighlightedDocumentCount);
                    Log.InfoFormat("totalHighlightTimeSpan: {0:c}", totalHighlightTimeSpan);
                    Log.InfoFormat("totalResourceFileLoadTimeSpan: {0:c}", totalResourceFileLoadTimeSpan);
                    Log.InfoFormat("Total Run Time: {0:c}", runtimeTimer.Elapsed);
                    Log.InfoFormat("Total Resource Highlight Average: {0} ms", totalResourceHighlightAvg);
                    Log.InfoFormat("Total ResourceFile Insert Average: {0} ms", totalResourceFileInsertAvg);
                    Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");

                    if (_indexTermHighlightSettings.MaxIndexBatches <= 0 ||
                        _indexTermHighlightSettings.MaxIndexBatches != batchNumber) continue;

                    Log.InfoFormat("MAXIMUM HIGHLIGHTING BATCHES REACHED: {0}",
                        _indexTermHighlightSettings.MaxIndexBatches);
                    break;
                }

                runtimeTimer.Stop();

                TaskResult.Information =
                    $"Resource Count: {totalHighlightedResourceCount}, Document Count: {totalHighlightedDocumentCount}, Highlight Time: {totalHighlightTimeSpan:c}, Database Update Time: {totalResourceFileLoadTimeSpan:c}, Total Task Time: {runtimeTimer.Elapsed:c}";

                Log.InfoFormat("APP COMPLETE -- run time: {0:c}", runtimeTimer.Elapsed);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}