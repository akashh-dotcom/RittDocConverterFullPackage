#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.Web.Controllers.MyR2
{
    public static class UserContentTypeExtensions
    {
        public static UserContentType ToUserContentType(this string type)
        {
            var typeToLower = string.IsNullOrWhiteSpace(type) ? "" : type.ToLower();

            switch (typeToLower)
            {
                case "references":
                case "reference":
                    return UserContentType.Reference;

                case "images":
                case "image":
                    return UserContentType.Image;

                case "course-links":
                case "courselink":
                    return UserContentType.CourseLink;

                case "bookmarks":
                case "bookmark":
                default:
                    return UserContentType.Bookmark;
            }
        }
    }
}