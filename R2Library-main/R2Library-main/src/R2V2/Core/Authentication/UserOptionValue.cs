#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class UserOptionValue : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual UserOption Option { get; set; }

        public virtual int UserOptionId { get; set; }
        public virtual string Value { get; set; }

        public virtual int UserId { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("User = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", Value: {0}", Value)
                .AppendFormat(", UserId: {0}", Value)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .AppendFormat(", Option: {0}", Option.ToDebugString())
                .AppendLine().Append("]").ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}