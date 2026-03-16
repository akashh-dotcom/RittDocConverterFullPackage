#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Promotion;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class DoodyUpdateTask : TaskBase
    {
        private readonly DoodyUpdateService _doodyUpdateService;
        private readonly PdaRuleService _pdaRuleService;

        public DoodyUpdateTask(DoodyUpdateService doodyUpdateService, PdaRuleService pdaRuleService)
            : base("DoodyUpdateTask", "-DoodyUpdateTask", "16", TaskGroup.ContentLoading,
                "Task to handle DCT Updates from Prelude and tResource.DoodyReview", true)
        {
            _doodyUpdateService = doodyUpdateService;
            _pdaRuleService = pdaRuleService;
        }

        public override void Run()
        {
            TaskResult.Information =
                "This task will update tResourceCollection with DCT and DCT Essential changes. It also updates Doody Review on the Resource based on Prelude Data.";
            var step = new TaskResultStep { Name = "DoodyUpdateTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var resultBuilder = new StringBuilder();

            try
            {
                var dctResources = _doodyUpdateService.GetDctCoreResources();
                _doodyUpdateService.UpdateDct(out var inserted, out var updated, out var deleted);
                resultBuilder.AppendLine("<div>DCT Update</div>");
                resultBuilder.AppendFormat("<div>Inserted:    {0}</div>", inserted).AppendLine();
                resultBuilder.AppendFormat("<div>Updated:     {0}</div>", updated).AppendLine();
                resultBuilder.AppendFormat("<div>Deleted:     {0}</div>", deleted).AppendLine();
                resultBuilder.AppendLine("<div>&nbsp;</div>");

                var dctEssentialResources = _doodyUpdateService.GetDctEssentialCoreResources();
                _doodyUpdateService.UpdateDctEssential(out inserted, out updated, out deleted);
                resultBuilder.AppendLine("<div>DCT Essential Update</div>");
                resultBuilder.AppendFormat("<div>Inserted:    {0}</div>", inserted).AppendLine();
                resultBuilder.AppendFormat("<div>Updated:     {0}</div>", updated).AppendLine();
                resultBuilder.AppendFormat("<div>Deleted:     {0}</div>", deleted).AppendLine();
                resultBuilder.AppendLine("<div>&nbsp;</div>");

                _doodyUpdateService.UpdateDoodyReview(out inserted, out deleted);
                resultBuilder.AppendLine("<div>Doody Review Update</div>");
                resultBuilder.AppendFormat("<div>Inserted:    {0}</div>", inserted).AppendLine();
                resultBuilder.AppendFormat("<div>Deleted:     {0}</div>", deleted).AppendLine();
                resultBuilder.AppendLine("<div>&nbsp;</div>");

                _doodyUpdateService.UpdateDoodyRating(out inserted, out deleted);
                resultBuilder.AppendLine("<div>Doody Rating Update</div>");
                resultBuilder.AppendFormat("<div>Inserted:    {0}</div>", inserted).AppendLine();
                resultBuilder.AppendFormat("<div>Deleted:     {0}</div>", deleted).AppendLine();
                resultBuilder.AppendLine("<div>&nbsp;</div>");


                var isbns = new List<string>();
                if (dctResources.Any())
                {
                    isbns.AddRange(dctResources.Select(x => x.Isbn));
                }

                if (dctEssentialResources.Any())
                {
                    isbns.AddRange(dctEssentialResources.Select(x => x.Isbn));
                }

                if (isbns.Any())
                {
                    _pdaRuleService.SendOngoingPdaEventToMessageQueue(isbns, OngoingPdaEventType.DoodyUpdate);
                }

                step.Results = resultBuilder.ToString();
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