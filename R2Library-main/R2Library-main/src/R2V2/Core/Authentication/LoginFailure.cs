#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Authentication
{
    public class LoginFailure : IDebugInfo
    {
        public virtual int Id { get; set; }
        public virtual int InstitutionId { get; set; }

        public virtual short OctetA { get; set; }
        public virtual short OctetB { get; set; }
        public virtual short OctetC { get; set; }
        public virtual short OctetD { get; set; }
        public virtual long IpNumericValue { get; set; }

        public virtual string CountryCode { get; set; }
        public virtual DateTime LoginFailureDate { get; set; }

        public virtual string Username { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("IpAddressRange = [ ");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", [{0}.{1}.{2}.{3}]", OctetA, OctetB, OctetC, OctetD);
            sb.AppendFormat(", IpNumericValue: {0}", IpNumericValue);
            sb.AppendFormat(", InstituionId: {0}", InstitutionId);
            sb.AppendFormat(", CountryCode: {0}", CountryCode);
            sb.AppendFormat(", Username: {0}", Username);
            sb.AppendFormat(", LoginFailureDate: {0:u}", LoginFailureDate);
            sb.Append("]");
            return sb.ToString();
        }

        public static LoginFailure CreateLoginFailure(int institutionId, string ipAddress, string countryCode,
            string username, DateTime loginFailureDate)
        {
            var octets = IpAddressRange.ParseIpAddress(ipAddress);
            var loginFailure = new LoginFailure
            {
                InstitutionId = institutionId,
                CountryCode = countryCode,
                Username = username,
                LoginFailureDate = loginFailureDate,
                OctetA = octets[0],
                OctetB = octets[1],
                OctetC = octets[2],
                OctetD = octets[3],
                IpNumericValue = IpAddressRange.CalculateIpNumber(octets[0], octets[1], octets[2], octets[3])
            };
            return loginFailure;
        }
    }
}