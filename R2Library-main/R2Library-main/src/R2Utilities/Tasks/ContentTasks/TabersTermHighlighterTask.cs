#region

using System;
using System.Diagnostics;
using R2Utilities.DataAccess.Tabers;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class TabersTermHighlighterTask : TaskBase
    {
        private readonly TabersTermHighlightSettings _tabersTermHighlightSettings;
        private readonly TermHighlighterService _termHighlighterService;

        public TabersTermHighlighterTask(TermHighlighterService termHighlighterService
            , TabersTermHighlightSettings tabersTermHighlightSettings
            , TabersDataService tabersDataService /*This needs to be of type TabersDataService, not simply ITermDataService*/
        )
            : base("TabersTermHighlighterTask", "-TabersTermHighlighterTask", "08", TaskGroup.ContentLoading,
                "Task to highlight Taber's terms in content", true)
        {
            _tabersTermHighlightSettings = tabersTermHighlightSettings;

            _termHighlighterService = termHighlighterService;
            _termHighlighterService.Init(_tabersTermHighlightSettings, tabersDataService, "TabersTermHighlighterTask");
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

                    if (_tabersTermHighlightSettings.MaxIndexBatches <= 0 ||
                        _tabersTermHighlightSettings.MaxIndexBatches != batchNumber) continue;

                    Log.InfoFormat("MAXIMUM HIGHLIGHTING BATCHES REACHED: {0}",
                        _tabersTermHighlightSettings.MaxIndexBatches);
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