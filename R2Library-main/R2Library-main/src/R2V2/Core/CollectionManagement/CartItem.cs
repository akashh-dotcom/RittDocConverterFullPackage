#region

using System;
using System.Collections.Generic;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class CartItem : AuditableEntity, ISoftDeletable, ICartItem, IDiscountResource
    {
        private int _cartId;

        private Product _product;
        public virtual Cart Cart { get; set; }

        /// <summary>
        ///     Not Retrieved or Saved to Database
        /// </summary>
        public virtual int? InstitutionResourceLicenseId { get; set; }

        public virtual int CartId
        {
            get
            {
                if (_cartId == 0)
                {
                    return Cart.Id;
                }

                return _cartId;
            }
            set => _cartId = value;
        }

        public virtual int? ResourceId { get; set; }

        public virtual IProduct Product
        {
            get => _product;
            set
            {
                _product = (Product)value;

                if (_product != null)
                {
                    ProductId = _product.Id;
                }
            }
        }

        public virtual int? ProductId { get; set; }

        public virtual int NumberOfLicenses { get; set; }
        public virtual decimal ListPrice { get; set; }
        public virtual decimal DiscountPrice { get; set; }

        public virtual DateTime? PurchaseDate { get; set; }
        public virtual bool Include { get; set; }
        public virtual bool Agree { get; set; }

        public virtual short OriginalSourceId { get; set; }

        public virtual string SpecialText { get; set; }
        public virtual string SpecialIconName { get; set; }

        public virtual bool PdaPromotionApplied { get; set; }
        public virtual bool IsBundle { get; set; }

        public virtual DateTime? AddedByNewEditionDate { get; set; }

        /// <summary>
        ///     Not Retrieved or Saved to Database
        /// </summary>
        public virtual List<string> AutomatedReasonCodes { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("CartItem = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Cart.Id: {0}", Cart?.Id.ToString() ?? "not set");
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", ProductId: {0}", ProductId);
            sb.AppendFormat(", NumberOfLicenses: {0}", NumberOfLicenses);
            sb.AppendFormat(", ListPrice: {0}", ListPrice);
            sb.AppendFormat(", DiscountPrice: {0}", DiscountPrice);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", PurchaseDate: {0}", PurchaseDate);
            sb.AppendFormat(", Include: {0}", Include);
            sb.AppendFormat(", Agree: {0}", Agree);
            sb.AppendFormat(", OriginalSourceId: {0}", OriginalSourceId);
            sb.AppendFormat(", CreationDate: {0}", CreationDate);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", SpecialText: {0}", SpecialText);
            sb.AppendFormat(", SpecialIconName: {0}", SpecialIconName);
            sb.AppendFormat(", PdaPromotionApplied: {0}", PdaPromotionApplied);
            sb.AppendFormat(", AddedByNewEditionDate: {0}", AddedByNewEditionDate.GetValueOrDefault());
            sb.AppendFormat(", SpecialDiscountId: {0}", SpecialDiscountId);
            sb.AppendFormat(", PdaPromotionId: {0}", PdaPromotionId);
            return sb.ToString();
        }

        public virtual decimal Discount { get; set; }
        public virtual decimal BundlePrice { get; set; }

        /// <summary>
        ///     Not Retrieved or Saved to Database
        /// </summary>
        public virtual int? PdaPromotionId { get; set; }

        /// <summary>
        ///     Not Retrieved or Saved to Database
        /// </summary>
        public virtual int? SpecialDiscountId { get; set; }

        public virtual bool RecordStatus { get; set; }
    }
}