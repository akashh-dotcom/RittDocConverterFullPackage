#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.RequestLogger
{
    public class IpAddress
    {
        public IpAddress()
        {
        }

        public IpAddress(string ipAddress)
        {
            var octets = ipAddress.Split('.');
            OctetA = GetOctetValue(octets, 0);
            OctetB = GetOctetValue(octets, 1);
            OctetC = GetOctetValue(octets, 2);
            OctetD = GetOctetValue(octets, 3);
        }

        public short OctetA { get; set; }
        public short OctetB { get; set; }
        public short OctetC { get; set; }
        public short OctetD { get; set; }

        public long IntegerValue =>
            IpAddressRange.CalculateIpNumber(OctetA, OctetB, OctetC, OctetD);

        private short GetOctetValue(string[] octets, int index)
        {
            short.TryParse(octets[index], out var octet);
            return octet;
        }

        public override string ToString()
        {
            return $"{OctetA}.{OctetB}.{OctetC}.{OctetD}";
        }
    }
}