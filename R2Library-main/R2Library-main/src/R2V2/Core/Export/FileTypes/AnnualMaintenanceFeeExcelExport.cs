#region

using System.Collections.Generic;
using R2V2.Core.Reports;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class AnnualMaintenanceFeeExcelExport : ExcelBase
    {
        public AnnualMaintenanceFeeExcelExport(IEnumerable<AnnualFeeReportDataItem> annualFeeReportDataItems)
        {
            SpecifyColumn("Account #", "String");
            SpecifyColumn("Account Name", "String");
            SpecifyColumn("Consortia", "String");
            SpecifyColumn("Contact Name", "String");
            SpecifyColumn("Contact Email", "String");
            SpecifyColumn("Active Date", "Datetime");
            SpecifyColumn("Renewal Date", "Datetime");

            foreach (var annualFeeReportDataItem in annualFeeReportDataItems)
            {
                PopulateFirstColumn(annualFeeReportDataItem.AccountNumber);
                PopulateNextColumn(annualFeeReportDataItem.InstitutionName);
                PopulateNextColumn(annualFeeReportDataItem.Consortia);
                PopulateNextColumn(annualFeeReportDataItem.ContactName);
                PopulateNextColumn(annualFeeReportDataItem.ContactEmail);
                PopulateNextColumn(annualFeeReportDataItem.ActiveDate.Date);
                PopulateLastColumn(annualFeeReportDataItem.RenewalDate);
            }
        }
    }
}