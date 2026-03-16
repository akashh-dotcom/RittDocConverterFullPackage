#region

using System;

#endregion

namespace R2V2.Core.Institution
{
    public enum ResourceFilterType
    {
        All = 0,
        FreeResources = 100,
        ContainsVideo = 400,
        SpecialOffer = 500,
        FeaturedTitles = 999
    }

    public static class ResourceFilterTypeExtensions
    {
        public static string ToDescription(this ResourceFilterType resourceFilterType)
        {
            switch (resourceFilterType)
            {
                case ResourceFilterType.FeaturedTitles:
                    return "Featured Titles";
                case ResourceFilterType.SpecialOffer:
                    return "Special Offer";
                case ResourceFilterType.ContainsVideo:
                    return "Resources with Video";
                case ResourceFilterType.FreeResources:
                    return "Open Access Resources";
                default:
                    throw new ArgumentOutOfRangeException(resourceFilterType.ToString());
            }
        }
    }

    public enum PdaStatus
    {
        None = 0,
        Active = 10,
        Purchased = 20,
        Deleted = 30,
        NotPurchased = 40
    }

    public static class PdaStatusExtensions
    {
        public static string ToDescription(this PdaStatus pdaStatus)
        {
            switch (pdaStatus)
            {
                case PdaStatus.Active:
                    return "Active";
                case PdaStatus.Purchased:
                    return "Purchased";
                case PdaStatus.Deleted:
                    return "Deleted";
                case PdaStatus.NotPurchased:
                    return "Not Purchased";
                default:
                    return "";
            }
        }
    }
}