#region

using System.Linq;

#endregion

namespace R2V2.Core.Reports
{
    public static class QueryableDailyCountExtension
    {
        public static IQueryable<IDailyCount> FilterByReportRequest(this IQueryable<IDailyCount> dailyCounts,
            ReportRequest reportRequest)
        {
            var query = dailyCounts
                .Where(x => x.Date >= reportRequest.DateRangeStart && x.Date <= reportRequest.DateRangeEnd);

            query = reportRequest.InstitutionId > 0
                ? query.Where(x => x.Institution.Id == reportRequest.InstitutionId)
                : query.Where(x => x.Institution.HouseAccount == false || x.Institution == null);
            return query;
        }
    }
}