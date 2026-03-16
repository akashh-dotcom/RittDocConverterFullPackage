#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Publisher
{
    [Serializable]
    public class
        CachedPublisher : IPublisher,
        IDebugInfo // , ISoftDeletable  // certain inactive publishers still have active resources, which ends up breaking functionality
    {
        public CachedPublisher(IPublisher publisher)
        {
            if (publisher == null)
            {
                return;
            }

            Id = publisher.Id;
            Name = publisher.Name;
            Address = publisher.Address;
            Address2 = publisher.Address2;
            City = publisher.City;
            State = publisher.State;
            Zip = publisher.Zip;
            ResourceCount = publisher.ResourceCount;
            RecordStatus = publisher.RecordStatus;

            ConsolidatedPublisher = publisher.ConsolidatedPublisher == null
                ? null
                : publisher.Id == publisher.ConsolidatedPublisher.Id
                    ? null
                    : new CachedPublisher(publisher.ConsolidatedPublisher);

            IsFeaturedPublisher = publisher.IsFeaturedPublisher;
            ImageFileName = publisher.ImageFileName;
            DisplayName = publisher.DisplayName;
            Description = publisher.Description;
            CreatedBy = publisher.CreatedBy;
            CreationDate = publisher.CreationDate;
            UpdatedBy = publisher.UpdatedBy;
            LastUpdated = publisher.LastUpdated;
            ProductDescription = publisher.ProductDescription;
            NotSaleableDate = publisher.NotSaleableDate;
            VendorNumber = publisher.VendorNumber;
        }

        public string ToDebugString()
        {
            return new StringBuilder("CachedPublisher = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", Name: {0}", Name)
                .AppendFormat(", VendorNumber: {0}", VendorNumber)
                .AppendFormat(", Address: {0}", Address)
                .AppendFormat(", Address2: {0}", Address2)
                .AppendFormat(", City: {0}", City)
                .AppendFormat(", State: {0}", State)
                .AppendFormat(", Zip: {0}", Zip)
                .AppendFormat(", ResourceCount: {0}", ResourceCount)
                .AppendFormat(", ChildrenResourceCount: {0}", ChildrenResourceCount)
                .AppendFormat(", RecordStatus: {0}", RecordStatus)
                .AppendFormat(", IsFeaturedPublisher: {0}", IsFeaturedPublisher)
                .AppendFormat(", ImageFileName: {0}", ImageFileName)
                .AppendFormat(", DisplayName: {0}", DisplayName)
                .AppendFormat(", NotSaleableDate: {0}", NotSaleableDate)
                .AppendFormat(", Description: {0}", Description)
                .AppendFormat("\t, ConsolidatedPublisher: {0}",
                    ConsolidatedPublisher == null ? "null" : ((CachedPublisher)ConsolidatedPublisher).ToDebugString())
                .Append("]")
                .ToString();
        }

        public string Name { get; set; }
        public string VendorNumber { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int ResourceCount { get; set; }
        public int ChildrenResourceCount { get; set; }
        public int ParentResourceCount { get; set; }
        public string ProductDescription { get; set; }
        public DateTime? NotSaleableDate { get; set; }
        public bool RecordStatus { get; set; }
        public IPublisher ConsolidatedPublisher { get; set; }
        public bool IsFeaturedPublisher { get; set; }
        public string ImageFileName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int Id { get; set; }
    }
}