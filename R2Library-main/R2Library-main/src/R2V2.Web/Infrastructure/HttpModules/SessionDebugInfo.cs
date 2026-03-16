#region

using System.Text;
using R2V2.Core;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class SessionDebugInfo : IDebugInfo
    {
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public string InstitutionAccountNumber { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string AuthenticationType { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("SessionDebugInfo = [");
            sb.AppendFormat("InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", InstitutionName: {0}", InstitutionName);
            sb.AppendFormat(", InstitutionAccountNumber: {0}", InstitutionAccountNumber);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", UserFullName: {0}", UserFullName);
            sb.AppendFormat(", AuthenticationType: {0}", AuthenticationType);
            sb.Append("]");
            return sb.ToString();
        }
    }
}