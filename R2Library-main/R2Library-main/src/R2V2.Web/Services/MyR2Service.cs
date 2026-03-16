#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using R2V2.Core.MyR2;
using R2V2.Core.Resource;
using R2V2.Core.Search;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Web.Models.MyR2;
using R2V2.Web.Models.Resource;
using R2V2.Web.Models.Search;
using UserContentFolder = R2V2.Web.Models.MyR2.UserContentFolder;
using UserContentItem = R2V2.Web.Models.MyR2.UserContentItem;

#endregion

namespace R2V2.Web.Services
{
    public class MyR2Service
    {
        private readonly ILog<MyR2Service> _log;
        private readonly MyR2CookieService _myR2CookieService;

        private readonly IResourceService _resourceService;
        private readonly IUserContentService _userContentService;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public MyR2Service(
            IResourceService resourceService
            , ILog<MyR2Service> log
            , MyR2CookieService myR2CookieService
            , IUserSessionStorageService userSessionStorageService
            , IUserContentService userContentService
        )
        {
            _resourceService = resourceService;
            _log = log;
            _myR2CookieService = myR2CookieService;
            _userSessionStorageService = userSessionStorageService;
            _userContentService = userContentService;
        }

        private static string MyR2BookmarksKey(int institutionId, int userId)
        {
            return $"MyR2.Bookmarks.User.Data_{institutionId}_{userId}";
        }

        private static string MyR2CourseLinksKey(int institutionId, int userId)
        {
            return $"MyR2.CourseLinks.User.Data_{institutionId}_{userId}";
        }

        private static string MyR2ImagesKey(int institutionId, int userId)
        {
            return $"MyR2.Images.User.Data_{institutionId}_{userId}";
        }

        private static string MyR2ReferecesKey(int institutionId, int userId)
        {
            return $"MyR2.Refereces.User.Data_{institutionId}_{userId}";
        }

        #region Folders

        public IEnumerable<UserContentFolder> GetUserContentFolders(UserContentType userContentType, int userId,
            int institutionId)
        {
            IEnumerable<UserContentFolder> folders =
                GetContentFoldersFromSession(userContentType, userId, institutionId);
            if (folders == null)
            {
                if (userId > 0)
                {
                    folders = _userContentService.GetUserContent(userId, userContentType)
                        .ToUserContentFolders(userContentType);
                }
                else if (institutionId > 0)
                {
                    folders = _myR2CookieService.GetUserContentFolders(userContentType, institutionId);
                }

                if (folders != null)
                {
                    foreach (var userContentFolder in folders)
                    {
                        foreach (var item in userContentFolder.UserContentItems)
                        {
                            item.Resource = _resourceService.GetResource(item.ResourceId).ToResourceDetail();
                        }
                    }
                }

                SetContentFoldersToSession(userContentType, folders, institutionId, userId);
            }

            return folders;
        }

        public int CreateUserContentFolder(string folderName, UserContentType userContentType, int userId,
            int institutionId)
        {
            var folderId = 0;
            if (institutionId > 0)
            {
                folderId = userId > 0
                    ? _userContentService.CreateUserContentFolder(userId, folderName, userContentType)
                    : CreateSessionUserContentFolder(folderName, userContentType, institutionId);
            }

            RemoveContentFoldersFromSession(userContentType, userId, institutionId);
            return folderId;
        }

        public bool RenameUserContentFolder(int folderId, string folderName, UserContentType userContentType,
            int userId, int institutionId)
        {
            var success = false;
            if (institutionId > 0)
            {
                success = userId > 0
                    ? _userContentService.RenameUserContentFolder(userId, folderId, folderName, userContentType)
                    : RenameSessionUserContentFolder(folderId, folderName, userContentType, institutionId);
            }

            RemoveContentFoldersFromSession(userContentType, userId, institutionId);
            return success;
        }

        public bool DeleteUserContentFolder(int folderId, UserContentType userContentType, int userId,
            int institutionId)
        {
            var success = false;
            if (institutionId > 0)
            {
                success = userId > 0
                    ? _userContentService.DeleteUserContentFolder(userId, folderId, userContentType)
                    : DeleteSessionUserContentFolder(folderId, userContentType, institutionId);
            }

            RemoveContentFoldersFromSession(userContentType, userId, institutionId);
            return success;
        }

