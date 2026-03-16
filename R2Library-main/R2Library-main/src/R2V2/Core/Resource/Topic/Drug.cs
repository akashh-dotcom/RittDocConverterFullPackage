#region

using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Topic
{
    public class Drug : AuditableEntity, ISoftDeletable
    {
        public virtual string Name { get; set; }

        public virtual IList<DrugResource> DrugResources { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}