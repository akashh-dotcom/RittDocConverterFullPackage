#region

using System;
using System.IO;
using System.Linq;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Compression;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class BackupResourceContentTask : TaskBase
    {
        private readonly IContentSettings _contentSettings;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IQueryable<Resource> _resources;
        private int _exceptionCount;
        private string _isbns;
        private int _maxResourcesToBackup = 25000;

        private int _maxResourcesToProcess = 25000;
        private int _resourcesBackedUp;

        private int _resourcesProcessed;

        /// <summary>
        ///     -BackupResourceContentTask -maxResourcesToBackup=5 -isbns=061526154X,007145912X,007149992X
        /// </summary>
        public BackupResourceContentTask(IQueryable<Resource> resources, IContentSettings contentSettings,
            IR2UtilitiesSettings r2UtilitiesSettings)
            : base("BackupResourceContentTask", "-BackupResourceContentTask", "26", TaskGroup.DiagnosticsMaintenance,
                "Compresses & backs up resource content", true)
        {
            _resources = resources;
            _contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public override void Run()
        {
            TaskResult.Information = TaskDescription;
            var step = new TaskResultStep { Name = "BackupContent", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                _isbns = GetArgument("isbns");
                int.TryParse(GetArgument("maxResourcesToProcess") ?? "0", out _maxResourcesToProcess);
                int.TryParse(GetArgument("maxResourcesToBackup") ?? "0", out _maxResourcesToBackup);

                UpdateTaskResult();

                BackupResources();

                if (_exceptionCount > 0)
                {
                    step.Results =
                        $"Error - {_exceptionCount} exceptions, {_resourcesBackedUp} resources backed up, {_resourcesProcessed} resources processed";
                    step.CompletedSuccessfully = false;
                }
                else
                {
                    step.Results =
                        $"OK - {_resourcesBackedUp} resources backed up, {_resourcesProcessed} resources processed";
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

        private void BackupResources()
        {
            int[] validResourceStatusIds = { 6, 7 };
            IOrderedQueryable<Resource> query;
            if (!string.IsNullOrWhiteSpace(_isbns))
            {
                var isbns = _isbns.Split(',');
                query = _maxResourcesToProcess > 0
                    ? _resources.Where(r => isbns.Contains(r.Isbn)).Take(_maxResourcesToProcess)
                        .OrderByDescending(r => r.Id)
                    : _resources.Where(r => isbns.Contains(r.Isbn)).OrderByDescending(r => r.Id);
            }
            else
            {
                query = _maxResourcesToProcess > 0
                    ? _resources.Take(_maxResourcesToProcess).OrderByDescending(r => r.Id)
                    : _resources.OrderByDescending(r => r.Id);
            }

            var resources = query.Where(x => validResourceStatusIds.Contains(x.StatusId)).ToList();

            foreach (var resource in resources)
            {
                _resourcesProcessed++;
                Log.InfoFormat("Processing ISBN: {0}, {1} of {2} - {3} resources backed up", resource.Isbn,
                    _resourcesProcessed, resources.Count(), _resourcesBackedUp);

                var resourcePaths = new ResourceBackup(resource, _contentSettings, _r2UtilitiesSettings);
                Log.Debug(resourcePaths.ToDebugString());

                if (BackupDirectory(resourcePaths))
                {
                    _resourcesBackedUp++;
                }

                if (_resourcesBackedUp >= _maxResourcesToBackup)
                {
                    Log.Info("MAX RESOURCES BACKED UP!");
                    break;
                }
            }
        }

        private bool BackupDirectory(ResourceBackup resourceBackup)
        {
            if (!resourceBackup.Xml.ResourceDirectory.Exists)
            {
                Log.InfoFormat("-- XML directory does not exist: {0}", resourceBackup.Isbn);
                return false;
            }

            if (!resourceBackup.ResourceBackupRequired)
            {
                Log.InfoFormat("-- ZIP file does not require update: {0}", resourceBackup.Isbn);
                return false;
            }

            try
            {
                var zipFileInfo = new FileInfo(resourceBackup.BackupZipFile.FullName);
                if (zipFileInfo.Exists)
                {
                    zipFileInfo.Delete();
                }

                CompressResourceContentDirectory(resourceBackup.Xml, resourceBackup.BackupZipFile.FullName);
                CompressResourceContentDirectory(resourceBackup.Html, resourceBackup.BackupZipFile.FullName);
                CompressResourceContentDirectory(resourceBackup.Images, resourceBackup.BackupZipFile.FullName);
                CompressResourceContentDirectory(resourceBackup.Media, resourceBackup.BackupZipFile.FullName);

                if (resourceBackup.BookCoverImage.Exists)
                {
                    var bookCoverImageDirectoryName = resourceBackup.BookCoverImage.DirectoryName;
                    if (bookCoverImageDirectoryName != null)
                    {
                        Log.InfoFormat("++ Compressing book cover image file {0}", resourceBackup.BookCoverImage.Name);
                        ZipHelper.CompressFile(resourceBackup.BookCoverImage.FullName,
                            resourceBackup.BackupZipFile.FullName, "book-covers");
                    }
                    else
                    {
                        Log.ErrorFormat("--bookCoverImageDirectoryName is null, for {0}",
                            resourceBackup.BookCoverImage.FullName);
                    }
                }
                else
                {
                    Log.WarnFormat("-- Book cover image file does not exist {0}",
                        resourceBackup.BookCoverImage.FullName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                _exceptionCount++;
            }

            return true;
        }
    }
}