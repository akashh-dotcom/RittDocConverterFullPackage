#region

using System.Text;

#endregion

namespace R2V2.Core.Promotion
{
    public class PromoteUser : IDebugInfo
    {
        public int UserId { get; set; }
        public string UserNameFirst { get; set; }
        public string UserNameLast { get; set; }
        public string UserEmailAddress { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("PromoteUser = [");
            sb.AppendFormat("UserId: {0}", UserId);
            sb.AppendFormat(", UserNameFirst: {0}", UserNameFirst);
            sb.AppendFormat(", UserNameLast: {0}", UserNameLast);
            sb.AppendFormat(", UserEmailAddress: {0}", UserEmailAddress);
            sb.Append("]");
            return sb.ToString();
        }
    }
}