        private int CreateSessionUserContentFolder(string folderName, UserContentType userContentType,
            int institutionId)
        {
            var userContentFolders = _myR2CookieService.GetUserContentFolders(userContentType, institutionId);
            var folderId = userContentFolders.Count + 1;

            var userContentFolder = CreateSessionContentFolder(folderId, folderName, userContentType, folderId == 1);
            _myR2CookieService.CreateUserContentFolder(userContentFolder, userContentType, institutionId);

            return folderId;
        }

        private bool RenameSessionUserContentFolder(int folderId, string folderName, UserContentType userContentType,
            int institutionId)
        {
            var userContentFolders = _myR2CookieService.GetUserContentFolders(userContentType, institutionId);
            var userContentFolder = userContentFolders.FirstOrDefault(x => x.Id == folderId);
            if (userContentFolder != null)
            {
                _myR2CookieService.RenameUserContentFolder(userContentType, userContentFolder.FolderName, folderName,
                    institutionId);
            }

            return true;
        }

        private bool DeleteSessionUserContentFolder(int folderId, UserContentType userContentType, int institutionId)
        {
            var userContentFolders = _myR2CookieService.GetUserContentFolders(userContentType, institutionId);
            var userContentFolder = userContentFolders.FirstOrDefault(x => x.Id == folderId);
            _myR2CookieService.DeleteUserContentFolder(userContentFolder, userContentType, institutionId);
            return true;
        }

        #endregion

        #region Items

        public int CreateUserContentItem(UserContentItem userContentItem, int userId, int institutionId)
        {
            var userContentItemId = 0;
            if (institutionId > 0)
            {
                userContentItemId = userId > 0
                    ? _userContentService.SaveUserContentItem(userId, userContentItem.FolderId, userContentItem.Type,
                        userContentItem.ToUserContentItem())
                    : CreateSessionUserContentItem(userContentItem, institutionId);
            }

            RemoveContentFoldersFromSession(userContentItem.Type, userId, institutionId);
            return userContentItemId;
        }

        public bool MoveUserContentItem(UserContentItem userContentItem, int newFolderId, int userId, int institutionId)
        {
            var success = false;
            if (institutionId > 0)
            {
                success = userId > 0
                    ? _userContentService.MoveUserContentItem(userId, userContentItem.Id, userContentItem.FolderId,
                        newFolderId,
                        userContentItem.Type)
                    : MoveSessionUserContentItem(userContentItem.Id, userContentItem.FolderId, newFolderId,
                        userContentItem.Type, institutionId);
            }

            RemoveContentFoldersFromSession(userContentItem.Type, userId, institutionId);
            return success;
        }

        public bool DeleteUserContentItem(UserContentItem userContentItem, int userId, int institutionId)
        {
            var success = false;
            if (institutionId > 0)
            {
                success = userId > 0
                    ? _userContentService.DeleteUserContentItem(userId, userContentItem.Id, userContentItem.FolderId,
                        userContentItem.Type)
                    : RemoveSessionUserContentItem(userContentItem.Id, userContentItem.Type, institutionId);
            }

            RemoveContentFoldersFromSession(userContentItem.Type, userId, institutionId);
            return success;
        }

        public UserContentItem GetUserContentItem(UserContentType userContentType, int userContentItemId, int userId,
            int institutionId)
        {
            var userContentFolders = GetUserContentFolders(userContentType, userId, institutionId);
            if (userContentFolders != null)
            {
                return (from userContentFolder in userContentFolders
                    from contentItem in userContentFolder.UserContentItems
                    where contentItem.Id == userContentItemId
                    select contentItem).FirstOrDefault();
            }

            return null;
        }

