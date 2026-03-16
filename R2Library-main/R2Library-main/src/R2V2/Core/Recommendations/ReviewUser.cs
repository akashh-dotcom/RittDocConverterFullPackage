#region

using System;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Recommendations
{
    public interface IReviewUser : IDebugInfo
    {
        int Id { get; set; }
        int ReviewId { get; set; }
        int UserId { get; set; }
        int AddedByUserId { get; set; }
        DateTime? LastAlertDate { get; set; }
        int? DeletedByUserId { get; set; }
        DateTime? DeletedDate { get; set; }
        bool RecordStatus { get; set; }
        IUser User { get; set; }
    }

    public class ReviewUser : AuditableEntity, IReviewUser
    {
        public virtual int ReviewId { get; set; }
        public virtual int UserId { get; set; }
        public virtual int AddedByUserId { get; set; }
        public virtual DateTime? LastAlertDate { get; set; }
        public virtual int? DeletedByUserId { get; set; }
        public virtual DateTime? DeletedDate { get; set; }
        public virtual bool RecordStatus { get; set; }
        public virtual IUser User { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("ReviewUser = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", ReviewId: {0}", ReviewId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", AddedByUserId: {0}", AddedByUserId);
            sb.AppendFormat(", DeletedDate: {0}", LastAlertDate == null ? "null" : LastAlertDate.Value.ToString("G"));
            sb.AppendFormat(", DeletedByUserId: {0}", DeletedByUserId);
            sb.AppendFormat(", DeletedDate: {0}", DeletedDate == null ? "null" : DeletedDate.Value.ToString("G"));
            sb.Append("]");
            return sb.ToString();
        }
    }
}