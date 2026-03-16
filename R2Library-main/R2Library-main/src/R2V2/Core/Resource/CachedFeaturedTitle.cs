#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Resource
{
    [Serializable]
    public class CachedFeaturedTitle : IFeaturedTitle
    {
        public CachedFeaturedTitle(IFeaturedTitle featuredTitle, IResource resource)
        {
            Id = featuredTitle.Id;
            StartDate = featuredTitle.StartDate;
            EndDate = featuredTitle.EndDate;
            RecordStatus = featuredTitle.RecordStatus;
            ResourceId = featuredTitle.ResourceId;

            ResourceTitle = resource.Title;
            ResourceIsbn = resource.Isbn;
            ResourcePublisherName = resource.Publisher.Name;
            ResourceListPrice = resource.ListPrice;
            ResourceImageFileName = resource.ImageFileName;

            CreatedBy = featuredTitle.CreatedBy;
            CreationDate = featuredTitle.CreationDate;
            UpdatedBy = featuredTitle.UpdatedBy;
            LastUpdated = featuredTitle.LastUpdated;
        }

        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool RecordStatus { get; set; }
        public int ResourceId { get; set; }
        public string ResourceIsbn { get; set; }
        public string ResourceTitle { get; set; }
        public decimal ResourceListPrice { get; set; }
        public string ResourcePublisherName { get; set; }
        public string ResourceImageFileName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("FeaturedTitle = [");

            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", StartDate: {0}", StartDate);
            sb.AppendFormat(", EndDate: {0}", EndDate);
            sb.AppendFormat(", ResourceIsbn: {0}", ResourceIsbn);
            sb.AppendFormat(", ResourceTitle: {0}", ResourceTitle);
            sb.AppendFormat(", ResourceListPrice: {0}", ResourceListPrice);
            sb.AppendFormat(", ResourcePublisherName: {0}", ResourcePublisherName);
            sb.AppendFormat(", ResourceImageFileName: {0}", ResourceImageFileName);

            sb.AppendFormat(", CreatedBy: {0}", CreatedBy);
            sb.AppendFormat(", CreationDate: {0}", CreationDate);
            sb.AppendFormat(", UpdatedBy: {0}", UpdatedBy);
            sb.AppendFormat(", LastUpdated: {0}", LastUpdated);
            sb.Append("]");

            return sb.ToString();
        }
    }
}