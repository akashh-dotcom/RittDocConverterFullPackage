#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserReferencesFolder : UserContentFolder
    {
        private readonly IList<UserReference> _userReferences;

        public UserReferencesFolder()
        {
            _userReferences = new List<UserReference>();
            Type = MyR2Type.Reference;
        }

        public virtual IEnumerable<UserReference> UserReferences => _userReferences;

        public override IEnumerable<UserContentItem> UserContentItems => _userReferences;
    }
}