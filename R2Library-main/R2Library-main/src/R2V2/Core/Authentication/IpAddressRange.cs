#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class IpAddressRange : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual Institution.Institution Institution { get; set; }
        public virtual int InstitutionId { get; set; }

        public virtual int OctetA { get; set; }
        public virtual int OctetB { get; set; }
        public virtual int OctetCStart { get; set; }
        public virtual int OctetCEnd { get; set; }
        public virtual int OctetDStart { get; set; }
        public virtual int OctetDEnd { get; set; }
        public virtual long IpNumberStart { get; set; }
        public virtual long IpNumberEnd { get; set; }

        public virtual string Description { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("IpAddressRange = [ ");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", [{0}.{1}.{2}.{3} to {0}.{1}.{4}.{5}]", OctetA, OctetB, OctetCStart, OctetDStart,
                OctetCEnd, OctetDEnd);
            sb.AppendFormat(", [{0} to {1}]", IpNumberStart, IpNumberEnd);
            sb.AppendFormat(", Institution.Id: {0}", Institution?.Id);
            sb.AppendFormat(", Institution.AccountNumber: {0}", Institution?.AccountNumber);
            sb.AppendFormat(", Institution.Name: {0}", Institution?.Name);
            sb.AppendFormat(", Institution.AccountStatusId: {0}", Institution?.AccountStatusId);
            sb.AppendFormat(", Institution.StartDate: {0}",
                Institution?.Trial?.StartDate == null ? "" : $"{Institution.Trial.StartDate.Value:d}");
            sb.AppendFormat(", Institution.EndDate: {0}",
                Institution?.Trial?.EndDate == null ? "" : $"{Institution.Trial.EndDate.Value:d}");
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }

        // todo: not really a big fan of this implementation
        public virtual void PopulateDecimals()
        {
            IpNumberStart = CalculateIpNumber(OctetA, OctetB, OctetCStart, OctetDStart);
            IpNumberEnd = CalculateIpNumber(OctetA, OctetB, OctetCEnd, OctetDEnd);
        }

        public static long CalculateIpNumber(int octetA, int octetB, int octetC, int octetD)
        {
            var ipNumberA = 16777216L * octetA;
            var ipNumberB = 65536L * octetB;
            var ipNumberC = 256L * octetC;
            var ipNumber = ipNumberA + ipNumberB + ipNumberC + octetD;
            return ipNumber;
        }

        public static IpAddressRange ParseIpAddressRange(string startAddress, string endAddress)
        {
            try
            {
                var octets = startAddress.Split('.');

                var ipAddressRange = new IpAddressRange
                {
                    OctetA = int.Parse(octets[0]),
                    OctetB = int.Parse(octets[1]),
                    OctetCStart = int.Parse(octets[2]),
                    OctetDStart = int.Parse(octets[3])
                };

                octets = endAddress.Split('.');
                ipAddressRange.OctetCEnd = int.Parse(octets[2]);
                ipAddressRange.OctetDEnd = int.Parse(octets[3]);

                ipAddressRange.PopulateDecimals();

                return ipAddressRange;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static short[] ParseIpAddress(string ipAddress)
        {
            try
            {
                var octets = ipAddress.Split('.');
                short[] returnValues = { 0, 0, 0, 0 };
                returnValues[0] = short.Parse(octets[0]);
                returnValues[1] = short.Parse(octets[1]);
                returnValues[2] = short.Parse(octets[2]);
                returnValues[3] = short.Parse(octets[3]);
                return returnValues;
            }
            catch (Exception)
            {
                return new short[] { 0, 0, 0, 0 };
            }
        }


        public virtual string GetIpAddressRangeStart()
        {
            return $"{OctetA}.{OctetB}.{OctetCStart}.{OctetDStart}";
        }

        public virtual string GetIpAddressRangeEnd()
        {
            return $"{OctetA}.{OctetB}.{OctetCEnd}.{OctetDEnd}";
        }

        public virtual string ToAuditString()
        {
            var sb = new StringBuilder("IpAddressRange = [ ");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", [{0}.{1}.{2}.{3} to {0}.{1}.{4}.{5}]", OctetA, OctetB, OctetCStart, OctetDStart,
                OctetCEnd, OctetDEnd);
            sb.AppendFormat(", [{0} to {1}]", IpNumberStart, IpNumberEnd);
            sb.AppendFormat(", Institution.AccountNumber: {0}", Institution?.AccountNumber);
            sb.AppendFormat(", Institution.Name: {0}", Institution?.Name);
            sb.Append("]");
            return sb.ToString();
        }
    }
}