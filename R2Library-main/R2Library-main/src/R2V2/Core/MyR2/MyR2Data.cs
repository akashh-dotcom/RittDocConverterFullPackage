#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class MyR2Data : AuditableEntity, ISoftDeletable
    {
        public virtual string GuidCookieValue { get; set; }
        public virtual int Type { get; set; }
        public virtual string FolderName { get; set; }
        public virtual bool DefaultFolder { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual string Json { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}