#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    [Serializable]
    public class PublisherUsageItem
    {
        public int ResourceId { get; set; }
        public string ResourceTitle { get; set; }

        public string ResourceSortTitle { get; set; }

        public string ResourceIsbn { get; set; }

        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string EIsbn { get; set; }
        public string Publisher { get; set; }
        public string Authors { get; set; }
        public string Affiliation { get; set; }

        public string ResourceImageName { get; set; }
        public int ResourceStatusId { get; set; }

        public int ContentRetrievalCount { get; set; }
        public int TocRetrievalCount { get; set; }
        public int ConcurrencyTurnawayCount { get; set; }
        public int AccessTurnawayCount { get; set; }

        public int TotalLicenseCount { get; set; }

        public decimal ResourceListPrice { get; set; }

        public int TotalPdaAccess { get; set; }

        public int SessionCount { get; set; }

        public string PracticeAreaString { get; set; }
        public string SpecialtyString { get; set; }

        public DateTime? ReleaseDate { get; set; }
        public int? CopyRightYear { get; set; }
        public decimal ResourceTotalPrice => TotalLicenseCount == 0 ? 0.00m : TotalLicenseCount * ResourceListPrice;
    }
}