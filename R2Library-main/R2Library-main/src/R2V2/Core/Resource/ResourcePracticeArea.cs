#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourcePracticeArea : AuditableEntity, ISoftDeletable
    {
        public virtual int PracticeAreaId { get; set; }
        public virtual PracticeArea.PracticeArea PracticeArea { get; set; }
        public virtual int? ResourceId { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}