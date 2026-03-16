#region

using System;

#endregion

namespace R2V2.Core.SuperType
{
    [Serializable]
    public abstract class AuditableEntity<T> : Entity<T>, IAuditable
    {
        public virtual string CreatedBy { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual string UpdatedBy { get; set; }

        public virtual DateTime? LastUpdated { get; set; }
    }

    [Serializable]
    public abstract class AuditableEntity : AuditableEntity<int>
    {
    }
}