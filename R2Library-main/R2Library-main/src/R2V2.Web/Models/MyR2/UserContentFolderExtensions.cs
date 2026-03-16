#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.MyR2;

#endregion

namespace R2V2.Web.Models.MyR2
{
    public static class UserContentFolderExtensions
    {
        public static IEnumerable<UserContentFolder> ToUserContentFolders(
            this IEnumerable<Core.MyR2.UserContentFolder> userContentFolders, UserContentType userContentType)
        {
            return userContentFolders
                .Select(userContentFolder => new UserContentFolder(userContentFolder, userContentType)).ToList();
        }
    }
}