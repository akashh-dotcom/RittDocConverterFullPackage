#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Institution;
using R2V2.Core.Resource.Author;
using R2V2.Web.Areas.Admin.Models.Recommendations;
using R2V2.Web.Areas.Admin.Models.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public interface IInstitutionResource : IAdminResource
    {
        int InstitutionId { get; set; }
        decimal Discount { get; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        decimal DiscountPrice { get; }

        bool ShowLicenseCount { get; }

        [Display(Name = @"Total Licenses Purchased: ")]
        int LicenseCount { get; }

        [Display(Name = @"License Count in Cart: ")]
        int CartLicenseCount { get; }

        bool IsPdaEligible { get; }

        [Display(Name = @"PDA Activation Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        DateTime? PdaCreatedDate { get; }

        [Display(Name = @"PDA Added to Cart Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        DateTime? PdaAddedToCartDate { get; }

        [Display(Name = @"PDA Access Count: ")]
        int PdaAccessCount { get; }

        [Display(Name = @"Order Source: ")] LicenseOriginalSource OriginalSource { get; }

        [Display(Name = @"First Purchase Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        DateTime? FirstPurchaseDate { get; }

        [Display(Name = @"PDA Resource Not Saleable: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        DateTime? ResourceNotSaleableDate { get; }

        LicenseType LicenseType { get; }
        bool PdaCartItemExpired { get; }
        bool PdaResourceArchived { get; }
        IList<Recommended> Recommendeds { get; }
        string OriginalSourceString { get; }

        IAuthor FirstAuthor { get; set; }
    }
}