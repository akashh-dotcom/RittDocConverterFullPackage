#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2V2.Core.Promotion;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.WindowsService.DataServices;
using R2V2.WindowsService.Entities;
using R2V2.WindowsService.Infrastructure.Settings;

#endregion

namespace R2V2.WindowsService.Threads.Promotion
{
    public class PromotionService
    {
        private readonly LicensingDataService _licensingDataService;
        private readonly ILog<PromotionService> _log;
        private readonly PromoteDataService _promoteDataService;
        private readonly ResourceMinDataService _resourceMinDataService;
        private readonly ResourceToPromoteDataService _resourceToPromoteDataService;
        private readonly ResourceUploadQueDataService _resourceUploadQueDataService;

        private readonly StringBuilder _results = new StringBuilder();
        private readonly TransformQueueDataService _transformQueueDataService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly WindowsServiceSettings _windowsServiceSettings;
        private ResourceMin _destinationResourceAfter;

        private ResourceMin _destinationResourceBefore;

        /// <summary>
        /// </summary>
        public PromotionService(ILog<PromotionService> log
            , PromoteDataService promoteDataService
            , WindowsServiceSettings windowsServiceSettings
            , ResourceToPromoteDataService resourceToPromoteDataService
            , LicensingDataService licensingDataService
            , ResourceUploadQueDataService resourceUploadQueDataService
            , ResourceMinDataService resourceMinDataService
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _promoteDataService = promoteDataService;
            _windowsServiceSettings = windowsServiceSettings;
            _resourceToPromoteDataService = resourceToPromoteDataService;
            _licensingDataService = licensingDataService;
            _resourceUploadQueDataService = resourceUploadQueDataService;
            _resourceMinDataService = resourceMinDataService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _transformQueueDataService = new TransformQueueDataService();
        }

        public bool Successful { get; private set; }

        public string XmlSourceDirectory { get; private set; }
        public string XmlDestinationDirectory { get; private set; }
        public string ImageSourceDirectory { get; private set; }
        public string ImageDestinationDirectory { get; private set; }

        public string StagingFrontEndUrl { get; private set; }
        public string StagingBackEndUrl { get; private set; }
        public string ProductionFrontEndUrl { get; private set; }
        public string ProductionBackEndUrl { get; private set; }

        public string Results => _results.ToString();
        public PromoteRequest PromoteRequest { get; private set; }
        public ResourceToPromote ResourceToPromote { get; private set; }

        public List<ResourceToPromote> OverlapResourceIsbns { get; private set; }

        public bool RePromotion { get; private set; }

        public string EmailMessageStatus()
        {
            if (Successful)
            {
                if (OverlapResourceIsbns != null && OverlapResourceIsbns.Any())
                {
                    return "WARNING";
                }

                return "Ok";
            }

            return "ERROR";
        }

