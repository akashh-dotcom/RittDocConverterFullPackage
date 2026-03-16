#region

using System.Collections.Generic;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class ConsolidatedIpRange
    {
        public PairedIpAddressRanges PairedIpRanges { get; set; } = new PairedIpAddressRanges();
        public List<WebIpRange> IpAddressRanges { get; set; } = new List<WebIpRange>();

        public int IpAddressRangeCount => IpAddressRanges.Count;

        public string GetScroll()
        {
            return IpAddressRangeCount > 7 ? "consolidate-ip-range-scroll" : "consolidate-ip-range ";
        }
    }
}