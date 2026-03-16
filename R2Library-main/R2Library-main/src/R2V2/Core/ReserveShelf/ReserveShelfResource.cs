#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.ReserveShelf
{
    [Serializable]
    public class ReserveShelfResource : AuditableEntity, ISoftDeletable
    {
        public virtual int ResourceId { get; set; }

        public virtual int ReserveShelfListId { get; set; }

        public virtual bool RecordStatus { get; set; }
    }
}