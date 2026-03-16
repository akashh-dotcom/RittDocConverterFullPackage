#region

using System;
using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Promotion
{
    public class OngoingPdaEvent : AuditableEntity
    {
        private readonly IList<OngoingPdaEventResource> _resources = new List<OngoingPdaEventResource>();
        public virtual int EventTypeId { get; set; }
        public virtual bool Processed { get; set; }
        public virtual int LicenseCountAdded { get; set; }
        public virtual string ProcessData { get; set; }
        public virtual Guid TransactionId { get; set; }

        public virtual IEnumerable<OngoingPdaEventResource> Resources => _resources;

        public virtual void AddResource(int resourceId, string isbn, OngoingPdaEvent ongoingPdaEvent)
        {
            _resources.Add(new OngoingPdaEventResource
            {
                ResourceId = resourceId,
                Isbn = isbn,
                OngoingPdaEvent = ongoingPdaEvent
            });
        }
    }
}