        private int CreateSessionUserContentItem(UserContentItem userContentItem, int institutionId)
        {
            userContentItem.CreationDate = DateTime.Now;

            var userContentType = userContentItem.Type;

            var userContentFolders = _myR2CookieService.GetUserContentFolders(userContentType, institutionId);

            var foldersCount = 0;
            var itemsCount = 0;
            UserContentFolder userContentFolder;
            if (!userContentFolders.Any())
            {
                foldersCount++;
                itemsCount++;
                userContentItem.Id = itemsCount;
                userContentItem.FolderId = foldersCount;
                userContentFolder = CreateSessionContentFolder(foldersCount, "", userContentItem.Type, true);
                userContentFolder.AddUserContentItem(userContentItem);
            }
            else
            {
                foldersCount = userContentFolders.Select(x => x.UserContentItems).Count() + 1;
                itemsCount = userContentFolders.Sum(x => x.UserContentItems.Count) + 1;

                userContentItem.Id = itemsCount;

                if (userContentItem.FolderId == 0)
                {
                    //Add Item to existing folder
                    userContentFolder = userContentFolders.FirstOrDefault(x =>
                        x.FolderName == GetDefaultFolderName(userContentItem.Type));
                    //If no folder is found create one.
                    if (userContentItem.FolderId == 0)
                    {
                        userContentItem.FolderId = foldersCount;
                        userContentFolder = CreateSessionContentFolder(foldersCount, "", userContentItem.Type, true);
                        userContentFolder.AddUserContentItem(userContentItem);
                    }
                }
                else
                {
                    userContentFolder = userContentFolders.FirstOrDefault(x => x.Id == userContentItem.FolderId);
                    if (userContentFolder != null)
                    {
                        userContentFolder.AddUserContentItem(userContentItem);
                    }
                    else
                    {
                        userContentItem.FolderId = foldersCount;
                        userContentFolder = CreateSessionContentFolder(foldersCount, "", userContentItem.Type, true);
                        userContentFolder.AddUserContentItem(userContentItem);
                    }
                }
            }

            _myR2CookieService.SaveUserContentItem(userContentType, userContentItem, userContentFolder, institutionId);
            return itemsCount;
        }

        private bool MoveSessionUserContentItem(int contentItemId, int folderId, int newFolderId,
            UserContentType userContentType, int institutionId)
        {
            var userContentFolders = _myR2CookieService.GetUserContentFolders(userContentType, institutionId);

            var newFolderForContentItem = userContentFolders.FirstOrDefault(x => x.Id == newFolderId);
            var oldFolderForContentItem = userContentFolders.FirstOrDefault(x => x.Id == folderId);
            if (!(newFolderForContentItem == null || oldFolderForContentItem == null))
            {
                var contentItem = oldFolderForContentItem.UserContentItems.FirstOrDefault(x => x.Id == contentItemId);
                if (contentItem != null)
                {
                    oldFolderForContentItem.RemoveUserContentItem(contentItem);
                    contentItem.FolderId = newFolderId;
                    newFolderForContentItem.AddUserContentItem(contentItem);
                    _myR2CookieService.DeleteUserContentItem(userContentType, contentItem.Id, institutionId);
                    _myR2CookieService.SaveUserContentItem(userContentType, contentItem, newFolderForContentItem,
                        institutionId);
                }
            }

            return true;
        }

        private bool RemoveSessionUserContentItem(int contentItemId, UserContentType userContentType, int institutionId)
        {
            _myR2CookieService.DeleteUserContentItem(userContentType, contentItemId, institutionId);
            return true;
        }

        #endregion

        #region Session

        public void RemoveContentFoldersFromSession(UserContentType userContentType, int userId, int institutionId)
        {
            _log.Info($"RemoveContentFoldersFromSession userContentType: {userContentType}");
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                switch (userContentType)
                {
                    case UserContentType.Bookmark:
                        if (_userSessionStorageService.Has(MyR2BookmarksKey(institutionId, userId)))
                        {
                            _userSessionStorageService.Remove(MyR2BookmarksKey(institutionId, userId));
                        }

                        break;
                    case UserContentType.CourseLink:
                        if (_userSessionStorageService.Has(MyR2CourseLinksKey(institutionId, userId)))
                        {
                            _userSessionStorageService.Remove(MyR2CourseLinksKey(institutionId, userId));
                        }

                        break;
                    case UserContentType.Image:
                        if (_userSessionStorageService.Has(MyR2ImagesKey(institutionId, userId)))
                        {
                            _userSessionStorageService.Remove(MyR2ImagesKey(institutionId, userId));
                        }

                        break;
                    case UserContentType.Reference:
                        if (_userSessionStorageService.Has(MyR2ReferecesKey(institutionId, userId)))
                        {
                            _userSessionStorageService.Remove(MyR2ReferecesKey(institutionId, userId));
                        }

                        break;
                }
            }
        }

