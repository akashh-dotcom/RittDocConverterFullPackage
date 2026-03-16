#region

using R2V2.Core.Resource.Discipline;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceSpecialty : AuditableEntity, ISoftDeletable
    {
        public virtual int? ResourceId { get; set; }
        public virtual int SpecialtyId { get; set; }
        public virtual Specialty Specialty { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}