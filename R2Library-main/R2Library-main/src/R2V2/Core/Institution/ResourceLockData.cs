#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class ResourceLockData
    {
        public ResourceLockData(LockType lockType)
        {
            LockType = lockType;
        }

        public LockType LockType { get; private set; }
        public int RequestCount { get; set; }
        public DateTime FirstRequesTimestamp { get; set; }
        public List<ResourceLockUserData> UserData { get; set; }
    }
}