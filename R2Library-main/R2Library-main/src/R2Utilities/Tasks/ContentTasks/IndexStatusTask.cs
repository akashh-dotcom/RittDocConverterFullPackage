#region

using System;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Tasks.ContentTasks.Services;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class IndexStatusTask : TaskBase
    {
        private readonly DtSearchService _dtSearchService;

        public IndexStatusTask(DtSearchService dtSearchService)
            : base("IndexStatusTask", "-IndexStatusTask", "21", TaskGroup.DiagnosticsMaintenance,
                "Task to report the status of the dtSearch index", true)
        {
            _dtSearchService = dtSearchService;
        }

        public override void Run()
        {
            TaskResult.Information = "This task will log the dtSearch index status";
            var step = new TaskResultStep { Name = "IndexStatusTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                UpdateTaskResult();

                var indexStatus = _dtSearchService.GetIndexStatus();

                Log.Info(indexStatus);

                step.Results = indexStatus;
                step.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }
    }
}