#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class DiscountResourceDetail
    {
        public DiscountResourceDetail(DiscountResource discountResource)
        {
            DiscountPercentage = discountResource.DiscountPercentage;
            AccountNumber = discountResource.AccountNumber;
            InstitutionId = discountResource.InstitutionId;
            Isbn10 = discountResource.Isbn10;
            Isbn13 = discountResource.Isbn13;
            Title = discountResource.Title;
            Publisher = discountResource.Publisher;
            DiscountPrice = discountResource.DiscountPrice;
            Licenses = discountResource.Licenses;
            Total = discountResource.Total;
            ListPrice = discountResource.ListPrice;
            CartId = discountResource.CartId;
            ResourceId = discountResource.ResourceId;
            OrderNumber = discountResource.OrderNumber;
            InstitutionName = discountResource.InstitutionName;
            PurchaseDate = discountResource.PurchaseDate;
            IsFreeResource = discountResource.IsFreeResource;
        }

        [Display(Name = "Isbn10:")] public string Isbn10 { get; set; }

        [Display(Name = "Isbn13:")] public string Isbn13 { get; set; }

        [Display(Name = "Publisher:")] public string Publisher { get; set; }

        public int DiscountPercentage { get; set; }

        public decimal DiscountPrice { get; set; }

        public decimal ListPrice { get; set; }

        public string AccountNumber { get; set; }
        public int InstitutionId { get; set; }
        public string Title { get; set; }
        public int Licenses { get; set; }

        [DisplayFormat(DataFormatString = "${0:#,##0.00}")]
        public decimal Total { get; set; }


        public int CartId { get; set; }

        public int ResourceId { get; set; }

        [Display(Name = "Order #:")] public string OrderNumber { get; set; }

        [Display(Name = "Institution:")] public string InstitutionName { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime PurchaseDate { get; set; }

        public bool IsFreeResource { get; set; }
    }
}