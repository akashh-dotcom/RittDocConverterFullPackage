#region

using System;

#endregion

namespace R2V2.Core.Institution
{
    public enum ResourceListType
    {
        All = 0,
        FeaturedTitles = 1,
        FeaturedPublisher = 2,


        Purchased = 10,
        Archived = 11,
        NewEditionPurchased = 12,
        PdaAdded = 13,
        PdaAddedToCart = 14,
        PdaNewEdition = 15
    }

    public static class ResourceListTypeExtensions
    {
        public static string ToDescription(this ResourceListType resourceListType)
        {
            switch (resourceListType)
            {
                case ResourceListType.FeaturedTitles:
                    return "Featured Titles";

                case ResourceListType.FeaturedPublisher:
                    return "Featured Publisher";

                case ResourceListType.Purchased:
                    return "Purchased Titles";
                case ResourceListType.Archived:
                    return "Purchased Titles Archived";
                case ResourceListType.NewEditionPurchased:
                    return "New Editions of Purchased Titles";
                case ResourceListType.PdaAdded:
                    return "PDA Titles Added";
                case ResourceListType.PdaAddedToCart:
                    return "PDA Titles Triggered";
                case ResourceListType.PdaNewEdition:
                    return "PDA New Editions";


                case ResourceListType.All:
                default:
                    throw new ArgumentOutOfRangeException("resourceListType");
            }
        }
    }
}