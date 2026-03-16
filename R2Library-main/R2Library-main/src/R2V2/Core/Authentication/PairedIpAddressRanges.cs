namespace R2V2.Core.Authentication
{
    public class PairedIpAddressRanges
    {
        public PairedIpAddressRanges()
        {
        }

        public PairedIpAddressRanges(IpAddressRange ipAddressRange1, IpAddressRange ipAddressRange2)
        {
            IpAddressRange1 = ipAddressRange1;
            IpAddressRange2 = ipAddressRange2;
            PopulateMergedIpAddressRange();
        }

        public int IpAddressRangeId1 { get; set; }
        public int IpAddressRangeId2 { get; set; }
        public IpAddressRange IpAddressRange1 { get; set; }
        public IpAddressRange IpAddressRange2 { get; set; }

        public IpAddressRange MergedIpAddressRange { get; set; }

        public void PopulateMergedIpAddressRange()
        {
            var minStart = IpAddressRange1.IpNumberStart < IpAddressRange2.IpNumberStart
                ? IpAddressRange1
                : IpAddressRange2;
            var maxEnd = IpAddressRange1.IpNumberEnd > IpAddressRange2.IpNumberEnd ? IpAddressRange1 : IpAddressRange2;

            MergedIpAddressRange = new IpAddressRange
            {
                InstitutionId = minStart.InstitutionId,
                OctetA = minStart.OctetA,
                OctetB = minStart.OctetB,
                OctetCStart = minStart.OctetCStart,
                OctetDStart = minStart.OctetDStart,
                OctetCEnd = maxEnd.OctetCEnd,
                OctetDEnd = maxEnd.OctetDEnd,
                Description =
                    !string.IsNullOrWhiteSpace(minStart.Description) && !string.IsNullOrWhiteSpace(maxEnd.Description)
                        ? minStart.Description
                        : !string.IsNullOrWhiteSpace(minStart.Description) &&
                          string.IsNullOrWhiteSpace(maxEnd.Description)
                            ? minStart.Description
                            : maxEnd.Description
            };
            MergedIpAddressRange.PopulateDecimals();
        }
    }
}