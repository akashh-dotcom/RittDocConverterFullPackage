#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    [Serializable]
    public class
        ResourceConcurrency : AuditableEntity // , ISoftDeletable  // Don't think we want this.  Even though there is a RecordStatus column on this table, records always seem to be hard deleted. 
    {
        public virtual string SessionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int InstitutionId { get; set; }
    }
}