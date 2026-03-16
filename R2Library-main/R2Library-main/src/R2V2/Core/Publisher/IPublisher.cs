#region

using System;

#endregion

namespace R2V2.Core.Publisher
{
    public interface IPublisher
    {
        string Name { get; set; }
        string VendorNumber { get; set; }
        string Address { get; set; }
        string Address2 { get; set; }
        string City { get; set; }
        string State { get; set; }
        string Zip { get; set; }
        bool RecordStatus { get; set; }
        IPublisher ConsolidatedPublisher { get; set; }
        bool IsFeaturedPublisher { get; set; }
        string ImageFileName { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime CreationDate { get; set; }
        string UpdatedBy { get; set; }
        DateTime? LastUpdated { get; set; }
        int Id { get; set; }
        int ResourceCount { get; set; }
        int ChildrenResourceCount { get; set; }
        int ParentResourceCount { get; set; }
        string ProductDescription { get; set; }
        DateTime? NotSaleableDate { get; set; }
    }
}