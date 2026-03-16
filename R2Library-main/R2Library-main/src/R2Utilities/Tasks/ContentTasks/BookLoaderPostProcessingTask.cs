#region

using System;
using System.IO;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Tasks.ContentTasks.Services;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class BookLoaderPostProcessingTask : TaskBase
    {
        private readonly IContentSettings _contentSettings;
        private readonly LicensingDataService _licensingDataService;
        private readonly ILog<BookLoaderPostProcessingTask> _log;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        private readonly ResourceCoreDataService _resourceCoreDataService;
        private readonly ResourcePracticeAreaDataService _resourcePracticeAreaDataService;
        private readonly ResourceSpecialtyDataService _resourceSpecialtyDataService;
        private readonly TocXmlService _tocXmlService;
        private readonly TransformXmlService _transformXmlService;
        private bool _includeChapterNumbersInToc;

        private string _isbn;

        /// <summary>
        ///     -BookLoaderPostProcessingTask -isbn=1433820579 -includeChapterNumbersInToc=true
        /// </summary>
        /// <param name="contentSettings"> </param>
        /// <param name="transformXmlService"> </param>
        /// <param name="licensingDataService"> </param>
        public BookLoaderPostProcessingTask(ILog<BookLoaderPostProcessingTask> log
            , IR2UtilitiesSettings r2UtilitiesSettings
            , IContentSettings contentSettings
            , TransformXmlService transformXmlService
            , LicensingDataService licensingDataService
            , TocXmlService tocXmlService
        )
            : base("BookLoaderPostProcessingTask", "-BookLoaderPostProcessingTask", "02", TaskGroup.ContentLoading,
                "Book loading task to be run after the Java based book loader", true)
        {
            _log = log;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _contentSettings = contentSettings;
            _transformXmlService = transformXmlService;
            _licensingDataService = licensingDataService;
            _tocXmlService = tocXmlService;
            _resourceCoreDataService = new ResourceCoreDataService();
            _resourceSpecialtyDataService = new ResourceSpecialtyDataService();
            _resourcePracticeAreaDataService = new ResourcePracticeAreaDataService();
        }

        public override void Run()
        {
            _isbn = GetArgument("isbn");
            _includeChapterNumbersInToc = GetArgumentBoolean("includeChapterNumbersInToc", false);

            _log.InfoFormat("========== STARTING BookLoaderPostProcessingTask ==========");
            _log.InfoFormat("ISBN: {0}", _isbn);
            _log.InfoFormat("Include Chapter Numbers in TOC: {0}", _includeChapterNumbersInToc);
            _log.InfoFormat("Validation URL: {0}{1}", _r2UtilitiesSettings.ResourceValidationBaseUrl, _isbn);

            TaskResult.Information = string.Format("Validation URL: <a href=\"{0}{1}\">{0}{1}</a>",
                _r2UtilitiesSettings.ResourceValidationBaseUrl, _isbn);

            var step = new TaskResultStep
            { Name = $"Book Loader Post Processing for ISBN: {_isbn}", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            EmailSubject = $"ISBN: {_isbn}";

            try
            {
                _log.InfoFormat("Retrieving resource by ISBN: {0}", _isbn);
                var resource = _resourceCoreDataService.GetResourceByIsbn(_isbn, false);

                if (resource == null || resource.Id <= 0)
                {
                    var errorMessage = $"CRITICAL FAILURE: Resource not found by ISBN: {_isbn}";
                    step.Results = errorMessage;
                    step.CompletedSuccessfully = false;
                    _log.ErrorFormat(errorMessage);
                    _log.ErrorFormat("========== TASK FAILED - Resource Lookup ==========");
                    throw new Exception(errorMessage);
                }

                _log.InfoFormat("Resource found successfully - ID: {0}, Title: {1}", resource.Id, resource.Title);

                var results = new StringBuilder();
                results.AppendFormat("ISBN: {0}", resource.Isbn).AppendLine();
                results.AppendFormat("Id: {0}", resource.Id).AppendLine();
                results.AppendFormat("Title: {0}<br/>", resource.Title).AppendLine();

                step.Results = results.ToString();

                // step 1
                _log.InfoFormat("========== PHASE 1: Update Resource Data ==========");
                if (!UpdateResourceData(resource))
                {
                    _log.ErrorFormat("PHASE 1 FAILED: Update Resource Data failed for ISBN: {0}", _isbn);
                    _log.ErrorFormat("========== TASK FAILED - Phase 1 ==========");
                    _log.ErrorFormat("Pipeline stopped. Subsequent phases (Copy Content, TOC Update, Transform, Set Active, Licensing) were NOT executed.");
                    return;
                }
                _log.InfoFormat("PHASE 1 COMPLETED SUCCESSFULLY: Resource data updated for ISBN: {0}", _isbn);

                // step 2
                _log.InfoFormat("========== PHASE 2: Copy Content ==========");
                if (!CopyContent(resource.Isbn))
                {
                    _log.ErrorFormat("PHASE 2 FAILED: Copy Content failed for ISBN: {0}", _isbn);
                    _log.ErrorFormat("========== TASK FAILED - Phase 2 ==========");
                    _log.ErrorFormat("Pipeline stopped. Subsequent phases (TOC Update, Transform, Set Active, Licensing) were NOT executed.");
                    return;
                }
                _log.InfoFormat("PHASE 2 COMPLETED SUCCESSFULLY: Content copied for ISBN: {0}", _isbn);

                // step 2A - update the toc.xml with chapter numbers
                if (_includeChapterNumbersInToc)
                {
                    _log.InfoFormat("========== PHASE 2A: Update TOC XML ==========");
                    if (!UpdateTocXml(resource))
                    {
                        _log.ErrorFormat("PHASE 2A FAILED: Update TOC XML failed for ISBN: {0}", _isbn);
                        _log.ErrorFormat("========== TASK FAILED - Phase 2A ==========");
                        _log.ErrorFormat("Pipeline stopped. Subsequent phases (Transform, Set Active, Licensing) were NOT executed.");
                        return;
                    }
                    _log.InfoFormat("PHASE 2A COMPLETED SUCCESSFULLY: TOC XML updated for ISBN: {0}", _isbn);
                }
                else
                {
                    _log.InfoFormat("PHASE 2A SKIPPED: includeChapterNumbersInToc is false");
                }

                // step 3
                _log.InfoFormat("========== PHASE 3: Transform XML Content ==========");
                if (!TransformXmlContent(resource))
                {
                    _log.ErrorFormat("PHASE 3 FAILED: Transform XML Content failed for ISBN: {0}", _isbn);
                    _log.ErrorFormat("========== TASK FAILED - Phase 3 ==========");
                    _log.ErrorFormat("Pipeline stopped. Subsequent phases (Set Active, Licensing) were NOT executed.");
                    return;
                }
                _log.InfoFormat("PHASE 3 COMPLETED SUCCESSFULLY: XML content transformed for ISBN: {0}", _isbn);

                // set resource as active
                _log.InfoFormat("========== PHASE 4: Set Resource Status to Active ==========");
                _log.InfoFormat("Setting resource ID {0} (ISBN: {1}) to Active status", resource.Id, _isbn);
                _resourceCoreDataService.SetResourceStatus(resource.Id, ResourceStatus.Active, TaskName);
                _log.InfoFormat("PHASE 4 COMPLETED SUCCESSFULLY: Resource set to Active status");

                // create license for this resource
                _log.InfoFormat("========== PHASE 5: Add Missing Auto Licenses ==========");
                _log.InfoFormat("Calling AddMissingAutoLicenses with AutoLicensesNumberOfLicenses: {0}", _r2UtilitiesSettings.AutoLicensesNumberOfLicenses);

                var institutions =
                    _licensingDataService.AddMissingAutoLicenses(true,
                        _r2UtilitiesSettings.AutoLicensesNumberOfLicenses);

                if (institutions == null || institutions.Count == 0)
                {
                    _log.WarnFormat("No institutions returned from AddMissingAutoLicenses. This may mean no new licenses were needed or there was an issue.");
                    results.AppendFormat("<br/>No new licenses were added (no institutions returned)").AppendLine();
                }
                else
                {
                    _log.InfoFormat("AddMissingAutoLicenses returned {0} institution(s)", institutions.Count);
                    var totalLicensesAdded = 0;

                    foreach (var institution in institutions)
                    {
                        var licenseMessage = string.Format(
                            "Licenses added for {0} resources for institution '{1}', account number: {2}",
                            institution.ResourceLicensesAdded, institution.Name, institution.AccountNumber);

                        _log.InfoFormat(licenseMessage);
                        totalLicensesAdded += institution.ResourceLicensesAdded;

                        results.AppendFormat(licenseMessage + "<br/>").AppendLine();
                    }

                    _log.InfoFormat("PHASE 5 COMPLETED SUCCESSFULLY: Total of {0} license(s) added across {1} institution(s)",
                        totalLicensesAdded, institutions.Count);
                }

                step.Results = results.ToString();
                step.CompletedSuccessfully = true;

                _log.InfoFormat("========== TASK COMPLETED SUCCESSFULLY ==========");
                _log.InfoFormat("All phases completed for ISBN: {0}", _isbn);
            }
            catch (Exception ex)
            {
                step.Results = ex.Message;
                step.CompletedSuccessfully = false;
                _log.ErrorFormat("========== TASK FAILED WITH EXCEPTION ==========");
                _log.ErrorFormat("Exception Type: {0}", ex.GetType().Name);
                _log.ErrorFormat("Exception Message: {0}", ex.Message);
                _log.ErrorFormat("Exception StackTrace: {0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _log.ErrorFormat("Inner Exception Type: {0}", ex.InnerException.GetType().Name);
                    _log.ErrorFormat("Inner Exception Message: {0}", ex.InnerException.Message);
                }

                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                var duration = step.EndTime - step.StartTime;
                _log.InfoFormat("Total execution time: {0:0.00} seconds", duration.HasValue ? duration.Value.TotalSeconds : 0);
                UpdateTaskResult();
            }
        }

        private bool UpdateResourceData(ResourceCore resource)
        {
            _log.InfoFormat(">+++> STEP 1 - Update Resource Data for ISBN: {0}", _isbn);
            var step = new TaskResultStep
            { Name = $"Update Resource Data for ISBN: {_isbn}", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            var results = new StringBuilder();
            try
            {
                _log.InfoFormat("Processing resource - ID: {0}, Title: {1}", resource.Id, resource.Title);

                var sortTitle = resource.Title;
                var alphaChar = "";
                var originalTitle = resource.Title;

                if (!string.IsNullOrWhiteSpace(sortTitle) && sortTitle.Length >= 5)
                {
                    if (sortTitle.StartsWith("A ", StringComparison.OrdinalIgnoreCase))
                    {
                        sortTitle = $"{sortTitle.Substring(2)}, A";
                        _log.DebugFormat("Title starts with 'A ' - adjusted sort title");
                    }
                    else if (sortTitle.StartsWith("AN ", StringComparison.OrdinalIgnoreCase))
                    {
                        sortTitle = $"{sortTitle.Substring(3)}, {sortTitle.Substring(0, 2)}";
                        _log.DebugFormat("Title starts with 'AN ' - adjusted sort title");
                    }
                    else if (sortTitle.StartsWith("THE ", StringComparison.OrdinalIgnoreCase))
                    {
                        sortTitle = $"{sortTitle.Substring(4)}, {sortTitle.Substring(0, 3)}";
                        _log.DebugFormat("Title starts with 'THE ' - adjusted sort title");
                    }

                    alphaChar = sortTitle.Substring(0, 1);
                }

                _log.InfoFormat("Sort Title Calculation - Original: '{0}', Sort: '{1}', AlphaChar: '{2}'",
                    originalTitle, sortTitle, alphaChar);

                // Parse ISBN to extract ISBN-10, ISBN-13, and eISBN
                string isbn10 = null;
                string isbn13 = null;
                string eisbn = null;

                if (!string.IsNullOrWhiteSpace(_isbn))
                {
                    var cleanIsbn = _isbn.Replace("-", "").Replace(" ", "").Trim();
                    
                    if (cleanIsbn.Length == 13 && cleanIsbn.StartsWith("978"))
                    {
                        // Input is ISBN-13, derive ISBN-10
                        isbn13 = cleanIsbn;
                        isbn10 = DeriveIsbn10FromIsbn13(cleanIsbn);
                        _log.InfoFormat("Parsed ISBN-13: {0}, Derived ISBN-10: {1}", isbn13, isbn10);
                    }
                    else if (cleanIsbn.Length == 10)
                    {
                        // Input is ISBN-10, convert to ISBN-13
                        isbn10 = cleanIsbn;
                        isbn13 = ConvertIsbn10ToIsbn13(cleanIsbn);
                        _log.InfoFormat("Parsed ISBN-10: {0}, Converted ISBN-13: {1}", isbn10, isbn13);
                    }
                    else
                    {
                        _log.WarnFormat("ISBN format not recognized (expected 10 or 13 digits): {0}", _isbn);
                    }
                }

                // set new tResource columns - r.vchResourceSortTitle, r.chrAlphaKey, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn
                _log.InfoFormat("Updating resource fields in database for Resource ID: {0}", resource.Id);
                var resourceUpdateCount =
                    _resourceCoreDataService.UpdateNewResourceFields(resource.Id, sortTitle, alphaChar, isbn10, isbn13, eisbn, TaskName);

                if (resourceUpdateCount == 0)
                {
                    _log.WarnFormat("UpdateNewResourceFields returned 0 - no rows updated for Resource ID: {0}", resource.Id);
                }
                else
                {
                    _log.InfoFormat("UpdateNewResourceFields succeeded - {0} row(s) updated", resourceUpdateCount);
                }

                results.AppendFormat("tResource update count: {0}", resourceUpdateCount);

                // set practice area and specialty
                _log.InfoFormat("Updating resource specialties for Resource ID: {0}", resource.Id);
                var specialtyInsertCount = UpdateResourceSpecialties(resource);
                results.AppendFormat(", tResourceSpecialty insert count: {0}", specialtyInsertCount);

                _log.InfoFormat("Updating resource practice areas for Resource ID: {0}", resource.Id);
                var practiceAreaInsertCount = UpdateResourcePracticeAreas(resource);
                results.AppendFormat(", tResourcePracticeArea insert count: {0}", practiceAreaInsertCount);

                _log.InfoFormat("Updating A-to-Z index terms for Resource ID: {0}", resource.Id);
                var atoZResults = UpdateAtoIndexTerms(resource.Id);
                results.Append(atoZResults);

                step.Results = results.ToString();
                step.CompletedSuccessfully = true;

                _log.InfoFormat("STEP 1 COMPLETED: {0}", results.ToString().Replace("<br/>", " ").Replace("\r\n", " "));
            }
            catch (Exception ex)
            {
                step.Results = ex.Message;
                step.CompletedSuccessfully = false;
                _log.ErrorFormat("STEP 1 FAILED with exception: {0}", ex.Message);
                _log.Error("Exception details:", ex);
            }
            finally
            {
                step.EndTime = DateTime.Now;
                var duration = step.EndTime - step.StartTime;
                _log.InfoFormat("STEP 1 Duration: {0:0.00} seconds, Success: {1}", duration.HasValue ? duration.Value.TotalSeconds : 0, step.CompletedSuccessfully);
                UpdateTaskResult();
            }

            return step.CompletedSuccessfully;
        }

        private bool CopyContent(string isbn)
        {
            _log.InfoFormat(">+++> STEP 2 - Copying Resource Content for ISBN: {0}", _isbn);
            var step = new TaskResultStep
            { Name = $"Copying Resource Content for ISBN: {_isbn}", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();
            var results = new StringBuilder();

            try
            {
                var xmlSourcePath = $@"{_r2UtilitiesSettings.BookLoaderSourceRootDirectory}\{isbn}\xml\";
                var xmlDestinationPath = $@"{_contentSettings.ContentLocation}\{isbn}\";

                _log.InfoFormat("XML Copy - Source: {0}", xmlSourcePath);
                _log.InfoFormat("XML Copy - Destination: {0}", xmlDestinationPath);

                var xmlCopySuccessful = CopyDirectory(xmlSourcePath, xmlDestinationPath, "XML", results);
                _log.InfoFormat("XML Copy Result: {0}", xmlCopySuccessful ? "SUCCESS" : "FAILED");

                var imageSourcePath = $@"{_r2UtilitiesSettings.BookLoaderSourceRootDirectory}\{isbn}\images\";
                var imageDestinationPath = $@"{_r2UtilitiesSettings.BookLoaderImageDestinationDirectory}\{isbn}\";

                _log.InfoFormat("Image Copy - Source: {0}", imageSourcePath);
                _log.InfoFormat("Image Copy - Destination: {0}", imageDestinationPath);

                var imageCopySuccessful = CopyDirectory(imageSourcePath, imageDestinationPath, "Images", results);
                _log.InfoFormat("Image Copy Result: {0}", imageCopySuccessful ? "SUCCESS" : "FAILED");

                step.CompletedSuccessfully = xmlCopySuccessful && imageCopySuccessful;
                step.Results = results.ToString();

                if (!step.CompletedSuccessfully)
                {
                    _log.ErrorFormat("STEP 2 FAILED: XML Copy Success={0}, Image Copy Success={1}",
                        xmlCopySuccessful, imageCopySuccessful);
                }
                else
                {
                    _log.InfoFormat("STEP 2 COMPLETED: Both XML and Image copies succeeded");
                }
            }
            catch (Exception ex)
            {
                step.Results = ex.Message;
                step.CompletedSuccessfully = false;
                _log.ErrorFormat("STEP 2 FAILED with exception: {0}", ex.Message);
                _log.Error("Exception details:", ex);
            }
            finally
            {
                step.EndTime = DateTime.Now;
                var duration = step.EndTime - step.StartTime;
                _log.InfoFormat("STEP 2 Duration: {0:0.00} seconds, Success: {1}", duration.HasValue ? duration.Value.TotalSeconds : 0, step.CompletedSuccessfully);
                UpdateTaskResult();
            }

            return step.CompletedSuccessfully;
        }

        private bool TransformXmlContent(ResourceCore resource)
        {
            _log.InfoFormat(">+++> STEP 3 - Transform Resource Content for ISBN: {0}", _isbn);
            var step = new TaskResultStep
            { Name = $"Transform Resource Content for ISBN: {_isbn}", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                _log.InfoFormat("Starting XML transformation for Resource ID: {0}, ISBN: {1}", resource.Id, resource.Isbn);

                var resourceTransformData = _transformXmlService.TransformResource(resource);

                step.Results = resourceTransformData.ToDebugString();
                step.CompletedSuccessfully = resourceTransformData.Successful;

                if (resourceTransformData.Successful)
                {
                    _log.InfoFormat("STEP 3 COMPLETED: Transformation succeeded for ISBN: {0}", _isbn);
                    _log.DebugFormat("Transform Details: {0}", resourceTransformData.ToDebugString());
                }
                else
                {
                    _log.ErrorFormat("STEP 3 FAILED: Transformation failed for ISBN: {0}", _isbn);
                    _log.ErrorFormat("Transform Details: {0}", resourceTransformData.ToDebugString());
                }
            }
            catch (Exception ex)
            {
                step.Results = ex.Message;
                step.CompletedSuccessfully = false;
                _log.ErrorFormat("STEP 3 FAILED with exception: {0}", ex.Message);
                _log.Error("Exception details:", ex);
            }
            finally
            {
                step.EndTime = DateTime.Now;
                var duration = step.EndTime - step.StartTime;
                _log.InfoFormat("STEP 3 Duration: {0:0.00} seconds, Success: {1}", duration.HasValue ? duration.Value.TotalSeconds : 0, step.CompletedSuccessfully);
                UpdateTaskResult();
            }

            return step.CompletedSuccessfully;
        }

        private bool CopyDirectory(string sourcePath, string destinationPath, string contentType, StringBuilder results)
        {
            var fileCopyCount = 0;
            long totalBytesCopied = 0;

            try
            {
                _log.InfoFormat("CopyDirectory started - Type: {0}", contentType);
                _log.InfoFormat("Source Path: {0}", sourcePath);
                _log.InfoFormat("Destination Path: {0}", destinationPath);

                var sourceDirectory = new DirectoryInfo(sourcePath);
                _log.InfoFormat("Source directory exists: {0}", sourceDirectory.Exists);

                if (!sourceDirectory.Exists)
                {
                    var errorMessage = $"Source directory does not exist: {sourcePath}";
                    _log.ErrorFormat("COPY FAILED: {0}", errorMessage);
                    results.AppendFormat("<div style='color:red;'>ERROR: {0}</div>", errorMessage).AppendLine();
                    return false;
                }

                var destinationDirectory = new DirectoryInfo(destinationPath);
                _log.InfoFormat("Destination directory exists (before processing): {0}", destinationDirectory.Exists);

                if (destinationDirectory.Exists)
                {
                    _log.InfoFormat("Deleting existing destination directory: {0}", destinationPath);
                    destinationDirectory.Delete(true);
                    _log.InfoFormat("Destination directory deleted successfully");
                }

                _log.InfoFormat("Creating destination directory: {0}", destinationPath);
                destinationDirectory.Create();
                _log.InfoFormat("Destination directory created successfully");

                results.AppendFormat("<div>{0} Source: {1}</div>", contentType, sourceDirectory).AppendLine();
                results.AppendFormat("<div>{0} Destination: {1}</div>", contentType, destinationPath).AppendLine();

                var filesToCopy = sourceDirectory.GetFiles();
                _log.InfoFormat("Found {0} file(s) to copy in source directory", filesToCopy.Length);

                if (filesToCopy.Length == 0)
                {
                    _log.WarnFormat("No files found in source directory: {0}", sourcePath);
                }

                foreach (var fileInfo in filesToCopy)
                {
                    var filename = $"{destinationPath}{fileInfo.Name}";
                    _log.DebugFormat("Copying file: {0} ({1:N0} bytes)", fileInfo.Name, fileInfo.Length);

                    fileInfo.CopyTo(filename, true);
                    fileCopyCount++;
                    totalBytesCopied += fileInfo.Length;
                }

                var sizeMB = totalBytesCopied / (1024.0m * 1024.0m);
                _log.InfoFormat("Copy completed - {0} of {1} files copied, {2:0.000} MB total",
                    fileCopyCount, filesToCopy.Length, sizeMB);

                results.AppendFormat("<div>{0} {1} files copied (out of {2} files), {3:0.000} MB</div>"
                    , fileCopyCount, contentType, filesToCopy.Length, sizeMB).AppendLine();

                var success = fileCopyCount == filesToCopy.Length;

                if (!success)
                {
                    _log.ErrorFormat("COPY INCOMPLETE: Only {0} of {1} files were copied", fileCopyCount, filesToCopy.Length);
                }

                return success;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("CopyDirectory FAILED with exception - Type: {0}, Error: {1}", contentType, ex.Message);
                _log.Error("Exception details:", ex);
                results.AppendFormat("<div style='color:red;'>ERROR during {0} copy: {1}</div>", contentType, ex.Message).AppendLine();
                return false;
            }
        }

        private int UpdateResourcePracticeAreas(ResourceCore resource)
        {
            _log.InfoFormat("Checking existing practice areas for Resource ID: {0}", resource.Id);

            var resourcePracticeAreas = _resourcePracticeAreaDataService.GetResourcePracticeArea(resource.Id);

            _log.InfoFormat("Found {0} existing practice area(s)", resourcePracticeAreas.Count);

            if (resourcePracticeAreas.Count == 0)
            {
                _log.InfoFormat("Inserting default practice area '{0}' for Resource ID: {1}",
                    _r2UtilitiesSettings.DefaultPracticeAreaCode, resource.Id);

                var insertCount = _resourcePracticeAreaDataService.Insert(resource.Id,
                    _r2UtilitiesSettings.DefaultPracticeAreaCode, TaskName);

                _log.InfoFormat("Practice area insert completed - {0} row(s) inserted", insertCount);
                return insertCount;
            }

            _log.InfoFormat("Skipping practice area insert - resource already has practice areas");
            return 0;
        }

        private int UpdateResourceSpecialties(ResourceCore resource)
        {
            _log.InfoFormat("Checking existing specialties for Resource ID: {0}", resource.Id);

            var resourceSpecialties = _resourceSpecialtyDataService.GetResourceSpecialty(resource.Id);

            _log.InfoFormat("Found {0} existing specialty(ies)", resourceSpecialties.Count);

            if (resourceSpecialties.Count == 0)
            {
                _log.InfoFormat("Inserting default specialty '{0}' for Resource ID: {1}",
                    _r2UtilitiesSettings.DefaultSpecialtyCode, resource.Id);

                var insertCount = _resourceSpecialtyDataService.Insert(resource.Id,
                    _r2UtilitiesSettings.DefaultSpecialtyCode, TaskName);

                _log.InfoFormat("Specialty insert completed - {0} row(s) inserted", insertCount);
                return insertCount;
            }

            _log.InfoFormat("Skipping specialty insert - resource already has specialties");
            return 0;
        }

        private string UpdateAtoIndexTerms(int resourceId)
        {
            _log.InfoFormat("Starting A-to-Z index update for Resource ID: {0}", resourceId);

            var results = new StringBuilder();
            var atoZIndexDataService = new AtoZIndexDataService();

            _log.InfoFormat("Deleting existing A-to-Z index records for Resource ID: {0}", resourceId);
            var deleteCount = atoZIndexDataService.DeleteAtoZIndexRecordsForResource(resourceId);
            _log.InfoFormat("Deleted {0} existing A-to-Z index record(s)", deleteCount);
            results.AppendFormat(", tAtoZIndex records deleted: {0}", deleteCount);

            _log.InfoFormat("Inserting drug names into A-to-Z index");
            var drugsInsertCount = atoZIndexDataService.InsertDrugNameIntoAtoZIndexForResource(resourceId);
            _log.InfoFormat("Inserted {0} drug name(s)", drugsInsertCount);

            _log.InfoFormat("Inserting drug synonyms into A-to-Z index");
            var drugSynonymsInsertCount = atoZIndexDataService.InsertDrugNameSynonymsIntoAtoZIndexForResource(resourceId);
            _log.InfoFormat("Inserted {0} drug synonym(s)", drugSynonymsInsertCount);

            _log.InfoFormat("Inserting disease names into A-to-Z index");
            var diseaseInsertCount = atoZIndexDataService.InsertDiseaseNamesIntoAtoZIndexForResource(resourceId);
            _log.InfoFormat("Inserted {0} disease name(s)", diseaseInsertCount);

            _log.InfoFormat("Inserting disease synonyms into A-to-Z index");
            var diseaseSynonymsInsertCount = atoZIndexDataService.InsertDiseaseSynonymsIntoAtoZIndexForResource(resourceId);
            _log.InfoFormat("Inserted {0} disease synonym(s)", diseaseSynonymsInsertCount);

            _log.InfoFormat("Inserting keywords into A-to-Z index");
            var keywordsInsertCount = atoZIndexDataService.InsertKeywordsIntoAtoZIndexForResource(resourceId);
            _log.InfoFormat("Inserted {0} keyword(s)", keywordsInsertCount);

            var totalInserts = drugsInsertCount + drugSynonymsInsertCount + diseaseInsertCount +
                             diseaseSynonymsInsertCount + keywordsInsertCount;

            _log.InfoFormat("A-to-Z index update completed - Total: {0} records inserted", totalInserts);

            results.AppendFormat(
                ", tAtoZIndex insert count: {0} [{1} drugs, {2} drug synonyms, {3} diseases, {4} disease synonyms, {5} keywords]",
                totalInserts,
                drugsInsertCount, drugSynonymsInsertCount, diseaseInsertCount, diseaseSynonymsInsertCount,
                keywordsInsertCount
            );

            return results.ToString();
        }

        private bool UpdateTocXml(ResourceCore resource)
        {
            _log.InfoFormat(">+++> STEP 2A - Update TOC XML for ISBN: {0}", resource.Isbn);
            _log.InfoFormat("Calling TocXmlService.UpdateTocXml for Resource ID: {0}", resource.Id);

            var step = _tocXmlService.UpdateTocXml(resource.Isbn, TaskResult, resource.Id);

            _log.InfoFormat("STEP 2A Result: {0}, Details: {1}",
                step.CompletedSuccessfully ? "SUCCESS" : "FAILED",
                step.Results);

            UpdateTaskResult();
            return step.CompletedSuccessfully;
        }

        /// <summary>
        /// Converts ISBN-10 to ISBN-13 by adding "978" prefix and recalculating check digit
        /// </summary>
        private string ConvertIsbn10ToIsbn13(string isbn10)
        {
            if (string.IsNullOrWhiteSpace(isbn10) || isbn10.Length != 10)
            {
                return null;
            }

            var isbn13Base = "978" + isbn10.Substring(0, 9);
            var checkDigit = CalculateIsbn13CheckDigit(isbn13Base);
            return isbn13Base + checkDigit;
        }

        /// <summary>
        /// Derives ISBN-10 from ISBN-13 by removing "978" prefix and recalculating check digit
        /// </summary>
        private string DeriveIsbn10FromIsbn13(string isbn13)
        {
            if (string.IsNullOrWhiteSpace(isbn13) || isbn13.Length != 13 || !isbn13.StartsWith("978"))
            {
                return null;
            }

            var isbn10Base = isbn13.Substring(3, 9);
            var checkDigit = CalculateIsbn10CheckDigit(isbn10Base);
            return isbn10Base + checkDigit;
        }

        /// <summary>
        /// Calculates the check digit for ISBN-13
        /// </summary>
        private string CalculateIsbn13CheckDigit(string isbn12)
        {
            var sum = 0;
            for (var i = 0; i < 12; i++)
            {
                var digit = int.Parse(isbn12[i].ToString());
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            var checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit.ToString();
        }

        /// <summary>
        /// Calculates the check digit for ISBN-10
        /// </summary>
        private string CalculateIsbn10CheckDigit(string isbn9)
        {
            var sum = 0;
            for (var i = 0; i < 9; i++)
            {
                var digit = int.Parse(isbn9[i].ToString());
                sum += digit * (10 - i);
            }
            var remainder = sum % 11;
            var checkDigit = (11 - remainder) % 11;
            return checkDigit == 10 ? "X" : checkDigit.ToString();
        }
    }
}