#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NHibernate.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.MyR2
{
    public class UserContentService : IUserContentService
    {
        private readonly IContentSettings _contentSettings;
        private readonly ILog<UserContentService> _log;
        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<User> _user;
        private readonly IQueryable<UserBookmarkFolder> _userBookmarkFolders;
        private readonly IQueryable<UserCourseLinksFolder> _userCourseLinkFolders;
        private readonly IQueryable<UserImagesFolder> _userImageFolders;
        private readonly IQueryable<UserReferencesFolder> _userReferenceFolders;
        private readonly IQueryable<UserSavedFolder> _userSavedSearches;
        private readonly IQueryable<UserSavedSearchResult> _userSavedSearchResult;
        private readonly IQueryable<UserSavedSearchResultFolder> _userSavedSearchResultFolder;
        private readonly IQueryable<UserSearchHistory> _userSearchHistories;

        public UserContentService(ILog<UserContentService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<User> user
            , IResourceService resourceService
            , IQueryable<UserBookmarkFolder> userBookmarkFolders
            , IQueryable<UserCourseLinksFolder> userCourseLinkFolders
            , IQueryable<UserImagesFolder> userImageFolders
            , IQueryable<UserReferencesFolder> userReferenceFolders
            , IQueryable<UserSearchHistory> userSearchHistories
            , IQueryable<UserSavedFolder> userSavedSearches
            , IQueryable<UserSavedSearchResultFolder> userSavedSearchResultFolder
            , IQueryable<UserSavedSearchResult> userSavedSearchResult
            , IContentSettings contentSettings)
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _user = user;
            _resourceService = resourceService;
            _userBookmarkFolders = userBookmarkFolders;
            _userCourseLinkFolders = userCourseLinkFolders;
            _userImageFolders = userImageFolders;
            _userReferenceFolders = userReferenceFolders;
            _userSearchHistories = userSearchHistories;
            _userSavedSearches = userSavedSearches;
            _userSavedSearchResultFolder = userSavedSearchResultFolder;
            _userSavedSearchResult = userSavedSearchResult;
            _contentSettings = contentSettings;
        }

        public IEnumerable<UserContentFolder> GetUserContent(int userId, UserContentType userContentType)
        {
            IEnumerable<UserContentFolder> userContentFolders;

            switch (userContentType)
            {
                case UserContentType.Reference:
                    userContentFolders = _userReferenceFolders.Where(x => x.UserId == userId)
                        .FetchMany(userContentFolder => userContentFolder.UserReferences);
                    break;

                case UserContentType.Image:
                    userContentFolders = _userImageFolders.Where(x => x.UserId == userId)
                        .FetchMany(userContentFolder => userContentFolder.UserImages);
                    break;

                case UserContentType.CourseLink:
                    userContentFolders = _userCourseLinkFolders.Where(x => x.UserId == userId)
                        .FetchMany(userContentFolder => userContentFolder.UserCourseLinks);
                    break;

                case UserContentType.SavedSearch:
                    userContentFolders = _userSavedSearches.Where(x => x.UserId == userId)
                        .FetchMany(userSearchFolder => userSearchFolder.SavedSearches);
                    break;

                case UserContentType.SavedSearchResult:
                    userContentFolders = _userSavedSearchResultFolder.Where(x => x.UserId == userId)
                        .FetchMany(userSearchResult => userSearchResult.SavedSearchResults);
                    break;

                default:
                    userContentFolders = _userBookmarkFolders.Where(x => x.UserId == userId)
                        .FetchMany(userContentFolder => userContentFolder.UserBookmarks);
                    break;
            }

            foreach (var userContentFolder in userContentFolders.ToList())
            {
                SetUserContentItemResources(userContentFolder.UserContentItems);
            }

            return userContentFolders;
        }

        public int CreateUserContentFolder(int userId, string folderName, UserContentType userContentType,
            bool defaultFolder = false)
        {
            var folder = new UserContentFactory(userContentType).CreateUserContentFolder(folderName);

            if (defaultFolder)
            {
                folder.DefaultFolder = true;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                var user = _user.SingleOrDefault(x => x.Id == userId);
                if (user != null)
                {
                    user.AddUserContentFolder(folder);

                    uow.SaveOrUpdate(user);
                }

                uow.Commit();
            }

            return folder.Id;
        }

        public bool RenameUserContentFolder(int userId, int userContentFolderId, string newFolderName,
            UserContentType userContentType)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var userContentFolder = GetUserContent(userId, userContentType)
                    .SingleOrDefault(x => x.Id == userContentFolderId);
                if (userContentFolder != null)
                {
                    userContentFolder.FolderName = newFolderName;
                    uow.SaveOrUpdate(userContentFolder);
                }

                uow.Commit();
            }

            return true;
        }

        public bool DeleteSavedSearch(int userId, int savedSearchId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var savedSearchFolders =
                        _userSavedSearches.Where(x => x.UserId == userId).Select(y => y.SavedSearches);
                    var userSavedSearch =
                        Enumerable.FirstOrDefault(
                            from folder in savedSearchFolders
                            from savedSearch in folder
                            where savedSearch.Id == savedSearchId
                            select savedSearch);

                    uow.Delete(userSavedSearch);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }

            return true;
        }

        public bool DeleteUserContentFolder(int userId, int userContentFolderId, UserContentType userContentType)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var userContentFolders = GetUserContent(userId, userContentType);
                var userContentFolder = userContentFolders.SingleOrDefault(x => x.Id == userContentFolderId);
                if (userContentFolder == null)
                {
                    return false;
                }

                uow.Delete(userContentFolder);
                uow.Commit();
            }

            return true;
        }

        public int SaveUserContentItem(int userId, int userContentFolderId, UserContentType userContentType,
            UserContentItem userContentItem)
        {
            var id = -1;

            if (userContentItem.ResourceId > 0 /*&& !string.IsNullOrWhiteSpace(userContentItem.Title)*/
               ) //R2V2 Squish #621 - Allow saving of images when title is missing -DRJ
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    if (userContentFolderId == 0)
                    {
                        var defaultFolder = GetDefaultFolder(userId, userContentType);
                        if (defaultFolder != null)
                        {
                            userContentFolderId = defaultFolder.Id;
                        }

                        if (userContentFolderId == 0)
                        {
                            userContentFolderId = CreateUserContentFolder(userId,
                                $"MY {userContentType.ToString().ToUpper()}S", userContentType, true);
                        }
                    }

                    var userContentFolder = GetUserContent(userId, userContentType)
                        .SingleOrDefault(x => x.Id == userContentFolderId);
                    if (userContentFolder != null)
                    {
                        userContentItem.UserContentFolder = userContentFolder;

                        uow.SaveOrUpdate(userContentItem);
                        uow.Commit();

                        id = userContentItem.Id;
                    }
                }
            }

            return id;
        }

        public bool MoveUserContentItem(int userId, int userContentItemId, int userContentFolderId,
            int newUserContentFolderId, UserContentType userContentType)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var userContentFolders = GetUserContent(userId, userContentType).ToList();
                var userContentFolder = userContentFolders.SingleOrDefault(x => x.Id == userContentFolderId);

                if (userContentFolder != null)
                {
                    var userContentItem =
                        userContentFolder.UserContentItems.SingleOrDefault(x => x.Id == userContentItemId);
                    if (userContentItem != null)
                    {
                        var newUserContentFolder =
                            userContentFolders.SingleOrDefault(x => x.Id == newUserContentFolderId);
                        userContentItem.UserContentFolder = newUserContentFolder;

                        uow.SaveOrUpdate(userContentItem);
                    }
                }

                uow.Commit();
            }

            return true;
        }

        public bool DeleteUserContentItem(int userId, int userContentItemId, int userContentFolderId,
            UserContentType userContentType)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var userContentFolder = GetUserContent(userId, userContentType)
                    .SingleOrDefault(x => x.Id == userContentFolderId);
                if (userContentFolder != null)
                {
                    var userContentItem =
                        userContentFolder.UserContentItems.SingleOrDefault(x => x.Id == userContentItemId);
                    if (userContentItem == null)
                    {
                        return false;
                    }

                    uow.Delete(userContentItem);
                }

                uow.Commit();
            }

            return true;
        }

        public IEnumerable<UserSearchHistory> GetUserSearchHistory(int userId, int max)
        {
            var userSearchHistories = _userSearchHistories
                .Where(x => x.UserId == userId)
                .Where(x => x.SearchQuery != null)
                .OrderByDescending(x => x.CreationDate)
                .Take(max)
                .ToList();
            return userSearchHistories;
        }

        public IEnumerable<UserSavedSearch> GetUserSavedSearch(int userId)
        {
            var userSavedFolders = _userSavedSearches.Where(x => x.UserId == userId)
                .FetchMany(userSearchFolder => userSearchFolder.SavedSearches);

            var userSavedSearches = new List<UserSavedSearch>();
            foreach (var userSavedFolder in userSavedFolders)
            {
                userSavedSearches.AddRange(userSavedFolder.SavedSearches);
            }

            return userSavedSearches.OrderByDescending(x => x.CreationDate);
        }

        public byte[] GetImageForExportWithCitation(int userId, int userContentItemId, string resourceUrl)
        {
            var userContent = GetUserContent(userId, UserContentType.Image);
            var userContentItem = userContent.SelectMany(userContentFolder => userContentFolder.UserContentItems)
                .SingleOrDefault(x => x.Id == userContentItemId);
            return userContentItem == null
                ? null
                : GetImageForExportWithCitation(userContentItem.ImageUrl, userContentItem.Resource, resourceUrl);
        }

        /// <summary>
        ///     Need this to export from session
        /// </summary>
        public byte[] GetImageForExportWithCitation(string imageUrl, IResource resource, string resourceUrl)
        {
            var filePath = $"{_contentSettings.ImageBaseFileLocation}{imageUrl.Replace("/", @"\")}";
            if (File.Exists(filePath))
            {
                using (var backgroundImage = Image.FromFile(filePath))
                {
                    using (var font = new Font("Arial", 8, FontStyle.Bold))
                    {
                        using (var stringFormat = new StringFormat(StringFormat.GenericTypographic))
                        {
                            return GetImageBytes(backgroundImage, resource, font, stringFormat, resourceUrl);
                        }
                    }
                }
            }

            return null;
        }

        public int SaveUserSearchResultIntoDefaultFolder(UserSavedSearchResult userSavedSearchResult, int userId)
        {
            try
            {
                IList<UserSavedSearchResultFolder> userSavedFolders = _userSavedSearchResultFolder
                    .Where(x => x.UserId == userId).Where(x => x.DefaultFolder).ToList();

                UserSavedSearchResultFolder defaultFolder;
                if (userSavedFolders.Count == 0)
                {
                    defaultFolder = new UserSavedSearchResultFolder
                    {
                        DefaultFolder = true,
                        FolderName = "My Saved Search Results",
                        RecordStatus = true,
                        //User = user,
                        UserId = userId
                    };

                    using (var uow = _unitOfWorkProvider.Start())
                    {
                        uow.Save(defaultFolder);
                        uow.Commit();
                    }
                }
                else
                {
                    defaultFolder = userSavedFolders.First();
                }

                userSavedSearchResult.Folder = defaultFolder;

                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.SaveOrUpdate(userSavedSearchResult);
                    uow.Commit();
                }

                return userSavedSearchResult.Id;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return -2;
            }
        }

        public IEnumerable<UserSavedSearchResult> GetUserSavedSearchResults(int userId)
        {
            if (userId == 0)
            {
                return null;
            }

            var userSavedResultsFolders = _userSavedSearchResultFolder.Where(x => x.UserId == userId)
                .FetchMany(userSearchFolder => userSearchFolder.SavedSearchResults);

            var userSavedSearches = new List<UserSavedSearchResult>();
            foreach (var userSavedFolder in userSavedResultsFolders)
            {
                userSavedSearches.AddRange(userSavedFolder.SavedSearchResults);
            }

            return userSavedSearches.OrderByDescending(x => x.CreationDate);
        }

        public UserSavedSearchResult GetUserSavedSearchResult(int savedSearchResultId, int userId)
        {
            var userSavedSearchResults = GetUserSavedSearchResults(userId);
            return userSavedSearchResults != null
                ? userSavedSearchResults.FirstOrDefault(x => x.Id == savedSearchResultId)
                : null;
        }

        public bool DeleteSavedSearchResult(int userId, int savedSearchResultId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    var savedSearchResult = _userSavedSearchResult.FirstOrDefault(x =>
                        x.Id == savedSearchResultId && x.Folder.UserId == userId);

                    uow.Delete(savedSearchResult);
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }

            return true;
        }

        public void SetUserContentItemResources(IEnumerable<UserContentItem> userContentItems)
        {
            if (userContentItems == null)
                return;

            foreach (var userContentItem in userContentItems)
            {
                userContentItem.Resource = _resourceService.GetResource(userContentItem.ResourceId);
            }
        }

        private UserContentFolder GetDefaultFolder(int userId, UserContentType userContentType)
        {
            var userContent = GetUserContent(userId, userContentType);
            if (userContent != null)
            {
                var userContentList = userContent.ToList();
                if (userContentList.Any())
                {
                    return userContentList.SingleOrDefault(x => x.DefaultFolder);
                }
            }

            return null;
        }

        private byte[] GetImageBytes(Image backgroundImage, IResource resource, Font font, StringFormat stringFormat,
            string resourceUrl)
        {
            using (var bitmap = new Bitmap(1, 1))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var height = backgroundImage.Height;
                    var width = backgroundImage.Width;

                    var citation =
                        $"© {resource.Copyright}\r\n\r\n{_resourceService.GetCitation(resource, resourceUrl)}";

                    // measure the text height - we need this to position it at the base
                    var textHeight =
                        (int)(graphics.MeasureString(citation, font, new PointF(0, 0), stringFormat).Height +
                              (citation.Length / (width * 0.2) + 1) * 12 + 24);

                    // recreate bitmap and graphic objects with the new measurements
                    using (var newBitmap = new Bitmap(width, height + textHeight))
                    {
                        using (var newGraphics = Graphics.FromImage(newBitmap))
                        {
                            newGraphics.DrawImage(backgroundImage, 0, 0, width, height);

                            // add our text background's bar using a rectangle at the base of the image
                            var rectangle = new Rectangle(0, height, width, textHeight);
                            newGraphics.FillRectangle(new SolidBrush(Color.FromArgb(167, 167, 167)), rectangle);

                            // create text
                            newGraphics.DrawString(citation, font, new SolidBrush(Color.Black), rectangle);

                            using (var ms = new MemoryStream())
                            {
                                newBitmap.Save(ms, ImageFormat.Jpeg);
                                var bmpBytes = ms.GetBuffer();
                                newBitmap.Dispose();
                                return bmpBytes;
                            }
                        }
                    }
                }
            }
        }

        public int SaveUserSearchIntoDefaultFolder(UserSavedSearch userSavedSearch, int userId)
        {
            try
            {
                IList<UserSavedFolder> userSavedFolders = _userSavedSearches.Where(x => x.UserId == userId)
                    .Where(x => x.DefaultFolder).ToList();

                UserSavedFolder defaultFolder;
                if (userSavedFolders.Count == 0)
                {
                    defaultFolder = new UserSavedFolder
                    {
                        DefaultFolder = true,
                        FolderName = "My Saved Searches",
                        RecordStatus = true,
                        UserId = userId,
                    };

                    using (var uow = _unitOfWorkProvider.Start())
                    {
                        uow.Save(defaultFolder);
                        uow.Commit();
                    }
                }
                else
                {
                    defaultFolder = userSavedFolders.First();
                }

                userSavedSearch.Folder = defaultFolder;

                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Save(userSavedSearch);
                    uow.Commit();
                }

                return userSavedSearch.Id;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return -2;
            }
        }
    }
}