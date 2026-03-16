#region

using System;
using System.Diagnostics;
using System.Linq;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;
using R2V2.Core.Resource;
using R2V2.Core.Resource.BookSearch;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class DtSearchHtmlIndexerTask : TaskBase, ITask
    {
        private readonly IContentSettings _contentSettings;
        private readonly DtSearchBatchIndexer _dtSearchBatchIndexer;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IQueryable<IResource> _resources;

        private bool _indexForthcoming;

        private int _maxBatchSize;
        private int _maxResourceId;
        private int _minResourceId;

        /// <summary>
        ///     -DtSearchHtmlIndexerTask -maxBatchSize=10 -minResourceId=1000 -maxResourceId=2000
        ///     -DtSearchHtmlIndexerTask -maxBatchSize=1 -minResourceId=1 -maxResourceId=20000
        /// </summary>
        public DtSearchHtmlIndexerTask(
            DtSearchBatchIndexer dtSearchBatchIndexer
            , IR2UtilitiesSettings r2UtilitiesSettings
            , IQueryable<IResource> resources
            , IContentSettings contentSettings
        )
            : base("DtSearchHtmlIndexerTask", "-DtSearchHtmlIndexerTask", "01", TaskGroup.ContentLoading,
                "Indexes HTML content produced by TransformXmlTask based in IndexQueue table", true)
        {
            _dtSearchBatchIndexer = dtSearchBatchIndexer;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _resources = resources;
            _contentSettings = contentSettings;

            SetSummaryEmailSetting(false, true, 10);
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _maxBatchSize = GetArgumentInt32("maxBatchSize", _r2UtilitiesSettings.HtmlIndexerBatchSize);
            _minResourceId = GetArgumentInt32("minResourceId", 0);
            _maxResourceId = GetArgumentInt32("maxResourceId", 100000);


            _indexForthcoming = GetArgumentBoolean("indexForthcoming", false);

            Log.InfoFormat("_maxBatchSize: {0}, _minResourceId: {1}, _maxResourceId: {2}", _maxBatchSize,
                _minResourceId, _maxResourceId);
        }

        public override void Run()
        {
            if (_indexForthcoming)
            {
                IndexForthcomingSearchData();
                return;
            }


            var runtimeTimer = new Stopwatch();
            runtimeTimer.Start();

            try
            {
                var totalIndexedResourceCount = 0;
                var totalIndexedDocumentCount = 0;
                var totalIndexTimeSpan = new TimeSpan();
                var totalResourceFileLoadTimeSpan = new TimeSpan();

                var batchNumber = 0;
                while (_dtSearchBatchIndexer.ProcessNextBatch(TaskResult, _maxBatchSize, _minResourceId,
                           _maxResourceId))
                {
                    batchNumber++;
                    Log.InfoFormat("batchNumber: {0} - COMPLETE", batchNumber);

                    totalIndexedResourceCount += _dtSearchBatchIndexer.IndexedResourceCount;
                    totalIndexedDocumentCount += _dtSearchBatchIndexer.IndexedDocumentCount;
                    totalIndexTimeSpan = totalIndexTimeSpan.Add(_dtSearchBatchIndexer.IndexTimeSpan);
                    totalResourceFileLoadTimeSpan =
                        totalResourceFileLoadTimeSpan.Add(_dtSearchBatchIndexer.ResourceFileLoadTimeSpan);
                    Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");
                    Log.InfoFormat("IndexedResourceCount: {0}", _dtSearchBatchIndexer.IndexedResourceCount);
                    Log.InfoFormat("IndexedDocumentCount: {0}", _dtSearchBatchIndexer.IndexedDocumentCount);
                    Log.InfoFormat("IndexTimeSpan: {0:c}", _dtSearchBatchIndexer.IndexTimeSpan);
                    Log.InfoFormat("ResourceFileLoadTimeSpan: {0:c}", _dtSearchBatchIndexer.ResourceFileLoadTimeSpan);
                    Log.InfoFormat("Resource Index Average: {0} ms",
                        _dtSearchBatchIndexer.IndexTimeSpan.TotalMilliseconds /
                        _dtSearchBatchIndexer.IndexedResourceCount);
                    Log.InfoFormat("ResourceFile Insert Average: {0} ms",
                        _dtSearchBatchIndexer.ResourceFileLoadTimeSpan.TotalMilliseconds /
                        _dtSearchBatchIndexer.IndexedDocumentCount);
                    Log.Info("+    +    +    +    +    +    +    +    +    +    +");
                    Log.InfoFormat("totalIndexedResourceCount: {0}", totalIndexedResourceCount);
                    Log.InfoFormat("totalIndexedDocumentCount: {0}", totalIndexedDocumentCount);
                    Log.InfoFormat("totalIndexTimeSpan: {0:c}", totalIndexTimeSpan);
                    Log.InfoFormat("totalResourceFileLoadTimeSpan: {0:c}", totalResourceFileLoadTimeSpan);
                    Log.InfoFormat("Total Run Time: {0:c}", runtimeTimer.Elapsed);
                    Log.InfoFormat("Total Resource Index Average: {0} ms",
                        totalIndexTimeSpan.TotalMilliseconds / totalIndexedResourceCount);
                    Log.InfoFormat("Total ResourceFile Insert Average: {0} ms",
                        totalResourceFileLoadTimeSpan.TotalMilliseconds / totalIndexedDocumentCount);
                    Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");

                    if (_r2UtilitiesSettings.HtmlIndexerMaxIndexBatches > 0 &&
                        _r2UtilitiesSettings.HtmlIndexerMaxIndexBatches == batchNumber)
                    {
                        Log.InfoFormat("MAXIMUM INDEX BATCHES REACHED: {0}",
                            _r2UtilitiesSettings.HtmlIndexerMaxIndexBatches);
                        break;
                    }
                }

                runtimeTimer.Stop();

                TaskResult.Information =
                    $"Resource Count: {totalIndexedResourceCount}, Document Count: {totalIndexedDocumentCount}, Index Time: {totalIndexTimeSpan:c}, Database Update Time: {totalResourceFileLoadTimeSpan:c}, Total Task Time: {runtimeTimer.Elapsed:c}";

                Log.InfoFormat("APP COMPLETE -- run time: {0:c}", runtimeTimer.Elapsed);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        ///     This is only used to create Forthcoming search files and only needs to run 1 time.
        /// </summary>
        public void IndexForthcomingSearchData()
        {
            var runtimeTimer = new Stopwatch();
            runtimeTimer.Start();

            try
            {
                var totalIndexedResourceCount = 0;
                var totalIndexedDocumentCount = 0;
                var totalIndexTimeSpan = new TimeSpan();
                var totalResourceFileLoadTimeSpan = new TimeSpan();


                var resources = _resources.Where(x => !x.NotSaleable && x.StatusId == (int)ResourceStatus.Forthcoming)
                    .ToList();
                var contentLocation = _contentSettings.NewContentLocation;
                //IndexQueueDataService indexQueueDataService = new IndexQueueDataService();
                //indexQueueDataService.AddResourceToQueue(resource.Id, resource.Isbn);
                foreach (var resource in resources)
                {
                    var bsr = new BookSearchResource(resource, contentLocation);
                    if (!bsr.DoesR2BookSearchXmlExist())
                    {
                        bsr.SaveR2BookSearchXml();
                    }

                    _dtSearchBatchIndexer.InsertForthcomingResource(resource);
                }

                _dtSearchBatchIndexer.ProcessForthcoming(TaskResult);


                totalIndexedResourceCount += _dtSearchBatchIndexer.IndexedResourceCount;
                totalIndexedDocumentCount += _dtSearchBatchIndexer.IndexedDocumentCount;
                totalIndexTimeSpan = totalIndexTimeSpan.Add(_dtSearchBatchIndexer.IndexTimeSpan);
                totalResourceFileLoadTimeSpan =
                    totalResourceFileLoadTimeSpan.Add(_dtSearchBatchIndexer.ResourceFileLoadTimeSpan);
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");
                Log.InfoFormat("IndexedResourceCount: {0}", _dtSearchBatchIndexer.IndexedResourceCount);
                Log.InfoFormat("IndexedDocumentCount: {0}", _dtSearchBatchIndexer.IndexedDocumentCount);
                Log.InfoFormat("IndexTimeSpan: {0:c}", _dtSearchBatchIndexer.IndexTimeSpan);
                Log.InfoFormat("ResourceFileLoadTimeSpan: {0:c}", _dtSearchBatchIndexer.ResourceFileLoadTimeSpan);
                Log.InfoFormat("Resource Index Average: {0} ms",
                    _dtSearchBatchIndexer.IndexTimeSpan.TotalMilliseconds / _dtSearchBatchIndexer.IndexedResourceCount);
                Log.InfoFormat("ResourceFile Insert Average: {0} ms",
                    _dtSearchBatchIndexer.ResourceFileLoadTimeSpan.TotalMilliseconds /
                    _dtSearchBatchIndexer.IndexedDocumentCount);
                Log.Info("+    +    +    +    +    +    +    +    +    +    +");
                Log.InfoFormat("totalIndexedResourceCount: {0}", totalIndexedResourceCount);
                Log.InfoFormat("totalIndexedDocumentCount: {0}", totalIndexedDocumentCount);
                Log.InfoFormat("totalIndexTimeSpan: {0:c}", totalIndexTimeSpan);
                Log.InfoFormat("totalResourceFileLoadTimeSpan: {0:c}", totalResourceFileLoadTimeSpan);
                Log.InfoFormat("Total Run Time: {0:c}", runtimeTimer.Elapsed);
                Log.InfoFormat("Total Resource Index Average: {0} ms",
                    totalIndexTimeSpan.TotalMilliseconds / totalIndexedResourceCount);
                Log.InfoFormat("Total ResourceFile Insert Average: {0} ms",
                    totalResourceFileLoadTimeSpan.TotalMilliseconds / totalIndexedDocumentCount);
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}