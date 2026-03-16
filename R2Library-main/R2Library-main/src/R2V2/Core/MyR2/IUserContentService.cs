#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.MyR2
{
    public interface IUserContentService
    {
        IEnumerable<UserContentFolder> GetUserContent(int userId, UserContentType userContentType);

        int CreateUserContentFolder(int userId, string folderName, UserContentType userContentType,
            bool defaultFolder = false);

        bool RenameUserContentFolder(int userId, int userContentFolderId, string newFolderName,
            UserContentType userContentType);

        bool DeleteUserContentFolder(int userId, int userContentFolderId, UserContentType userContentType);

        int SaveUserContentItem(int userId, int userContentFolderId, UserContentType userContentType,
            UserContentItem userContentItem);

        bool MoveUserContentItem(int userId, int userContentItemId, int userContentFolderId, int newUserContentFolderId,
            UserContentType userContentType);

        bool DeleteUserContentItem(int userId, int userContentItemId, int userContentFolderId,
            UserContentType userContentType);

        IEnumerable<UserSearchHistory> GetUserSearchHistory(int userId, int max);
        IEnumerable<UserSavedSearch> GetUserSavedSearch(int userId);

        bool DeleteSavedSearch(int userId, int savedSearchId);

        byte[] GetImageForExportWithCitation(int userId, int userContentItemId, string resourceUrl);

        byte[] GetImageForExportWithCitation(string imageUrl, IResource resource, string resourceUrl);

        int SaveUserSearchResultIntoDefaultFolder(UserSavedSearchResult userSavedSearchResult, int userId);
        IEnumerable<UserSavedSearchResult> GetUserSavedSearchResults(int userId);
        UserSavedSearchResult GetUserSavedSearchResult(int savedSearchResultId, int userId);

        bool DeleteSavedSearchResult(int userId, int savedSearchResultId);
    }
}