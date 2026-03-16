#region

using System;
using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public abstract class UserContentFolder : AuditableEntity, ISoftDeletable, IMyR2Folder
    {
        public virtual IEnumerable<UserContentItem> UserContentItems { get; set; }
        public virtual MyR2Type Type { get; protected set; }
        public virtual int UserId { get; set; }
        public virtual string FolderName { get; set; }
        public virtual bool DefaultFolder { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}