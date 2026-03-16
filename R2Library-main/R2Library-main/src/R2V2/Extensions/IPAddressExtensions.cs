#region

using System.Net;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Extensions
{
    public static class IpAddressExtensions
    {
        /// <summary>
        ///     Extension method to convert IP address into it's decimal value
        ///     (octet1 * 256^3) + (octet2 * 256^2) + (octet3 * 256) + octet4
        /// </summary>
        public static long ToIpNumber(this IPAddress address)
        {
            var octets = address.ToString().Split('.');
            if (octets.Length != 4)
            {
                octets = address.GetIPv4Address().Split('.');
                if (octets.Length != 4)
                {
                    return -1;
                }
            }

            return IpAddressRange.CalculateIpNumber(int.Parse(octets[0]), int.Parse(octets[1]), int.Parse(octets[2]),
                int.Parse(octets[3]));
        }

        /// <summary>
        ///     Extension method to return the IPv4
        /// </summary>
        public static string GetIPv4Address(this IPAddress address)
        {
            var ip4Address = string.Empty;

            foreach (var ipAddress in Dns.GetHostAddresses(address.ToString()))
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    ip4Address = ipAddress.ToString();
                    break;
                }
            }

            if (ip4Address != string.Empty)
            {
                return ip4Address;
            }

            foreach (var ipAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    ip4Address = ipAddress.ToString();
                    break;
                }
            }

            return ip4Address;
        }
    }
}