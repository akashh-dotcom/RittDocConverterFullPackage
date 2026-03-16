#region

using System;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.Reports
{
    public class ResourceReportDataItem
    {
        public virtual int ResourceId { get; set; }
        public virtual int ContentRetrievalCount { get; set; }
        public virtual int TocRetrievalCount { get; set; }
        public virtual int SessionCount { get; set; }
        public virtual int PrintCount { get; set; }
        public virtual int EmailCount { get; set; }
        public virtual int AccessTurnawayCount { get; set; }
        public virtual int ConcurrentTurnawayCount { get; set; }
        public virtual int PdaViews { get; set; }
        public virtual DateTime FirstPurchaseDate { get; set; }
        public virtual DateTime? PdaAddedToCartDate { get; set; }
        public virtual DateTime? PdaCreatedDate { get; set; }
        public virtual LicenseOriginalSource OriginalSource { get; set; }
        public virtual int LicenseCount { get; set; }
        public virtual string ResourceTitle { get; set; }
        public virtual string ResourceSortTitle { get; set; }
        public virtual decimal ResourceListPrice { get; set; }
        public virtual decimal ResourceAveragePrice { get; set; }
        public string ResourceIsbn { get; set; }
        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string EIsbn { get; set; }
        public string ResourceImageName { get; set; }
        public virtual int ResourceStatusId { get; set; }
        public virtual string Authors { get; set; }
        public virtual string NewEditionResourceIsbn { get; set; }
    }
}