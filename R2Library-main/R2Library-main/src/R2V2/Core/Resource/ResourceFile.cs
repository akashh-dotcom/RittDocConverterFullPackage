#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceFile : Entity
    {
        public virtual int ResourceId { get; set; }
        public virtual string FilenameFull { get; set; }
        public virtual string FilenamePart1 { get; set; }
        public virtual string FilenamePart3 { get; set; }
        public virtual int? DocumentId { get; set; }

        public virtual Resource Resource { get; set; }
    }
}