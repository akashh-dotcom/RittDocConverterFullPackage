#region

using R2V2.Core.Reports;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ReportLogMap : BaseMap<ReportLog>
    {
        public ReportLogMap()
        {
            Table("dbo.tReportLog");
            Id(x => x.Id, "iReportLogId").GeneratedBy.Identity();
            Map(x => x.Type, "iReportType").CustomType(typeof(ReportType));
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.Period, "iPeriod").CustomType(typeof(ReportPeriod));
            Map(x => x.DateRangeStart, "dtDateRangeStart");
            Map(x => x.DateRangeEnd, "dtDateRangeEnd");
            //Map(x => x.IpFilter, "vchIpFilter");
            Map(x => x.IpFilter).Column("vchIpFilter").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.PracticeAreaId, "iPracticeAreaId");
            Map(x => x.SpecialtyId, "iSpecialtyId");
            Map(x => x.PublisherId, "iPublisherId");
            Map(x => x.ResourceId, "iResourceId");
            Map(x => x.IncludePdaTitles, "tiIncludePdaTitles");
            Map(x => x.IncludePurchasedTitles, "tiIncludePurchasedTitles");
            Map(x => x.IncludeTocTitles, "tiIncludeTocTitles");
            Map(x => x.IncludeTrialStats, "tiIncludeTrialStats");
            Map(x => x.InstitutionTypeId, "iInstitutionTypeId");
            Map(x => x.TerritoryCode, "vchTerritoryCode");
            Map(x => x.IsDefaultQuery, "tiDefaultQuery");
            Map(x => x.SortBy, "iSortById").CustomType(typeof(ReportSortBy));
        }
    }
}