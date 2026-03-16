#region

using System.Collections.Generic;
using R2V2.Core.AutomatedCart;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class AutomatedCartExcelExport : ExcelBase
    {
        public AutomatedCartExcelExport(IEnumerable<AutomatedCartReport> automatedCartReports)
        {
            SetBase();
            SpecifyColumn("New Edition", "Boolean");
            SpecifyColumn("Triggered PDA", "Boolean");
            SpecifyColumn("Reviewed", "Boolean");
            SpecifyColumn("Turnaway", "Boolean");
            foreach (var automatedCartReport in automatedCartReports)
            {
                PopulateFirstColumn(automatedCartReport.Institution.AccountNumber);
                PopulateNextColumn(automatedCartReport.Institution.Name);
                PopulateNextColumn(automatedCartReport.Institution.Address?.Address1);
                PopulateNextColumn(automatedCartReport.Institution.Address?.Address2);
                PopulateNextColumn(automatedCartReport.Institution.Address?.City);
                PopulateNextColumn(automatedCartReport.Institution.Address?.State);
                PopulateNextColumn(automatedCartReport.Institution.Address?.Zip);
                PopulateNextColumn(automatedCartReport.Institution.Territory?.Code);
                PopulateNextColumn(automatedCartReport.Institution.Type?.Name);
                PopulateNextColumn(automatedCartReport.NewEdition);
                PopulateNextColumn(automatedCartReport.TriggeredPda);
                PopulateNextColumn(automatedCartReport.Reviewed);
                PopulateLastColumn(automatedCartReport.Turnaway);
            }
        }

        public AutomatedCartExcelExport(IEnumerable<AutomatedCartPricedReport> automatedCartPricedReports)
        {
            SetBase();

            SpecifyColumn("New Edition", "Int32");
            SpecifyColumn("Triggered PDA", "Int32");
            SpecifyColumn("Reviewed", "Int32");
            SpecifyColumn("Turnaway", "Int32");
            SpecifyColumn("Titles", "Int32");
            SpecifyColumn("List Price", "Decimal");
            SpecifyColumn("Discount Price", "Decimal");

            foreach (var automatedCartPricedReport in automatedCartPricedReports)
            {
                PopulateFirstColumn(automatedCartPricedReport.Institution.AccountNumber);
                PopulateNextColumn(automatedCartPricedReport.Institution.Name);
                PopulateNextColumn(automatedCartPricedReport.Institution.Address?.Address1);
                PopulateNextColumn(automatedCartPricedReport.Institution.Address?.Address2);
                PopulateNextColumn(automatedCartPricedReport.Institution.Address?.City);
                PopulateNextColumn(automatedCartPricedReport.Institution.Address?.State);
                PopulateNextColumn(automatedCartPricedReport.Institution.Address?.Zip);
                PopulateNextColumn(automatedCartPricedReport.Institution.Territory?.Code);
                PopulateNextColumn(automatedCartPricedReport.Institution.Type?.Name);
                PopulateNextColumn(automatedCartPricedReport.NewEditionCount);
                PopulateNextColumn(automatedCartPricedReport.TriggeredPdaCount);
                PopulateNextColumn(automatedCartPricedReport.ReviewedCount);
                PopulateNextColumn(automatedCartPricedReport.TurnawayCount);
                PopulateNextColumn(automatedCartPricedReport.ResourceCount);
                PopulateNextColumn(automatedCartPricedReport.ListPrice);
                PopulateLastColumn(automatedCartPricedReport.DiscountPrice);
            }
        }

        public AutomatedCartExcelExport(IEnumerable<AutomatedCartInstitutionSummary> summaries, bool displayEmailCounts)
        {
            SetBase();

            SpecifyColumn("New Edition", "Int32");
            SpecifyColumn("Triggered PDA", "Int32");
            SpecifyColumn("Reviewed", "Int32");
            SpecifyColumn("Turnaway", "Int32");
            SpecifyColumn("Titles", "Int32");
            SpecifyColumn("List Price", "Decimal");
            SpecifyColumn("Discount Price", "Decimal");
            if (displayEmailCounts)
            {
                SpecifyColumn("Emails Sent", "Int32");
            }

            foreach (var summary in summaries)
            {
                PopulateFirstColumn(summary.AccountNumber);
                PopulateNextColumn(summary.InstitutionName);
                PopulateNextColumn(summary.Address?.Address1);
                PopulateNextColumn(summary.Address?.Address2);
                PopulateNextColumn(summary.Address?.City);
                PopulateNextColumn(summary.Address?.State);
                PopulateNextColumn(summary.Address?.Zip);
                PopulateNextColumn(summary.Territory);
                PopulateNextColumn(summary.InstitutionType);
                PopulateNextColumn(summary.NewEditionCount);
                PopulateNextColumn(summary.PdaCount);
                PopulateNextColumn(summary.ReviewedCount);
                PopulateNextColumn(summary.TurnawayCount);
                PopulateNextColumn(summary.TitleCount);
                PopulateNextColumn(summary.ListPrice);

                if (displayEmailCounts)
                {
                    PopulateNextColumn(summary.DiscountPrice);
                    PopulateLastColumn(summary.EmailCount);
                }
                else
                {
                    PopulateLastColumn(summary.DiscountPrice);
                }
            }
        }


        private void SetBase()
        {
            SpecifyColumn("Account #", "String");
            SpecifyColumn("Institution Name", "String");
            SpecifyColumn("Address 1", "String");
            SpecifyColumn("Address 2", "String");
            SpecifyColumn("City", "String");
            SpecifyColumn("State", "String");
            SpecifyColumn("Zip", "String");
            SpecifyColumn("Territory", "String");
            SpecifyColumn("Library Type", "String");
        }
    }
}