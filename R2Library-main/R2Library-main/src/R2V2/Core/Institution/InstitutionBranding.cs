#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class InstitutionBranding : AuditableEntity, ISoftDeletable
    {
        public virtual string Message { get; set; }
        public virtual string InstitutionDisplayName { get; set; }
        public virtual string LogoFileName { get; set; }
        public virtual Institution Institution { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}