#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.CollectionManagement
{
    [Serializable]
    public class CachedPdaPromotion
    {
        public CachedPdaPromotion(PdaPromotion pdaPromotion)
        {
            Id = pdaPromotion.Id;
            Name = pdaPromotion.Name;
            Description = pdaPromotion.Description;
            Discount = pdaPromotion.Discount;
            StartDate = pdaPromotion.StartDate;
            EndDate = pdaPromotion.EndDate;
            PromotionText = pdaPromotion.PdaPromotionText();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Discount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PromotionText { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("CachedPdaPromotion = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", Discount: {0}", Discount);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", Description: {0}", Description);
            sb.AppendFormat(", PromotionText: {0}", PromotionText);
            sb.Append("]");
            return sb.ToString();
        }
    }
}