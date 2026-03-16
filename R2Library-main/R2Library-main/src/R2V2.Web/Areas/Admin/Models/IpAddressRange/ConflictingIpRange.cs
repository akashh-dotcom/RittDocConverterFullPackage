#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.Web.Areas.Admin.Models.IpAddressRange
{
    public class ConflictingIpRange
    {
        public ConflictingIpRange()
        {
        }

        public ConflictingIpRange(PairedIpAddressRanges conflictingIpRanges)
        {
            IpAddressRange1 = conflictingIpRanges.IpAddressRange1.ToIpAddressRange();
            IpAddressRange2 = conflictingIpRanges.IpAddressRange2.ToIpAddressRange();
            MergedIpAddressRange = conflictingIpRanges.MergedIpAddressRange.ToIpAddressRange();
        }

        public ConflictingIpRange(PairedIpAddressRanges conflictingIpRanges, int rowId)
        {
            IpAddressRange1 = conflictingIpRanges.IpAddressRange1.ToIpAddressRange();
            IpAddressRange2 = conflictingIpRanges.IpAddressRange2.ToIpAddressRange();
            MergedIpAddressRange = conflictingIpRanges.MergedIpAddressRange.ToIpAddressRange();
            ConflictedRowId = rowId;
        }

        public int ConflictedRowId { get; set; }
        public WebIpRange IpAddressRange1 { get; set; }
        public WebIpRange IpAddressRange2 { get; set; }
        public WebIpRange MergedIpAddressRange { get; set; }
    }
}