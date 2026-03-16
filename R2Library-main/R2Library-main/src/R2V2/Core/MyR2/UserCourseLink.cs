#region

using System;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserCourseLink : UserContentItem
    {
        public virtual UserCourseLinksFolder UserCourseLinksFolder { get; set; }

        public override UserContentFolder UserContentFolder
        {
            get => UserCourseLinksFolder;
            set => UserCourseLinksFolder = (UserCourseLinksFolder)value;
        }
    }
}