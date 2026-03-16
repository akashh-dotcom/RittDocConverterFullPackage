#region

using System;
using System.Linq;
using System.Text;

#endregion

namespace R2V2.Core.CollectionManagement
{
    [Serializable]
    public class CachedPromotion
    {
        public CachedPromotion(Promotion promotion)
        {
            Id = promotion.Id;
            Code = promotion.Code;
            Name = promotion.Name;
            Description = promotion.Description;
            Discount = promotion.Discount;
            StartDate = promotion.StartDate;
            EndDate = promotion.EndDate;
            OrderSource = promotion.OrderSource;
            PromotionProductIds = promotion.PromotionProducts != null
                ? promotion.PromotionProducts.Select(x => x.ProductId).ToArray()
                : null;
            RecordStatus = promotion.RecordStatus;
            MaximumUses = promotion.MaximumUses;
            EnableCartAlert = promotion.EnableCartAlert;
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Discount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OrderSource { get; set; }
        public bool RecordStatus { get; set; }
        public int[] PromotionProductIds { get; set; }
        public int MaximumUses { get; set; }
        public bool EnableCartAlert { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("CachedPromotion = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Code: {0}", Code);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", OrderSource: {0}", OrderSource);
            sb.AppendFormat(", Description: {0}", Description);
            sb.AppendFormat(", MaximumUses: {0}", MaximumUses);
            sb.AppendFormat(", EnableCartAlert: {0}", EnableCartAlert);
            sb.Append("]");
            return sb.ToString();
        }
    }
}