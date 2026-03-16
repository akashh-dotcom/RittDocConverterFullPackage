#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Services;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Report
{
    public class WebReportServiceBase : ReportServiceBase
    {
        protected const string ResourceUsageReportKey = "Report.Resource.User.Data";
        protected const int PageSize = 50;
        private readonly IAdminContext _adminContext;
        private readonly IpAddressRangeService _ipAddressRangeService;

        protected readonly IReportService ReportService;

        public WebReportServiceBase(ILog<ReportServiceBase> log2
            , IQueryable<SavedReport> savedReports
            , IReportService reportService
            , IAdminContext adminContext
            , IpAddressRangeService ipAddressRangeService
        ) : base(log2, savedReports)
        {
            ReportService = reportService;
            _adminContext = adminContext;
            _ipAddressRangeService = ipAddressRangeService;
        }

        public IAdminInstitution Institution { get; set; }
        public List<Core.Authentication.IpAddressRange> IpAddressRanges { get; set; }

        public void Init(ReportQuery reportQuery, bool updateReportQuery = true)
        {
            if (reportQuery.ReportId > 0)
            {
                InitBase(reportQuery.InstitutionId, reportQuery.ReportId);
                Institution = _adminContext.GetAdminInstitution(ReportRequest.InstitutionId);
                IpAddressRanges = GetInstitutionsIpAddressRanges(ReportRequest.InstitutionId);
            }
            else
            {
                Institution = _adminContext.GetAdminInstitution(reportQuery.InstitutionId);
                IpAddressRanges = GetInstitutionsIpAddressRanges(reportQuery.InstitutionId);
                InitBase(reportQuery.ToBaseReportQuery(), IpAddressRanges,
                    reportQuery.EditableIpAddressRange?.GetIpAddressRange());
            }

            if (updateReportQuery)
            {
                reportQuery.DateRangeStart = BaseReportQuery.PeriodStartDate;
                reportQuery.DateRangeEnd = BaseReportQuery.PeriodEndDate;
                reportQuery.Period = BaseReportQuery.Period;

                reportQuery.PracticeAreaId = BaseReportQuery.PracticeAreaId;
                reportQuery.PublisherId = BaseReportQuery.PublisherId;
                reportQuery.ResourceId = BaseReportQuery.ResourceId;

                reportQuery.IncludePurchasedTitles = BaseReportQuery.IncludePurchased;
                reportQuery.IncludePdaTitles = BaseReportQuery.IncludePda;
                reportQuery.IncludeTocTitles = BaseReportQuery.IncludeToc;
                reportQuery.IncludeTrialStats = BaseReportQuery.IncludeTrialStats;

                if (!reportQuery.FilterByIpRanges)
                {
                    reportQuery.EditableIpAddressRange = null;
                    reportQuery.SelectedIpAddressRangeIds = null;
                }
            }
        }

        protected List<Core.Authentication.IpAddressRange> GetInstitutionsIpAddressRanges(int institutionId)
        {
            return institutionId > 0
                ? _ipAddressRangeService.GetInstitutionIpRanges(institutionId)
                : new List<Core.Authentication.IpAddressRange>();
        }

        public static ReportType GetReportType(string reportTypeText)
        {
            return reportTypeText == "AppUsage" ? ReportType.ApplicationUsageReport : ReportType.ResourceUsageReport;
        }

        public static int GetReportTypeId(string reportTypeText)
        {
            return (int)GetReportType(reportTypeText);
        }

        public static string GetReportTypeText(ReportType reportType)
        {
            return reportType == ReportType.ApplicationUsageReport ? "AppUsage" : "ResUsage";
        }

        public static string GetReportTypeText(int reportTypeId)
        {
            return GetReportTypeText((ReportType)reportTypeId);
        }
    }
}