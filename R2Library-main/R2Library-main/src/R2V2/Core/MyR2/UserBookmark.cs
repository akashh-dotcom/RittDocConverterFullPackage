#region

using System;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserBookmark : UserContentItem
    {
        public virtual UserBookmarkFolder UserBookmarkFolder { get; set; }

        public override UserContentFolder UserContentFolder
        {
            get => UserBookmarkFolder;
            set => UserBookmarkFolder = (UserBookmarkFolder)value;
        }
    }
}