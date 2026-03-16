#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using R2V2.Core.Audit;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Email;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Services;
using R2V2.Web.Infrastructure.Email;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public class ResourceAdminService
    {
        private readonly IAdminSettings _adminSettings;
        private readonly AuditService _auditService;
        private readonly ICartService _cartService;
        private readonly ICollectionManagementService _collectionManagementService;
        private readonly ICollectionService _collectionService;
        private readonly EmailSiteService _emailService;
        private readonly IFeaturedTitleService _featuredTitleService;
        private readonly ILog<ResourceAdminService> _log;
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly ResourceQaEmailBuildService _resourceQaEmailBuildService;
        private readonly IResourceService _resourceService;
        private readonly SpecialDiscountResourceService _specialDiscountResourceService;
        private readonly ISpecialtyService _specialtyService;
        private readonly IWebImageSettings _webImageSettings;
        private readonly IWebSettings _webSettings;

        public ResourceAdminService(
            IPracticeAreaService practiceAreaService
            , ISpecialtyService specialtyService
            , ICollectionService collectionService
            , ILog<ResourceAdminService> log
            , IWebImageSettings webImageSettings
            , IResourceService resourceService
            , IWebSettings webSettings
            , AuditService auditService
            , ICollectionManagementService collectionManagementService
            , ICartService cartService
            , SpecialDiscountResourceService specialDiscountResourceService
            , ResourceQaEmailBuildService resourceQaEmailBuildService
            , IAdminSettings adminSettings
            , EmailSiteService emailService
            , IFeaturedTitleService featuredTitleService
        )
        {
            _practiceAreaService = practiceAreaService;
            _specialtyService = specialtyService;
            _collectionService = collectionService;
            _log = log;
            _webImageSettings = webImageSettings;
            _resourceService = resourceService;
            _webSettings = webSettings;
            _auditService = auditService;
            _collectionManagementService = collectionManagementService;
            _cartService = cartService;
            _specialDiscountResourceService = specialDiscountResourceService;
            _resourceQaEmailBuildService = resourceQaEmailBuildService;
            _adminSettings = adminSettings;
            _emailService = emailService;
            _featuredTitleService = featuredTitleService;
        }

        /// <summary>
        ///     Used to set the properties of the Resource before saving.
        /// </summary>
        public string SetResourceProperties(Core.Resource.Resource databaseResource, ResourceEdit resourceEdit)
        {
            var editedResource = resourceEdit.Resource;
            //bool lockedFieldsEditable = editedResource.StatusId == (int)ResourceStatus.Forthcoming && databaseResource.StatusId == (int)ResourceStatus.Forthcoming;
            var lockedFieldsEditable = databaseResource.StatusId == (int)ResourceStatus.Forthcoming;

            var auditData = new StringBuilder();

            if (lockedFieldsEditable && !string.IsNullOrWhiteSpace(editedResource.Title))
            {
                AppendFieldLevelAuditDetails(auditData, editedResource.Title, databaseResource.Title, "Title");
                databaseResource.Title = editedResource.Title;
                AppendFieldLevelAuditDetails(auditData, GetSortByTitle(editedResource.Title),
                    databaseResource.SortTitle, "SortTitle");
                databaseResource.SortTitle = GetSortByTitle(databaseResource.Title);

                // SJS - Added for forthcoming title
                AppendFieldLevelAuditDetails(auditData,
                    string.IsNullOrWhiteSpace(databaseResource.SortTitle)
                        ? ""
                        : GetSortByTitle(databaseResource.SortTitle.Substring(0, 1)), databaseResource.AlphaKey,
                    "AlphaKey");
                databaseResource.AlphaKey = string.IsNullOrWhiteSpace(databaseResource.SortTitle)
                    ? ""
                    : GetSortByTitle(databaseResource.SortTitle.Substring(0, 1));
            }

            AppendFieldLevelAuditDetails(auditData, editedResource.Subtitle, databaseResource.SubTitle, "SubTitle");
            databaseResource.SubTitle = editedResource.Subtitle;


            AppendFieldLevelAuditDetails(auditData, editedResource.StatusId, databaseResource.StatusId, "StatusId");
            databaseResource.StatusId = editedResource.StatusId;

            if (lockedFieldsEditable)
            {
                AppendFieldLevelAuditDetails(auditData, editedResource.DueDate, databaseResource.ForthcomingDate,
                    "ForthcomingDate");
                databaseResource.ForthcomingDate = editedResource.DueDate;
            }

            AppendFieldLevelAuditDetails(auditData, editedResource.ReleaseDate, databaseResource.ReleaseDate,
                "ReleaseDate");
            databaseResource.ReleaseDate = editedResource.ReleaseDate;

            AppendFieldLevelAuditDetails(auditData, editedResource.IsDrugMonograph, databaseResource.DrugMonograph,
                "DrugMonograph");
            databaseResource.DrugMonograph = editedResource.IsDrugMonograph;

            AppendFieldLevelAuditDetails(auditData, editedResource.ListPrice, databaseResource.ListPrice, "ListPrice");
            databaseResource.ListPrice = editedResource.ListPrice;

            AppendFieldLevelAuditDetails(auditData, editedResource.BundlePrice3, databaseResource.BundlePrice3,
                "BundlePrice3");
            databaseResource.BundlePrice3 = editedResource.BundlePrice3;

            if (lockedFieldsEditable)
            {
                AppendFieldLevelAuditDetails(auditData, editedResource.Authors, databaseResource.Authors, "Authors");
                databaseResource.Authors = editedResource.Authors;
            }

            AppendFieldLevelAuditDetails(auditData, editedResource.AdditionalContributors,
                databaseResource.AdditionalContributors, "AdditionalContributors");
            databaseResource.AdditionalContributors = editedResource.AdditionalContributors;

            //Publisher cannot be set. Only the Publisher Id.
            if (lockedFieldsEditable)
            {
                AppendFieldLevelAuditDetails(auditData, editedResource.PublisherId, databaseResource.PublisherId,
                    "PublisherId");
                databaseResource.PublisherId = editedResource.PublisherId;
            }

            if (editedResource.PublicationDateYear > 0)
            {
                AppendFieldLevelAuditDetails(auditData, DateTime.Parse($"1/1/{editedResource.PublicationDateYear}"),
                    databaseResource.PublicationDate, "PublicationDate");
                databaseResource.PublicationDate = DateTime.Parse($"1/1/{editedResource.PublicationDateYear}");
            }

            AppendFieldLevelAuditDetails(auditData, editedResource.DoodyReview, databaseResource.DoodyReview,
                "DoodyReview");
            databaseResource.DoodyReview = editedResource.DoodyReview;

            AppendFieldLevelAuditDetails(auditData, editedResource.NlmCall, databaseResource.NlmCall, "NlmCall");
            databaseResource.NlmCall = editedResource.NlmCall;

            AppendFieldLevelAuditDetails(auditData,
                !string.IsNullOrWhiteSpace(editedResource.Edition) ? editedResource.Edition.Trim() : null,
                databaseResource.Edition, "Edition");
            databaseResource.Edition = !string.IsNullOrWhiteSpace(editedResource.Edition)
                ? editedResource.Edition.Trim()
                : null; // SJS - 1/23/2014 - trim edition text - https://www.squishlist.com/technotects/r2cl/60/

            AppendFieldLevelAuditDetails(auditData, editedResource.ResourceDescription, databaseResource.Description,
                "Description");
            databaseResource.Description = editedResource.ResourceDescription;

            //ISBN changes are handled in the ValidateAndSetIsbns
            var isbn10 = string.IsNullOrWhiteSpace(editedResource.Isbn10)
                ? editedResource.Isbn10
                : editedResource.Isbn10.Trim();
            var isbn13 = string.IsNullOrWhiteSpace(editedResource.Isbn13)
                ? editedResource.Isbn13
                : editedResource.Isbn13.Trim();
            var eIsbn = string.IsNullOrWhiteSpace(editedResource.EIsbn)
                ? editedResource.EIsbn
                : editedResource.EIsbn.Trim();

            if (lockedFieldsEditable)
            {
                if (!string.IsNullOrWhiteSpace(isbn10))
                {
                    AppendFieldLevelAuditDetails(auditData, isbn10, databaseResource.Isbn10, "Isbn10");
                    //databaseResource.Isbn10 = isbn10;
                }

                if (!string.IsNullOrWhiteSpace(isbn13))
                {
                    AppendFieldLevelAuditDetails(auditData, isbn13, databaseResource.Isbn13, "Isbn13");
                    //databaseResource.Isbn13 = isbn13;
                }
            }

            AppendFieldLevelAuditDetails(auditData, eIsbn, databaseResource.EIsbn, "EIsbn");
            //databaseResource.EIsbn = eIsbn;

            if (editedResource.ImageFileName != null && databaseResource.ImageFileName != editedResource.ImageFileName)
            {
                AppendFieldLevelAuditDetails(auditData, editedResource.ImageFileName, databaseResource.ImageFileName,
                    "ImageFileName");
                databaseResource.ImageFileName = editedResource.ImageFileName;
            }

            AppendFieldLevelAuditDetails(auditData, editedResource.PageCount, databaseResource.PageCount, "PageCount");
            databaseResource.PageCount = editedResource.PageCount;

            AppendFieldLevelAuditDetails(auditData, editedResource.ContainsVideo, databaseResource.ContainsVideo,
                "ContainsVideo");
            databaseResource.ContainsVideo = editedResource.ContainsVideo;

            AppendFieldLevelAuditDetails(auditData, editedResource.ExcludeFromAutoArchive,
                databaseResource.ExcludeFromAutoArchive, "ExcludeFromAutoArchive");
            databaseResource.ExcludeFromAutoArchive = editedResource.ExcludeFromAutoArchive;

            if (!databaseResource.NotSaleable && editedResource.NotSaleable)
            {
                var notSaleableDate = DateTime.Now;
                AppendFieldLevelAuditDetails(auditData, true, databaseResource.NotSaleable, "NotSaleable");
                databaseResource.NotSaleable = true;

                AppendFieldLevelAuditDetails(auditData, notSaleableDate, databaseResource.NotSaleableDate,
                    "NotSaleableDate");
                databaseResource.NotSaleableDate = notSaleableDate;
            }
            else if (databaseResource.NotSaleable && !editedResource.NotSaleable)
            {
                AppendFieldLevelAuditDetails(auditData, false, databaseResource.NotSaleable, "NotSaleable");
                databaseResource.NotSaleable = false;

                AppendFieldLevelAuditDetails(auditData, null, databaseResource.NotSaleableDate, "NotSaleableDate");
                databaseResource.NotSaleableDate = null;
            }

            databaseResource.IsFreeResource = editedResource.IsFreeResource;

            if (databaseResource.IsFreeResource)
            {
                AppendFieldLevelAuditDetails(auditData, 0, databaseResource.ListPrice, "ListPrice");
                databaseResource.ListPrice = 0;
            }

            if (!databaseResource
                    .AffiliationUpdatedByPrelude) // resourceEdit.AffiliationUpdatedByPrelude will not be posted back. rely on the coreresource
            {
                databaseResource.Affiliation = editedResource.Affiliation;
            }

            SetSelectedPracticeAreas(databaseResource, resourceEdit.PracticeAreaSelected, auditData);
            SetSelectedSpecialties(databaseResource, resourceEdit.SpecialtiesSelected, auditData);
            SetSelectedCollections(databaseResource, resourceEdit.CollectionsSelected, auditData);
            return auditData.ToString();
        }

        /// <summary>
        ///     Returns Null if image was processed properly or no image.
        ///     Returns ModelState Error Message for "Resource.ImageUrl" if error
        /// </summary>
        public Dictionary<string, string> SaveBookCoverImage(ResourceEdit resourceEdit, HttpPostedFileBase file)
        {
            resourceEdit.SetBookCoverLimits(_webImageSettings);
            var errorMessageDictionary = new Dictionary<string, string>();
            if (file == null || file.ContentLength == 0)
            {
                _log.Debug("File is null or empty");
                return errorMessageDictionary;
            }

            var maxImageSize = 1024 * _webImageSettings.BookCoverMaxSizeInKb;
            _log.DebugFormat("file.ContentLength: {0}, file.ContentType: {1}, file.FileName: {2}, maxImageSize: {3}",
                file.ContentLength, file.ContentType, file.FileName, maxImageSize);
            if (file.ContentLength > maxImageSize)
            {
                _log.WarnFormat(
                    "Image too large, file.ContentLength: {0} bytes, _webImageSettings.BookCoverMaxSizeInKb: {1} KB",
                    file.ContentLength, _webImageSettings.BookCoverMaxSizeInKb);
                errorMessageDictionary.Add("Resource.ImageUrl",
                    $"Image can not exceed {_webImageSettings.BookCoverMaxSizeInKb} KB");
                return errorMessageDictionary;
            }

            var extension = GetFileExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                _log.Warn("Invalid file extension.");
                errorMessageDictionary.Add("Resource.ImageUrl",
                    "Invalid file type. Only .PNG, .JPG, .JPEG and .GIF are supported.");
                return errorMessageDictionary;
            }

            var tempFileName = Path.GetFileName(file.FileName);
            _log.InfoFormat("tempFileName: '{0}'", tempFileName);
            var imageFilename = $"{resourceEdit.Resource.Isbn}.{extension}";
            _log.InfoFormat("imageFilename: '{0}'", imageFilename);

            var directoryInfo = new DirectoryInfo(_webImageSettings.BookCoverDirectory);
            _log.DebugFormat("_institutionSettings.LocalImageLocation: {0}, directoryInfo.Exists: {1}",
                _webImageSettings.BookCoverDirectory,
                directoryInfo.Exists);

            if (tempFileName == null)
            {
                _log.Error("tempFileName is null, this should never happen");
            }

            var tempPath = Path.Combine($"{_webImageSettings.BookCoverDirectory}\\Temp", imageFilename);
            _log.DebugFormat("path: {0}", tempPath);
            if (File.Exists(tempPath))
            {
                _log.Debug("Path Already Exists Deleting Old");
                File.Delete(tempPath);
            }

            file.SaveAs(tempPath);
            _log.DebugFormat("File Saved, {0}", tempPath);

            using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
            {
                using (var imageFile = Image.FromStream(fs))
                {
                    if (imageFile.Width > _webImageSettings.BookCoverMaxWidth ||
                        imageFile.Height > _webImageSettings.BookCoverMaxHeight)
                    {
                        _log.DebugFormat("File's Width: {0} || Height: {1}", imageFile.Width, imageFile.Height);
                        imageFile.Dispose();
                        errorMessageDictionary.Add("Resource.ImageUrl",
                            "Your image file is not within the specific size restrictions. Please resize your image and upload again.");
                        return errorMessageDictionary;
                    }
                }
            }

            var fileInfo = new FileInfo(tempPath);
            var imageFilePath = Path.Combine(_webImageSettings.BookCoverDirectory, imageFilename);
            _log.DebugFormat("imageFilePath: {0}", imageFilePath);
            fileInfo.CopyTo(imageFilePath, true);
            _log.Debug("File successfully copied");
            resourceEdit.Resource.ImageFileName = imageFilename;
            return errorMessageDictionary;
        }

        private Dictionary<string, string> ValidateResource(ResourceEdit resourceEdit, Core.Resource.Resource resource)
        {
            var errorDictionary = new Dictionary<string, string>();

            //This is for when you change a resource to forthcoming.
            if (string.IsNullOrWhiteSpace(resource.Title) && resource.StatusId == (int)ResourceStatus.Forthcoming &&
                string.IsNullOrWhiteSpace(resourceEdit.Resource.Title))
            {
                errorDictionary.Add("Resource.Title", "Title cannot be null. Please try again.");
            }

            if (string.IsNullOrWhiteSpace(resource.Authors) && resource.StatusId == (int)ResourceStatus.Forthcoming &&
                string.IsNullOrWhiteSpace(resourceEdit.Resource.Authors))
            {
                errorDictionary.Add("Resource.Authors", "Primary Author cannot be null. Please try again.");
            }

            return errorDictionary;
        }

        private Dictionary<string, string> ValidateAndSetIsbns(ResourceEdit resourceEdit,
            Core.Resource.Resource databaseResource, bool statusChangedToForthcoming)
        {
            //Active and Archived ISBN10 and ISBN13 are not editable.
            if (databaseResource.StatusId == (int)ResourceStatus.Forthcoming && !statusChangedToForthcoming)
            {
                if (string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn10) &&
                    string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn13))
                {
                    return new Dictionary<string, string>
                    {
                        { "Resource.Isbn10", @"ISBN 10 and ISBN 13 cannot be empty. Please try again" },
                        { "Resource.Isbn13", @"ISBN 13 and ISBN 10 cannot be empty. Please try again" }
                    };
                }
            }

            Dictionary<string, string> errorDictionary;

            if (statusChangedToForthcoming)
            {
                errorDictionary = ValidateAndSetOtherIsbns(resourceEdit, databaseResource);
            }
            else
            {
                errorDictionary = ValidateAndSetBaseIsbn(resourceEdit, databaseResource);
                if (!errorDictionary.Any())
                {
                    errorDictionary = ValidateIsbn13(resourceEdit);
                    if (!errorDictionary.Any())
                    {
                        errorDictionary = ValidateAndSetOtherIsbns(resourceEdit, databaseResource);
                    }
                }
            }

            return errorDictionary;
        }

        private Dictionary<string, string> ValidateAndSetBaseIsbn(ResourceEdit resourceEdit,
            Core.Resource.Resource newResource)
        {
            var errorDictionary = new Dictionary<string, string>();
            var isbn10 = string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn10)
                ? null
                : resourceEdit.Resource.Isbn10.Trim();
            var isbn13 = string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn13)
                ? null
                : resourceEdit.Resource.Isbn13.Trim();

            if (IsIsbn13Prefix979(isbn13))
            {
                isbn10 = null;
                resourceEdit.Resource.Isbn10 = null;
            }

            var isbn = string.IsNullOrWhiteSpace(isbn10) && !string.IsNullOrWhiteSpace(isbn13)
                ? isbn13
                : isbn10;

            var testResource = _resourceService.GetResource(isbn);

            if (testResource != null && testResource.Id != newResource.Id)
            {
                if (isbn.Length == 10)
                {
                    errorDictionary.Add("Resource.Isbn10", @"This ISBN 10 is already in use. Please try again");
                }
                else
                {
                    errorDictionary.Add("Resource.Isbn13", @"This ISBN 13 is already in use. Please try again");
                }

                return errorDictionary;
            }

            if (string.IsNullOrWhiteSpace(newResource.Isbn) && !string.IsNullOrWhiteSpace(isbn))
            {
                newResource.Isbn = isbn;
            }

            return errorDictionary;
        }

        private Dictionary<string, string> ValidateIsbn13(ResourceEdit resourceEdit)
        {
            var errorDictionary = new Dictionary<string, string>();

            var isbn10 = string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn10)
                ? null
                : resourceEdit.Resource.Isbn10.Trim();
            var isbn13 = string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn13)
                ? null
                : resourceEdit.Resource.Isbn13.Trim();

            string convertedIsbn13 = null;

            if (IsIsbn13Prefix979(isbn13))
            {
                resourceEdit.Resource.Isbn10 = null;
                return errorDictionary;
            }

            if (!string.IsNullOrWhiteSpace(isbn10))
            {
                convertedIsbn13 = _resourceService.ConvertIsbn10To13(isbn10);

                if (convertedIsbn13 != isbn13 && !string.IsNullOrWhiteSpace(isbn13) &&
                    !string.IsNullOrWhiteSpace(convertedIsbn13))
                {
                    errorDictionary.Add("Resource.Isbn13",
                        $"The ISBN 13 does not appear to be correct for the ISBN 10 entered. Should the ISBN 13 be '{convertedIsbn13}'.");
                }
            }

            if (string.IsNullOrWhiteSpace(isbn13) && !string.IsNullOrWhiteSpace(convertedIsbn13))
            {
                resourceEdit.Resource.Isbn13 = convertedIsbn13;
            }

            return errorDictionary;
        }

        private static bool IsIsbn13Prefix979(string isbn13)
        {
            if (string.IsNullOrWhiteSpace(isbn13))
            {
                return false;
            }

            var cleaned = isbn13.Replace("-", string.Empty).Replace(" ", string.Empty);
            return cleaned.StartsWith("979", StringComparison.Ordinal);
        }

        private Dictionary<string, string> ValidateAndSetOtherIsbns(ResourceEdit resourceEdit,
            Core.Resource.Resource resource)
        {
            var errorDictionary = new Dictionary<string, string>();

            var isbn10 = string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn10)
                ? null
                : resourceEdit.Resource.Isbn10.Trim();
            var isbn13 = string.IsNullOrWhiteSpace(resourceEdit.Resource.Isbn13)
                ? null
                : resourceEdit.Resource.Isbn13.Trim();
            var eIsbn = string.IsNullOrWhiteSpace(resourceEdit.Resource.EIsbn)
                ? null
                : resourceEdit.Resource.EIsbn.Trim();

            //this will only be triggered if the title != forthcoming and there is no eISBN
            if (string.IsNullOrWhiteSpace(isbn10) && string.IsNullOrWhiteSpace(isbn13) &&
                string.IsNullOrWhiteSpace(eIsbn))
            {
                resource.EIsbn = null;
                return errorDictionary;
            }

            var overLappingResourceIsbns =
                _resourceService.GetDuplicateIsbns(isbn10, isbn13, eIsbn, resourceEdit.Resource.Id);
            //Figure out which isbn is overlapping
            if (overLappingResourceIsbns != null && overLappingResourceIsbns.Any())
            {
                var overlapIsbn10 = overLappingResourceIsbns.Where(x => x.Isbn10 == isbn10).ToList();
                var overlapIsbn13 = overLappingResourceIsbns.Where(x => x.Isbn13 == isbn13).ToList();
                var overlapEIsbn = overLappingResourceIsbns.Where(x => x.EIsbn == eIsbn).ToList();
                var sb = new StringBuilder();
                if (overlapIsbn10.Any())
                {
                    sb.Append("Conflicts with Resource Titles: ");
                    overlapIsbn10.ForEach(x => sb.AppendFormat("  \"{0}\"  ", x.Title));
                    errorDictionary.Add("Resource.Isbn10", sb.ToString());
                    return errorDictionary;
                }

                if (overlapIsbn13.Any())
                {
                    sb = new StringBuilder();
                    sb.Append("Conflicts with Resource Titles: ");
                    overlapIsbn13.ForEach(x => sb.AppendFormat("  \"{0}\"  ", x.Title));
                    errorDictionary.Add("Resource.Isbn13", sb.ToString());
                    return errorDictionary;
                }

                if (overlapEIsbn.Any())
                {
                    sb = new StringBuilder();
                    sb.Append("Conflicts with Resource Titles: ");
                    overlapEIsbn.ForEach(x => sb.AppendFormat("  \"{0}\"  ", x.Title));
                    errorDictionary.Add("Resource.EIsbn", sb.ToString());
                    return errorDictionary;
                }
            }

            //TODO: Make sure this works for Archived and Not available.
            //Active and Forthcoming
            if (resourceEdit.Resource.StatusId != (int)ResourceStatus.Active &&
                resourceEdit.Resource.StatusId != (int)ResourceStatus.Archived)
            {
                resource.Isbn10 = isbn10;
                resource.Isbn13 = isbn13;
            }

            resource.EIsbn = eIsbn;

            return errorDictionary;
        }


        public Dictionary<string, string> ValidateAndSaveResource(ResourceEdit resourceEdit, IUser user,
            AuthenticatedInstitution institution)
        {
            var databaseResource = _resourceService.GetResourceForEdit(resourceEdit.Resource.Id);

            if (databaseResource == null && resourceEdit.Resource.StatusId == (int)ResourceStatus.Forthcoming)
            {
                databaseResource = new Core.Resource.Resource { StatusId = 8 };
            }

            //This should never happen
            if (databaseResource == null)
            {
                return new Dictionary<string, string>
                    { { "Resource.Title", "Resource Cannot not be found. Please try again." } };
            }

            //Validate resource values before you set them
            var errorMessageDictionary = ValidateResource(resourceEdit, databaseResource);
            if (errorMessageDictionary.Any())
            {
                return errorMessageDictionary;
            }

            //This has to be set before the database resource gets edited
            var hasFreeResourceChanged = databaseResource.IsFreeResource != resourceEdit.Resource.IsFreeResource;

            var statusChangedToForthcoming = databaseResource.StatusId != (int)ResourceStatus.Forthcoming &&
                                             resourceEdit.Resource.StatusId == (int)ResourceStatus.Forthcoming;

            var hasResourceBeenArchived = databaseResource.StatusId != (int)ResourceStatus.Archived &&
                                          resourceEdit.Resource.StatusId == (int)ResourceStatus.Archived;

            //Sets new values for database resource and returns a log of changes
            var auditResults = SetResourceProperties(databaseResource, resourceEdit);

            //Status change to Forthcoming will result in null isbn10 and isbn13
            errorMessageDictionary = ValidateAndSetIsbns(resourceEdit, databaseResource, statusChangedToForthcoming);

            if (errorMessageDictionary.Any())
            {
                return errorMessageDictionary;
            }

            var isQaApprovalSet = SetQaApproval(databaseResource, resourceEdit, user);

            if (databaseResource.IsFreeResource && databaseResource.ListPrice > 0)
            {
                databaseResource.ListPrice = 0;
            }

            if (hasResourceBeenArchived)
            {
                databaseResource.ArchiveDate = DateTime.Now;
            }
            else if (databaseResource.StatusId != (int)ResourceStatus.Archived)
            {
                databaseResource.ArchiveDate = null;
            }

            _resourceService.SaveResource(databaseResource);
            //Need to set Id so ActionResult New can redirect to Detail Page
            var saveAuditResults = resourceEdit.Resource.Id > 0;
            resourceEdit.Resource.Id = databaseResource.Id;
            //Do not save Audit results for new Resources
            if (!string.IsNullOrWhiteSpace(auditResults) && saveAuditResults)
            {
                _auditService.LogResourceAudit(databaseResource.Id, ResourceAuditType.Unspecificed, auditResults);
            }

            if (hasFreeResourceChanged)
            {
                _collectionManagementService.UpdateFreeLicenses(databaseResource.Id, databaseResource.IsFreeResource);
                _cartService.UpdateFreeResourceLicenseCountInCart(databaseResource.Id, databaseResource.IsFreeResource);
            }

            switch (databaseResource.StatusId)
            {
                case (int)ResourceStatus.Active:
                case (int)ResourceStatus.Archived:
                    _resourceService.AddToTransformQueue(databaseResource);
                    break;
                case (int)ResourceStatus.Forthcoming:
                    var resource = _resourceService.GetResourceForEdit(databaseResource.Id);
                    _resourceService.UpdateAdminSearchFile(resource);
                    break;
            }


            if (isQaApprovalSet)
            {
                SendQaApprovalEmail(databaseResource, user, institution);
            }

            _specialDiscountResourceService.UpdateResourceSpecials(databaseResource.Id, resourceEdit.SpecialsSelected);


            if (resourceEdit.Resource.FeaturedTitleId > 0 || resourceEdit.Resource.IsFeaturedTitle)
            {
                var editFeaturedTitle = _featuredTitleService.GetFeaturedTitleForEdit(resourceEdit.Resource.Id) ??
                                        new FeaturedTitle();

                var featuredTitle = editFeaturedTitle.ToFeaturedTitle(resourceEdit.Resource.Id,
                    resourceEdit.Resource.FeaturedTitleStartDate,
                    resourceEdit.Resource.FeaturedTitleEndDate,
                    resourceEdit.Resource.IsFeaturedTitle);

                _featuredTitleService.SaveFeaturedTitle(featuredTitle);
            }

            return errorMessageDictionary;
        }

        private void SendQaApprovalEmail(IResource resource, IUser user,
            AuthenticatedInstitution authenticatedInstitution)
        {
            var emailMessage =
                _resourceQaEmailBuildService.BuildResourceQaEmail(user, resource, authenticatedInstitution);

            var emailPage = new EmailPage
            {
                To = _adminSettings.QaApprovalEmailTo,
                Cc = _adminSettings.QaApprovalEmailCc,
                Subject = emailMessage.Subject
            };
            var emailStatus = _emailService.SendEmailMessageToQueue(emailMessage.Body, emailPage);
            _log.DebugFormat("emailStatus: {0}", emailStatus);
        }

        public bool SetQaApproval(Core.Resource.Resource resource, ResourceEdit resourceEdit, IUser user)
        {
            if (user.IsRittenhouseAdmin() && _webSettings.DisplayPromotionFields)
            {
                if (resource.QaApprovalDate == null && resourceEdit.QaApproval)
                {
                    resource.QaApprovalDate = DateTime.Now;
                    return true;
                }

                if (resource.QaApprovalDate != null && !resourceEdit.QaApproval)
                {
                    resource.QaApprovalDate = null;
                }
            }

            return false;
        }

        private string GetFileExtension(string filename)
        {
            _log.DebugFormat("image file name: {0}", filename);
            var parts = filename.Split('.');

            if (parts.Length == 1)
            {
                return null;
            }

            var extension = parts.Last().ToLower();
            if (extension == "png" || extension == "gif" || extension == "jpg" || extension == "jpeg")
            {
                _log.DebugFormat("extension: {0}", extension);
                return extension;
            }

            return null;
        }

        private void SetSelectedPracticeAreas(Core.Resource.Resource resource, IList<int> practiceAreaSelected,
            StringBuilder auditdata)
        {
            if (practiceAreaSelected != null)
            {
                // add
                foreach (var practiceAreaId in practiceAreaSelected)
                {
                    var resourcePracticeArea =
                        resource.ResourcePracticeAreas.FirstOrDefault(x => x.PracticeAreaId == practiceAreaId);
                    if (resourcePracticeArea == null)
                    {
                        resourcePracticeArea = new ResourcePracticeArea
                        {
                            PracticeAreaId = practiceAreaId,
                            RecordStatus = true,
                            PracticeArea = _practiceAreaService.GetPracticeArea(practiceAreaId)
                        };
                        resource.ResourcePracticeAreas.Add(resourcePracticeArea);
                        AppendFieldLevelAuditDetails(auditdata, resourcePracticeArea);
                    }
                }

                // delete
                foreach (var resourcePracticeArea in resource.ResourcePracticeAreas)
                {
                    if (!practiceAreaSelected.Contains(resourcePracticeArea.PracticeAreaId))
                    {
                        resourcePracticeArea.RecordStatus = false;
                        AppendFieldLevelAuditDetails(auditdata, resourcePracticeArea);
                    }
                }
            }
        }

        private void SetSelectedSpecialties(Core.Resource.Resource resource, IList<int> specialtiesSelected,
            StringBuilder auditdata)
        {
            if (specialtiesSelected != null)
            {
                // add
                foreach (var specialtyId in specialtiesSelected)
                {
                    var resourceSpecialty =
                        resource.ResourceSpecialties.FirstOrDefault(x => x.SpecialtyId == specialtyId);
                    if (resourceSpecialty == null)
                    {
                        resourceSpecialty = new ResourceSpecialty
                        {
                            SpecialtyId = specialtyId,
                            RecordStatus = true,
                            Specialty = _specialtyService.GetSpecialtyForEdit(specialtyId)
                        };
                        resource.ResourceSpecialties.Add(resourceSpecialty);
                        AppendFieldLevelAuditDetails(auditdata, resourceSpecialty);
                    }
                }

                // delete
                foreach (var resourceSpecialty in resource.ResourceSpecialties)
                {
                    if (!specialtiesSelected.Contains(resourceSpecialty.SpecialtyId))
                    {
                        resourceSpecialty.RecordStatus = false;
                        AppendFieldLevelAuditDetails(auditdata, resourceSpecialty);
                    }
                }
            }
        }

        private void SetSelectedCollections(Core.Resource.Resource resource, IList<int> collectionsSelected,
            StringBuilder auditdata)
        {
            if (collectionsSelected != null)
            {
                // add
                foreach (var collectionId in collectionsSelected)
                {
                    var resourceCollection =
                        resource.ResourceCollections.FirstOrDefault(x => x.CollectionId == collectionId);
                    if (resourceCollection == null)
                    {
                        resourceCollection = new ResourceCollection
                        {
                            CollectionId = collectionId,
                            Collection = _collectionService.GetCollection(collectionId),
                            RecordStatus = true
                        };
                        resource.ResourceCollections.Add(resourceCollection);
                        AppendFieldLevelAuditDetails(auditdata, resourceCollection);
                    }
                }

                // delete
                foreach (var resourceCollection in resource.ResourceCollections.Where(x =>
                             !collectionsSelected.Contains(x.CollectionId)))
                {
                    resourceCollection.RecordStatus = false;
                    AppendFieldLevelAuditDetails(auditdata, resourceCollection);
                }
            }
            else
            {
                foreach (var resourceCollection in resource.ResourceCollections)
                {
                    resourceCollection.RecordStatus = false;
                    AppendFieldLevelAuditDetails(auditdata, resourceCollection);
                }
            }
        }

        /// <summary>
        ///     Build the sort by title
        /// </summary>
        private static string GetSortByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "";
            }

            if (title.StartsWith("A ") && title.Length > 2)
            {
                return $"{title.Substring(2)}, A";
            }

            if (title.StartsWith("AN ") && title.Length > 3)
            {
                return $"{title.Substring(3)}, AN";
            }

            if (title.StartsWith("THE ") && title.Length > 4)
            {
                return $"{title.Substring(4)}, THE";
            }

            return title;
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, string newValue, string originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, int newValue, int originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, bool newValue, bool originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, DateTime? newValue, DateTime? originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, decimal newValue, decimal originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, decimal? newValue, decimal? originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, ResourcePracticeArea practiceArea)
        {
            sb.AppendFormat(" [Id: {0} || Name: {1} has been {2} ] ", practiceArea.PracticeAreaId,
                practiceArea.PracticeArea.Name, practiceArea.RecordStatus ? "Added" : "Removed");
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, ResourceSpecialty specialty)
        {
            sb.AppendFormat(" [Id: {0} || Name: {1} has been {2} ] ", specialty.SpecialtyId, specialty.Specialty.Name,
                specialty.RecordStatus ? "Added" : "Removed");
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, ResourceCollection collection)
        {
            sb.AppendFormat(" [Id: {0} || Name: {1} has been {2} ] ", collection.CollectionId,
                collection.Collection.Name, collection.RecordStatus ? "Added" : "Removed");
        }
    }
}
