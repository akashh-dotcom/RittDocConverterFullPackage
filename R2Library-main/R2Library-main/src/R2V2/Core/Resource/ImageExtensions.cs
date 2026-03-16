#region

using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Resource
{
    public static class ImageExtensions
    {
        public static string ToImageUrl(this string fileName, IContentSettings contentSettings)
        {
            return string.IsNullOrWhiteSpace(fileName) ? "" : $"{contentSettings.BookCoverUrl}/{fileName}";
        }
    }
}