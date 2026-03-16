#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.CollectionManagement;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.OrderHistory
{
    public class DbOrderHistory : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int CartId { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual string OrderNumber { get; set; }
        public virtual string PurchaseOrderNumber { get; set; }
        public virtual string PurchaseOrderComment { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual string PromotionCode { get; set; }
        public virtual string PromotionDescription { get; set; }
        public virtual int? PromotionId { get; set; }
        public virtual DateTime PurchaseDate { get; set; }
        public virtual BillingMethodEnum BillingMethod { get; set; }
        public virtual ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }
        public virtual string CartName { get; set; }
        public virtual Reseller Reseller { get; set; }
        public virtual string ResellerName { get; set; }
        public virtual decimal? ResellerDiscount { get; set; }
        public virtual string OrderFile { get; set; }
        public virtual int DiscountTypeId { get; set; }

        public virtual ICollection<DbOrderHistoryItem> OrderHistoryItems { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("OrderHistory = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", CartId: {0}", CartId)
                .AppendFormat(", InstitutionId: {0}", InstitutionId)
                .AppendFormat(", OrderNumber: {0}", OrderNumber)
                .AppendFormat(", PurchaseOrderNumber: {0}", PurchaseOrderNumber)
                .AppendFormat(", PurchaseOrderComment: {0}", PurchaseOrderComment)
                .AppendFormat(", Discount: {0}", Discount)
                .AppendFormat(", PromotionCode: {0}", PromotionCode)
                .AppendFormat(", PromotionDescription: {0}", PromotionDescription)
                .AppendFormat(", PromotionId: {0}", PromotionId)
                .AppendFormat(", PurchaseDate: {0}", PurchaseDate)
                .AppendFormat(", BillingMethod: {0}", BillingMethod)
                .AppendFormat(", ForthcomingTitlesInvoicingMethod: {0}", ForthcomingTitlesInvoicingMethod)
                .AppendFormat(", CartName: {0}", CartName)
                .AppendFormat(", ResellerId: {0}", Reseller != null ? Reseller.Id.ToString() : null)
                .AppendFormat(", ResellerName: {0}", ResellerName)
                .AppendFormat(", ResellerDiscount: {0}", ResellerDiscount)
                .AppendFormat(", BillingMethod: {0}", BillingMethod)
                .AppendFormat(", OrderFile: {0}", OrderFile)
                .AppendFormat(", CreatedBy: {0}", CreatedBy)
                .AppendFormat(", CreationDate: {0}", CreationDate)
                .AppendFormat(", UpdatedBy: {0}", UpdatedBy)
                .AppendFormat(", LastUpdated: {0}", LastUpdated)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .AppendLine(", OrderHistoryItems = [");

            foreach (var items in OrderHistoryItems)
            {
                sb.AppendFormat("\t{0}", items.ToDebugString()).AppendLine();
            }

            sb.Append("]");

            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}