#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class Note : AuditableEntity, ISoftDeletable
    {
        public virtual int InstitutionId { get; set; }
        public virtual int UserId { get; set; }

        public virtual string Comment { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}