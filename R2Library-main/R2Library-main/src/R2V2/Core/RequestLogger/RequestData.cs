#region

using System;
using System.Text;
using Newtonsoft.Json;
using R2V2.Infrastructure.MessageQueue;

#endregion

namespace R2V2.Core.RequestLogger
{
    [Serializable]
    public class RequestData : IDebugInfo, IR2V2Message
    {
        public int InstitutionId { get; set; }
        public int UserId { get; set; }
        public IpAddress IpAddress { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public int RequestDuration { get; set; }
        public string Url { get; set; }
        public string RequestId { get; set; }

        public string Referrer { get; set; }
        public string CountryCode { get; set; }

        public ApplicationSession Session { get; set; }
        public ContentView ContentView { get; set; }
        public SearchRequest SearchRequest { get; set; }

        public MediaView MediaView { get; set; }

        public bool LogRequest { get; set; } = true;

        public int FailedSaveAttempts { get; set; }

        public int ServerNumber { get; set; }

        public string AuthenticationType { get; set; }

        public string HttpMethod { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("RequestData = [");
            sb.AppendFormat("RequestId: {0}", RequestId);
            sb.AppendFormat(", Url: {0}", Url);
            sb.AppendFormat(", LogRequest: {0}", LogRequest);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", IpAddress: {0}", IpAddress);
            sb.AppendFormat(", RequestTimestamp: {0}", RequestTimestamp);
            sb.AppendFormat(", RequestDuration: {0} ms", RequestDuration);
            sb.AppendFormat(", Session: [{0}]", Session);
            sb.AppendFormat(", Referrer: {0}", Referrer);
            sb.AppendFormat(", ServerNumber: {0}", ServerNumber);
            sb.AppendFormat(", CountryCode: [{0}]", CountryCode);
            sb.AppendFormat(", ContentView: [{0}]", ContentView != null ? ContentView.ToDebugString() : "null");
            sb.AppendFormat(", SearchRequest: [{0}]", SearchRequest != null ? SearchRequest.ToDebugString() : "null");
            sb.AppendFormat(", MediaView: [{0}]", MediaView != null ? MediaView.ToDebugString() : "null");
            sb.AppendFormat(", AuthenticationType: [{0}]", AuthenticationType);
            sb.AppendFormat(", FailedSaveAttempts: [{0}]", FailedSaveAttempts);
            sb.AppendFormat(", MessageId: [{0}]", MessageId);
            sb.AppendFormat(", HttpMethod: [{0}]", HttpMethod);
            sb.Append("]");
            return sb.ToString();
        }

        public Guid MessageId { get; set; } = Guid.NewGuid();

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void DoNotLogRequest()
        {
            LogRequest = false;
        }
    }
}