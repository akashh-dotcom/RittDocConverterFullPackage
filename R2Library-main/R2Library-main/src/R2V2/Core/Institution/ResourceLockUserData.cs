#region

using System;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class ResourceLockUserData
    {
        public ResourceLockUserData(LockType lockType)
        {
            LockType = lockType;
        }

        public LockType LockType { get; private set; }

        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string EmailAddress { get; set; }
        public string IpAddress { get; set; }
        public long IpNumber { get; set; }
        public int RequestCount { get; set; }
        public DateTime FirstRequesTimestamp { get; set; }
        public DateTime LastRequesTimestamp { get; set; }
        public string SessionId { get; set; }
        public DateTime SessionStartTime { get; set; }
    }
}