#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class InstitutionReferrer : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual string ValidReferer { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual IInstitution Institution { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionReferrer = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", ValidReferer: {0}", ValidReferer);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}