#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaRulePracticeArea : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int? PdaRuleId { get; set; }

        public virtual int PracticeAreaId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("RulePracticeArea = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", PracticeArea Id: {0}", PracticeAreaId);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}