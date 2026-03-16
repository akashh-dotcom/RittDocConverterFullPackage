#region

using System;
using System.Text;
using Newtonsoft.Json;
using R2V2.Core.MessageQueue;

#endregion

namespace R2V2.Core.Promotion
{
    public class PromoteRequest : R2MessageBase, IDebugInfo
    {
        public int ResourceId { get; set; }
        public string Isbn { get; set; }

        public PromoteUser PromotedByUser { get; set; }
        public PromoteUser AddedByUser { get; set; }

        public Guid BatchKey { get; set; }
        public string BatchName { get; set; }
        public int ErrorCount { get; set; }

        public ResourcePromoteQueue ResourcePromoteQueue { get; set; }

        public new string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("PromoteRequest = [");
            sb.AppendFormat("ResourceId: {0}", ResourceId);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", BatchKey: {0}", BatchKey);
            sb.AppendFormat(", BatchName: {0}", BatchName);
            sb.AppendFormat(", {0}", base.ToDebugString());
            sb.AppendLine().Append("\t\t");
            sb.AppendFormat(", PromotedByUser: {0}", PromotedByUser == null ? "" : PromotedByUser.ToDebugString());
            sb.AppendLine().Append("\t\t");
            sb.AppendFormat(", AddedByUser: {0}", AddedByUser == null ? "" : AddedByUser.ToDebugString());
            sb.AppendLine().Append("\t\t");
            sb.AppendFormat(", ErrorCount: {0}", ErrorCount);
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