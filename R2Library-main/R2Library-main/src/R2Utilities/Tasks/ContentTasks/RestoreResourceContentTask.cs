#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Resource;
using R2V2.Extensions;
using R2V2.Infrastructure.Compression;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class RestoreResourceContentTask : TaskBase
    {
        private readonly IContentSettings _contentSettings;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IQueryable<Resource> _resources;

//	    private bool _ftp;
        private bool _fileCopy;
        private string _isbns;

        private int _maxResourcesToProcess = 25000;
        private int _maxResourcesToRestore = 25000;

        private int _resourcesProcessed;

        private int _resourcesRestored;

        //private int _downloadExceptionCount;
        private int _restoreExceptionCount;

        /// <summary>
        ///     -RestoreResourceContentTask -ftp=false -maxResourcesToRestore=10
        ///     -RestoreResourceContentTask -ftp=false -maxResourcesToRestore=500
        ///     -isbns=0000901032,1285458761,0803643683,0128018887
        /// </summary>
        public RestoreResourceContentTask(IQueryable<Resource> resources, IContentSettings contentSettings,
            IR2UtilitiesSettings r2UtilitiesSettings)
            : base("RestoreResourceContentTask", "-RestoreResourceContentTask", "27", TaskGroup.DiagnosticsMaintenance,
                "Copy compressed files locally and restore", true)
        {
            _resources = resources;
            _contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public override void Run()
        {
            TaskResult.Information = TaskDescription;
            var step = new TaskResultStep { Name = "RestoreContent", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                _isbns = GetArgument("isbns");
                int.TryParse(GetArgument("maxResourcesToProcess") ?? "0", out _maxResourcesToProcess);
                int.TryParse(GetArgument("maxResourcesToRestore") ?? "0", out _maxResourcesToRestore);
//                Boolean.TryParse(GetArgument("ftp") ?? "false", out _ftp);
                bool.TryParse(GetArgument("fileCopy") ?? "false", out _fileCopy);

                UpdateTaskResult();

                var downloadedFileCount = 0;

                RestoreResources();

                if (_restoreExceptionCount > 0)
                {
                    step.Results =
                        $"Error - {_restoreExceptionCount} exceptions, {downloadedFileCount} resources downloaded, {_resourcesRestored} resources restored, {_resourcesProcessed} resources processed";
                    step.CompletedSuccessfully = false;
                }
                else
                {
                    step.Results =
                        $"OK - {downloadedFileCount} resources downloaded, {_resourcesRestored} resources restored, {_resourcesProcessed} resources processed";
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

        private void RestoreResources()
        {
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

            var resources = query.ToList();

            var indexQueueDataService = new IndexQueueDataService();
            var transformQueueDataService = new TransformQueueDataService();

            var step = new TaskResultStep { Name = "Restoring resources", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            var stepResults = new StringBuilder();

            foreach (var resource in resources)
            {
                _resourcesProcessed++;
                Log.InfoFormat("Processing ISBN: {0}, {1} of {2} - {3} resources restored", resource.Isbn,
                    _resourcesProcessed, resources.Count(), _resourcesRestored);

                var resourceToRestore = new ResourceToRestore(resource, _contentSettings, _r2UtilitiesSettings);
                Log.Debug(resourceToRestore.ToDebugString());

                var saveResourceRestoreResult = false;
                var resourceRestoreResult = new ResourceRestoreResult
                {
                    Isbn = resourceToRestore.Isbn,
                    BackupFileDateTime = resourceToRestore.BackupZipFile.LastWriteTime,
                    BackupFileFullPath = resourceToRestore.BackupZipFile.FullName,
                    RestoreXmlDirectory = resourceToRestore.Xml.ResourceDirectory.FullName,
                    RestoreStartTime = DateTime.Now
                };

                try
                {
                    if (RestoreToWorkingDirectory(resourceToRestore))
                    {
                        // delete destination directories if they exist
                        resourceToRestore.Xml.ResourceDirectory.Empty();
                        resourceToRestore.Html.ResourceDirectory.Empty();
                        resourceToRestore.Images.ResourceDirectory.Empty();
                        resourceToRestore.Media.ResourceDirectory.Empty();

                        // move files from working directory to content directories
                        resourceRestoreResult.XmlFileCount = MoveFiles(resourceToRestore.XmlWorkingDirectory,
                            resourceToRestore.Xml.ResourceDirectory);
                        resourceRestoreResult.HtmlFileCount = MoveFiles(resourceToRestore.HtmlWorkingDirectory,
                            resourceToRestore.Html.ResourceDirectory);
                        resourceRestoreResult.ImageFileCount = MoveFiles(resourceToRestore.ImagesWorkingDirectory,
                            resourceToRestore.Images.ResourceDirectory);
                        resourceRestoreResult.MediaFileCount = MoveFiles(resourceToRestore.MediaWorkingDirectory,
                            resourceToRestore.Media.ResourceDirectory);

                        // delete book cover images if exist
                        var bookCoverImages = resourceToRestore.BookCoverDirectory.GetFiles(
                            $"{resourceToRestore.Isbn}.*");
                        foreach (var bookCoverImage in bookCoverImages)
                        {
                            bookCoverImage.Delete();
                        }

                        // copy book cover image
                        bookCoverImages = resourceToRestore.BookCoverImageWorkingDirectory.GetFiles(
                            $"{resourceToRestore.Isbn}.*");
                        foreach (var bookCoverImage in bookCoverImages)
                        {
                            var bookCoverImageFullPath = Path.Combine(resourceToRestore.BookCoverDirectory.FullName,
                                bookCoverImage.Name);
                            bookCoverImage.MoveTo(bookCoverImageFullPath);
                            Log.InfoFormat("++ Book cover image file moved to {0}", bookCoverImageFullPath);
                        }

                        if (bookCoverImages.Length == 0)
                        {
                            Log.ErrorFormat("Resource missing cover image - {0}",
                                resourceToRestore.BookCoverImageWorkingDirectory.FullName);
                        }

                        // delete the working directory
                        resourceToRestore.WorkingDirectory.Refresh();
                        resourceToRestore.WorkingDirectory.Empty();
                        resourceToRestore.WorkingDirectory.Delete();

                        if (resourceRestoreResult.HtmlFileCount > 0 && resourceRestoreResult.HtmlFileCount >
                            resourceRestoreResult.XmlFileCount - 5)
                        {
                            indexQueueDataService.AddResourceToQueue(resource.Id, resource.Isbn);
                        }
                        else if (resourceRestoreResult.XmlFileCount > 0)
                        {
                            transformQueueDataService.Insert(resource.Id, resource.Isbn, "A");
                        }

                        stepResults.AppendFormat(
                            "ISBN: {0} - {1} XML files, {2} HTML files, {3} Image files, {4} Media files - Resource Id: {5}",
                            resource.Isbn,
                            resourceRestoreResult.XmlFileCount, resourceRestoreResult.HtmlFileCount,
                            resourceRestoreResult.ImageFileCount,
                            resourceRestoreResult.MediaFileCount, resource.Id).AppendLine();

                        saveResourceRestoreResult = true;
                        resourceRestoreResult.WasRestoreSuccessful = true;

                        _resourcesRestored++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    _restoreExceptionCount++;
                    stepResults.AppendFormat("ISBN: {0} - Exception: {1}", resource.Isbn, ex.Message).AppendLine();

                    saveResourceRestoreResult = true;
                    resourceRestoreResult.WasRestoreSuccessful = false;
                    resourceRestoreResult.ErrorMessage = ex.Message;
                }
                finally
                {
                    resourceRestoreResult.RestoreEndTime = DateTime.Now;

                    if (saveResourceRestoreResult)
                    {
                        var resourceRestoreResultFilePath = GetRestoreResultFilePath(resourceToRestore.Isbn);
                        File.WriteAllText(resourceRestoreResultFilePath, resourceRestoreResult.ToJsonString());

                        if (!resourceRestoreResult.WasRestoreSuccessful)
                        {
                            // save to error directory as well so we have a easy list of resources that errorred out.
                            resourceRestoreResultFilePath = GetRestoreResultFilePath(resourceToRestore.Isbn, true);
                            File.WriteAllText(resourceRestoreResultFilePath, resourceRestoreResult.ToJsonString());
                        }
                    }
                }

                if (_maxResourcesToRestore > 0 && _resourcesRestored >= _maxResourcesToRestore)
                {
                    Log.Info("MAX RESOURCES RESTORED!");
                    break;
                }
            }

            step.Results = stepResults.ToString();
            step.CompletedSuccessfully = _restoreExceptionCount == 0;
            step.EndTime = DateTime.Now;
            UpdateTaskResult();
        }

        private bool RestoreToWorkingDirectory(ResourceToRestore resourceToRestore)
        {
            if (!resourceToRestore.BackupZipFile.Exists)
            {
                Log.InfoFormat("-- Backup file does not exist: {0}", resourceToRestore.Isbn);
                return false;
            }

            var previousRestoreResultFilePath = GetRestoreResultFilePath(resourceToRestore.Isbn);
            if (File.Exists(previousRestoreResultFilePath))
            {
                var json = File.ReadAllText(previousRestoreResultFilePath);
                var previousRestoreResult = ResourceRestoreResult.ParseJson(json);

                if (previousRestoreResult.BackupFileDateTime >= resourceToRestore.BackupZipFile.LastWriteTime)
                {
                    Log.InfoFormat("-- BackupFileDateTime: {0} >= BackupZipFile.LastWriteTime: {1}",
                        previousRestoreResult.BackupFileDateTime, resourceToRestore.BackupZipFile.LastWriteTime);
                    Log.InfoFormat("-- Resources does not need to be restored: {0}", resourceToRestore.Isbn);
                    return false;
                }

                Log.InfoFormat("++ BackupFileDateTime: {0} < BackupZipFile.LastWriteTime: {1}",
                    previousRestoreResult.BackupFileDateTime, resourceToRestore.BackupZipFile.LastWriteTime);
            }

            Log.InfoFormat("++ Restore ISBN: {0}", resourceToRestore.Isbn);

            resourceToRestore.WorkingDirectory.Empty();

            ZipHelper.ExtractAll(resourceToRestore.BackupZipFile.FullName, resourceToRestore.WorkingDirectory.FullName);

            return true;
        }

        private int MoveFiles(DirectoryInfo sourceDirectoryInfo, DirectoryInfo destinationDirectoryInfo)
        {
            if (!sourceDirectoryInfo.Exists)
            {
                Log.InfoFormat("-- Directory does not exist {0}", sourceDirectoryInfo.FullName);
                return 0;
            }

            var fileCount = 0;
            if (!destinationDirectoryInfo.Exists)
            {
                destinationDirectoryInfo.Create();
            }

            var sourceFiles = sourceDirectoryInfo.GetFiles();

            foreach (var sourceFileInfo in sourceFiles)
            {
                // to remove name collusion
                var destinationFileInfo =
                    new FileInfo(Path.Combine(destinationDirectoryInfo.FullName, sourceFileInfo.Name));
                if (destinationFileInfo.Exists)
                {
                    destinationFileInfo.Delete();
                }

                sourceFileInfo.MoveTo(destinationFileInfo.FullName);
                fileCount++;
            }

            Log.InfoFormat("++ {0} files moved to {1}", fileCount, destinationDirectoryInfo.FullName);
            return fileCount;
        }

        /// <summary>
        /// </summary>
        private string GetRestoreResultFilePath(string isbn, bool errorDirectory = false)
        {
            if (errorDirectory)
            {
                return Path.Combine(_r2UtilitiesSettings.ContentBackupDirectory, "_backupResults", "_errors",
                    $"{isbn}_restore-results.json");
            }

            return Path.Combine(_r2UtilitiesSettings.ContentBackupDirectory, "_backupResults",
                $"{isbn}_restore-results.json");
        }
    }
}