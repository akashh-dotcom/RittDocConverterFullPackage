#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Tasks.ContentTasks.Services;
using R2V2.Infrastructure.DependencyInjection;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    /// <summary>
    ///     -AuditXmlContentFilesOnDiskTask -MaxResources=10 -MinResourceId=5300 -MaxResourceId=5400
    /// </summary>
    public class AuditXmlContentFilesOnDiskTask : TaskBase
    {
        private bool _fixIssues;
        private string _isbns;
        private int _maxResourceId = 999999;
        private int _maxResources = 20000;
        private int _minResourceId = 1;
        private bool _restoreBackups;

        public AuditXmlContentFilesOnDiskTask()
            : base("AuditXmlContentFilesOnDiskTask", "-AuditXmlContentFilesOnDiskTask", "28",
                TaskGroup.DiagnosticsMaintenance, "Task to find resources with missing or too many XML files", true)
        {
        }

        private void InitProperties()
        {
            foreach (var arg in CommandLineArguments)
            {
                if (arg == "-FixIssues")
                {
                    _fixIssues = true;
                }

                if (arg == "-RestoreBackups")
                {
                    _restoreBackups = true;
                }

                if (arg.StartsWith("-MaxResources="))
                {
                    _maxResources = int.Parse(arg.Substring(14));
                }

                if (arg.StartsWith("-Isbns="))
                {
                    _isbns = arg.Substring(7);
                }

                if (arg.StartsWith("-MinResourceId="))
                {
                    _minResourceId = int.Parse(arg.Substring(15));
                }

                if (arg.StartsWith("-MaxResourceId="))
                {
                    _maxResourceId = int.Parse(arg.Substring(15));
                }
            }

            Log.InfoFormat("-MaxResources={0}, -MinResourceId={1}, -MaxResourceId={2}", _maxResources, _minResourceId,
                _maxResourceId);
            Log.InfoFormat("-Isbns={0}, -FixIssues={1}, -RestoreBackups={2}", _isbns, _fixIssues, _restoreBackups);
        }

        public override void Run()
        {
            InitProperties();

            TaskResult.Information = "This task will validate XML content files on disk.";
            var step = new TaskResultStep { Name = "AuditXmlContentFilesOnDiskTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var resourceCount = 0;
                var activeStatusCount = 0;
                var archivedStatusCount = 0;
                var backupsRestored = 0;

                var results = new StringBuilder();

                var resourceCoreDataService = new ResourceCoreDataService();
                var auditFilesOnDiskService = ServiceLocator.Current.GetInstance<AuditFilesOnDiskService>();

                var totalRunTime = new Stopwatch();
                totalRunTime.Start();

                if (_restoreBackups)
                {
                    backupsRestored = auditFilesOnDiskService.ResourceBackups(_isbns);
                }

                IList<ResourceCore> resourceCores = resourceCoreDataService.GetActiveAndArchivedResources(true,
                    _minResourceId, _maxResourceId, _maxResources,
                    _isbns == null ? null : _isbns.Split(','));
                var totalResourceCount = resourceCores.Count;

                foreach (var resourceCore in resourceCores)
                {
                    if (resourceCount >= _maxResources)
                    {
                        Log.WarnFormat("MAX RESOURCES reached! _maxResources = {0}", _maxResources);
                        break;
                    }

                    if (resourceCore.StatusId == 6)
                    {
                        activeStatusCount++;
                    }
                    else if (resourceCore.StatusId == 7)
                    {
                        archivedStatusCount++;
                    }

                    resourceCount++;
                    var resourceInfo =
                        $"Id: {resourceCore.Id}, ISBN: {resourceCore.Isbn} - status: {resourceCore.StatusId}, record status: {resourceCore.RecordStatus}";
                    Log.InfoFormat(">>> {0} of {1} - {2}", resourceCount, resourceCores.Count, resourceInfo);

                    results.AppendFormat(resourceInfo);
                    auditFilesOnDiskService.AuditFilesOnDisk(resourceCore);
                }

                var resourcesBackedUp = 0;
                var resourceXmlFileDeleted = 0;
                if (_fixIssues)
                {
                    resourcesBackedUp = auditFilesOnDiskService.BackupResourcesWithExtraContent();
                    resourceXmlFileDeleted = auditFilesOnDiskService.DeleteResourceFilesNotInBookXml();
                }

                var summaryMessage = new StringBuilder();
                summaryMessage.AppendFormat(" {0:#,##0} resources restored from backups", backupsRestored);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} resource processed", totalResourceCount);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} active resources", activeStatusCount);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} archived resources", archivedStatusCount);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} resources caused errors when processing",
                    auditFilesOnDiskService.ErrorResources.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} resources with extra XML content files",
                    auditFilesOnDiskService.ResourcesWithExtraContent.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} resources missing XML content files",
                    auditFilesOnDiskService.ResourcesMissingContent.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} resources without book.xml",
                    auditFilesOnDiskService.ResourcesWithoutTocXml.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} resources valid resources",
                    auditFilesOnDiskService.ValidatedResources.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} files to delete",
                    auditFilesOnDiskService.FilesToDelete.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} missing files",
                    auditFilesOnDiskService.MissingFiles.Count);
                summaryMessage.AppendFormat(",\r\n {0:#,##0} files confirmed in book.xml",
                    auditFilesOnDiskService.BookXmlFilesConfirmed);

                if (_fixIssues)
                {
                    summaryMessage.AppendFormat(",\r\n {0:#,##0} resources backed up", resourcesBackedUp);
                    summaryMessage.AppendFormat(",\r\n {0:#,##0} resource xml files deleted", resourceXmlFileDeleted);
                }

                summaryMessage.AppendFormat(",\r\n -MaxResources = {0:#,##0}", _maxResources);
                summaryMessage.AppendFormat(",\r\n -MinResourceId = {0:#,##0}", _minResourceId);
                summaryMessage.AppendFormat(",\r\n -MaxResourceId = {0:#,##0}", _maxResourceId);
                summaryMessage.AppendFormat(",\r\n -Isbns = {0}", _isbns);
                summaryMessage.AppendFormat(",\r\n -FixIssues = {0}", _fixIssues);
                summaryMessage.AppendFormat(",\r\n -RestoreBackups = {0}", _restoreBackups);

                var emailAttachmentData = new StringBuilder();
                emailAttachmentData.AppendLine(summaryMessage.ToString());
                emailAttachmentData.AppendLine().AppendFormat("Error Resources: {0:#,##0} resources",
                    auditFilesOnDiskService.ErrorResources.Count).AppendLine();
                foreach (var errorResource in auditFilesOnDiskService.ErrorResources)
                {
                    Log.DebugFormat("Error Resource - Id: {0}, ISBN: {1}, Title: {2}", errorResource.Id,
                        errorResource.Isbn, errorResource.Title);
                    emailAttachmentData.AppendFormat(" - Id: {0}, ISBN: {1}, Status: {2} Title: {3}", errorResource.Id,
                            errorResource.Isbn,
                            errorResource.StatusId, errorResource.Title)
                        .AppendLine();
                }

                emailAttachmentData.AppendLine().AppendFormat("Resources Missing toc.xml: {0:#,##0} resources",
                    auditFilesOnDiskService.ResourcesWithoutTocXml.Count).AppendLine();
                foreach (var resourceCore in auditFilesOnDiskService.ResourcesWithoutTocXml)
                {
                    Log.DebugFormat("Resource Missing book.xml - Id: {0}, ISBN: {1}, Title: {2}", resourceCore.Id,
                        resourceCore.Isbn, resourceCore.Title);
                    emailAttachmentData.AppendFormat(" - Id: {0}, ISBN: {1}, Status: {2} Title: {3}", resourceCore.Id,
                            resourceCore.Isbn,
                            resourceCore.StatusId, resourceCore.Title)
                        .AppendLine();
                }

                emailAttachmentData.AppendLine().AppendFormat("Resources With Extra Files: {0:#,##0} resources",
                    auditFilesOnDiskService.ResourcesWithExtraContent.Count).AppendLine();
                foreach (var resourceCore in auditFilesOnDiskService.ResourcesWithExtraContent)
                {
                    Log.DebugFormat("Resources with extra files - Id: {0}, ISBN: {1}, Title: {2}", resourceCore.Id,
                        resourceCore.Isbn, resourceCore.Title);
                    emailAttachmentData.AppendFormat(" - Id: {0}, ISBN: {1}, Status: {2} Title: {3}", resourceCore.Id,
                            resourceCore.Isbn,
                            resourceCore.StatusId, resourceCore.Title)
                        .AppendLine();

                    var result =
                        auditFilesOnDiskService.AuditFilesOnDiskResults.FirstOrDefault(x =>
                            x.ResourceCore.Id == resourceCore.Id);
                    if (result == null)
                    {
                        emailAttachmentData.AppendLine("\tdata not available");
                        continue;
                    }

                    emailAttachmentData.AppendFormat(
                            "\tExtra file count: {0}, Files with bad dates: {1}, Files confirmed in toc.xml: {2}, File on disk: {3}, Files in toc.xml: {4}, Missing files: {5}",
                            result.FilesNotInTocXmlCount, result.FilesDateDiffersFromTocCount,
                            result.FilesToDeleteCount, result.FilesConfirmedInTocXmlCount, result.FilesOnDiskCount,
                            result.FilesReferencedInTocXmlCount)
                        .AppendLine();
                    emailAttachmentData
                        .AppendFormat("\tGood file count if missing deleted: {0}", result.FilesToDeleteCount)
                        .AppendLine();
                }

                emailAttachmentData.AppendLine().AppendFormat("Resources Missing Files: {0:#,##0} files",
                    auditFilesOnDiskService.ResourcesMissingContent.Count).AppendLine();
                foreach (var resourceCore in auditFilesOnDiskService.ResourcesMissingContent)
                {
                    Log.DebugFormat("Resources missing files - Id: {0}, ISBN: {1}, Title: {2}", resourceCore.Id,
                        resourceCore.Isbn, resourceCore.Title);
                    emailAttachmentData.AppendFormat(" - Id: {0}, ISBN: {1}, Status: {2} Title: {3}", resourceCore.Id,
                            resourceCore.Isbn,
                            resourceCore.StatusId, resourceCore.Title)
                        .AppendLine();

                    var result =
                        auditFilesOnDiskService.AuditFilesOnDiskResults.FirstOrDefault(x =>
                            x.ResourceCore.Id == resourceCore.Id);
                    if (result == null)
                    {
                        emailAttachmentData.AppendLine("\tdata not available");
                        continue;
                    }

                    emailAttachmentData.AppendFormat(
                        "\tMissing files: {4}, Files with bad dates: {5}, Files confirmed in toc.xml: {1}, File on disk: {2}, Files in toc.xml: {3}, Extra file count: {0}",
                        result.FilesNotInTocXmlCount, result.FilesConfirmedInTocXmlCount, result.FilesOnDiskCount,
                        result.FilesReferencedInTocXmlCount,
                        result.FilesNotOnDiskCount, result.FilesDateDiffersFromTocCount).AppendLine();
                }

                emailAttachmentData.AppendLine().AppendFormat("Files to Delete: {0:#,##0} files",
                    auditFilesOnDiskService.FilesToDelete.Count).AppendLine();
                foreach (var filename in auditFilesOnDiskService.FilesToDelete)
                {
                    Log.DebugFormat("File to delete: {0}", filename);
                    emailAttachmentData.AppendFormat(" - {0}", filename).AppendLine();
                }

                emailAttachmentData.AppendLine()
                    .AppendFormat("Missing Files: {0:#,##0} files", auditFilesOnDiskService.MissingFiles.Count)
                    .AppendLine();
                foreach (var filename in auditFilesOnDiskService.MissingFiles)
                {
                    Log.DebugFormat("Missing file: {0}", filename);
                    emailAttachmentData.AppendFormat(" - {0}", filename).AppendLine();
                }

                step.Results = summaryMessage.ToString();
                step.CompletedSuccessfully = true;
                step.EndTime = DateTime.Now;
                TaskResult.EmailAttachmentData = emailAttachmentData.ToString();
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