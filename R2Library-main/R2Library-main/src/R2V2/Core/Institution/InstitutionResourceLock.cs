#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    public enum LockType
    {
        None = 0,
        All = 1,
        Print = 2,
        Email = 3
    }

    [Serializable]
    public class InstitutionResourceLock : Entity, IDebugInfo
    {
        public virtual int InstitutionId { get; set; }
        public virtual int? UserId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual DateTime LockStartDate { get; set; }
        public virtual DateTime LockEndDate { get; set; }
        public virtual DateTime? LockEmailAlertTimestamp { get; set; }
        public virtual string LockData { get; set; }
        public virtual string LockEmailAlertData { get; set; }

        public virtual LockType LockType { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResourceLock = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", LockType: {0}", LockType);
            sb.AppendFormat(", LockStartDate: {0}", LockStartDate);
            sb.AppendFormat(", LockEndDate: {0}", LockEndDate);
            sb.AppendFormat(", LockEmailAlertTimestamp: {0}",
                LockEmailAlertTimestamp == null ? "null" : $"{LockEmailAlertTimestamp.Value}");
            sb.AppendFormat(", LockData: {0}", LockData);
            sb.AppendFormat(", LockEmailAlertData: {0}", LockEmailAlertData);
            sb.Append("]");
            return sb.ToString();
        }
    }
}