#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.OrderHistory
{
    public class DbOrderHistoryItem : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int? OrderHistoryId { get; set; }
        public virtual int? ResourceId { get; set; }
        public virtual int? ProductId { get; set; }
        public virtual int? InstitutionResourceLicenseId { get; set; }
        public virtual int NumberOfLicenses { get; set; }
        public virtual decimal ListPrice { get; set; }
        public virtual decimal DiscountPrice { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual decimal BundlePrice { get; set; }
        public virtual bool IsBundle { get; set; }
        public virtual string SpecialText { get; set; }
        public virtual string SpecialIconName { get; set; }
        public virtual int? SpecialDiscountId { get; set; }
        public virtual int? PdaPromotionId { get; set; }
        public virtual int? DiscountTypeId { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("OrderHistoryItem = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", OrderHistoryId: {0}", OrderHistoryId)
                .AppendFormat(", ProductId: {0}", ProductId)
                .AppendFormat(", InstitutionResourceLicenseId: {0}", InstitutionResourceLicenseId)
                .AppendFormat(", NumberOfLicenses: {0}", NumberOfLicenses)
                .AppendFormat(", ListPrice: {0}", ListPrice)
                .AppendFormat(", DiscountPrice: {0}", DiscountPrice)
                .AppendFormat(", Discount: {0}", Discount)
                .AppendFormat(", SpecialText: {0}", SpecialText)
                .AppendFormat(", SpecialIconName: {0}", SpecialIconName)
                .AppendFormat(", SpecialDiscountId: {0}", SpecialDiscountId)
                .AppendFormat(", PdaPromotionId: {0}", PdaPromotionId)
                .AppendFormat(", CreatedBy: {0}", CreatedBy)
                .AppendFormat(", CreationDate: {0}", CreationDate)
                .AppendFormat(", UpdatedBy: {0}", UpdatedBy)
                .AppendFormat(", LastUpdated: {0}", LastUpdated)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}