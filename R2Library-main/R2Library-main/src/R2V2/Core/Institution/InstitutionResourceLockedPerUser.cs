#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    public class InstitutionResourceLockedPerUser : Entity, IDebugInfo
    {
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResourceLockedPerUser = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.Append("]");
            return sb.ToString();
        }
    }
}