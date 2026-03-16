#region

using System;
using System.Linq;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class UpdateTocXmlTask : TaskBase
    {
        private readonly IContentSettings _contentSettings;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IQueryable<Resource> _resources;
        private readonly TocXmlService _tocXmlService;
        private int _errorCount;
        private string _isbns;
        private int _maxResourceId = 999999;

        private int _maxResourcesToProcess = 25;
        private int _minResourceId = 1;

        private int _resourcesProcessed;

        /// <summary>
        ///     -UpdateTocXmlTask -maxResourcesToProcess=500  -minResourceId=0 -maxResourceId=499
        /// </summary>
        public UpdateTocXmlTask(IQueryable<Resource> resources, IContentSettings contentSettings,
            IR2UtilitiesSettings r2UtilitiesSettings, TocXmlService tocXmlService)
            : base("UpdateTocXmlTask", "-UpdateTocXmlTask", "17", TaskGroup.ContentLoading, "Bulk update toc.xml files",
                true)
        {
            _resources = resources;
            _contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _tocXmlService = tocXmlService;
        }

        public override void Run()
        {
            TaskResult.Information = TaskDescription;
            var step = new TaskResultStep { Name = "UpdateTocXml", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                _isbns = GetArgument("isbns");
                int.TryParse(GetArgument("maxResourcesToProcess") ?? "0", out _maxResourcesToProcess);
                int.TryParse(GetArgument("minResourceId") ?? "1", out _minResourceId);
                int.TryParse(GetArgument("maxResourceId") ?? "999999", out _maxResourceId);

                UpdateTaskResult();

                ProcessResourceTocs();

                if (_errorCount > 0)
                {
                    step.Results = $"Error - {_errorCount} errors, {_resourcesProcessed} resources processed";
                    step.CompletedSuccessfully = false;
                }
                else
                {
                    step.Results = $"OK - {_errorCount} errors, {_resourcesProcessed} resources processed";
                    step.CompletedSuccessfully = true;
                }
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

        private void ProcessResourceTocs()
        {
            int[] validResourceStatusIds = { 6, 7 };
            var query = _resources.Where(x => x.Id >= _minResourceId)
                .Where(x => x.Id <= _maxResourceId)
                .Where(x => validResourceStatusIds.Contains(x.StatusId))
                .OrderByDescending(r => r.Id);

            if (!string.IsNullOrWhiteSpace(_isbns))
            {
                var isbns = _isbns.Split(',');
                query = query.Where(r => isbns.Contains(r.Isbn)).OrderByDescending(r => r.Id);
            }

            var max = _maxResourcesToProcess < 0 ? 999999 : _maxResourcesToProcess;
            var resources = query.Take(max).ToList();

            foreach (var resource in resources)
            {
                _resourcesProcessed++;
                Log.InfoFormat("Processing ISBN: {0}, [{1}], {2} of {3} resource", resource.Isbn, resource.Id,
                    _resourcesProcessed, resources.Count());

                var resourcePaths = new ResourceBackup(resource, _contentSettings, _r2UtilitiesSettings);
                Log.Debug(resourcePaths.ToDebugString());

                var step = _tocXmlService.UpdateTocXml(resource.Isbn, TaskResult, resource.Id);
                UpdateTaskResult();
                if (!step.CompletedSuccessfully)
                {
                    _errorCount++;
                }

                if (_resourcesProcessed >= _maxResourcesToProcess)
                {
                    Log.Info("MAX RESOURCES PROCESSED!");
                    break;
                }
            }
        }
    }
}