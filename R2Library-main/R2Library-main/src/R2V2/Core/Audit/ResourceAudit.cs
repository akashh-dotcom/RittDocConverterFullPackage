#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Audit
{
    public class ResourceAudit
    {
        public virtual int Id { get; set; }

        public virtual int ResourceId { get; set; }

        public virtual ResourceAuditType ResourceAuditType { get; set; }

        public virtual string EventDescription { get; set; }

        public virtual string CreatedBy { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("ResourceAudit = [")
                .AppendFormat("ResourceId: {0}||", ResourceId)
                .AppendFormat("ResourceAuditType: {0}||", (int)ResourceAuditType)
                .AppendFormat("EventDescription: {0}]", EventDescription)
                .ToString();
        }
    }
}