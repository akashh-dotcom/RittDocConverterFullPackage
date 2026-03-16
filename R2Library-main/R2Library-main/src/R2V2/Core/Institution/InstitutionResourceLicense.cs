#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    public enum LicenseType
    {
        None = 0,
        Purchased = 1,
        Trial = 2,
        Pda = 3
    }

    public enum LicenseOriginalSource
    {
        FirmOrder = 1,
        Pda = 2
    }

    [Serializable]
    public class InstitutionResourceLicense : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int LicenseCount { get; set; }

        public virtual DateTime? FirstPurchaseDate { get; set; }

        public virtual DateTime? PdaAddedDate { get; set; }
        public virtual DateTime? PdaAddedToCartDate { get; set; }
        public virtual string PdaAddedToCartById { get; set; }
        public virtual int PdaViewCount { get; set; }
        public virtual int PdaMaxViews { get; set; }

        public virtual short LicenseTypeId { get; set; }
        public virtual short OriginalSourceId { get; set; }

        public virtual DateTime? PdaDeletedDate { get; set; }
        public virtual string PdaDeletedById { get; set; }


        public virtual DateTime? PdaCartDeletedDate { get; set; }
        public virtual int? PdaCartDeletedById { get; set; }
        public virtual string PdaCartDeletedByName { get; set; }

        public virtual DateTime? PdaRuleAddedDate { get; set; }
        public virtual int? PdaRuleId { get; set; }

        public virtual decimal? AveragePrice { get; set; }

        public virtual Guid? BatchId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResourceLicense = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", LicenseCount: {0}", LicenseCount);
            sb.AppendFormat(", FirstPurchaseDate: {0}",
                FirstPurchaseDate == null ? "null" : $"{FirstPurchaseDate.Value}");
            sb.AppendFormat(", PdaAddedDate: {0}", PdaAddedDate == null ? "null" : $"{PdaAddedDate.Value}");
            sb.AppendFormat(", PdaAddedToCartDate: {0}",
                PdaAddedToCartDate == null ? "null" : $"{PdaAddedToCartDate.Value}");
            sb.AppendFormat(", PdaAddedToCartById: {0}", PdaAddedToCartById);
            sb.AppendFormat(", PdaViewCount: {0}", PdaViewCount);
            sb.AppendFormat(", PdaMaxViews: {0}", PdaMaxViews);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", LicenseTypeId: {0}", LicenseTypeId);
            sb.AppendFormat(", OriginalSourceId: {0}", OriginalSourceId);
            sb.AppendFormat(", PdaCartDeletedDate: {0}",
                PdaCartDeletedDate == null ? "null" : $"{PdaCartDeletedDate.Value}");
            sb.AppendFormat(", PdaCartDeletedById: {0}", PdaCartDeletedById);
            sb.AppendFormat(", PdaRuleAddedDate: {0}", PdaRuleAddedDate);
            sb.AppendFormat(", PdaRuleId: {0}", PdaRuleId);
            sb.AppendFormat(", BatchId: {0}", BatchId == null ? "null" : BatchId.Value.ToString());
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}