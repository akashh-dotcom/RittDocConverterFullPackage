#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaRuleSpecialty : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int? PdaRuleId { get; set; }
        public virtual int SpecialtyId { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PdaRuleSpecialty = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", SpecialtyId: {0}", SpecialtyId);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}