#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Admin
{
    [Serializable]
    public class AlertImage : AuditableEntity, ISoftDeletable
    {
        public virtual string ImageFileName { get; set; }

        public virtual int AlertId { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}