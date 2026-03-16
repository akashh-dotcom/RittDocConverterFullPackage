#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Audit
{
    public class InstitutionAudit
    {
        public virtual int Id { get; set; }

        public virtual int InstitutionId { get; set; }

        public virtual InstitutionAuditType InstitutionAuditType { get; set; }

        public virtual string EventDescription { get; set; }

        public virtual string CreatedBy { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("InstitutionAudit = [")
                .AppendFormat("InstitutionId: {0}||", InstitutionId)
                .AppendFormat("InstitutionAuditType: {0}||", (int)InstitutionAuditType)
                .AppendFormat("EventDescription: {0}]", EventDescription)
                .ToString();
        }
    }
}