#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

#endregion

namespace R2V2.Core.CollectionManagement
{
    [Serializable]
    public class CachedCart : IDebugInfo
    {
        private readonly IList<CachedPromotion> _availablePromotions = new List<CachedPromotion>();
        private readonly IList<CachedCartItem> _items = new List<CachedCartItem>();

        public CachedCart(Cart cart, IEnumerable<Promotion> availablePromotions,
            ICollection<Promotion> appliedPromotions)
        {
            Id = cart.Id;
            InstitutionId = cart.InstitutionId;
            PurchaseOrderNumber = cart.PurchaseOrderNumber;
            PurchaseOrderComment = cart.PurchaseOrderComment;
            PurchaseDate = cart.PurchaseDate;
            BillingMethod = cart.BillingMethod;
            CartType = cart.CartType;
            ForthcomingTitlesInvoicingMethod = cart.ForthcomingTitlesInvoicingMethod;
            Discount = cart.Discount;
            Processed = cart.Processed;
            CartName = cart.CartName;
            OrderNumber = cart.OrderNumber;
            var promotionCode = cart.PromotionCode;

            CreatedDate = cart.CreationDate;
            UpdatedDate = cart.LastUpdated;
            CreatedBy = cart.CreatedBy;
            ConvertDate = cart.ConvertDate;
            var bracketIndex = cart.CreatedBy == null ? 0 : cart.CreatedBy.IndexOf("[", StringComparison.Ordinal);
            if (cart.CreatedBy != null && bracketIndex > 0)
            {
                var createdUser = cart.CreatedBy.Substring(bracketIndex + 1).Replace("]", "");
                CreatedBy = createdUser;
            }

            if (cart.CartType == CartTypeEnum.AutomatedCart)
            {
                CreatedBy = "R2 Library automated cart service";
            }


            var now = DateTime.Now;
            foreach (var availablePromotion in availablePromotions)
            {
                if (availablePromotion.StartDate <= now && availablePromotion.EndDate >= now &&
                    CanUsePromotion(availablePromotion, appliedPromotions))
                {
                    var cachedPromotion = new CachedPromotion(availablePromotion);
                    _availablePromotions.Add(cachedPromotion);

                    if (promotionCode != null && string.Equals(promotionCode, availablePromotion.Code,
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        Promotion = cachedPromotion;
                    }
                }
            }

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Include && cartItem.RecordStatus)
                {
                    if (cartItem.IsBundle)
                    {
                        Total += cartItem.BundlePrice;
                    }
                    else if (cartItem.NumberOfLicenses == 0 && cartItem.ProductId > 0)
                    {
                        Total += cartItem.DiscountPrice;
                    }
                    else
                    {
                        //Resources
                        if (!cartItem.ResourceId.HasValue)
                        {
                            continue;
                        }

                        Total += cartItem.NumberOfLicenses * cartItem.DiscountPrice;
                    }
                }

                if (cartItem.RecordStatus)
                {
                    _items.Add(new CachedCartItem(cartItem));
                }
            }

            if (cart.Reseller != null)
            {
                ResellerId = cart.Reseller.Id;
                ResellerDiscount = cart.Reseller.Discount;
                ResellerAccountNumber = cart.Reseller.AccountNumberOverride;
            }
        }

        public int Id { get; set; }
        public IEnumerable<CachedCartItem> CartItems => _items;
        public int InstitutionId { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PurchaseOrderComment { get; set; }
        public DateTime? PurchaseDate { get; set; }

        public DateTime? ConvertDate { get; set; }

        [Display(Name = "Created Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By: ")] public string CreatedBy { get; set; }

        [Display(Name = "Last Updated: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime? UpdatedDate { get; set; }

        public BillingMethodEnum BillingMethod { get; set; }
        public ForthcomingTitlesInvoicingMethodEnum ForthcomingTitlesInvoicingMethod { get; set; }
        public decimal Discount { get; set; }
        public bool Processed { get; set; }

        public CartTypeEnum CartType { get; set; }

        public string CartName { get; set; }

        [Display(Name = "Cart Item Count: ")]
        public int ItemCount
        {
            get
            {
                var itemCount = _items.Where(x => x.Include).Count(x => x.NumberOfLicenses > 0 || x.ProductId > 0);
                return itemCount;
            }
        }

        [Display(Name = "Total: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total { get; private set; }

        public string OrderNumber { get; set; }

        public CachedPromotion Promotion { get; set; }

        public bool IsPromotionAvailable
        {
            get
            {
                return _availablePromotions.Any(x => x.EnableCartAlert);
                //return (_availablePromotions.Count > 0);
            }
        }

        public IEnumerable<CachedPromotion> AvailablePromotions => _availablePromotions;

        public int ResellerId { get; set; }
        public decimal ResellerDiscount { get; set; }

        public string ResellerAccountNumber { get; set; }

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
            sb.AppendFormat(", PurchaseOrderComment: {0}", PurchaseOrderComment);

            sb.AppendFormat(", OrderNumber: {0}", OrderNumber);
            sb.AppendFormat(", CartType: {0}", CartType);
            sb.AppendLine().AppendFormat("\t, Promotion: {0}", Promotion == null ? "null" : Promotion.ToDebugString());

            sb.AppendLine().AppendFormat("\t, Total: {0}", Total);
            sb.AppendFormat(", ItemCount: {0}", ItemCount);

            sb.AppendLine(", Items = [");
            foreach (var cartItem in CartItems)
            {
                sb.AppendFormat("\t{0}", cartItem.ToDebugString()).AppendLine();
            }

            sb.Append("]");
            return sb.ToString();
        }

        private bool CanUsePromotion(Promotion promotion, IEnumerable<Promotion> appliedPromotionCodes)
        {
            var appliedPromotions = appliedPromotionCodes.Where(x => x.Id == promotion.Id).ToList();
            if (appliedPromotions.Any())
            {
                if (promotion.MaximumUses > appliedPromotions.Count())
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public void ClearItems()
        {
            _items.Clear();
        }
    }
}