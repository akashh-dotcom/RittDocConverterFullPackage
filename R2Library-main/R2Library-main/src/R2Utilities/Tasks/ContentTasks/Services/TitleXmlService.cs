#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Infrastructure.Compression;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2Utilities.Tasks.ContentTasks.Services
{
    public class TitleXmlService : R2UtilitiesBase
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(TocXmlService));

        private readonly IContentSettings _contentSettings;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly ResourceCoreDataService _resourceCoreDataService;

        public TitleXmlService(IContentSettings contentSettings,
            IR2UtilitiesSettings r2UtilitiesSettings, ResourceCoreDataService resourceCoreDataService)
        {
            _contentSettings = contentSettings;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _resourceCoreDataService = resourceCoreDataService;
        }

        /// <summary>
        ///     Updates the Title in all XML files for the resource specified
        ///     If TestMode = true the database will not be updated, but the documents will be updated.
        /// </summary>
        public TaskResultStep UpdateTitleXml(ResourceTitleChange resourceTitleChange, TaskResult taskResult,
            bool isTestMode)
        {
            Log.Info($">+++> STEP - Update title xml for ISBN: {resourceTitleChange.Isbn}");
            var step = new TaskResultStep
            {
                Name = $"Update title xml for ISBN: {resourceTitleChange.Isbn}, Id: {resourceTitleChange.ResourceId}",
                StartTime = DateTime.Now
            };
            taskResult.AddStep(step);

            var workingFilePath = GetWorkingFilePath(resourceTitleChange.Isbn, isTestMode);

            var errorMessage = new StringBuilder();
            var warningMessages = new List<string>();

            var documentsUpdated = 0;
            var revertChanges = false;
            ResourceBackup resourceBackup = null;
            try
            {
                resourceBackup = GetResourceBackup(warningMessages, resourceTitleChange, isTestMode);
                if (resourceBackup == null)
                {
                    return step;
                }

                var titleFromXml = GetTitleFromXml(resourceBackup, resourceTitleChange);
                var subTitleFromXml = GetSubTitleFromXml(resourceBackup, resourceTitleChange, false);

                if (string.IsNullOrWhiteSpace(titleFromXml))
                {
                    errorMessage.Append("ERROR - No Title was parsed from the bookXml. The book.xml might not exist");
                    Log.InfoFormat(errorMessage.ToString());
                    return step;
                }

                foreach (var file in resourceBackup.Xml.Files)
                {
                    var fileSucess = ProcessFile(file, resourceTitleChange, titleFromXml, subTitleFromXml,
                        workingFilePath, isTestMode);
                    if (fileSucess == 0)
                    {
                        if (!file.Name.Contains("preface") && !file.Name.Contains("appendix"))
                        {
                            errorMessage.Append($"ERROR - failed to update title nodes in XML for : {file.Name}");
                            Log.InfoFormat(errorMessage.ToString());
                            revertChanges = true;
                            return step;
                        }
                    }

                    documentsUpdated++;
                }

                if (!isTestMode)
                {
                    CopyNewFilesToContentLocation(workingFilePath, resourceBackup.Xml.ResourceDirectory.FullName);

                    var updateSuccess = _resourceCoreDataService.UpdateResourceTitle(resourceTitleChange,
                        _r2UtilitiesSettings.R2UtilitiesDatabaseName);
                    if (!updateSuccess)
                    {
                        errorMessage.Append(
                            $"ERROR - failed to update title in database : {resourceBackup.Xml.ResourceDirectory.Name}");
                        Log.InfoFormat(errorMessage.ToString());
                        revertChanges = true;
                        return step;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                errorMessage.Append($"Exception - {ex.Message}");
            }
            finally
            {
                var results = new StringBuilder();
                if (isTestMode)
                {
                    results.Append("  TEST MODE!! ");
                }

                var documentsFound = 0;
                if (resourceBackup != null)
                {
                    documentsFound = resourceBackup.Xml.Files.Length;
                }

                results.Append($"{documentsFound} total documents, {documentsUpdated} updated documents").AppendLine();


                if (errorMessage.Length != 0)
                {
                    results.AppendLine(errorMessage.ToString());
                }

                foreach (var warningMessage in warningMessages)
                {
                    results.AppendLine(warningMessage);
                }

                step.Results = results.ToString();
                step.HasWarnings = warningMessages.Count > 0;
                step.CompletedSuccessfully = errorMessage.Length == 0 && warningMessages.Count == 0;
                step.EndTime = DateTime.Now;

                if (!isTestMode && revertChanges)
                {
                    RestoreResourceContentDirectory(resourceBackup);
                }

                if (!isTestMode && errorMessage.Length == 0)
                {
                    Directory.Delete(workingFilePath, true);
                }
            }

            return step;
        }

        public string GetTitleFromXml(ResourceBackup resourceBackup, ResourceTitleChange resourceTitleChange)
        {
            var bookXmlFile =
                resourceBackup.Xml.Files.FirstOrDefault(x => x.Name.Contains("book") && !x.Name.Contains("comment"));
            if (bookXmlFile != null)
            {
                var text = File.ReadAllText(bookXmlFile.FullName);

                string title = null;

                var match = Regex.Match(text, @"(?i)<book[^>]*>\s*<title>.*?<\/title>");
                if (match.Success)
                {
                    var indexOfTitleStart = match.Value.IndexOf("<title>", StringComparison.Ordinal) + 7;
                    title = match.Value.Substring(indexOfTitleStart, match.Value.Length - (indexOfTitleStart + 8));
                }

                if (!string.IsNullOrWhiteSpace(title) &&
                    (title.Length > 255 || title.Contains("<") || title.Contains(">")))
                {
                    Log.ErrorFormat(">>> GetTitleFromXml - Title's Length > 255 or contains < > {0}", title);
                    return null;
                }

                return title;
            }

            return null;
        }

        public string GetSubTitleFromXml(ResourceBackup resourceBackup, ResourceTitleChange resourceTitleChange,
            bool isRevert)
        {
            if (!isRevert && resourceTitleChange.UpdateType != ResourceTitleUpdateType.RittenhouseEqualR2TitleAndSub)
            {
                return null;
            }

            var bookXmlFile =
                resourceBackup.Xml.Files.FirstOrDefault(x => x.Name.Contains("book") && !x.Name.Contains("comment"));
            if (bookXmlFile != null)
            {
                var text = File.ReadAllText(bookXmlFile.FullName);

                string subTitle = null;

                var match = Regex.Match(text, "(?i)<subtitle>.*?<\\/subtitle>");
                if (match.Success)
                {
                    subTitle = match.Value.Replace("<subtitle>", "").Replace("</subtitle>", "")
                        .Replace("<emphasis>", "").Replace("</emphasis>", "");
                }

                if (!string.IsNullOrWhiteSpace(subTitle) &&
                    (subTitle.Length > 255 || subTitle.Contains("<") || subTitle.Contains(">")))
                {
                    Log.ErrorFormat(">>> GetSubTitleFromXml - SubTitle's Length > 255 or contains < > {0}", subTitle);
                    return null;
                }

                return subTitle;
            }

            return null;
        }

        public int RestoreXmlFiles(List<ResourceTitleChange> rittenhouseResourceTitles, bool isTestMode)
        {
            var resourcesRestored = 0;
            foreach (var rittenhouseResourceTitle in rittenhouseResourceTitles)
            {
                try
                {
                    Log.Info($"Working on Title: {rittenhouseResourceTitle.Isbn}");
                    var resourceBackup = new ResourceBackup(_contentSettings.ContentLocation,
                        _r2UtilitiesSettings.UpdateTitleTaskXmlBackupLocation, rittenhouseResourceTitle.Isbn);
                    RestoreResourceContentDirectory(resourceBackup);
                    if (resourceBackup.BackupZipFile.Exists)
                    {
                        resourcesRestored++;

                        if (string.IsNullOrWhiteSpace(rittenhouseResourceTitle.AlternateTitle) && !isTestMode)
                        {
                            var originalTitle = GetTitleFromXml(resourceBackup, rittenhouseResourceTitle);
                            var originalSubTitle = GetSubTitleFromXml(resourceBackup, rittenhouseResourceTitle, true);

                            rittenhouseResourceTitle.AlternateTitle = originalTitle;
                            rittenhouseResourceTitle.AlternateSubTitle = originalSubTitle;
                            rittenhouseResourceTitle.IsRevert = true;
                            _resourceCoreDataService.UpdateResourceTitle(rittenhouseResourceTitle,
                                _r2UtilitiesSettings.R2UtilitiesDatabaseName);
                        }
                    }
                    else
                    {
                        Log.Info($"No backup File found for: {resourceBackup.BackupZipFile}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }

            return resourcesRestored;
        }

        public void RestoreResourceContentDirectory(ResourceBackup backup)
        {
            if (backup.BackupZipFile.Exists)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var tempPath = Path.Combine(backup.BackupZipFile.DirectoryName, "temp");
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }

                ZipHelper.ExtractAll(backup.BackupZipFile.FullName, tempPath);
                var test = Directory.GetFiles(Path.Combine(tempPath, "xml"));
                foreach (var filePath in test)
                {
                    var fileInfo = new FileInfo(filePath);
                    if (!Directory.Exists(backup.Xml.ResourceDirectory.FullName))
                    {
                        Directory.CreateDirectory(backup.Xml.ResourceDirectory.FullName);
                    }

                    File.Copy(fileInfo.FullName, Path.Combine(backup.Xml.ResourceDirectory.FullName, fileInfo.Name),
                        true);
                }

                Directory.Delete(tempPath, true);
            }
        }

        private void CopyNewFilesToContentLocation(string workingFilePath, string originalFileName)
        {
            var newFiles = Directory.GetFiles(workingFilePath);
            foreach (var newFile in newFiles)
            {
                var test = new FileInfo(newFile);
                File.Copy(newFile, Path.Combine(originalFileName, test.Name), true);
            }
        }

        private string GetWorkingFilePath(string isbn, bool isTestMode)
        {
            var workingFilePath = Path.Combine(_r2UtilitiesSettings.UpdateTitleTaskWorkingFolder, isbn);

            if (!isTestMode)
            {
                if (Directory.Exists(workingFilePath))
                {
                    Directory.Delete(workingFilePath, true);
                }

                Directory.CreateDirectory(workingFilePath);
            }

            return workingFilePath;
        }

        private ResourceBackup GetResourceBackup(List<string> warnMessages, ResourceTitleChange resourceTitleChange,
            bool isTestMode)
        {
            var resourceBackup = new ResourceBackup(_contentSettings.ContentLocation,
                _r2UtilitiesSettings.UpdateTitleTaskXmlBackupLocation, resourceTitleChange.Isbn);

            if (!resourceBackup.Xml.ResourceDirectory.Exists)
            {
                warnMessages.Add($"ERROR - directory does not exist: {resourceBackup.Xml.ResourceDirectory.Name}");
                Log.InfoFormat(warnMessages.First());
                return null;
            }

            if (!isTestMode)
            {
                var sucess = BackupDirectory(resourceBackup);
                if (!sucess)
                {
                    warnMessages.Add(
                        $"ERROR - failed to backup directory: {resourceBackup.Xml.ResourceDirectory.Name}");
                    Log.InfoFormat(warnMessages.First());
                    return null;
                }
            }

            return resourceBackup;
        }

        private string ValidateAndReplaceText(int bracketCount, string foundValue, string replacementValue,
            string titleInXml)
        {
            var foundGreatCount = foundValue.Count(x => x == '>');
            var foundLessCount = foundValue.Count(x => x == '<');
            if (foundGreatCount != bracketCount || foundLessCount != bracketCount)
            {
                return null;
            }

            return foundValue.Replace(titleInXml, replacementValue);
        }

        private string UpdateTextInFile(string text, string pattern, string replacementValue, string valueToFind,
            int bracketCount, out int replacementCount)
        {
            var replaceCount = 0;
            text = Regex.Replace(text, pattern, m =>
            {
                replaceCount++;
                var updatedText = ValidateAndReplaceText(bracketCount, m.Value, replacementValue, valueToFind);
                if (string.IsNullOrWhiteSpace(updatedText))
                {
                    Log.Error($"Failed to Validate bookMainTitle Found Text: {m.Value}");
                    text = null;
                }

                return updatedText;
            });

            replacementCount = replaceCount;
            return text;
        }

        private int ProcessFile(FileInfo file, ResourceTitleChange resourceTitleChange, string titleToFind,
            string subTitleToFind, string workingLocation, bool isTestMode)
        {
            var text = File.ReadAllText(file.FullName);
            var newTitle = WebUtility.HtmlEncode(resourceTitleChange.GetNewTitle().Trim());

            var bytesTextOrig = GetByteCount(text);
            var bytesTitleOrig = GetByteCount(titleToFind);
            var bytesSubtitleOrig = GetByteCount(subTitleToFind);
            var bytesTitleNew = GetByteCount(newTitle);
            var bytesSubtitleNew = GetByteCount("");

            Log.InfoFormat("File: {0}", file.Name);
            var changeCount = 0;

            var bookMainTitlePattern = $@"(?i)<book[^>]*>\s*<title>{Regex.Escape(titleToFind)}<\/title>";
            var bookTitlePattern = $@"(?i)<bookinfo>\s*<title>{Regex.Escape(titleToFind)}<\/title>";
            var bookContentPattern = $@"(?i)<booktitle>{Regex.Escape(titleToFind)}<\/booktitle>";
            var bookTocPattern = $@"(?i)<\/tocinfo>\s*<title>{Regex.Escape(titleToFind)}<\/title>";
            var subTitlePattern = $@"(?i)<subtitle>{Regex.Escape(titleToFind)}<\/subtitle>";

            text = UpdateTextInFile(text, bookMainTitlePattern, newTitle, titleToFind, 3, out var bookMainTitleCount);
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            changeCount += bookMainTitleCount;

            text = UpdateTextInFile(text, bookTitlePattern, newTitle, titleToFind, 3, out var bookTitleCount);
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            changeCount += bookTitleCount;

            text = UpdateTextInFile(text, bookContentPattern, newTitle, titleToFind, 2, out var bookContentCount);
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            changeCount += bookContentCount;

            text = UpdateTextInFile(text, bookTocPattern, newTitle, titleToFind, 3, out var bookTocCount);
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            changeCount += bookTocCount;

            text = UpdateTextInFile(text, subTitlePattern, newTitle, titleToFind, 2, out var subTitleCount);
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            changeCount += subTitleCount;

            Log.Info(
                $"Total change Count: {changeCount} bookMainTitle Count: {bookMainTitleCount} | bookTitle Count: {bookTitleCount} | bookContent Count: {bookContentCount} | bookToc Count: {bookTocCount} | subTitle Count: {subTitleCount}");

            var bytesTextNew = GetByteCount(text);
            var matchCountBook = bookMainTitleCount + bookTitleCount + bookContentCount + bookTocCount;
            var matchCountSubtitle = subTitleCount;
            var bytesPredicted = PredictByteCount(bytesTextOrig, bytesTitleNew, bytesTitleOrig, bytesSubtitleNew,
                bytesSubtitleOrig, matchCountBook, matchCountSubtitle);

            if (bytesPredicted != bytesTextNew)
            {
                Log.ErrorFormat(
                    $"Byte Count Validation Failed - ISBN:{resourceTitleChange.Isbn}, Predicted Byte Count:{bytesPredicted}, Actual Byte Count:{bytesTextNew}, FileName: {file.Name}");
                return 0;
            }

            if (!isTestMode)
            {
                if (!Directory.Exists(workingLocation))
                {
                    Directory.CreateDirectory(workingLocation);
                }

                var newFileLocation = Path.Combine(workingLocation, file.Name);
                File.WriteAllText(newFileLocation, text, Encoding.UTF8);
            }

            return changeCount;
        }

        private bool BackupDirectory(ResourceBackup resourceBackup)
        {
            try
            {
                var zipFileInfo = new FileInfo(resourceBackup.BackupZipFile.FullName);
                if (zipFileInfo.Exists)
                {
                    zipFileInfo.Delete();
                }

                CompressResourceContentDirectory(resourceBackup.Xml, resourceBackup.BackupZipFile.FullName);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return true;
        }

        private static int GetByteCount(string s)
        {
            var encoding = Encoding.UTF8;
            return s == null ? 0 : encoding.GetByteCount(s);
        }

        private static int PredictByteCount(int bytesTextOrig, int bytesTitleNew, int bytesTitleOrig,
            int bytesSubtitleNew, int bytesSubtitleOrig, int matchCountBook, int matchCountSubtitle)
        {
            var bytesTitleDelta = bytesTitleNew - bytesTitleOrig;
            var bytesSubtitleDelta = bytesSubtitleNew - bytesSubtitleOrig;

            var bytesDelta = matchCountBook * bytesTitleDelta + matchCountSubtitle * bytesSubtitleDelta;
            var bytesTextPredicted = bytesTextOrig + bytesDelta;

            return bytesTextPredicted;
        }
    }
}