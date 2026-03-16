#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.Institution;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class Cart : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        private readonly IList<CartItem> _cartItems = new List<CartItem>();

        public virtual IEnumerable<CartItem> CartItems => _cartItems;

        public virtual int InstitutionId { get; set; }

        public virtual string PurchaseOrderNumber { get; set; }
        public virtual string PurchaseOrderComment { get; set; }

        public virtual DateTime? PurchaseDate { get; set; }

        public virtual DateTime? ConvertDate { get; set; }

        public virtual BillingMethodEnum BillingMethod { get; set; }
        public virtual ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }

        public virtual decimal Discount { get; set; }

        public virtual bool Processed { get; set; }

        public virtual string OrderNumber { get; set; }
        public virtual string PromotionCode { get; set; }

        public virtual decimal PromotionDiscount { get; set; }

        public virtual CartTypeEnum CartType { get; set; }
        public virtual string CartName { get; set; }

        public virtual Reseller Reseller { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Cart = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", Processed: {0}", Processed);
            sb.AppendFormat(", PurchaseDate: {0}", PurchaseDate);
            sb.AppendFormat(", BillingMethod: {0}", BillingMethod);
            sb.AppendFormat(", ForthcomingTitlesInvoicingMethodEnum: {0}", ForthcomingTitlesInvoicingMethod);
            sb.AppendFormat(", PurchaseOrderNumber: {0}", PurchaseOrderNumber);
            sb.AppendFormat(", OrderNumber: {0}", OrderNumber);
            sb.AppendFormat(", PromotionCode: {0}", PromotionCode);
            sb.AppendFormat(", PromotionDiscount: {0}", PromotionDiscount);
            sb.AppendFormat(", PurchaseOrderComment: {0}", PurchaseOrderComment);
            sb.AppendFormat(", CartType: {0}", CartType);
            sb.AppendFormat(", ConvertDate: {0}", ConvertDate.GetValueOrDefault());
            sb.AppendFormat(", CartName: {0}", CartName);
            sb.AppendFormat(", Reseller: [{0}]", Reseller != null ? Reseller.ToDebugString() : "NULL");
            sb.AppendLine(", Items = [");

            foreach (var cartItem in CartItems)
            {
                sb.AppendFormat("\t{0}", cartItem.ToDebugString()).AppendLine();
            }

            sb.Append("]");

            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }

        public virtual void AddProduct(Product product)
        {
            _cartItems.Add(new CartItem
            {
                Cart = this,
                Product = product,
                ListPrice = product.Price,
                DiscountPrice = product.Price,
                Include = !product.Optional,
                // if product is optional, default it to not included, otherwise include it as it's not optional
                OriginalSourceId = (int)LicenseOriginalSource.FirmOrder
            });
        }
    }
}