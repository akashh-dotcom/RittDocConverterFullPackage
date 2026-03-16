#region

using System.Collections.Generic;
using R2V2.Core.MyR2;

#endregion

namespace R2V2.Core.Authentication
{
    public interface IUserWithFolders : IUser
    {
        IEnumerable<UserBookmarkFolder> UserBookmarkFolders { get; }
        IEnumerable<UserCourseLinksFolder> UserCourseLinksFolders { get; }
        IEnumerable<UserImagesFolder> UserImagesFolders { get; }
        IEnumerable<UserReferencesFolder> UserReferencesFolders { get; }
    }
}