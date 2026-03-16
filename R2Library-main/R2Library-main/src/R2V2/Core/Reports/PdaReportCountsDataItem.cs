namespace R2V2.Core.Reports
{
    public class PdaCountsReportDataItem
    {
        public int InstitutionId { get; set; }
        public string AccountNumber { get; set; }
        public string InstitutionName { get; set; }
        public string TerritoryCode { get; set; }
        public int PdaPurchasedLicenses { get; set; }
        public int PdaPurchasedResources { get; set; }
        public int FirmPurchasedLicenses { get; set; }
        public int FirmPurchasedResources { get; set; }
        public int PdaTitlesAdded { get; set; }
        public int PdaTitleViews { get; set; }
        public int PdaTitlesDeleted { get; set; }


        public int PdaTitlesAddedToCart { get; set; }
        public int PdaTitlesAddedByRule { get; set; }
        public int PdaWizardTitlesAddedToCart { get; set; }
        public int PdaWizardTitlesDeleted { get; set; }
        public int PdaWizardPurchasedResources { get; set; }
        public int PdaWizardPurchasedLicenses { get; set; }
        public int PdaWizardTitleViews { get; set; }
        public int PdaRuleCount { get; set; }
        public int PdaRulesFuture { get; set; }
        public int PdaRulesNewEditionFirm { get; set; }
        public int PdaRulesNewEditionPda { get; set; }

        public string PdaPurchasingPercent
        {
            get
            {
                var totalPurchased = PdaPurchasedLicenses + FirmPurchasedLicenses;
                return totalPurchased > 0 ? ((double)PdaPurchasedLicenses / totalPurchased).ToString("0.0%") : "0.0%";
            }
        }

        public string PdaWizardPurchasingPercent
        {
            get
            {
                var totalPurchased = PdaPurchasedLicenses + FirmPurchasedLicenses;
                return totalPurchased > 0
                    ? ((double)PdaWizardPurchasedLicenses / totalPurchased).ToString("0.0%")
                    : "0.0%";
            }
        }

        public bool ArePdaWizardRulesDefined => PdaRuleCount > 0;

        public bool ArePdaWizardRulesDefinedForFutureTitles => PdaRulesFuture > 0;
    }
}