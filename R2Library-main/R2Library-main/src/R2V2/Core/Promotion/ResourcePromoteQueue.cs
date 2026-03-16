#region

using System;
using System.Text;
using Newtonsoft.Json;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Promotion
{
    public enum ResourcePromoteStatus
    {
        AddedToQueue,
        DeleteFromQueue,
        BatchInitialized,
        PromotionStarted,
        CompletedSuccessfully,
        CompletedWithErrors
    }

    public class ResourcePromoteQueue : AuditableEntity, ISoftDeletable
    {
        public virtual int ResourceId { get; set; }
        public virtual int AddedByUserId { get; set; }
        public virtual int? PromotedByUserId { get; set; }
        public virtual string PromoteBatchName { get; set; }
        public virtual DateTime? PromoteInitDate { get; set; }
        public virtual ResourcePromoteStatus PromoteStatus { get; set; }
        public virtual Guid? BatchKey { get; set; }
        public virtual string Isbn { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("ResourcePromoteQueue = [");
            sb.AppendFormat("ResourceId: {0}", ResourceId);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", AddedByUserId: {0}", AddedByUserId);
            sb.AppendFormat(", PromotedByUserId: {0}", PromotedByUserId == null ? -1 : PromotedByUserId.Value);
            sb.AppendFormat(", PromoteBatchName: {0}", PromoteBatchName);
            sb.AppendFormat(", PromoteInitDate: {0}", PromoteInitDate);
            sb.AppendFormat(", PromoteStatus: {0}", PromoteStatus);
            sb.AppendFormat(", CreationDate: {0} ms", CreationDate);
            sb.AppendFormat(", CreatedBy: {0}", CreatedBy);
            sb.AppendFormat(", LastUpdated: {0}", LastUpdated == null ? "" : LastUpdated.Value.ToString());
            sb.AppendFormat(", UpdatedBy: {0}", UpdatedBy);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}