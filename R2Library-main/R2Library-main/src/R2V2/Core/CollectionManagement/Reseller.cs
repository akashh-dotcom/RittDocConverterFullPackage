#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class Reseller : ISoftDeletable, IDebugInfo, IEntity
    {
        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual string AccountNumberOverride { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("Reseller = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", Name: {0}", Name)
                .AppendFormat(", DisplayName: {0}", DisplayName)
                .AppendFormat(", Discount: {0}", Discount)
                .AppendFormat(", AccountNumberOverride: {0}", AccountNumberOverride)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .Append("]").ToString();
        }

        public virtual int Id { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}