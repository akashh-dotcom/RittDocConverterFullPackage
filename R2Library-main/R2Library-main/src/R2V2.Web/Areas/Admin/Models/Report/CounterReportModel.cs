#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class CounterReportModel : ReportModel
    {
        public CounterReportModel(IAdminInstitution institution, ReportQuery reportQuery) : base(institution,
            reportQuery)
        {
            DisableTocLicenseType = true;
            IsLicenseTypeEnabled = true;

            //This erroneously changes the report query after it has been submitted by the user.    -DRJ
            //SetReportQueryDefaults();
        }

        public CounterReportModel(IAdminInstitution institution) : base(institution)
        {
            DisableTocLicenseType = true;
            IsLicenseTypeEnabled = true;

            //This erroneously changes the report query after it has been submitted by the user.    -DRJ
            //SetReportQueryDefaults();
        }

        private void SetReportQueryDefaults()
        {
            ReportQuery.IncludePurchasedTitles = true;

            if (!IsPublisherUser)
            {
                ReportQuery.IncludePdaTitles = true;
            }
        }
    }
}