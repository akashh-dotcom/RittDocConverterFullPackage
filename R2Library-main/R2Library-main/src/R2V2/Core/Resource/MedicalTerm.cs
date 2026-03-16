#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class MedicalTerm : AuditableEntity, ISoftDeletable
    {
        public virtual string Name { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}