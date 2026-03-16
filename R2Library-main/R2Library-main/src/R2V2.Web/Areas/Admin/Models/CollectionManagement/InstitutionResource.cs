#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Author;
using R2V2.Web.Areas.Admin.Models.Recommendations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.CollectionManagement
{
    public class InstitutionResource : Resource.Resource, IInstitutionResource
    {
        public InstitutionResource()
        {
        }

        public InstitutionResource(CollectionManagementResource collectionManagementResource,
            IAdminInstitution institution, IEnumerable<Recommendation> recommendations)
            : base(collectionManagementResource.Resource, null, collectionManagementResource.SpecialText,
                collectionManagementResource.SpecialIconName)
        {
            InstitutionId = institution.Id;
            Discount = collectionManagementResource.Discount;
            DiscountPrice = collectionManagementResource.DiscountPrice;
            LicenseCount = collectionManagementResource.LicenseCount;
            CartLicenseCount = collectionManagementResource.CartLicenseCount;
            FreeLicenseInCart = collectionManagementResource.FreeLicenseInCart;

            if (collectionManagementResource.LicenseType == LicenseType.Purchased &&
                collectionManagementResource.LicenseCount == 0)
            {
                LicenseType = LicenseType.None;
            }
            else
            {
                FirstPurchaseDate = collectionManagementResource.FirstPurchaseDate;
                LicenseType = collectionManagementResource.LicenseType;
            }

            IsPdaEligible = CartLicenseCount == 0 && !IsFreeResource && IsForSale;

            OriginalSource = LicenseOriginalSource.FirmOrder;


            ShowLicenseCount = institution.AccountStatus == InstitutionAccountStatus.Active;

            var resourceLicense = institution.Licenses.FirstOrDefault(x =>
                x.ResourceId == collectionManagementResource.Resource.Id && x.LicenseType != LicenseType.Trial);
            if (resourceLicense != null)
            {
                OriginalSource = resourceLicense.OriginalSource;
                PdaAccessCount = resourceLicense.PdaViewCount;
                PdaCreatedDate = resourceLicense.PdaAddedDate;
                PdaRuleAddedDate = resourceLicense.PdaRuleAddedDate;
                PdaAddedToCartDate = resourceLicense.PdaAddedToCartDate;
                PdaDeletedDate = resourceLicense.PdaDeletedDate;

                PdaCartDeletedDate = resourceLicense.PdaCartDeletedDate;
                PdaCartDeletedName = resourceLicense.PdaCartDeletedByName;

                IsPdaEligible = IsPdaEligible && resourceLicense.FirstPurchaseDate == null;
                if (OriginalSource == LicenseOriginalSource.Pda)
                {
                    if (LicenseType == LicenseType.Pda)
                    {
                        PdaResourceArchived = collectionManagementResource.Resource.StatusId ==
                                              (int)ResourceStatus.Archived;
                        PdaCartItemExpired = PdaAddedToCartDate < DateTime.Now.AddDays(-30);

                        IsActivePdaResource = PdaAddedToCartDate == null &&
                                              PdaDeletedDate == null &&
                                              PdaAccessCount < resourceLicense.PdaMaxViews &&
                                              IsForSale;
                    }

                    ResourceNotSaleableDate = collectionManagementResource.Resource.NotSaleableDate;

                    if ((IsPdaEligible && PdaAccessCount == resourceLicense.PdaMaxViews) ||
                        (IsPdaEligible && PdaAddedToCartDate == null && PdaDeletedDate == null &&
                         PdaAccessCount < resourceLicense.PdaMaxViews))
                    {
                        IsPdaEligible = false;
                    }
                }
            }

            Recommendeds = new List<Recommended>();
            var recommendationList = recommendations?.ToList();
            if (recommendationList != null && recommendationList.Any())
            {
                foreach (var recommendation in recommendationList.Where(recommendation =>
                             recommendation.ResourceId == Id))
                {
                    Recommendeds.Add(new Recommended(recommendation));
                }
            }

            FirstAuthor = collectionManagementResource.Resource.AuthorList.FirstOrDefault(x => x.Order == 1);

            ConcurrentTurnawayCount = collectionManagementResource.ConcurrentTurnawayCount;

            if (IsForSale)
            {
                if (IsFreeResource && collectionManagementResource.LicenseType == LicenseType.Purchased &&
                    LicenseCount != 0)
                {
                    IsForSale = false;
                }

                if (CartLicenseCount > 0)
                {
                    IsForSale = false;
                }
            }

            if (IsForSale && IsFreeResource &&
                ((collectionManagementResource.LicenseType == LicenseType.Purchased && LicenseCount != 0) ||
                 CartLicenseCount != 0))
            {
                IsForSale = false;
            }
        }

        public bool IsActivePdaResource { get; }

        [Display(Name = @"PDA Activation Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy} via PDA Wizard")]
        public DateTime? PdaRuleAddedDate { get; }

        [Display(Name = @"PDA Deleted from cart: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? PdaCartDeletedDate { get; }

        [Display(Name = @" By ")] public string PdaCartDeletedName { get; }

        [Display(Name = @"PDA Removed Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? PdaDeletedDate { get; set; }

        public int CurrentUserReviewId { get; set; }

        [Display(Name = @"Concurrent Turnaways: ")]
        public int ConcurrentTurnawayCount { get; }

        public bool FreeLicenseInCart { get; set; }

        public int ListIndex { get; set; }

        public int InstitutionId { get; set; }

        public decimal Discount { get; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountPrice { get; }

        public bool ShowLicenseCount { get; }

        [Display(Name = @"Total Licenses Purchased: ")]
        public int LicenseCount { get; }

        [Display(Name = @"License Count in Cart: ")]
        public int CartLicenseCount { get; }

        public bool IsPdaEligible { get; }

        [Display(Name = @"PDA Activation Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? PdaCreatedDate { get; }

        [Display(Name = @"PDA Added to Cart Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? PdaAddedToCartDate { get; }

        [Display(Name = @"PDA Access Count: ")]
        public int PdaAccessCount { get; }

        [Display(Name = @"Order Source: ")] public LicenseOriginalSource OriginalSource { get; }

        [Display(Name = @"First Purchase Date: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? FirstPurchaseDate { get; }

        [Display(Name = @"PDA Resource Not Saleable: ")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ResourceNotSaleableDate { get; }

        public LicenseType LicenseType { get; }
        public bool PdaCartItemExpired { get; }
        public bool PdaResourceArchived { get; }

        public IList<Recommended> Recommendeds { get; }

        public IAuthor FirstAuthor { get; set; }

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

        public new string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResource = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", LicenseCount: {0}", LicenseCount);
            sb.AppendFormat(", CartLicenseCount: {0}", CartLicenseCount);
            sb.AppendFormat(", PdaAccessCount: {0}", PdaAccessCount);
            sb.AppendFormat(", ShowLicenseCount: {0}", ShowLicenseCount);
            sb.AppendFormat(", IsPdaEligible: {0}", IsPdaEligible);
            sb.AppendFormat(", PdaCreatedDate: {0}", PdaCreatedDate);
            sb.AppendFormat(", PdaAddedToCartDate: {0}", PdaAddedToCartDate);
            sb.Append("]");
            return sb.ToString();
        }

        public void SetRecommendationId(IUser user)
        {
            CurrentUserReviewId = Recommendeds.FirstOrDefault(x => x.ExpertReviewerUser.Id == user.Id)?.Id ?? 0;
        }

        public bool CanRecommendBeAltered(bool isExpertReviewer)
        {
            if (Recommendeds != null && Recommendeds.Any())
            {
                var hasRecommends = Recommendeds.Any(x => x.DeletedDate == null);
                if (!isExpertReviewer)
                {
                    return hasRecommends;
                }
                else
                {
                    if (FirstPurchaseDate.HasValue || !IsPdaEligible || ArchiveDate.HasValue)
                    {
                        return true;
                    }
                }

                //return hasRecommends
            }

            return false;
        }
    }
}