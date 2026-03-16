#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Promotion
{
    public class OngoingPdaEventResource : AuditableEntity
    {
        public virtual int ResourceId { get; set; }
        public virtual string Isbn { get; set; }
        public virtual OngoingPdaEvent OngoingPdaEvent { get; set; }
    }
}