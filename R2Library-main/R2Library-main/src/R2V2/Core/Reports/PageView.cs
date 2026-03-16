#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Reports
{
    public class PageView : Entity, IDebugInfo
    {
        // pageViewId, institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger
        public virtual int InstitutionId { get; set; }
        public virtual int UserId { get; set; }
        public virtual short IpAddressOctetA { get; set; }
        public virtual short IpAddressOctetB { get; set; }
        public virtual short IpAddressOctetC { get; set; }
        public virtual short IpAddressOctetD { get; set; }
        public virtual long IpAddressInteger { get; set; }

        // pageViewTimestamp, pageViewRunTime, sessionId, url, requestId, referrer, countryCode, serverNumber
        public virtual DateTime Timestamp { get; set; }
        public virtual int RunTime { get; set; }
        public virtual string SessionId { get; set; }
        public virtual string Url { get; set; }
        public virtual string RequestId { get; set; }
        public virtual string Referrer { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual short ServerNumber { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PageView = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", IpAddressOctetA: {0}", IpAddressOctetA);
            sb.AppendFormat(", IpAddressOctetB: {0}", IpAddressOctetB);
            sb.AppendFormat(", IpAddressOctetC: {0}", IpAddressOctetC);
            sb.AppendFormat(", IpAddressOctetD: {0}", IpAddressOctetD);
            sb.AppendFormat(", IpAddressInteger: {0}", IpAddressInteger);
            sb.AppendFormat(", Timestamp: {0}", Timestamp);
            sb.AppendFormat(", RunTime: {0}", RunTime);
            sb.AppendFormat(", SessionId: {0}", SessionId);
            sb.AppendFormat(", Url: {0}", Url);
            sb.AppendFormat(", RequestId: {0}", RequestId);
            sb.AppendFormat(", CountryCode: {0}", CountryCode);
            sb.AppendFormat(", ServerNumber: {0}", ServerNumber);
            sb.AppendFormat(", RequestId: {0}", RequestId);
            sb.Append("]");
            return sb.ToString();
        }
    }
}