        public bool Promote(PromoteRequest promoteRequest)
        {
            Init(promoteRequest);

            Successful = Validate();

            if (Successful)
            {
                var dbResults = RunDatabaseScript();
                foreach (var promote in dbResults)
                {
                    _results.AppendLine(promote.ToDebugString());
                    Successful = Successful && !promote.TableName.Equals("EXCEPTION");
                    _log.DebugFormat("Successful: {0} - {1}", Successful, promote.TableName);
                }
            }

            _destinationResourceAfter = _resourceMinDataService.GetResourceMinByIsbn(ResourceToPromote.Isbn);
            if (_destinationResourceAfter != null && _destinationResourceAfter.Id > 0)
            {
                ProductionBackEndUrl =
                    $@"http://{_windowsServiceSettings.PromoteProductionDomain}/Admin/Resource/Detail/{_destinationResourceAfter.Id}";
                ProductionFrontEndUrl =
                    $@"http://{_windowsServiceSettings.PromoteProductionDomain}/Resource/Title/{_destinationResourceAfter.Isbn}";
            }
            else
            {
                _log.Debug("_destinationResourceAfter is null or _destinationResourceAfter.Id <= 0");
                Successful = false;
            }

            // copy XML, HTML & Images
            Successful = Successful && CopyContentFiles();

            // copy cover image
            Successful = Successful && CopyCoverImage(ResourceToPromote.ImageName);

            // copy media files
            Successful = Successful && CopyMediaFiles(ResourceToPromote.Isbn);

            // add tResourceUploadQue record
            Successful = Successful && AddResourceUploadQueRecord(promoteRequest);

            // Always update the promotion date even if it failed (SJS - 02/9/2016)
            _promoteDataService.SetResourceLastPromotionDate(promoteRequest.ResourceId);

            OverlapResourceIsbns =
                _resourceUploadQueDataService.GetOverlapingResourcesByIsbn(promoteRequest.ResourceId);

            if (Successful)
            {
                // add licenses
                var institutions =
                    _licensingDataService.AddMissingAutoLicenses(true, _windowsServiceSettings.PromoteAutoLicenseCount);
                foreach (var institution in institutions)
                {
                    _results.AppendFormat("Licenses added for {0} resources for institution '{1}', account number: {2}",
                            institution.ResourceLicensesAdded, institution.Name, institution.AccountNumber)
                        .AppendLine();
                }

                // add to transform queue
                var transformQueueId =
                    _transformQueueDataService.Insert(promoteRequest.ResourceId, promoteRequest.Isbn, "A");
                Successful = transformQueueId > 0;
                _results.AppendFormat("Record added to the transform queue, id: {0}.", transformQueueId).AppendLine();
            }
            else
            {
                // on error, update status of destination resource
                UpdateDestinationResourceStatusOnError();
            }

            _log.InfoFormat("Promotion Results: {0}", _results);

            return Successful;
        }

        private void Init(PromoteRequest promoteRequest)
        {
            Successful = false;
            _results.Clear();
            PromoteRequest = promoteRequest;
            ResourceToPromote =
                _resourceToPromoteDataService.GetResourceToPromote(promoteRequest.ResourceId, promoteRequest.Isbn);

            XmlSourceDirectory = $@"{_windowsServiceSettings.PromoteXmlSourceDirectory}{ResourceToPromote.Isbn}\";
            XmlDestinationDirectory =
                $@"{_windowsServiceSettings.PromoteXmlDestinationDirectory}{ResourceToPromote.Isbn}\";
            ImageSourceDirectory = $@"{_windowsServiceSettings.PromoteImagesSourceDirectory}{ResourceToPromote.Isbn}\";
            ImageDestinationDirectory =
                $@"{_windowsServiceSettings.PromoteImagesDestinationDirectory}{ResourceToPromote.Isbn}\";

            StagingBackEndUrl =
                $@"http://{_windowsServiceSettings.PromoteStagingDomain}/Admin/Resource/Detail/{ResourceToPromote.Id}";
            StagingFrontEndUrl =
                $@"http://{_windowsServiceSettings.PromoteStagingDomain}/Resource/Title/{ResourceToPromote.Isbn}";
            ProductionBackEndUrl = null;
            ProductionFrontEndUrl = null;

            _destinationResourceAfter = null;
            _destinationResourceBefore = null;

            RePromotion = false;
        }

        private bool Validate()
        {
            // does resource exist at the source?
            _destinationResourceBefore = _resourceMinDataService.GetResourceMinByIsbn(ResourceToPromote.Isbn);
            if (_destinationResourceBefore != null && _destinationResourceBefore.Id > 0)
            {
                _results.AppendLine("Re-promoting resource");
                RePromotion = true;
            }

            // validate content source directory
            var isValid = ValidateSourceDirectory(XmlSourceDirectory, "XML", false) &&
                          ValidateSourceDirectory(ImageSourceDirectory, "Image", true);
            _log.DebugFormat("Validate() --> {0}", isValid);
            return isValid;
        }

