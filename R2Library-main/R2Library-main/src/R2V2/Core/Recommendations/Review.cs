#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Recommendations
{
    public interface IReview : IDebugInfo
    {
        int Id { get; set; }
        int InstitutionId { get; set; }
        int CreatedByUserId { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        int DeletedByUserId { get; set; }
        DateTime? DeletedDate { get; set; }
        bool RecordStatus { get; set; }

        IEnumerable<ReviewResource> ReviewResources { get; set; }
        IList<ReviewUser> ReviewUsers { get; set; }
    }

    public class Review : AuditableEntity, IReview
    {
        public virtual int InstitutionId { get; set; }
        public virtual int CreatedByUserId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int DeletedByUserId { get; set; }
        public virtual DateTime? DeletedDate { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual IEnumerable<ReviewResource> ReviewResources { get; set; }
        public virtual IList<ReviewUser> ReviewUsers { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Review = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", CreatedByUserId: {0}", CreatedByUserId);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", DeletedByUserId: {0}", DeletedByUserId);
            sb.AppendFormat(", DeletedDate: {0}", DeletedDate == null ? "null" : DeletedDate.Value.ToString("G"));
            sb.AppendFormat(", Description: {0}", Description);
            sb.Append("]");
            return sb.ToString();
        }
    }
}