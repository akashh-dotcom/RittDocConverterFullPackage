#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface IProduct
    {
        int Id { get; set; }
        string Name { get; set; }
        decimal Price { get; set; }
        bool Optional { get; set; }
    }

    [Serializable]
    public class Product : AuditableEntity, ISoftDeletable, IDebugInfo, IProduct
    {
        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Product = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", Price: {0}", Price);
            sb.AppendFormat(", Optional: {0}", Optional);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            return sb.ToString();
        }

        public virtual string Name { get; set; }
        public virtual decimal Price { get; set; }

        public virtual bool Optional { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}