#region

using System;
using System.Diagnostics;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Tasks.ContentTasks.Services;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class DtSearchIndexCompressTask : TaskBase
    {
        private readonly DtSearchService _dtSearchService;

        public DtSearchIndexCompressTask(DtSearchService dtSearchService)
            : base("DtSearchIndexCompressTask", "-DtSearchIndexCompressTask", "x20", TaskGroup.Deprecated,
                "Task to compress the dtSearch index", false)
        {
            _dtSearchService = dtSearchService;
        }

        public override void Run()
        {
            var runtimeTimer = new Stopwatch();
            runtimeTimer.Start();

            var step = new TaskResultStep { Name = "CompressIndex", StartTime = DateTime.Now };
            TaskResult.AddStep(step);

            try
            {
                //DtSearchService dtSearchService = new DtSearchService();
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");
                Log.Info("COMPRESSING INDEX ... (please wait)");

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _dtSearchService.CompressIndex();
                stopwatch.Stop();
                Log.InfoFormat("Index compressed in {0:c}", stopwatch.Elapsed);
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
            }
        }
    }
}