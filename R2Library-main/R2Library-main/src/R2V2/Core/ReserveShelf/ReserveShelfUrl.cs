#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.ReserveShelf
{
    [Serializable]
    public class ReserveShelfUrl : AuditableEntity, ISoftDeletable
    {
        public virtual string Url { get; set; }
        public virtual string Description { get; set; }

        public virtual int ReserveShelfId { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}