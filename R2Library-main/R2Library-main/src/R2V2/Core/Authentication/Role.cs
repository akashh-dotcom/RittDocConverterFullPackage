#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class Role : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual RoleCode Code { get; set; }
        public virtual string Description { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Role = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Code: {0}", Code);
            sb.AppendFormat(", Description: {0}", Description);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}