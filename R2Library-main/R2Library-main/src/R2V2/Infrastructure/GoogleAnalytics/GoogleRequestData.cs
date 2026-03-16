#region

using System;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace R2V2.Infrastructure.GoogleAnalytics
{
    [Serializable]
    public class GoogleRequestData
    {
        public string UrlToSendData { get; set; }
        public string RequestData { get; set; }
        public string SentFromUrl { get; set; }
        public string UserAgent { get; set; }
        public int FailedSaveAttempts { get; set; }
        public string MessageId { get; set; }
        public string Timestamp { get; set; }
        public string Server { get; set; }

        public string ToDebugString()
        {
            return new StringBuilder("GoogleRequestData = [")
                .AppendFormat("MessageId: {0}", MessageId)
                .AppendFormat(", Timestamp: {0}", Timestamp)
                .AppendFormat(", UrlToSendData: {0}", UrlToSendData)
                .AppendFormat(", SentFromUrl: {0}", SentFromUrl)
                .AppendFormat(", UserAgent: {0}", UserAgent)
                .AppendFormat(", RequestData: {0}", RequestData)
                .AppendFormat(", FailedSaveAttempts: {0}", FailedSaveAttempts)
                .AppendFormat(", Server: {0}", Server)
                .Append("]")
                .ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string CreateMessageId()
        {
            var guid = Guid.NewGuid();
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        public static string CreateTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}