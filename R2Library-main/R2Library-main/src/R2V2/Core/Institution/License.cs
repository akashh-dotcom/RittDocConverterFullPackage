#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class License : IDebugInfo
    {
        public License(InstitutionResourceLicense institutionResourceLicense)
        {
            Id = institutionResourceLicense.Id;
            InstitutionId = institutionResourceLicense.InstitutionId;
            ResourceId = institutionResourceLicense.ResourceId;
            LicenseType = (LicenseType)institutionResourceLicense.LicenseTypeId;
            OriginalSource = (LicenseOriginalSource)institutionResourceLicense.OriginalSourceId;
            FirstPurchaseDate = institutionResourceLicense.FirstPurchaseDate;

            PdaAddedDate = institutionResourceLicense.PdaAddedDate;
            PdaAddedToCartDate = institutionResourceLicense.PdaAddedToCartDate;
            PdaAddedToCartById = institutionResourceLicense.PdaAddedToCartById;
            PdaViewCount = institutionResourceLicense.PdaViewCount;
            PdaMaxViews = institutionResourceLicense.PdaMaxViews;

            LicenseCount = LicenseType == LicenseType.Pda
                ? PdaMaxViews - PdaViewCount
                : institutionResourceLicense.LicenseCount;

            //ResourceNotSaleableDate = institutionResourceLicense.ResourceNotSaleableDate;

            RecordStatus = institutionResourceLicense.RecordStatus;

            PdaDeletedDate = institutionResourceLicense.PdaDeletedDate;
            PdaDeletedById = institutionResourceLicense.PdaDeletedById;

            PdaCartDeletedByName = institutionResourceLicense.PdaCartDeletedByName;
            PdaCartDeletedById = institutionResourceLicense.PdaCartDeletedById ?? 0;
            PdaCartDeletedDate = institutionResourceLicense.PdaCartDeletedDate;

            PdaRuleId = institutionResourceLicense.PdaRuleId.GetValueOrDefault(0);
            PdaRuleAddedDate = institutionResourceLicense.PdaRuleAddedDate;
        }

        public License()
        {
        }

        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public int ResourceId { get; set; }
        public int LicenseCount { get; set; }
        public LicenseType LicenseType { get; set; }
        public LicenseOriginalSource OriginalSource { get; set; }
        public DateTime? FirstPurchaseDate { get; set; }

        public DateTime? PdaAddedDate { get; set; }
        public DateTime? PdaAddedToCartDate { get; set; }
        public string PdaAddedToCartById { get; set; }
        public int PdaViewCount { get; set; }
        public int PdaMaxViews { get; set; }

        //public DateTime? ResourceNotSaleableDate { get; set; }

        public bool RecordStatus { get; set; }

        public DateTime? PdaDeletedDate { get; set; }
        public string PdaDeletedById { get; set; }


        public string PdaCartDeletedByName { get; set; }
        public int PdaCartDeletedById { get; set; }
        public DateTime? PdaCartDeletedDate { get; set; }

        public int PdaRuleId { get; set; }
        public DateTime? PdaRuleAddedDate { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("License = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", LicenseCount: {0}", LicenseCount);
            sb.AppendFormat(", OriginalSource: {0}", OriginalSource);
            sb.AppendFormat(", LicenseType: {0}", LicenseType);
            sb.AppendFormat(", FirstPurchaseDate: {0}",
                FirstPurchaseDate == null ? "null" : FirstPurchaseDate.Value.ToLongDateString());
            sb.AppendFormat(", PdaAddedDate: {0}",
                PdaAddedDate == null ? "null" : PdaAddedDate.Value.ToLongDateString());
            sb.AppendFormat(", PdaAddedToCartDate: {0}",
                PdaAddedToCartDate == null ? "null" : PdaAddedToCartDate.Value.ToLongDateString());
            sb.AppendFormat(", PdaAddedToCartById: {0}", PdaAddedToCartById);
            sb.AppendFormat(", PdaViewCount: {0}", PdaViewCount);
            sb.AppendFormat(", PdaMaxViews: {0}", PdaMaxViews);
            sb.AppendFormat(", PdaViewCount: {0}", RecordStatus);
            sb.AppendFormat(", PdaRuleId: {0}", PdaRuleId);
            sb.AppendFormat(", PdaRuleAddedDate: {0}", PdaRuleAddedDate);
            sb.Append("]");
            return sb.ToString();
        }
    }
}