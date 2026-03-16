#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.RequestLogger
{
    [Serializable]
    public class ApplicationSession : IDebugInfo
    {
        public string SessionId { get; set; }
        public DateTime SessionStartTime { get; set; }
        public DateTime SessionLastRequestTime { get; set; }
        public int HitCount { get; set; }
        public string Referrer { get; set; }

        public string ToDebugString()
        {
            return ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ApplicationSession = [");
            sb.AppendFormat("SessionId: {0}", SessionId);
            sb.AppendFormat(", HitCount: {0}", HitCount);
            sb.AppendFormat(", SessionStartTime: {0}", SessionStartTime);
            sb.AppendFormat(", SessionLastRequestTime: {0}", SessionLastRequestTime);
            sb.AppendFormat(", Referrer: {0}", Referrer);
            sb.Append("]");
            return sb.ToString();
        }
    }
}