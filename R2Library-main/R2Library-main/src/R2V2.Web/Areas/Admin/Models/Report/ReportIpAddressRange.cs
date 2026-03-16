#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    [Serializable]
    public class ReportIpAddressRange
    {
        public int Id { get; set; }

        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int? OctetA { get; set; }

        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int? OctetB { get; set; }

        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int? OctetCStart { get; set; }

        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int? OctetCEnd { get; set; }

        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int? OctetDStart { get; set; }

        [Range(0, 255, ErrorMessage = "Valid Ranges are from 0-255")]
        public int? OctetDEnd { get; set; }

        public bool Selected { get; set; }
        public string Checked { get; set; }

        public string GetIpAddressStart()
        {
            return $"{OctetA}.{OctetB}.{OctetCStart}.{OctetDStart}";
        }

        public string GetIpAddressEnd()
        {
            return $"{OctetA}.{OctetB}.{OctetCEnd}.{OctetDEnd}";
        }

        public bool IsValid()
        {
            if (OctetA == null || OctetB == null || OctetCStart == null || OctetCEnd == null || OctetDStart == null ||
                OctetDEnd == null)
            {
                return false;
            }

            return IsValidIpRange();
        }

        public bool IsValidIpRange()
        {
            var octetA = OctetA ?? 0;
            var octetB = OctetB ?? 0;
            var octetCStart = OctetCStart ?? 0;
            var octetCEnd = OctetCEnd ?? 0;
            var octetDStart = OctetDStart ?? 0;
            var octetDEnd = OctetDEnd ?? 0;

            return IsValidOctet(octetA) && IsValidOctet(octetB) && IsValidOctet(octetCStart) &&
                   IsValidOctet(octetCEnd) && IsValidOctet(octetDStart) &&
                   IsValidOctet(octetDEnd);
        }

        public static bool IsValidOctet(int octet)
        {
            return octet >= 0 && octet <= 255;
        }

        public Core.Authentication.IpAddressRange GetIpAddressRange()
        {
            if (IsValid())
            {
                var ipAddressRange = new Core.Authentication.IpAddressRange
                {
                    // ReSharper disable PossibleInvalidOperationException
                    OctetA = (int)OctetA,
                    OctetB = (int)OctetB,
                    OctetCStart = (int)OctetCStart,
                    OctetCEnd = (int)OctetCEnd,
                    OctetDStart = (int)OctetDStart,
                    OctetDEnd = (int)OctetDEnd
                    // ReSharper restore PossibleInvalidOperationException
                };
                ipAddressRange.PopulateDecimals(); // todo: not really a big fan of this implementation
                return ipAddressRange;
            }

            return null;
        }

        public static ReportIpAddressRange CreateReportIpAddressRange(string startAddress, string endAddress)
        {
            var octets = startAddress.Split('.');

            var reportIpAddressRange = new ReportIpAddressRange
            {
                OctetA = int.Parse(octets[0]),
                OctetB = int.Parse(octets[1]),
                OctetCStart = int.Parse(octets[2]),
                OctetDStart = int.Parse(octets[3])
            };

            octets = endAddress.Split('.');
            reportIpAddressRange.OctetCEnd = int.Parse(octets[2]);
            reportIpAddressRange.OctetDEnd = int.Parse(octets[3]);
            return reportIpAddressRange;
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder("ReportIpAddressRange = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Selected: {0}", Selected);
            sb.AppendFormat(", Checked: {0}", Checked);
            sb.AppendFormat(", {0}.{1}.{2}.{3} to {0}.{1}.{4}.{5}", OctetA, OctetB, OctetCStart, OctetCEnd, OctetDStart,
                OctetDEnd);
            sb.Append("]");
            return sb.ToString();
        }
    }
}