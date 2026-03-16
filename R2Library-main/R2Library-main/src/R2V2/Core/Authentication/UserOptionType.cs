#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class UserOptionType : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual OptionTypeCode Code { get; set; }
        public virtual string Description { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("User = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", Code: {0}", Code)
                .AppendFormat(", Description: {0}", Description)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .AppendLine().Append("]").ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}