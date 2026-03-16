#region

using System;

#endregion

namespace R2V2.Core.Resource
{
    public class SpecialDiscountResource
    {
        public SpecialDiscountResource()
        {
        }

        public SpecialDiscountResource(SpecialResource specialResource)
        {
            SpecialDiscountResourceId = specialResource.Id;
            SpecialName = specialResource.Discount.Special.Name;
            StartDate = specialResource.Discount.Special.StartDate;
            EndDate = specialResource.Discount.Special.EndDate;
            SpecialDiscountId = specialResource.Discount.Id;
            ResourceId = specialResource.ResourceId;
            DiscountPercentage = specialResource.Discount.DiscountPercentage;
            IconName = specialResource.Discount.IconName;
            SpecialId = specialResource.Discount.SpecialId;
        }

        public string SpecialName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int SpecialId { get; set; }
        public int SpecialDiscountResourceId { get; set; }
        public int SpecialDiscountId { get; set; }
        public int ResourceId { get; set; }
        public int DiscountPercentage { get; set; }

        public string IconUrl { get; set; }
        public string IconName { get; set; }

        public string SpecialText()
        {
            return string.Format("{0}: {2}% Discount (Ends: {1}) ", SpecialName, EndDate.ToShortDateString(),
                DiscountPercentage);
        }
    }
}