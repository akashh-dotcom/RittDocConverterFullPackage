#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace R2V2.Core.CollectionManagement
{
    [Serializable]
    public class CachedCartItem : ICartItem, IDiscountResource
    {
        public CachedCartItem(CartItem cartItem)
        {
            Id = cartItem.Id;
            CartId = cartItem.Cart.Id;
            ResourceId = cartItem.ResourceId;
            Product = cartItem.Product;
            ProductId = cartItem.ProductId;
            NumberOfLicenses = cartItem.NumberOfLicenses;
            ListPrice = cartItem.ListPrice;
            DiscountPrice = cartItem.DiscountPrice;
            PurchaseDate = cartItem.PurchaseDate;
            Include = cartItem.Include;
            Agree = cartItem.Agree;
            OriginalSourceId = cartItem.OriginalSourceId;
            CreationDate = cartItem.CreationDate;

            SpecialText = cartItem.SpecialText;
            SpecialIconName = cartItem.SpecialIconName;

            AddedByNewEditionDate = cartItem.AddedByNewEditionDate;

            PdaPromotionId = cartItem.PdaPromotionId;
            SpecialDiscountId = cartItem.SpecialDiscountId;

            IsBundle = cartItem.IsBundle;
            BundlePrice = cartItem.BundlePrice;
        }

        public int Id { get; set; }
        public int CartId { get; set; }
        public int? ResourceId { get; set; }
        public IProduct Product { get; set; }
        public int? ProductId { get; set; }
        public int NumberOfLicenses { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public bool Include { get; set; }
        public bool Agree { get; set; }
        public short OriginalSourceId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsBundle { get; set; }

        public bool PdaPromotionApplied { get; set; }
        public DateTime? AddedByNewEditionDate { get; set; }

        public string SpecialText { get; set; }
        public string SpecialIconName { get; set; }

        public List<string> AutomatedReasonCodes { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("CachedCartItem = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", CartId: {0}", CartId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", ProductId: {0}", ProductId);
            sb.AppendFormat(", NumberOfLicenses: {0}", NumberOfLicenses);
            sb.AppendFormat(", ListPrice: {0}", ListPrice);
            sb.AppendFormat(", DiscountPrice: {0}", DiscountPrice);
            sb.AppendFormat(", PurchaseDate: {0}", PurchaseDate);
            sb.AppendFormat(", Include: {0}", Include);
            sb.AppendFormat(", Agree: {0}", Agree);
            sb.AppendFormat(", OriginalSourceId: {0}", OriginalSourceId);
            sb.AppendFormat(", CreationDate: {0}", CreationDate);
            sb.AppendFormat(", SpecialText: {0}", SpecialText);
            sb.AppendFormat(", SpecialIconName: {0}", SpecialIconName);
            sb.AppendFormat(", AddedByNewEditionDate: {0}]", AddedByNewEditionDate);

            return sb.ToString();
        }

        public decimal Discount { get; set; }
        public int? PdaPromotionId { get; set; }
        public int? SpecialDiscountId { get; set; }
        public decimal BundlePrice { get; set; }
    }
}