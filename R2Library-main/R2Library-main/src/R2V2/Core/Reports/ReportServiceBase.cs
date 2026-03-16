#region

using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Core.Reports
{
    public class ReportServiceBase
    {
        private readonly ILog<ReportServiceBase> _log;
        private readonly IQueryable<SavedReport> _savedReports;

        public BaseReportQuery BaseReportQuery;

        public ReportServiceBase(ILog<ReportServiceBase> log, IQueryable<SavedReport> savedReports)
        {
            _log = log;
            _savedReports = savedReports;
        }

        public ReportRequest ReportRequest { get; set; }
        public SavedReport SavedReport { get; set; }

        //Used for SavedReports
        public void InitBase(int institutionId, int reportId)
        {
            var savedReport = GetSavedReport(institutionId, reportId);
            SavedReport = savedReport;

            BaseReportQuery = new BaseReportQuery(savedReport);

            ReportRequest = new ReportRequest();

            ReportRequest = GetReportRequest();

            if (savedReport.HasIpFilter)
            {
                foreach (var ipFilter in savedReport.IpFilters)
                {
                    var ipAddressRange = IpAddressRange.ParseIpAddressRange(ipFilter.IpStartRange, ipFilter.IpEndRange);
                    if (ipAddressRange != null)
                    {
                        ReportRequest.AddIpAddressRange(ipAddressRange);
                    }
                    else
                    {
                        _log.WarnFormat("Invalid IP Address: {0} - {1}, Id: {2}", ipFilter.IpStartRange,
                            ipFilter.IpEndRange, ipFilter.Id);
                    }
                }
            }
        }

        public void InitBase(int institutionId, int reportId, string territoryCode)
        {
            var savedReport = GetSavedReport(institutionId, reportId);
            SavedReport = savedReport;

            BaseReportQuery = new BaseReportQuery(savedReport);

            ReportRequest = new ReportRequest();

            ReportRequest = GetReportRequest();

            if (savedReport.HasIpFilter)
            {
                foreach (var ipFilter in savedReport.IpFilters)
                {
                    var ipAddressRange = IpAddressRange.ParseIpAddressRange(ipFilter.IpStartRange, ipFilter.IpEndRange);
                    if (ipAddressRange != null)
                    {
                        ReportRequest.AddIpAddressRange(ipAddressRange);
                    }
                    else
                    {
                        _log.WarnFormat("Invalid IP Address: {0} - {1}, Id: {2}", ipFilter.IpStartRange,
                            ipFilter.IpEndRange, ipFilter.Id);
                    }
                }
            }

            ReportRequest.TerritoryCode = territoryCode;
        }

        public void InitBase(BaseReportQuery baseReportQuery, List<IpAddressRange> institutionIpAddresses,
            IpAddressRange specificIpAddress)
        {
            BaseReportQuery = baseReportQuery;

            ReportRequest = new ReportRequest();

            ReportRequest = GetReportRequest();

            ReportRequest.TerritoryCode = baseReportQuery.TerritoryCode;

            if (specificIpAddress != null)
            {
                ReportRequest.AddIpAddressRange(specificIpAddress);
            }

            if (!baseReportQuery.HasIpFilter) return;

            if (institutionIpAddresses == null) return;

            if (baseReportQuery.InstitutionIpRangeIds == null) return;


            foreach (var institutionIpAddress in institutionIpAddresses.Where(institutionIpAddress =>
                         baseReportQuery.InstitutionIpRangeIds.Contains(institutionIpAddress.Id)))
            {
                ReportRequest.AddIpAddressRange(institutionIpAddress);
            }
        }

        public void InitBase(BaseReportQuery baseReportQuery)
        {
            baseReportQuery.HasIpFilter = false;
            InitBase(baseReportQuery, null, null);
        }

        /// <summary>
        ///     Create a ReportRequest object using the data in the ReportQuery object populated from the request
        /// </summary>
        private ReportRequest GetReportRequest()
        {
            ReportDatesService.SetDates(BaseReportQuery);

            var reportRequest = new ReportRequest
            {
                InstitutionId = BaseReportQuery.InstitutionId,
                PracticeAreaId = BaseReportQuery.PracticeAreaId,
                SpecialtyId = BaseReportQuery.SpecialtyId,
                ResourceId = BaseReportQuery.ResourceId,
                PublisherId = BaseReportQuery.PublisherId,
                DateRangeStart = BaseReportQuery.PeriodStartDate.GetValueOrDefault(),
                DateRangeEnd = BaseReportQuery.PeriodEndDate.GetValueOrDefault(),
                IncludePurchasedTitles = BaseReportQuery.IncludePurchased,
                IncludePdaTitles = BaseReportQuery.IncludePda,
                IncludeTocTitles = BaseReportQuery.IncludeToc,
                IncludeTrialStats = BaseReportQuery.IncludeTrialStats,
                SortBy = BaseReportQuery.SortBy,
                InstitutionTypeId = BaseReportQuery.InstitutionTypeId,
                Period = BaseReportQuery.Period,
                Type = BaseReportQuery.Type
            };

            return reportRequest;
        }

        public SavedReport GetSavedReport(int institutionId, int reportId)
        {
            var savedReport = _savedReports
                .FetchMany(x => x.IpFilters)
                .SingleOrDefault(x => x.Id == reportId && (x.InstitutionId == institutionId || institutionId == 0));

            if (savedReport == null)
            {
                _log.Error($"Can't find saved report, report id: {reportId}, institution id: {institutionId}");
                return null;
            }

            return savedReport;
        }
    }
}