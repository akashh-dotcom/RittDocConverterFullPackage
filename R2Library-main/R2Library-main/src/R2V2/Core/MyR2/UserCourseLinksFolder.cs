#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserCourseLinksFolder : UserContentFolder
    {
        private readonly IList<UserCourseLink> _userCourseLinks;

        public UserCourseLinksFolder()
        {
            _userCourseLinks = new List<UserCourseLink>();
            Type = MyR2Type.CourseLink;
        }

        public virtual IEnumerable<UserCourseLink> UserCourseLinks => _userCourseLinks;

        public override IEnumerable<UserContentItem> UserContentItems => _userCourseLinks;
    }
}