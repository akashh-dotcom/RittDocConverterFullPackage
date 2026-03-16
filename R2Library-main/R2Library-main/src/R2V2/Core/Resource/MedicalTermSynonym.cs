#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class MedicalTermSynonym : AuditableEntity, ISoftDeletable
    {
        public virtual string Name { get; set; }

        public virtual MedicalTerm MedicalTerm { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}