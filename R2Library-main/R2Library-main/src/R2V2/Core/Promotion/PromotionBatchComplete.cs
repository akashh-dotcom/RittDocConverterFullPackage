#region

using System;
using System.Text;
using Newtonsoft.Json;
using R2V2.Core.MessageQueue;

#endregion

namespace R2V2.Core.Promotion
{
    public class PromotionBatchComplete : R2MessageBase
    {
        public string BatchName { get; set; }
        public Guid BatchKey { get; set; }
        public DateTime StartTimestamp { get; set; }

        public new string ToDebugString()
        {
            var sb = new StringBuilder("ResourcePromoteQueue = [");
            sb.AppendFormat("BatchName: {0}", BatchName);
            sb.AppendFormat(", BatchKey: {0}", BatchKey);
            sb.AppendFormat(", StartTimestamp: {0}", StartTimestamp);
            sb.AppendFormat(", {0}", base.ToDebugString());
            sb.AppendLine().Append("]");
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}