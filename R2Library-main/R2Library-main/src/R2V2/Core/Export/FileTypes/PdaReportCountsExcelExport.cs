#region

using System.Collections.Generic;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class PdaReportCountsExcelExport : ExcelBase
    {
        public PdaReportCountsExcelExport(IEnumerable<PdaCountsReportDataItem> pdaReportCountsDataItem)
        {
            SpecifyColumn("Institution ID", "String");
            SpecifyColumn("Account #", "String");
            SpecifyColumn("Account Name", "String");
            SpecifyColumn("Territory", "String");

            SpecifyColumn("Firm Purchased Titles", "Int32");
            SpecifyColumn("Firm Purchased Licenses", "Int32");

            SpecifyColumn("PDA Purchased Titles", "Int32");
            SpecifyColumn("PDA Purchased Licenses", "Int32");
            SpecifyColumn("PDA Purchasing Percent", "String");
            SpecifyColumn("PDA Views", "Int32");
            SpecifyColumn("PDA Titles Added", "Int32");
            SpecifyColumn("PDA Titles Deleted", "Int32");
            SpecifyColumn("PDA Titles Added To Cart", "Int32");

            SpecifyColumn("PDA Wizard Rules Defined", "String");
            SpecifyColumn("PDA Wizard Purchased Titles", "Int32");
            SpecifyColumn("PDA Wizard Purchased Licenses", "Int32");
            SpecifyColumn("PDA Wizard Purchasing Percent", "String");
            SpecifyColumn("PDA Wizard Titles Added", "Int32");
            SpecifyColumn("PDA Wizard Titles Deleted", "Int32");
            SpecifyColumn("PDA Wizard Views", "Int32");
            SpecifyColumn("PDA Wizard Titles Added To Cart", "Int32");

            foreach (var pdaCountsReportDataItem in pdaReportCountsDataItem)
            {
                PopulateFirstColumn(pdaCountsReportDataItem.InstitutionId);
                PopulateNextColumn(pdaCountsReportDataItem.AccountNumber);
                PopulateNextColumn(pdaCountsReportDataItem.InstitutionName);
                PopulateNextColumn(pdaCountsReportDataItem.TerritoryCode);

                PopulateNextColumn(pdaCountsReportDataItem.FirmPurchasedResources);
                PopulateNextColumn(pdaCountsReportDataItem.FirmPurchasedLicenses);

                PopulateNextColumn(pdaCountsReportDataItem.PdaPurchasedResources);
                PopulateNextColumn(pdaCountsReportDataItem.PdaPurchasedLicenses);
                PopulateNextColumn(pdaCountsReportDataItem.PdaPurchasingPercent);
                PopulateNextColumn(pdaCountsReportDataItem.PdaTitleViews);
                PopulateNextColumn(pdaCountsReportDataItem.PdaTitlesAdded);
                PopulateNextColumn(pdaCountsReportDataItem.PdaTitlesDeleted);
                PopulateNextColumn(pdaCountsReportDataItem.PdaTitlesAddedToCart);

                PopulateNextColumn(pdaCountsReportDataItem.ArePdaWizardRulesDefinedForFutureTitles ? "Yes" : "No");
                PopulateNextColumn(pdaCountsReportDataItem.PdaWizardPurchasedResources);
                PopulateNextColumn(pdaCountsReportDataItem.PdaWizardPurchasedLicenses);
                PopulateNextColumn(pdaCountsReportDataItem.PdaWizardPurchasingPercent);
                PopulateNextColumn(pdaCountsReportDataItem.PdaWizardTitleViews);
                PopulateNextColumn(pdaCountsReportDataItem.PdaTitlesAddedByRule);
                PopulateNextColumn(pdaCountsReportDataItem.PdaWizardTitlesDeleted);
                PopulateLastColumn(pdaCountsReportDataItem.PdaWizardTitlesAddedToCart);
            }
        }
    }
}