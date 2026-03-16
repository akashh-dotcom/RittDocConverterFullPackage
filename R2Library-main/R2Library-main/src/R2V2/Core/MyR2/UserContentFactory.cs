namespace R2V2.Core.MyR2
{
    public class UserContentFactory
    {
        private readonly UserContentType _userContentType;

        public UserContentFactory(UserContentType userContentType)
        {
            _userContentType = userContentType;
        }

        public UserContentItem CreateUserContentItem()
        {
            switch (_userContentType)
            {
                case UserContentType.CourseLink:
                    return new UserCourseLink();

                case UserContentType.Image:
                    return new UserImage();

                case UserContentType.Reference:
                    return new UserReference();

                case UserContentType.Bookmark:
                default:
                    return new UserBookmark();
            }
        }

        public UserContentFolder CreateUserContentFolder(string folderName)
        {
            switch (_userContentType)
            {
                case UserContentType.CourseLink:
                    return new UserCourseLinksFolder { FolderName = folderName };

                case UserContentType.Image:
                    return new UserImagesFolder { FolderName = folderName };

                case UserContentType.Reference:
                    return new UserReferencesFolder { FolderName = folderName };

                case UserContentType.Bookmark:
                default:
                    return new UserBookmarkFolder { FolderName = folderName };
            }
        }
    }
}