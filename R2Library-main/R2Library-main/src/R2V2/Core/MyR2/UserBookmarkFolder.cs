#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserBookmarkFolder : UserContentFolder
    {
        private readonly IList<UserBookmark> _userBookmarks;

        public UserBookmarkFolder()
        {
            _userBookmarks = new List<UserBookmark>();
            Type = MyR2Type.Bookmark;
        }

        public virtual IEnumerable<UserBookmark> UserBookmarks => _userBookmarks;

        public override IEnumerable<UserContentItem> UserContentItems => _userBookmarks;

        public virtual void Add(UserBookmark userBookmark)
        {
            _userBookmarks.Add(userBookmark);
            userBookmark.UserBookmarkFolder = this;
        }
    }
}