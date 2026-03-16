#region

using System;

#endregion

namespace R2V2.Core.Resource
{
    public interface IFeaturedTitle
    {
        int Id { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        bool RecordStatus { get; set; }
        int ResourceId { get; set; }

        string ResourceIsbn { get; set; }
        string ResourceTitle { get; set; }
        decimal ResourceListPrice { get; set; }
        string ResourcePublisherName { get; set; }
        string ResourceImageFileName { get; set; }

        string CreatedBy { get; set; }
        DateTime CreationDate { get; set; }
        string UpdatedBy { get; set; }
        DateTime? LastUpdated { get; set; }

        string ToDebugString();
    }
}