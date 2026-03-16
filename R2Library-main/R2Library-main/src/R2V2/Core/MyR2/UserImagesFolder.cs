#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserImagesFolder : UserContentFolder
    {
        private readonly IList<UserImage> _userImages;

        public UserImagesFolder()
        {
            _userImages = new List<UserImage>();
            Type = MyR2Type.Image;
        }

        public virtual IEnumerable<UserImage> UserImages => _userImages;

        public override IEnumerable<UserContentItem> UserContentItems => _userImages;
    }
}