#region

using System;
using System.Text;
using System.Web;
using R2V2.Core;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class RequestLoggerData : IDebugInfo
    {
        /// <param name="requestId"> </param>
        public RequestLoggerData(string rawUrl, string ipAddress, string requestId)
        {
            StartTime = DateTime.Now;
            RawUrl = rawUrl;
            IpAddress = ipAddress;
            RequestId = requestId;
            IsSecureConnection = GetIsSecureConnection();
        }

        public string RequestId { get; set; }
        public string RawUrl { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; private set; }
        public TimeSpan TimeSpan { get; private set; }
        public string IpAddress { get; }

        public string Referrer { get; set; }
        public string CountryCode { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }
        public int InstitutionId { get; set; }
        public string InstitutionAccountNumber { get; set; }
        public string InstitutionName { get; set; }
        public string AuthenticationType { get; set; }
        public string AspSessionId { get; private set; }
        public bool IsSecureConnection { get; set; }

        public string HttpMethod { get; set; }


        public string ToDebugString()
        {
            var sb = new StringBuilder("RequestLoggerData = [");
            sb.AppendFormat("RequestId: {0}", RequestId);
            sb.AppendFormat(", RawUrl: {0}", RawUrl);
            sb.AppendFormat(", StartTime: {0:T}", StartTime);
            sb.AppendFormat(", EndTime: {0:T}", EndTime);
            sb.AppendFormat(", TimeSpan: {0:0.###}", TimeSpan.TotalSeconds);
            sb.AppendFormat(", IPAddress: {0}", IpAddress);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", UserName: {0}", UserName);
            sb.AppendFormat(", IsAuthenticated: {0}", IsAuthenticated);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", InstitutionAccountNumber: {0}", InstitutionAccountNumber);
            sb.AppendFormat(", InstitutionName: {0}", InstitutionName);
            sb.AppendFormat(", AuthenticationType: {0}", AuthenticationType);
            sb.AppendFormat(", AspSessionId: {0}", AspSessionId);
            sb.AppendFormat(", Referrer: {0}", Referrer);
            sb.AppendFormat(", CountryCode: {0}", CountryCode);
            sb.AppendFormat(", HttpMethod: {0}", HttpMethod);
            sb.Append("]");
            return sb.ToString();
        }

        public void EndRequestTiming()
        {
            EndTime = DateTime.Now;
            TimeSpan = EndTime.Subtract(StartTime);
        }

        public TimeSpan GetCurrentRuntime()
        {
            return DateTime.Now.Subtract(StartTime);
        }

        public void SetAspSessionId()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                AspSessionId = HttpContext.Current.Session.SessionID;
            }
            else
            {
                AspSessionId = "n/a";
            }
        }

        private bool GetIsSecureConnection()
        {
            if (HttpContext.Current.Request.IsSecureConnection)
            {
                return true;
            }

            var xForwardPort = HttpContext.Current.Request.Headers["X-Forwarded-Port"];
            var xForwardProto = HttpContext.Current.Request.Headers["X-Forwarded-Proto"];
            var isSecure = !string.IsNullOrWhiteSpace(xForwardPort) && !string.IsNullOrWhiteSpace(xForwardProto) &&
                           xForwardPort == "443" && xForwardProto == "https";
            return isSecure;
        }
    }
}