#region

using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Topic
{
    public class Keyword : AuditableEntity, ISoftDeletable
    {
        public virtual string Description { get; set; }

        public virtual IList<KeywordResource> KeywordResources { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}