#region

using System;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserReference : UserContentItem
    {
        public virtual UserReferencesFolder UserReferencesFolder { get; set; }

        public override UserContentFolder UserContentFolder
        {
            get => UserReferencesFolder;
            set => UserReferencesFolder = (UserReferencesFolder)value;
        }
    }
}