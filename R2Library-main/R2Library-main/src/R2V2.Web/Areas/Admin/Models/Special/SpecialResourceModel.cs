#region

using System;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Special
{
    public class SpecialResourceModel
    {
        public SpecialResourceModel()
        {
        }

        public SpecialResourceModel(CachedSpecialResource cachedSpecialResource, Uri baseIconUri)
        {
            SpecialName = cachedSpecialResource.SpecialName;
            StartDate = cachedSpecialResource.StartDate;
            EndDate = cachedSpecialResource.EndDate;
            SpecialDiscountId = cachedSpecialResource.SpecialDiscountId;
            ResourceId = cachedSpecialResource.ResourceId;
            DiscountPercentage = cachedSpecialResource.DiscountPercentage;
            IconName = cachedSpecialResource.IconName;
            if (baseIconUri != null)
            {
                SetIconUrl(baseIconUri);
            }

            SpecialText = cachedSpecialResource.SpecialText();
        }

        public SpecialResourceModel(CachedSpecialResource cachedSpecialResource)
        {
            if (cachedSpecialResource != null)
            {
                SpecialName = cachedSpecialResource.SpecialName;
                StartDate = cachedSpecialResource.StartDate;
                EndDate = cachedSpecialResource.EndDate;
                SpecialDiscountId = cachedSpecialResource.SpecialDiscountId;
                ResourceId = cachedSpecialResource.ResourceId;
                DiscountPercentage = cachedSpecialResource.DiscountPercentage;
                IconName = cachedSpecialResource.IconName;
                SpecialText = cachedSpecialResource.SpecialText();
            }
        }

        public string SpecialName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int SpecialDiscountId { get; set; }
        public int ResourceId { get; set; }
        public int DiscountPercentage { get; set; }

        public string IconUrl { get; set; }
        public string IconName { get; set; }

        public string SpecialText { get; set; }

        public void SetIconUrl(Uri baseIconUri)
        {
            IconUrl = new Uri(baseIconUri, IconName).ToString();
        }
    }
}