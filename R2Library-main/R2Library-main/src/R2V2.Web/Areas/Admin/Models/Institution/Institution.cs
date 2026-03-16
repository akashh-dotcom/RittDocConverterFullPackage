#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Institution
{
    public class Institution : AdminBaseModel
    {
        public Institution()
        {
        }

        public Institution(IInstitution institution)
            : base(new AdminInstitution(institution))
        {
            AccountStatus = institution.AccountStatus;
            AccountNumber = institution.AccountNumber;
            TrialEndDate = institution.Trial.EndDate;
            InstitutionName = institution.Name;
            NameKey = institution.NameKey;
            Address = new Address
            {
                Address1 = institution.Address.Address1,
                Address2 = institution.Address.Address2,
                City = institution.Address.City,
                State = institution.Address.State,
                Zip = institution.Address.Zip
            };
            DisplayAllProducts = institution.DisplayAllProducts;
            AccessType = institution.AccessType;
            Discount = institution.Discount;
            HouseAccount = institution.HouseAccount;
            //AthensOrgId = institution.AthensOrgId;
            AthensAffiliation = institution.AthensAffiliation;
            LogUrl = institution.LogUrl;
            HomePage = institution.HomePage;
            TrustedKey = institution.TrustedKey;

            EnableExpertReviewerUser = institution.ExpertReviewerUserEnabled;
            IncludeArchivedTitlesByDefault = institution.IncludeArchivedTitlesByDefault;

            InstitutionTerritory = new InstitutionTerritory(institution.Territory);
            ProxyPrefix = institution.ProxyPrefix;
            UrlSuffix = institution.UrlSuffix;

            AnnualFeeDate = institution.AnnualFee?.FeeDate;
            OclcSymbol = institution.OclcSymbol;
            Type = institution.Type;
            EnableIpPlus = institution.EnableIpPlus;
            EnableHomePageCollectionLink = institution.EnableHomePageCollectionLink;
        }

        public Institution(IInstitution institution, IUser adminUser)
            : this(institution)
        {
            if (adminUser == null)
            {
                return;
            }

            AdministratorUserId = adminUser.Id;
            AdministratorEmail = adminUser.Email;
            AdministratorName = string.IsNullOrWhiteSpace(adminUser.LastName)
                ? "Not Specified"
                : $"{adminUser.LastName}, {adminUser.FirstName}";
        }

        [Display(Name = "Institution Name:")]
        [Required]
        public string InstitutionName { get; set; }

        [Display(Name = "Account Number:")] public string AccountNumber { get; set; }

        public string NameKey { get; set; }

        [Display(Name = "Address:")] public Address Address { get; set; }

        [Display(Name = "Trial End Date:")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        [DateTenYears("TrialEndDate")]
        public DateTime? TrialEndDate { get; set; }

        [Display(Name = "Annual Fee Date:")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? AnnualFeeDate { get; set; }

        [Display(Name = "Display Non-Purchased Titles:")]
        public string DisplayAllProductsText => DisplayAllProducts ? "Yes" : "No";

        public bool DisplayAllProducts { get; set; }

        [Display(Name = "Include Archived Titles By Default:")]
        public string IncludeArchivedTitlesByDefaultText => IncludeArchivedTitlesByDefault ? "Yes" : "No";

        public bool IncludeArchivedTitlesByDefault { get; set; }

        [Display(Name = "Account Status:")] public IInstitutionAccountStatus AccountStatus { get; set; }

        [Display(Name = "Access Type:")] public IInstitutionAccessType AccessType { get; set; }

        [Display(Name = "House Account:")] public string HouseAccountText => HouseAccount ? "Yes" : "No";

        public bool HouseAccount { get; set; }

        //[Display(Name = "Athens Organization ID:")]
        //public string AthensOrgId { get; set; }

        [Display(Name = "Athens Scope:")] public string AthensAffiliation { get; set; }

        [Display(Name = "Logout Preferences:")]
        public string LogUrl { get; set; }

        [Display(Name = "Home Page Display:")] public IInstitutionHomePage HomePage { get; set; }

        [Display(Name = "Trusted Security Key:")]
        public string TrustedKey { get; set; }

        [Display(Name = "Institution Discount:")]
        [DisplayFormat(DataFormatString = "{0}%")]
        public decimal Discount { get; set; }

        [Display(Name = "Administrator Name:")]
        public string AdministratorName { get; set; }

        [Display(Name = "Administrator Email:")]
        public string AdministratorEmail { get; set; }

        public int AdministratorUserId { get; set; }

        public InstitutionTerritory InstitutionTerritory { get; set; }

        [Display(Name = "Enable Expert Reviewer User:")]
        public string EnableExpertReviewerUserText => EnableExpertReviewerUser ? "Yes" : "No";

        public bool EnableExpertReviewerUser { get; set; }

        [Display(Name = "Resource URL Prefix:")]
        public string ProxyPrefix { get; set; }

        public string UrlSuffix { get; set; }

        [Display(Name = "Institution Type:")] public InstitutionType Type { get; set; }

        [Display(Name = "OCLC Symbol:")] public string OclcSymbol { get; set; }

        [Display(Name = "Enable IP Plus:")] public bool EnableIpPlus { get; set; }

        [Display(Name = "Enable Browse GuideLinesToGo Link:")]
        public bool EnableHomePageCollectionLink { get; set; }

        public string EnableHomePageCollectionLinkText => EnableHomePageCollectionLink ? "Yes" : "No";

        public string EnableIpPlusText => EnableIpPlus ? "Yes" : "No";
    }
}