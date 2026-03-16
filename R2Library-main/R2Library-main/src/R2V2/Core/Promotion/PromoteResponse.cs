#region

using System;
using System.Text;
using Newtonsoft.Json;
using R2V2.Core.MessageQueue;

#endregion

namespace R2V2.Core.Promotion
{
    public class PromoteResponse : R2MessageBase, IDebugInfo
    {
        public ResourcePromoteQueue ResourcePromoteQueue { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ResponseTimestamp { get; set; }

        public new string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("PromoteResponse = [");
            sb.AppendFormat("Success: {0}", Success);
            sb.AppendFormat(", ErrorMessage: {0}", ErrorMessage ?? "null");
            sb.AppendFormat(", ResponseTimestamp: {0}", ResponseTimestamp);
            sb.AppendFormat(", {0}", base.ToDebugString());
            sb.AppendLine().Append("\t\t");
            sb.AppendFormat(", ResourcePromoteQueue: {0}",
                ResourcePromoteQueue == null ? "null" : ResourcePromoteQueue.ToDebugString());
            sb.Append("]");
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string ToJsonString(Formatting formatting)
        {
            return JsonConvert.SerializeObject(this, formatting);
        }
    }
}