        private List<UserContentFolder> GetContentFoldersFromSession(UserContentType userContentType, int userId,
            int institutionId)
        {
            //_log.DebugFormat("GetContentFoldersFromSession userContentType: {0}", userContentType.ToString());
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                switch (userContentType)
                {
                    case UserContentType.Bookmark:
                        if (_userSessionStorageService.Has(MyR2BookmarksKey(institutionId, userId)))
                        {
                            //_log.Debug("returning MyR2Bookmarks");
                            return _userSessionStorageService.Get<List<UserContentFolder>>(
                                MyR2BookmarksKey(institutionId, userId));
                        }

                        break;
                    case UserContentType.CourseLink:
                        if (_userSessionStorageService.Has(MyR2CourseLinksKey(institutionId, userId)))
                        {
                            //_log.Debug("returning MyR2CourseLinksKey");
                            return _userSessionStorageService.Get<List<UserContentFolder>>(
                                MyR2CourseLinksKey(institutionId, userId));
                        }

                        break;
                    case UserContentType.Image:
                        if (_userSessionStorageService.Has(MyR2ImagesKey(institutionId, userId)))
                        {
                            //_log.Debug("returning MyR2ImagesKey");
                            return _userSessionStorageService.Get<List<UserContentFolder>>(MyR2ImagesKey(institutionId,
                                userId));
                        }

                        break;
                    case UserContentType.Reference:
                        if (_userSessionStorageService.Has(MyR2ReferecesKey(institutionId, userId)))
                        {
                            //_log.Debug("returning MyR2ReferecesKey");
                            return _userSessionStorageService.Get<List<UserContentFolder>>(
                                MyR2ReferecesKey(institutionId, userId));
                        }

                        break;
                }
            }
            else
            {
                _log.Info(
                    $"HttpContext.Current or HttpContext.Current.Session is null -- Indicates this is a crawler. -- userContentType: {userContentType}");
            }

