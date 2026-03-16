#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    public class UserOptionRole : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual Role Role { get; set; }
        public virtual UserOption Option { get; set; }
        public virtual string DefaultValue { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("User = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", DefaultValue: {0}", DefaultValue)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .AppendFormat(", Role: {0}", Role.ToDebugString())
                .AppendFormat(", Option: {0}", Option.ToDebugString())
                .AppendLine().Append("]").ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}