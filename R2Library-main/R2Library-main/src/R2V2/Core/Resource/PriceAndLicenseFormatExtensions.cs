#region

using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Core.Resource
{
    public static class PriceAndLicenseFormatExtensions
    {
        public static string ListPriceString(this IResource resource, bool returnEmpty = false)
        {
            if (resource.IsFreeResource)
            {
                return returnEmpty ? "" : "Free";
            }

            return $"{resource.ListPrice:C}";
        }

        public static string DiscountPriceString(this IResource resource, decimal discountPrice,
            bool returnEmpty = false)
        {
            if (resource.IsFreeResource)
            {
                return returnEmpty ? "" : "Free";
            }

            return $"{(discountPrice == 0 ? resource.ListPrice : discountPrice):C}";
        }

        public static string GetLicenseString(this CartItem cartItem, IResource resource)
        {
            return resource != null && resource.IsFreeResource ? "Unlimited" : cartItem.NumberOfLicenses.ToString();
        }

        public static int GetLicenseCount(this CartItem cartItem, IResource resource)
        {
            return resource != null && resource.IsFreeResource && cartItem.NumberOfLicenses > 0
                ? 1
                : cartItem.NumberOfLicenses;
        }

        public static string GetLicenseCount(this IResource resource, string licenseCount)
        {
            return resource.IsFreeResource ? 1.ToString() : licenseCount;
        }

        public static int GetLicenseCount(this IResourceOrderItem resourceOrderItem)
        {
            return resourceOrderItem.CoreResource.IsFreeResource ? 1 : resourceOrderItem.NumberOfLicenses;
        }
    }
}