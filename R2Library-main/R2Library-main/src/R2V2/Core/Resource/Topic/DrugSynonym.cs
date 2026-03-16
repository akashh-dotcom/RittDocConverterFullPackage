#region

using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Topic
{
    public class DrugSynonym : AuditableEntity, ISoftDeletable
    {
        public virtual string Name { get; set; }

        public virtual IList<DrugSynonymResource> DrugSynonymResources { get; set; }

        public virtual Drug Drug { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}