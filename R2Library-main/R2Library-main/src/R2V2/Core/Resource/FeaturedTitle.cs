#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class FeaturedTitle : AuditableEntity, ISoftDeletable, IFeaturedTitle
    {
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual int ResourceId { get; set; }

        public virtual string ResourceIsbn { get; set; }
        public virtual string ResourceTitle { get; set; }
        public virtual decimal ResourceListPrice { get; set; }
        public virtual string ResourcePublisherName { get; set; }
        public virtual string ResourceImageFileName { get; set; }

        /// <summary>
        ///     help with debugging via logs!!!
        /// </summary>
        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("FeaturedTitle = [");

            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", ResourceIsbn: {0}", ResourceIsbn);
            sb.AppendFormat(", ResourceTitle: {0}", ResourceTitle);
            sb.AppendFormat(", ResourceListPrice: {0}", ResourceListPrice);
            sb.AppendFormat(", ResourcePublisherName: {0}", ResourcePublisherName);
            sb.AppendFormat(", ResourceImageFileName: {0}", ResourceImageFileName);
            sb.Append("]");

            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}