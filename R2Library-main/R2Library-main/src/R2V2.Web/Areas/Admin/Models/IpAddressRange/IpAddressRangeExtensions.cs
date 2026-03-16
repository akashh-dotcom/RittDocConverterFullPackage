#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public static class IpAddressRangeExtensions
    {
        public static IEnumerable<WebIpRange> ToIpAddressRanges(
            this IEnumerable<Core.Authentication.IpAddressRange> ipAddressRanges)
        {
            return ipAddressRanges.Select(ToIpAddressRange);
        }

        public static WebIpRange ToIpAddressRange(this Core.Authentication.IpAddressRange ipAddressRange)
        {
            return new WebIpRange
            {
                Id = ipAddressRange.Id,
                OctetA = ipAddressRange.OctetA,
                OctetB = ipAddressRange.OctetB,
                OctetCStart = ipAddressRange.OctetCStart,
                OctetCEnd = ipAddressRange.OctetCEnd,
                OctetDStart = ipAddressRange.OctetDStart,
                OctetDEnd = ipAddressRange.OctetDEnd,
                Description = ipAddressRange.Description,
                AccountNumber = ipAddressRange.Institution != null ? ipAddressRange.Institution.AccountNumber : "N/A",
                InstitutionId = ipAddressRange.Institution != null ? ipAddressRange.Institution.Id : 0
            };
        }
    }
}