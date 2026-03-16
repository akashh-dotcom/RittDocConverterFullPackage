#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PdaRule : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual string Name { get; set; }
        public virtual decimal? MaxPrice { get; set; }
        public virtual bool ExecuteForFuture { get; set; }
        public virtual bool IncludeNewEditionFirm { get; set; }
        public virtual bool IncludeNewEditionPda { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual IList<PdaRuleCollection> Collections { get; set; }
        public virtual IList<PdaRulePracticeArea> PracticeAreas { get; set; }
        public virtual IList<PdaRuleSpecialty> Specialties { get; set; }

        public virtual int ResourcesAdded { get; set; }
        public virtual int ResourcesToAdd { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("PdaRule = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", MaxPrice: {0}", MaxPrice);
            sb.AppendFormat(", ExecuteForFuture: {0}", ExecuteForFuture);
            sb.AppendFormat(", IncludeNewEditionFirm: {0}", IncludeNewEditionFirm);
            sb.AppendFormat(", IncludeNewEditionPda: {0}", IncludeNewEditionPda);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);

            sb.AppendLine().Append("\t, Collections:\t");
            if (Collections != null)
            {
                var ids = Collections.Select(x => x.CollectionId).ToArray();
                sb.AppendFormat(" CollectionIds = [{0}]", string.Join(",", ids));
            }

            sb.AppendLine().Append("\t, PracticeAreas:\t");
            if (PracticeAreas != null)
            {
                var ids = PracticeAreas.Select(x => x.PracticeAreaId).ToArray();
                sb.AppendFormat(" PracticeAreaIds = [{0}]", string.Join(",", ids));
            }

            sb.AppendLine().Append("\t, Specialties:\t");
            if (Specialties != null)
            {
                var ids = Specialties.Select(x => x.SpecialtyId).ToArray();
                sb.AppendFormat(" SpecialtyIds = [{0}]", string.Join(",", ids));
            }

            sb.Append("]");

            return sb.ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}