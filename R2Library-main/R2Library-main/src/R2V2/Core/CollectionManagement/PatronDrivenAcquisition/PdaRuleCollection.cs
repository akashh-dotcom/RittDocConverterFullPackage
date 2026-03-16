#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaRuleCollection : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int? PdaRuleId { get; set; }
        public virtual int CollectionId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PdaRuleCollection = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Collection Id: {0}", CollectionId);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}