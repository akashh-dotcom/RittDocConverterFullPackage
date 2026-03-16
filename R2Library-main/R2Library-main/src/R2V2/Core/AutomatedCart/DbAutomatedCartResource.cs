#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class DbAutomatedCartResource : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual int AutomatedCartInstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int CartItemId { get; set; }

        public virtual decimal ListPrice { get; set; }
        public virtual decimal DiscountPrice { get; set; }
        public virtual int NewEditionCount { get; set; }
        public virtual int TriggeredPdaCount { get; set; }
        public virtual int ReviewedCount { get; set; }
        public virtual int RequestedCount { get; set; }
        public virtual int TurnawayCount { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("DbAutomatedCartResource=[")
                .Append($"AutomatedCartInstitutionId: {AutomatedCartInstitutionId},")
                .Append($"ResourceId: {ResourceId},")
                .Append($"CartItemId: {CartItemId},")
                .Append($"ListPrice: {ListPrice},")
                .Append($"DiscountPrice: {DiscountPrice},")
                .Append($"RecordStatus: {RecordStatus}]")
                .ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}