        private bool ValidateSourceDirectory(string path, string description, bool emptyDirectoryOk)
        {
            var dirInfo = new DirectoryInfo(path);
            _log.DebugFormat("exists: {0}, sourcePath: {1}", dirInfo.Exists, path);

            if (!dirInfo.Exists)
            {
                _results.AppendFormat("{0} source directory does not exist, '{1}'", description, path).AppendLine();
                return false;
            }

            var files = dirInfo.GetFiles();
            if (files.Length == 0)
            {
                _log.WarnFormat("{0} source directory is empty, '{1}', emptyDirectoryOk: {2}", description, path,
                    emptyDirectoryOk);
                _results.AppendFormat("{0} source directory is empty, '{1}', empty directory is {2}", description, path,
                        emptyDirectoryOk ? "OK" : "FATAL ERROR")
                    .AppendLine();
                return emptyDirectoryOk;
            }

            _results.AppendFormat("{0} source directory contains {1} file", description, files.Length).AppendLine();
            return true;
        }


        private IEnumerable<Promote> RunDatabaseScript()
        {
            var builder = new SqlConnectionStringBuilder(_windowsServiceSettings.RIT001ProductionConnectionString);

            var destinationDatabaseName = (string)builder["Database"];
            _log.DebugFormat("destinationDatabaseName: {0}", destinationDatabaseName);

            var results = _promoteDataService.PromoteResource(ResourceToPromote.Isbn, destinationDatabaseName);

            foreach (var promote in results)
            {
                _log.Debug(promote.ToDebugString());
            }

            return results;
        }

        private bool CopyContentFiles()
        {
            try
            {
                var successful =
                    CopyDirectory(XmlSourceDirectory, XmlDestinationDirectory, "xml",
                        true); // SJS - 10/30/2013 - Changed to deleted the contents of the directory before copying the new files.
                successful = CopyDirectory(ImageSourceDirectory, ImageDestinationDirectory, "images", true) &&
                             successful;
                _log.DebugFormat("CopyContentFiles() --> {0}", successful);
                return successful;
            }
            catch (Exception ex)
            {
                _log.Info("CopyContentFiles()");
                _log.Error(ex.Message, ex);
                _results.AppendFormat("EXCEPTION: {0}", ex.Message).AppendLine();
                return false;
            }
        }

