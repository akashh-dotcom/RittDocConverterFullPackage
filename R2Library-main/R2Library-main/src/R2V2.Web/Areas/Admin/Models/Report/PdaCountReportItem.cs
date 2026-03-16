#region

using System.ComponentModel.DataAnnotations;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class PdaCountsReportItem
    {
        public PdaCountsReportItem()
        {
        }

        public PdaCountsReportItem(PdaCountsReportDataItem dataItem)
        {
            InstitutionId = dataItem.InstitutionId;
            AccountNumber = dataItem.AccountNumber;
            InstitutionName = dataItem.InstitutionName;
            PdaPurchasedLicenses = dataItem.PdaPurchasedLicenses;
            PdaPurchasedResources = dataItem.PdaPurchasedResources;
            FirmPurchasedLicenses = dataItem.FirmPurchasedLicenses;
            FirmPurchasedResources = dataItem.FirmPurchasedResources;
            PdaTitlesAdded = dataItem.PdaTitlesAdded;
            PdaTitlesDeleted = dataItem.PdaTitlesDeleted;
            PdaTitleViews = dataItem.PdaTitleViews;
            PdaPurchasingPercent = dataItem.PdaPurchasingPercent;

            PdaTitlesAddedToCart = dataItem.PdaTitlesAddedToCart;
            PdaTitlesAddedByRule = dataItem.PdaTitlesAddedByRule;
            PdaWizardTitlesAddedToCart = dataItem.PdaWizardTitlesAddedToCart;
            PdaWizardTitlesDeleted = dataItem.PdaWizardTitlesDeleted;
            PdaWizardPurchasedResources = dataItem.PdaWizardPurchasedResources;
            PdaWizardPurchasedLicenses = dataItem.PdaWizardPurchasedLicenses;
            PdaWizardTitleViews = dataItem.PdaWizardTitleViews;
            PdaWizardPurchasingPercent = dataItem.PdaWizardPurchasingPercent;
            ArePdaWizardRulesDefined = dataItem.ArePdaWizardRulesDefinedForFutureTitles;
        }

        [Display(Name = "Institution Id: ")] public int InstitutionId { get; set; }

        [Display(Name = "Account Number: ")] public string AccountNumber { get; set; }

        [Display(Name = "Institution Name: ")] public string InstitutionName { get; set; }

        [Display(Name = "PDA Licenses Purchased: ")]
        public int PdaPurchasedLicenses { get; set; }

        [Display(Name = "PDA Titles Purchased: ")]
        public int PdaPurchasedResources { get; set; }

        [Display(Name = "Firm Licenses Purchased: ")]
        public int FirmPurchasedLicenses { get; set; }

        [Display(Name = "Firm Titles Purchased: ")]
        public int FirmPurchasedResources { get; set; }

        [Display(Name = "PDA Titles Added: ")] public int PdaTitlesAdded { get; set; }

        [Display(Name = "PDA Titles Deleted: ")]
        public int PdaTitlesDeleted { get; set; }

        [Display(Name = "PDA Title Views: ")] public int PdaTitleViews { get; set; }

        [Display(Name = "PDA Purchasing %: ")] public string PdaPurchasingPercent { get; set; }


        [Display(Name = "PDA Titles Added to Cart: ")]
        public int PdaTitlesAddedToCart { get; set; }

        [Display(Name = "PDA Wizard Titles Added: ")]
        public int PdaTitlesAddedByRule { get; set; }

        [Display(Name = "PDA Wizard Titles Added to Cart: ")]
        public int PdaWizardTitlesAddedToCart { get; set; }

        [Display(Name = "PDA Wizard Titles Deleted: ")]
        public int PdaWizardTitlesDeleted { get; set; }

        [Display(Name = "PDA Wizard Titles Purchased: ")]
        public int PdaWizardPurchasedResources { get; set; }

        [Display(Name = "PDA Wizard Licenses Purchased: ")]
        public int PdaWizardPurchasedLicenses { get; set; }

        [Display(Name = "PDA Wizard Title Views: ")]
        public int PdaWizardTitleViews { get; set; }

        [Display(Name = "PDA Wizard Purchasing %: ")]
        public string PdaWizardPurchasingPercent { get; set; }

        [Display(Name = "PDA Wizard Enabled: ")]
        public bool ArePdaWizardRulesDefined { get; set; }
    }
}