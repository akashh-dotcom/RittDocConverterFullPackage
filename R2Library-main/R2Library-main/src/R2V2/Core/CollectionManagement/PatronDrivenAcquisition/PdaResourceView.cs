#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaResourceView : IDebugInfo, ISoftDeletable
    {
        // iInstitutionPdaResourceViewId, iInstitutionId, iUserId, iResourceId, dtTimestamp, tiRecordStatus
        public virtual int Id { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual int? UserId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual DateTime Timestamp { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PdaResourceView = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", Timestamp: {0}", Timestamp);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}