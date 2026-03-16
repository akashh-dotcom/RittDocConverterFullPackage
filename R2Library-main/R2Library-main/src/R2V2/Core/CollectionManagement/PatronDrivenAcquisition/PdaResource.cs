#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaResource : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int ViewCount { get; set; }
        public virtual int MaxViews { get; set; }

        public virtual DateTime? AddedToCartDate { get; set; }

        public virtual string AddedToCartById { get; set; }
        //public virtual DateTime? LimitReachedDate { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PdaResource = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", ViewCount: {0}", ViewCount);
            sb.AppendFormat(", MaxViews: {0}", MaxViews);
            sb.AppendFormat(", CreatedBy: {0}", CreatedBy);
            sb.AppendFormat(", CreationDate: {0}", CreationDate);
            sb.AppendFormat(", UpdatedBy: {0}", UpdatedBy);
            sb.AppendFormat(", LastUpdated: {0}", LastUpdated);
            sb.AppendFormat(", AddedToCartDate: {0}", AddedToCartDate);
            sb.AppendFormat(", AddedToCartById: {0}", AddedToCartById);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }

        // iInstitutionPdaResourceId, iInstitutionId, iResourceId, dtCreationDate, vchCreatorId, 
        // dtAddedToCartDate, vchAddedToCartById, dtLastUpdate, vchUpdaterId, tiRecordStatus, iViewCount
        public virtual bool RecordStatus { get; set; }
    }
}