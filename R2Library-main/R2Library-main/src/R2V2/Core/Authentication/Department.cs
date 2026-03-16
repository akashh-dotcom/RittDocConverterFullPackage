#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class Department : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual bool List { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Department = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Code: {0}", Code);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", List: {0}", List);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}