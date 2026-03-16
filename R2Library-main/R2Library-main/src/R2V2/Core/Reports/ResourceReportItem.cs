#region

using System;
using R2V2.Core.Institution;

#endregion

namespace R2V2.Core.Reports
{
    [Serializable]
    public class ResourceReportItem
    {
        public int ResourceId { get; set; }
        public string ResourceTitle { get; set; }

        public string ResourceSortTitle { get; set; }

        public string ResourceIsbn { get; set; }

        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string EIsbn { get; set; }
        public string Publisher { get; set; }
        public string VendorNumber { get; set; }
        public string Authors { get; set; }
        public string Affiliation { get; set; }

        public string ResourceImageName { get; set; }
        public int ResourceStatusId { get; set; }

        public int ContentRetrievalCount { get; set; }
        public int TocRetrievalCount { get; set; }
        public int ConcurrencyTurnawayCount { get; set; }
        public int AccessTurnawayCount { get; set; }

        public int ContentPrintCount { get; set; } // 16
        public int ContentEmailCount { get; set; } // 17

        public int TotalLicenseCount { get; set; }
        public DateTime FirstPurchasedDate { get; set; }

        public decimal ResourceListPrice { get; set; }

        public decimal ResourceAveragePrice { get; set; }

        public DateTime? PdaCreatedDate { get; set; }
        public DateTime? PdaAddedToCartDate { get; set; }
        public LicenseOriginalSource OriginalSource { get; set; }

        public int TotalPdaAccess { get; set; }

        public string NewEditionResourceIsbn { get; set; }

        public int SessionCount { get; set; }

        public DateTime? ReleaseDate { get; set; }
        public int? CopyRightYear { get; set; }
        public string DctStatus { get; set; }

        public string PracticeAreaString { get; set; }
        public string SpecialtyString { get; set; }

        public string OriginalSourceString
        {
            get
            {
                switch (OriginalSource)
                {
                    case LicenseOriginalSource.FirmOrder:
                        return "Firm Order";
                    case LicenseOriginalSource.Pda:
                        return "PDA title selection";
                }

                return null;
            }
        }

        public decimal ResourceTotalPrice => TotalLicenseCount == 0 ? 0.00m : TotalLicenseCount * GetPurchasePrice();


        public decimal AverageAccessCost =>
            ContentRetrievalCount > 1 ? ResourceTotalPrice / ContentRetrievalCount : ResourceTotalPrice;

        public bool IsFreeResource { get; set; }

        public decimal GetPurchasePrice()
        {
            if (ResourceAveragePrice > 0)
            {
                return ResourceAveragePrice;
            }

            return ResourceListPrice;
        }
    }
}