#region

using R2V2.Core.MyR2;

#endregion

namespace R2V2.Web.Models.MyR2
{
    public static class UserContentExtensions
    {
        public static Core.MyR2.UserContentItem ToUserContentItem(this UserContentItem userContentItem)
        {
            var userContentType = userContentItem.Type;

            var contentItem = new UserContentFactory(userContentType).CreateUserContentItem();
            contentItem.Title = userContentItem.Title.TrimAtWordBoundary(255) ?? "";
            contentItem.ChapterSectionTitle = userContentItem.SectionTitle ?? "";
            contentItem.ResourceId = userContentItem.ResourceId;
            contentItem.SectionId = userContentItem.SectionId;
            contentItem.Library = userContentItem.Library;

            contentItem.TypeId = "1";

            contentItem.Isbn = userContentItem.Isbn;
            contentItem.Filename = userContentItem.Filename;

            return contentItem;
        }

        private static string TrimAtWordBoundary(this string s, int maximumCharacters)
        {
            const string ellipsis = "...";

            if (s == null)
            {
                return null;
            }

            if (s.Length <= maximumCharacters)
            {
                return s;
            }

            // index of last space after trimming 
            var index = s.Substring(0, maximumCharacters - ellipsis.Length).LastIndexOf(' ');

            return index == -1 ? s : $"{s.Substring(0, index)}...";
        }
    }
}