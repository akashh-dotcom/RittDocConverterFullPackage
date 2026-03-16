#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Publisher
{
    public class
        Publisher : AuditableEntity,
        IPublisher // , ISoftDeletable  // certain inactive publishers still have active resources, which ends up breaking functionality
    {
        public virtual string Name { get; set; }
        public virtual string Address { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }

        public virtual int ResourceCount { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual IPublisher ConsolidatedPublisher { get; set; }

        public virtual bool IsFeaturedPublisher { get; set; }
        public virtual string ImageFileName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Description { get; set; }

        public virtual int ChildrenResourceCount { get; set; }
        public virtual int ParentResourceCount { get; set; }

        public virtual string ProductDescription { get; set; }

        public virtual DateTime? NotSaleableDate { get; set; }

        public virtual string VendorNumber { get; set; }
    }
}