            _log.Info($"returning null, userContentType: {userContentType}");
            return null;
        }

        private void SetContentFoldersToSession(UserContentType userContentType,
            IEnumerable<UserContentFolder> userContentFolders, int institutionId, int userId)
        {
            switch (userContentType)
            {
                case UserContentType.Bookmark:
                    if (_userSessionStorageService.Has(MyR2BookmarksKey(institutionId, userId)))
                    {
                        _userSessionStorageService.Remove(MyR2BookmarksKey(institutionId, userId));
                    }

                    _userSessionStorageService.Put(MyR2BookmarksKey(institutionId, userId), userContentFolders);
                    break;
                case UserContentType.CourseLink:
                    if (_userSessionStorageService.Has(MyR2CourseLinksKey(institutionId, userId)))
                    {
                        _userSessionStorageService.Remove(MyR2CourseLinksKey(institutionId, userId));
                    }

                    _userSessionStorageService.Put(MyR2CourseLinksKey(institutionId, userId), userContentFolders);
                    break;
                case UserContentType.Image:
                    if (_userSessionStorageService.Has(MyR2ImagesKey(institutionId, userId)))
                    {
                        _userSessionStorageService.Remove(MyR2ImagesKey(institutionId, userId));
                    }

                    _userSessionStorageService.Put(MyR2ImagesKey(institutionId, userId), userContentFolders);

                    break;
                case UserContentType.Reference:
                    if (_userSessionStorageService.Has(MyR2ReferecesKey(institutionId, userId)))
                    {
                        _userSessionStorageService.Remove(MyR2ReferecesKey(institutionId, userId));
                    }

                    _userSessionStorageService.Put(MyR2ReferecesKey(institutionId, userId), userContentFolders);
                    break;
            }
        }

        #endregion

        #region Search

        public int SaveSavedSearch(SavedSearch savedSearch, int institutionId)
        {
            var savedSearches = _myR2CookieService.GetMyR2Searches(false, institutionId);
            savedSearch.Id = savedSearches != null && savedSearches.Any()
                ? savedSearches.OrderBy(x => x.Id).Last().Id + 1
                : 1;
            savedSearch.SearchDate = DateTime.Now;
            _myR2CookieService.SaveMyR2Search(savedSearch, false, institutionId);

            return savedSearch.Id;
        }

        public bool DeleteSavedSearch(int savedSearchId, int institutionId)
        {
            var savedSearches = _myR2CookieService.GetMyR2Searches(false, institutionId);
            var savedSearch = savedSearches.FirstOrDefault(x => x.Id == savedSearchId);
            _myR2CookieService.DeleteMyR2Search(savedSearch, false, institutionId);
            return true;
        }

        public IList<SavedSearch> GetSavedSearches(int institutionId)
        {
            return _myR2CookieService.GetMyR2Searches(false, institutionId);
        }

        public IList<SavedSearch> GetSearchHistory(int institutionId)
        {
            var searchHistory = _myR2CookieService.GetMyR2Searches(true, institutionId);
            return searchHistory.OrderByDescending(x => x.Id).ToList();
        }

        public void SaveSearchHistory(ISearchHistory searchHistory, SearchQuery query, int institutionId)
        {
            var searchHistories = _myR2CookieService.GetMyR2Searches(true, institutionId);
            if (searchHistories.Count > 19)
            {
                var test = searchHistories.OrderByDescending(x => x.Id).Skip(19).ToList();
                foreach (var search in test)
                {
                    _myR2CookieService.DeleteMyR2Search(search, true, institutionId);
                }

                searchHistories = _myR2CookieService.GetMyR2Searches(true, institutionId);
            }

            var savedSearch = ConvertSearchQueryToSavedSearch(query);
            savedSearch.Total = searchHistory.FileCount;
            savedSearch.Id = searchHistories != null && searchHistories.Any()
                ? searchHistories.OrderBy(x => x.Id).Last().Id + 1
                : 1;
            savedSearch.SearchDate = DateTime.Now;
            _myR2CookieService.SaveMyR2Search(savedSearch, true, institutionId);
        }

        #endregion

        #region misc

        private static UserContentFolder CreateSessionContentFolder(int folderId, string folderName,
            UserContentType userContentType, bool defaultFolder = false)
        {
            var folder = new UserContentFolder
            {
                Id = folderId,
                FolderName = string.IsNullOrWhiteSpace(folderName)
                    ? GetDefaultFolderName(userContentType)
                    : folderName,
                DefaultFolder = defaultFolder
            };

            return folder;
        }

        private static SavedSearch ConvertSearchQueryToSavedSearch(SearchQuery query)
        {
            return new SavedSearch
            {
                Page = query.Page,
                PageSize = query.PageSize,
                SortBy = query.SortBy,
                Within = query.Within,
                Disciplines = query.Disciplines,
                Filter = query.Filter,
                Field = query.Field,
                PracticeArea = query.PracticeArea,
                Author = query.Author,
                Title = query.Title,
                Publisher = query.Publisher,
                Editor = query.Editor,
                Isbn = query.Isbn,
                TocAvailable = query.TocAvailable,
                Include = query.Include,
                Year = query.Year,
                Q = query.Q
            };
        }

        private static string GetDefaultFolderName(UserContentType userContentType)
        {
            switch (userContentType)
            {
                case UserContentType.Reference:
                    return "my references".ToUpper();
                case UserContentType.Image:
                    return "my images".ToUpper();
                case UserContentType.CourseLink:
                    return "my course links".ToUpper();
                default:
                    return "my bookmarks".ToUpper();
            }
        }

        #endregion
    }
}