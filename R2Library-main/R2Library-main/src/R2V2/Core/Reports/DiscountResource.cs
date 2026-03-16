#region

using System;

#endregion

namespace R2V2.Core.Reports
{
    public class DiscountResource
    {
        public int DiscountPercentage { get; set; }
        public string AccountNumber { get; set; }
        public int InstitutionId { get; set; }
        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public decimal DiscountPrice { get; set; }
        public int Licenses { get; set; }
        public decimal Total { get; set; }

        public decimal ListPrice { get; set; }

        public int CartId { get; set; }
        public int ResourceId { get; set; }

        public string OrderNumber { get; set; }

        public string InstitutionName { get; set; }
        public DateTime PurchaseDate { get; set; }

        public byte FreeResource { private get; set; }

        public bool IsFreeResource => FreeResource == 1;
    }
}