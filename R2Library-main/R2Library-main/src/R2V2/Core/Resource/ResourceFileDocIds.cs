#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceFileDocIds : Entity
    {
        public virtual int MaxDocumentId { get; set; }
        public virtual int MinDocumentId { get; set; }
    }
}