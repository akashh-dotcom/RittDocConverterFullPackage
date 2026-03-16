#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserSearchHistory : AuditableEntity, ISoftDeletable
    {
        public virtual string SearchXml { get; set; }
        public virtual int UserId { get; set; }
        public virtual int ResultsCount { get; set; }
        public virtual string SearchQuery { get; set; }

        public virtual bool RecordStatus { get; set; }
    }
}