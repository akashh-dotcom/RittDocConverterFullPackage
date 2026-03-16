#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Resource.Author;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Dashboard
{
    public class DashboardResource
    {
        public DashboardResource(int count, InstitutionResource institutionResource, int institutionId,
            DateTime dateRangeStart, DateTime dateRangeEnd)
        {
            if (institutionResource == null)
            {
                return;
            }

            ResourceCount = count;
            InstitutionResource = institutionResource;
            InstitutionId = institutionId;
            DateRangeStart = dateRangeStart;
            DateRangeEnd = dateRangeEnd;
            Type = "test";

            ListPrice = institutionResource.ListPrice;
            DiscountPrice = institutionResource.DiscountPrice;
            FirstAuthor = GetAuthorDisplay(institutionResource.FirstAuthor);
        }

        public DashboardResource(InstitutionResource institutionResource, string type, int institutionId,
            DateTime dateRangeStart, DateTime dateRangeEnd)
        {
            InstitutionResource = institutionResource;
            Type = type;

            InstitutionId = institutionId;
            DateRangeStart = dateRangeStart;
            DateRangeEnd = dateRangeEnd;
            ListPrice = institutionResource.ListPrice;
            DiscountPrice = institutionResource.DiscountPrice;
            FirstAuthor = GetAuthorDisplay(institutionResource.FirstAuthor);
        }

        public InstitutionResource InstitutionResource { get; set; }

        [Display(Name = @"Count: ")] public int ResourceCount { get; set; }

        public string Type { get; set; }

        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }

        public int InstitutionId { get; set; }

        [Display(Name = @"List Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ListPrice { get; set; }

        [Display(Name = @"Discount Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountPrice { get; set; }

        public string FirstAuthor { get; set; }

        private string GetAuthorDisplay(IAuthor author)
        {
            if (author == null)
            {
                return InstitutionResource.Authors != null
                    ? $"{(InstitutionResource.Authors.Length > 50 ? $"{InstitutionResource.Authors.Substring(0, 50)} et al." : InstitutionResource.Authors)}"
                    : null;
            }

            return string.Format("{0}{1}{2}", author.LastName,
                string.IsNullOrWhiteSpace(author.FirstName)
                    ? ""
                    : $", {author.FirstName}",
                string.IsNullOrWhiteSpace(author.Degrees)
                    ? ""
                    : $" {(author.Degrees.Substring(0, 2).Contains(",") ? author.Degrees : $", {author.Degrees}")}"
            );
        }
    }
}