#region

using System;

#endregion

namespace R2V2.Core.Resource
{
    [Serializable]
    public class ResourceIsbn : IDebugInfo
    {
        public virtual int ResourceId { get; set; }
        public virtual int ResourceIsbnTypeId { get; set; }
        public virtual string Isbn { get; set; }
        public virtual bool IsTextMLIsbn { get; set; }

        public virtual string ToDebugString()
        {
            return
                $"ResourceIsbn [ResourceId: {ResourceId}, ResourceIsbnTypeId: {ResourceIsbnTypeId}, Isbn: {Isbn}, IsTextMLIsbn: {IsTextMLIsbn}]";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (ResourceIsbn)obj;
            return ResourceId == other.ResourceId && ResourceIsbnTypeId == other.ResourceIsbnTypeId;
        }

        public override int GetHashCode()
        {
            return ResourceId.GetHashCode() ^ ResourceIsbnTypeId.GetHashCode();
        }
    }
}