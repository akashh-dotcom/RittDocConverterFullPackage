#region

using System;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserImage : UserContentItem
    {
        public virtual UserImagesFolder UserImagesFolder { get; set; }

        public override UserContentFolder UserContentFolder
        {
            get => UserImagesFolder;
            set => UserImagesFolder = (UserImagesFolder)value;
        }
    }
}