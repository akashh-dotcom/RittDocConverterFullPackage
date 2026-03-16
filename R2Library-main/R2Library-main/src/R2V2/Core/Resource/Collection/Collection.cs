#region

using Newtonsoft.Json;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Collection
{
    public class Collection : AuditableEntity, ISoftDeletable, ICollection
    {
        public virtual string Name { get; set; }
        public virtual bool HideInFilter { get; set; }
        public virtual int Sequence { get; set; }
        public virtual bool IsSpecialCollection { get; set; }
        public virtual int SpecialCollectionSequence { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsPublic { get; set; }

        public virtual string ToDebugString()
        {
            return $"Collection = {ToJsonString()}";
        }

        public virtual bool RecordStatus { get; set; }

        public virtual string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}