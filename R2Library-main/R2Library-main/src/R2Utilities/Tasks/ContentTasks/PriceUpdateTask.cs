#region

using System;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class PriceUpdateTask : TaskBase
    {
        private readonly ILog<PriceUpdateTask> _log;
        private readonly ResourceCoreDataService _resourceCoreDataService;

        public PriceUpdateTask(ResourceCoreDataService resourceCoreDataService, ILog<PriceUpdateTask> log) : base(
            "PriceUpdateTask", "-PriceUpdateTask", "82", TaskGroup.ContentLoading,
            "Update Resource Prices from tResourcePriceUpdate run daily after midnight.",
            true)
        {
            _resourceCoreDataService = resourceCoreDataService;
            _log = log;
        }

        public override void Run()
        {
            TaskResult.Information = TaskDescription;
            var step = new TaskResultStep { Name = "PriceUpdateTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                var itemsToUpdate = _resourceCoreDataService.GetResourcePriceUpdates();
                var itemsUpdated = 0;
                _log.Info($"Resources that need price updates: {itemsToUpdate.Count}");

                var resultsBuilder = new StringBuilder();
                foreach (var resourcePriceUpdateItem in itemsToUpdate)
                {
                    _log.Info(
                        $"Update Price for: [tResourcePriceUpdate.iResourcePriceUpdateId: {resourcePriceUpdateItem.Id}, tResourcePriceUpdate.vchResourceISBN: {resourcePriceUpdateItem.ResourceIsbn}]");
                    var rows = _resourceCoreDataService
                        .UpdateResourcePrice(
                            resourcePriceUpdateItem); //Should be 3: tResourceAudit, tResource, tResourcePriceUpdate
                    _log.Info($"Rows updated: {rows}");
                    resultsBuilder.Append(
                        $"<div> Resource: {resourcePriceUpdateItem.ResourceIsbn} Price Updated: {resourcePriceUpdateItem.ListPrice}</div>");
                    itemsUpdated++;
                }

                step.Results = $"<div>{itemsUpdated} Resource prices updated. </div>{resultsBuilder}";
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