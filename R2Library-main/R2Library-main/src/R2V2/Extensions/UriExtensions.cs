#region

using System;

#endregion

namespace R2V2.Extensions
{
    public static class UriHelper
    {
        public static bool IsMatch(string uri1, string uri2)
        {
            var result = false;

            if (!string.IsNullOrEmpty(uri1) && !string.IsNullOrEmpty(uri2))
            {
                var u1 = new Uri(uri1);
                var u2 = new Uri(uri2);

                result = Uri.Compare(u1, u2, UriComponents.Host | UriComponents.PathAndQuery,
                    UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
            }

            return result;
        }

        public static bool IsDomainMatch(string uri1, string domainName)
        {
            var result = false;

            if (!string.IsNullOrEmpty(uri1) && !string.IsNullOrEmpty(domainName))
            {
                var uri = new Uri(uri1);

                result = string.Compare(uri.Host, domainName, StringComparison.OrdinalIgnoreCase) == 0 ||
                         uri.Host.EndsWith($".{domainName}", StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }
    }
}