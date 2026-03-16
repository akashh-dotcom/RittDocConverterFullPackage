#region

using System;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Recommendations
{
    /// <summary>
    /// </summary>
    public interface IReviewResource : IDebugInfo
    {
        int Id { get; set; }
        int ReviewId { get; set; }
        int ResourceId { get; set; }
        int AddedByUserId { get; set; }
        short ActionTypeId { get; set; } // 0=no action, 1=recommended, 2=not recommended
        int? ActionByUserId { get; set; }
        DateTime? ActionDate { get; set; }
        int? DeletedByUserId { get; set; }
        DateTime? DeletedDate { get; set; }
        string Notes { get; set; }
        bool RecordStatus { get; set; }

        IUser AddedByUser { get; set; }
        IUser ActionByUser { get; set; }
        IUser DeletedByUser { get; set; }
    }

    /// <summary>
    /// </summary>
    public class ReviewResource : AuditableEntity, IReviewResource
    {
        public virtual int ReviewId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int AddedByUserId { get; set; }
        public virtual short ActionTypeId { get; set; }
        public virtual int? ActionByUserId { get; set; }
        public virtual DateTime? ActionDate { get; set; }
        public virtual int? DeletedByUserId { get; set; }
        public virtual DateTime? DeletedDate { get; set; }
        public virtual string Notes { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual IUser AddedByUser { get; set; }
        public virtual IUser ActionByUser { get; set; }
        public virtual IUser DeletedByUser { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("ReviewResource = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", ReviewId: {0}", ReviewId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", AddedByUserId: {0}", AddedByUserId);
            sb.AppendFormat(", ActionTypeId: {0}", ActionTypeId);
            sb.AppendFormat(", ActionByUserId: {0}", ActionByUserId);
            sb.AppendFormat(", ActionDate: {0}", ActionDate == null ? "null" : ActionDate.Value.ToString("G"));
            sb.AppendFormat(", Notes: {0}", Notes);
            sb.AppendFormat(", DeletedByUserId: {0}", DeletedByUserId);
            sb.AppendFormat(", DeletedDate: {0}", DeletedDate == null ? "null" : DeletedDate.Value.ToString("G"));
            sb.Append("]");
            return sb.ToString();
        }
    }
}