#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Territory
{
    [Serializable]
    public class Territory : AuditableEntity, ISoftDeletable, ITerritory
    {
        public virtual bool RecordStatus { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
    }
}