#region

using System;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class UpdateWithPreludeDataTask : TaskBase
    {
        private readonly PdaRuleService _pdaRuleService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ResourceCoreDataService _resourceCoreDataService;
        protected new string TaskName = "UpdateWithPreludeDataTask";

        public UpdateWithPreludeDataTask(IR2UtilitiesSettings r2UtilitiesSettings, PdaRuleService pdaRuleService)
            : base("UpdateWithPreludeDataTask", "-UpdateWithPreludeDataTask", "09", TaskGroup.ContentLoading,
                "Auto archives R2 resources and set the previous additions based on Prelude Data", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _pdaRuleService = pdaRuleService;
            _resourceCoreDataService = new ResourceCoreDataService();
        }

        public override void Run()
        {
            TaskResult.Information = "This task will YBP Resources with R2library Resources.";
            var step = new TaskResultStep { Name = "UpdateWithPreludeDataTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var resultBuilder = new StringBuilder();

            try
            {
                var truncatedYbpTitles = _resourceCoreDataService.TruncateYbpResources();
                var insertYbpTitles = _resourceCoreDataService.InsertYbpResources();

                resultBuilder.AppendLine($"{truncatedYbpTitles} Deleted Resources");
                resultBuilder.AppendLine($"{insertYbpTitles} Inserted Resources");

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