        private bool CopyCoverImage(string coverImageFileName)
        {
            try
            {
                var sourceFile = $@"{_windowsServiceSettings.PromoteCoverImageSourceDirectory}{coverImageFileName}";
                var destinationFile =
                    $@"{_windowsServiceSettings.PromoteCoverImageDestinationDirectory}{coverImageFileName}";

                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                if (!Directory.Exists(destinationDirectory))
                {

                    _log.DebugFormat("Directory doesn't exist for cover image. Please manually assign one! Destination directory: {0}", destinationDirectory);
                    return true;
                }
                else
                {
                    File.Copy(sourceFile, destinationFile, true);
                    _results.AppendFormat("Cover image copied from '{0}' to '{1}'.", sourceFile, destinationFile)
                        .AppendLine();

                    _log.DebugFormat("CopyCoverImage() --> {0}", true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Info("CopyCoverImage()");
                _log.Error(ex.Message, ex);
                _results.AppendFormat("EXCEPTION: {0}", ex.Message).AppendLine();
                return false;
            }
        }

        private bool CopyMediaFiles(string folderName)
        {
            try
            {
                var sourceFolder = $@"{_windowsServiceSettings.PromoteMediaFilesSourceDirectory}{folderName}";
                var destinationFolder =
                    $@"{_windowsServiceSettings.PromoteMediaFilesDestinationDirectory}{folderName}\";

                var successful = CopyDirectory(sourceFolder, destinationFolder, "media", true);
                _log.DebugFormat("CopyMediaFiles() --> {0}", successful);
                return successful;
            }
            catch (Exception ex)
            {
                _log.Info("CopyMediaFiles()");
                _log.Error(ex.Message, ex);
                _results.AppendFormat("EXCEPTION: {0}", ex.Message).AppendLine();
                return false;
            }
        }

        private bool CopyDirectory(string sourcePath, string destinationPath, string contentType, bool emptyDirectoryOk)
        {
            var fileCopyCount = 0;
            long totalBytesCopied = 0;

            var sourceDirectory = new DirectoryInfo(sourcePath);
            _log.DebugFormat("exists: {0}, sourcePath: {1}", sourceDirectory.Exists, sourcePath);

            var destinationDirectory = new DirectoryInfo(destinationPath);
            _log.DebugFormat("exists: {0}, destinationPath: {1}", destinationDirectory.Exists, destinationPath);

            if (!sourceDirectory.Exists)
            {
                _log.WarnFormat("source directory does not exist: {0}", sourceDirectory);
                return emptyDirectoryOk;
            }

            var filesToCopy = sourceDirectory.GetFiles();
            if (filesToCopy.Length == 0)
            {
                _log.WarnFormat("source directory is empty: {0}", sourceDirectory);
                return emptyDirectoryOk;
            }

            if (destinationDirectory.Exists)
            {
                _log.Debug("deleting destination directory ...");
                destinationDirectory.Delete(true);
                _log.Debug("destination directory deleted");
                Thread.Sleep(
                    1000); // SJS - 10/7/2015 - added this just to check to see if this helps the issue where the copy fails below but then you check the server, the directory has already been deleted. HACK!!!
            }

            destinationDirectory.Create();

            _results.AppendFormat("{0} Source: {1}", contentType, sourceDirectory).AppendLine();
            _results.AppendFormat("{0} Destination: {1}", contentType, destinationPath).AppendLine();

            foreach (var fileInfo in filesToCopy)
            {
                var filename = $@"{destinationPath}{fileInfo.Name}";
                _log.DebugFormat("Copy file to '{0}', {1} bytes", filename, fileInfo.Length);
                fileInfo.CopyTo(filename, true);
                fileCopyCount++;
                totalBytesCopied += fileInfo.Length;
            }

            _log.DebugFormat("{0} files copied, {1:0.000} MB", fileCopyCount, totalBytesCopied / (1024.0m * 1024.0m));
            _results.AppendFormat("{0} files copied, {1:0.000} MB", fileCopyCount,
                totalBytesCopied / (1024.0m * 1024.0m)).AppendLine();

            return fileCopyCount > 0;
        }

        private bool AddResourceUploadQueRecord(PromoteRequest promoteRequest)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    _log.DebugFormat("uow.Session.IsOpen: {0}, uow.Session.IsConnected: {1}", uow.Session.IsOpen,
                        uow.Session.IsConnected);

                    var id = _resourceUploadQueDataService.AddResourceUploadQueRecord(promoteRequest.ResourceId,
                        promoteRequest.Isbn, promoteRequest.PromotedByUser.UserEmailAddress);
                    _log.DebugFormat("id: {0}", id);
                    _log.DebugFormat("AddResourceUploadQueRecord() --> {0}", true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }
        }

        private void UpdateDestinationResourceStatusOnError()
        {
            if (_destinationResourceAfter != null && _destinationResourceAfter.Id > 0)
            {
                // this mean the destination resource was inserted or updated
                if (_destinationResourceBefore != null && _destinationResourceBefore.Id > 0)
                {
                    // update - set status to original state
                    if (_destinationResourceAfter.StatusId != _destinationResourceBefore.StatusId)
                    {
                        _resourceMinDataService.UpdateResourceStatus(_destinationResourceBefore.Id,
                            _destinationResourceBefore.StatusId);
                        _log.DebugFormat("Updated destination resource status to {0}",
                            _destinationResourceBefore.StatusId);
                    }
                    else
                    {
                        _log.DebugFormat("Destination resource status did not change, {0}",
                            _destinationResourceBefore.StatusId);
                    }
                }
                else
                {
                    // inserted - set status to forthcoming
                    _resourceMinDataService.UpdateResourceStatus(_destinationResourceAfter.Id, 8);
                    _log.Debug("Updated destination resource status to forthcoming, 8");
                }

                return;
            }

            _log.DebugFormat("Destination resource does not exist!");
        }
    }
}