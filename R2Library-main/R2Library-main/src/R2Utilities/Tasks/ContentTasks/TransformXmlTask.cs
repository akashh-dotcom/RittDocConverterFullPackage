#region

using System;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;
using R2Utilities.Tasks.ContentTasks.Xsl;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class TransformXmlTask : TaskBase, ITask
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly TransformXmlService _transformXmlService;
        private bool _emailErrorOnValidationWarnings;
        private string _isbn;

        private int _maxBatchSize;
        private int _maxResourceId;
        private int _minResourceId;
        private bool _orderBatchDescending;
        private ResourceCoreDataService _resourceCoreDataService;
        private TransformQueueDataService _transformQueueDataService;

        /// <summary>
        ///     -TransformXmlTask -logFileSuffix=grp2 -maxBatchSize=2 -minResourceId=1 -maxResourceId=20000
        ///     -orderBatchDescending=false -emailErrorOnValidationWarnings=false
        /// </summary>
        public TransformXmlTask(IR2UtilitiesSettings r2UtilitiesSettings
            , TransformXmlService transformXmlService
        )
            : base("TransformXmlTask", "-TransformXmlTask", "00", TaskGroup.ContentLoading,
                "Transforms resource XML based on TransformQueue table", true)
        {
            //_resources = resources;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _transformXmlService = transformXmlService;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _maxBatchSize = GetArgumentInt32("maxBatchSize", _r2UtilitiesSettings.HtmlIndexerBatchSize);
            _minResourceId = GetArgumentInt32("minResourceId", 0);
            _maxResourceId = GetArgumentInt32("maxResourceId", 100000);
            _orderBatchDescending =
                GetArgumentBoolean("orderBatchDescending", _r2UtilitiesSettings.OrderBatchDescending);
            _isbn = GetArgument("isbn");
            _emailErrorOnValidationWarnings = GetArgumentBoolean("emailErrorOnValidationWarnings", false);

            Log.InfoFormat("-maxBatchSize: {0}, -minResourceId: {1}, -maxResourceId: {2}", _maxBatchSize,
                _minResourceId, _maxResourceId);
            Log.InfoFormat("-orderBatchDescending: {0}, -isbn: {1}, -emailErrorOnValidationWarnings: {2}",
                _orderBatchDescending, _isbn, _emailErrorOnValidationWarnings);

            SetSummaryEmailSetting(false, true, 10);
        }


        public override void Run()
        {
            var taskInfo = new StringBuilder();
            
            var summaryStep = new TaskResultStep
                { Name = "TransformSummary", StartTime = DateTime.Now, Results = string.Empty };
            TaskResult.AddStep(summaryStep);
            UpdateTaskResult();

            // init
            _resourceCoreDataService = new ResourceCoreDataService();
            _transformQueueDataService = new TransformQueueDataService();

            try
            {
                if (string.IsNullOrWhiteSpace(_isbn))
                {
                    taskInfo.AppendFormat("Transform XML for up to {0} resources in the transform queue. ",
                        _maxBatchSize);
                    Log.InfoFormat("Transform XML for up to {0} resources in the transform queue. ", _maxBatchSize);

                    var transformQueueSize = _transformQueueDataService.GetQueueSize();
                    Log.InfoFormat("Transform Queue Size: {0} = number of resources to transform", transformQueueSize);

                    var transformQueue =
                        _transformQueueDataService.GetNext(_orderBatchDescending, _minResourceId, _maxResourceId);
                    //int maxBatchSize = _r2UtilitiesSettings.HtmlIndexerBatchSize;
                    var resourceCount = 0;

                    summaryStep.Results =
                        $"maxBatchSize: {_maxBatchSize}, _minResourceId: {_minResourceId}, _maxResourceId: {_maxResourceId} - ISNBs:";

                    while (transformQueue != null && !string.IsNullOrWhiteSpace(transformQueue.Isbn))
                    {
                        resourceCount++;

                        summaryStep.Results =
                            $"{summaryStep.Results}{(resourceCount == 1 ? "" : ",")}{transformQueue.Isbn}";

                        TransformSingleResource(transformQueue.Isbn, resourceCount, transformQueue, transformQueue.Id,
                            transformQueue.ResourceId);

                        if (resourceCount >= _maxBatchSize)
                        {
                            Log.InfoFormat("MAX BATCH SIZE REACHED: {0}", _maxBatchSize);
                            break;
                        }

                        transformQueueSize = _transformQueueDataService.GetQueueSize();
                        Log.InfoFormat("Transform Queue Size: {0} = number of resources to transform",
                            transformQueueSize);

                        transformQueue =
                            _transformQueueDataService.GetNext(_orderBatchDescending, _minResourceId, _maxResourceId);
                    }

                    if (resourceCount == 0)
                    {
                        var step = new TaskResultStep
                        {
                            Name = "Transform Queue contains zero resources to transform.",
                            StartTime = DateTime.Now,
                            CompletedSuccessfully = true,
                            Results = "No resources",
                            EndTime = DateTime.Now
                        };
                        TaskResult.AddStep(step);
                        UpdateTaskResult();
                    }
                    else
                    {
                        summaryStep.Results = $"Transform count: {resourceCount}, {summaryStep.Results}";
                    }
                }
                else
                {
                    // process single ISBN
                    taskInfo.AppendFormat("Transform XML for ISBN: {0}", _isbn);

                    TransformSingleResource(_isbn, 1, null, -1, -1);

                    summaryStep.Results = $"Single ISBN: {_isbn}";
                }

                summaryStep.CompletedSuccessfully = true;

                if (_transformXmlService.ValidationWarningMessages.Length > 0)
                {
                    var validationWarningStep = new TaskResultStep
                    {
                        Name = "Validation Warning",
                        StartTime = DateTime.Now,
                        Results = _transformXmlService.ValidationWarningMessages,
                        CompletedSuccessfully = !_emailErrorOnValidationWarnings,
                        EndTime = DateTime.Now
                    };
                    TaskResult.AddStep(validationWarningStep);
                    UpdateTaskResult();
                }
            }
            catch (Exception ex)
            {
                summaryStep.Results = $"EXCEPTION: {ex.Message}\r\n\t{summaryStep.Results}";
                summaryStep.CompletedSuccessfully = false;
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                TaskResult.Information = taskInfo.ToString();
                summaryStep.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        /// <param name="transformQueueId"> </param>
        /// <param name="resourceId"> </param>
        private void TransformSingleResource(string isbn, int resourceCount, TransformQueue transformQueue,
            int transformQueueId, int resourceId)
        {
            var step = new TaskResultStep
            {
                Name =
                    $"Transform XML for {isbn}, Resource Id: {resourceId}, Transform Queue Id: {transformQueueId}",
                StartTime = DateTime.Now
            };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var stepResults = new StringBuilder();
            try
            {
                Log.Info("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                Log.InfoFormat("Processing {0} of {1}, ISBN: {2}", resourceCount, _maxBatchSize, isbn);

                stepResults = new StringBuilder();
                var data = ProcessIsbn(isbn);
                stepResults.AppendLine(data.ToDebugString());
                step.CompletedSuccessfully = data.Successful;
                step.HasWarnings = data.HasWarning;

                if (transformQueue != null)
                {
                    transformQueue.DateFinished = DateTime.Now;
                    // T=transformed;
                    // E=Error, 0 files;
                    // F=Fractional(partial), some transformed, some errors
                    transformQueue.Status = data.Successful ? "T" : data.TransferCount > 0 ? "F" : "E";
                    transformQueue.StatusMessage =
                        $"{data.TransferCount} files transformed, {data.ErrorCount} errors, {data.ValidationFailureCount} validation failures";
                    _transformQueueDataService.Update(transformQueue);
                }

                Log.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            }
            catch (Exception ex)
            {
                step.CompletedSuccessfully = false;
                stepResults.AppendLine().AppendFormat("\tEXCEPTION: {0}", ex.Message);
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                step.Results = stepResults.ToString();
                UpdateTaskResult();
            }
        }

        private ResourceTransformData ProcessIsbn(string isbn)
        {
            var resource = _resourceCoreDataService.GetResourceByIsbn(isbn, true);
            var resourceTransformData = _transformXmlService.TransformResource(resource);
            return resourceTransformData;
        }
    }
}