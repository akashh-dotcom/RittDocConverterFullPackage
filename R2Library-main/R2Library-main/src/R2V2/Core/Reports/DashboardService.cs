#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Transform;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Reports
{
    public class DashboardService
    {
        private readonly ILog<DashboardService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public DashboardService(IUnitOfWorkProvider unitOfWorkProvider, ILog<DashboardService> log)
        {
            _unitOfWorkProvider = unitOfWorkProvider;
            _log = log;
        }

        public List<int> GetFilteredResourceIds(ResourceListType resouorceListType, int institutionId,
            DateTime statisticsDate, DateTime? statisticsEndDate = null)
        {
            var sql = new StringBuilder()
                .Append(" select ResourceId, PdaAdded, PdaAddedToCart, PdaNewEdition ")
                .Append(" , Purchased, ArchivedPurchased, NewEditionPreviousPurchased ")
                .Append(" from vInstitutionResourceStatistics ")
                .Append(" where ");

            switch (resouorceListType)
            {
                case ResourceListType.Purchased:
                    sql.Append(" purchased = 1 ");
                    break;
                case ResourceListType.Archived:
                    sql.Append(" archivedPurchased = 1 ");
                    break;
                case ResourceListType.NewEditionPurchased:
                    sql.Append(" newEditionPreviousPurchased = 1 ");
                    break;
                case ResourceListType.PdaAdded:
                    sql.Append(" pdaAdded = 1 ");
                    break;
                case ResourceListType.PdaAddedToCart:
                    sql.Append(" pdaAddedToCart = 1 ");
                    break;
                case ResourceListType.PdaNewEdition:
                    sql.Append(" pdaNewEdition = 1 ");
                    break;
            }

            sql.Append(
                " and institutionId = :InstitutionId and aggregationDate between :StatisticsDate and :StatisticsEndDate ");


            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("InstitutionId", institutionId);
                query.SetParameter("StatisticsDate", statisticsDate.ToString("d"));
                query.SetParameter("StatisticsEndDate",
                    statisticsEndDate.GetValueOrDefault(DateTime.Now).ToString("d"));


                var results = query
                    .SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionResourceStatistics)))
                    .List<InstitutionResourceStatistics>();

                return results.Any() ? results.Select(x => x.ResourceId).ToList() : null;
            }
        }

        /// <summary>
        ///     Used to get the account highlights
        /// </summary>
        public InstitutionHighlights GetHighlights(int institutionId, DateTime statisticsDate,
            DateTime statisticsEndDate)
        {
            InstitutionHighlights highlights;

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(HighlightsFromView);

                query.SetParameter("InstitutionId", institutionId);
                query.SetParameter("StartDate", $"{statisticsDate:MM/dd/yyyy}");
                query.SetParameter("EndDate", $"{statisticsEndDate:MM/dd/yyyy}");

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionHighlights)))
                    .List<InstitutionHighlights>();
                highlights = results.ToList().FirstOrDefault();
            }

            return highlights;
        }

        public InstitutionHighlights GetPopularSpecialtyOfYear(int institutionId, DateTime statisticsDate)
        {
            InstitutionHighlights highlights;

            var sql = new StringBuilder()
                .Append(
                    " select top 1 MostPopularSpecialtyName, sum(MostPopularSpecialtyCount) as 'MostPopularSpecialtyCount' ")
                .Append(" from vInstitutionStatistics ")
                .Append(" where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate ")
                .Append(" group by MostPopularSpecialtyName ")
                .Append(" order by 2 desc ")
                .ToString();

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);

                query.SetParameter("InstitutionId", institutionId);
                query.SetParameter("StartDate", $"1/1/{statisticsDate.Year}");
                query.SetParameter("EndDate", statisticsDate.ToString("d"));

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionHighlights)))
                    .List<InstitutionHighlights>();
                highlights = results.ToList().FirstOrDefault();
            }

            return highlights;
        }


        /// <summary>
        ///     Used to get the account usage statistics
        /// </summary>
        public InstitutionAccountUsage GetAccountUsage(int institutionId, DateTime statisticsDate,
            DateTime statisticsEndDate)
        {
            InstitutionAccountUsage accountUsage;
            var sql = new StringBuilder()
                .Append("select ")
                .Append(
                    " sum(ContentCount) as ContentCount, sum(TocCount) as TocCount, sum(SessionCount) as SessionCount, sum(PrintCount) as PrintCount ")
                .Append(
                    " , sum(EmailCount) as EmailCount, sum(TurnawayConcurrencyCount) as TurnawayConcurrencyCount, sum(TurnawayAccessCount) as TurnawayAccessCount ")
                .Append("from vInstitutionStatistics ")
                .Append("where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate");

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("InstitutionId", institutionId);
                query.SetParameter("StartDate", $"{statisticsDate:MM/dd/yyyy}");
                query.SetParameter("EndDate", $"{statisticsEndDate:MM/dd/yyyy}");

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionAccountUsage)))
                    .List<InstitutionAccountUsage>();
                accountUsage = results.ToList().FirstOrDefault();
            }

            return accountUsage;
        }

        /// <summary>
        ///     Used to get the resources for ebook collection and pda collection
        /// </summary>
        public List<InstitutionResourceStatistics> GetResourceStatisticsList(int institutionId, DateTime statisticsDate,
            DateTime statisticsEndDate, bool pdaOnly = false)
        {
            var sql = new StringBuilder()
                .Append("select ResourceId, PdaAdded, PdaAddedToCart, PdaNewEdition ")
                .Append(pdaOnly ? "" : ", Purchased, ArchivedPurchased, NewEditionPreviousPurchased ")
                .Append("from vInstitutionResourceStatistics ")
                .Append("where (pdaAdded = 1 or pdaAddedToCart = 1 or pdaNewEdition = 1 ")
                .Append(pdaOnly
                    ? ")"
                    : " or purchased = 1 or archivedPurchased = 1 or newEditionPreviousPurchased = 1) ")
                .Append("and institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate");

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("InstitutionId", institutionId);
                query.SetParameter("StartDate", $"{statisticsDate:MM/dd/yyyy}");
                query.SetParameter("EndDate", $"{statisticsEndDate:MM/dd/yyyy}");

                var results = query
                    .SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionResourceStatistics)))
                    .List<InstitutionResourceStatistics>();
                return results.ToList();
            }
        }


        /// <summary>
        ///     Used by the Dashboard Email Task
        /// </summary>
        public InstitutionEmailStatistics GetInstitutionEmailStatistics(int institutionId, DateTime statisticsDate)
        {
            var stats = new InstitutionEmailStatistics();

            var statisticsEndDate = statisticsDate.AddMonths(1).AddDays(-1);

            var firstOfYear = new DateTime(statisticsDate.Year, 1, 1);

            stats.Highlights = GetHighlights(institutionId, statisticsDate, statisticsEndDate);
            stats.AccountUsage = GetAccountUsage(institutionId, statisticsDate, statisticsEndDate);

            stats.YearAccountUsage = GetAccountUsage(institutionId, firstOfYear, statisticsEndDate);

            var highlightWithSpecialtyOfYear = GetPopularSpecialtyOfYear(institutionId, statisticsDate);
            if (highlightWithSpecialtyOfYear != null)
            {
                stats.MostPopularSpecialtyNameOfYear = highlightWithSpecialtyOfYear.MostPopularSpecialtyName;
                stats.MostPopularSpecialtyCountOfYear = highlightWithSpecialtyOfYear.MostPopularSpecialtyCount;
            }

            stats.StartDate = statisticsDate;

            return stats;
        }

        /// <summary>
        ///     Used to Get the Institutions that need to be aggregated the AggregateInstitutionStatisticsTask
        /// </summary>
        public List<InstitutionStatistics> GetInstitutionsForStatistics()
        {
            var currentFormatedDate = DateTime.Parse($"{DateTime.Now.Month}/01/{DateTime.Now.Year}");
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(GetInstitutionsForStatisticsQuery);

                query.SetParameter("CurrentFormatedDate", currentFormatedDate);

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionStatistics)))
                    .List<InstitutionStatistics>();
                return results.ToList();
            }
        }


        /// <summary>
        ///     Used to Aggregate the data in the AggregateInstitutionStatisticsTask
        /// </summary>
        public InstitutionStatistics GetAggregatedInstitutionStatistics(InstitutionStatistics institutionStatistics)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(HighlightsQuery);

                query.SetTimeout(360);

                query.SetParameter("InstitutionId", institutionStatistics.InstitutionId);
                query.SetParameter("StartDate", institutionStatistics.StartDate);
                query.SetParameter("EndDate", institutionStatistics.StartDate.AddMonths(1).AddSeconds(-1));
                query.SetParameter("Plus1Month", institutionStatistics.StartDate.AddMonths(1));

                var results = query.SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionHighlights)))
                    .List<InstitutionHighlights>();

                var highlights = results.ToList().FirstOrDefault();

                query = uow.Session.CreateSQLQuery(AccountUsageQuery);

                query.SetParameter("InstitutionId", institutionStatistics.InstitutionId);
                query.SetParameter("StartDate", institutionStatistics.StartDate);
                query.SetParameter("EndDate", institutionStatistics.StartDate.AddMonths(1).AddSeconds(-1));

                var results2 = query.SetResultTransformer(Transformers.AliasToBean(typeof(InstitutionAccountUsage)))
                    .List<InstitutionAccountUsage>();

                var accountUsage = results2.ToList().FirstOrDefault();


                institutionStatistics.Highlights = highlights;
                institutionStatistics.AccountUsage = accountUsage;

                return institutionStatistics;
            }
        }

        #region "Sql"

        private const string GetInstitutionsForStatisticsQuery = @"
select iInstitutionId as InstitutionId, isnull(DATEADD(mm, 1, MAX(im.aggregationDate)), DATEFROMPARTS(year(i.dtCreationDate), month(i.dtCreationDate), 1)) as 'StartDate'
from tInstitution i
left join vInstitutionStatistics im on i.iInstitutionId = im.InstitutionId
where  im.aggregationDate is null or
(
	aggregationDate < :CurrentFormatedDate and iInstitutionId not in
	(
		select institutionId from vInstitutionStatistics
		where DATEADD(mm, 1, aggregationDate) = DATEFROMPARTS(year(getdate()), month(getdate()), 1)
		group by institutionId
	)
)
group by i.iInstitutionId, i.dtCreationDate
order by i.iInstitutionId
";

        private const string AccountUsageQuery = @" select
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
where dcvc.contentViewDate between :StartDate and :EndDate
and dcvc.institutionId = :InstitutionId and dcvc.chapterSectionId is not null) as 'ContentCount',
--TOC Retrievals
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
where dcvc.contentViewDate between :StartDate and :EndDate
and dcvc.institutionId = :InstitutionId and dcvc.chapterSectionId is null) as 'TocCount',
--Sessions
(select sum(dpvc.sessionCount)
from   vDailySessionCount dpvc
left outer join tInstitution i on i.iInstitutionId = dpvc.institutionId
where dpvc.sessionDate between :StartDate and :EndDate
and dpvc.institutionId = :InstitutionId) as 'SessionCount',
--Print Requests
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
where dcvc.contentViewDate between :StartDate and :EndDate
and dcvc.institutionId = :InstitutionId and dcvc.actionTypeId = 16) as 'PrintCount',
--Email Requests
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
where dcvc.contentViewDate between :StartDate and :EndDate
and dcvc.institutionId = :InstitutionId and dcvc.actionTypeId = 17) as 'EmailCount',
--Content Turnaways
(select sum(dctc.contentTurnawayCount)
from   vDailyContentTurnawayCount dctc
left outer join tInstitution i on i.iInstitutionId = dctc.institutionId
where dctc.contentTurnawayDate between :StartDate and :EndDate
and dctc.institutionId = :InstitutionId and dctc.TurnawayTypeId = 20) as 'TurnawayConcurrencyCount',
--Access Turnaways
(select sum(dctc.contentTurnawayCount)
from   vDailyContentTurnawayCount dctc
left outer join tInstitution i on i.iInstitutionId = dctc.institutionId
where dctc.contentTurnawayDate between :StartDate and :EndDate
and dctc.institutionId = :InstitutionId and dctc.TurnawayTypeId = 21) as 'TurnawayAccessCount'
";

        private const string HighlightsQuery = @"
select MostAccessedResourceId, MostAccessedCount, LeastAccessedResourceId, LeastAccessedCount
, MostTurnawayConcurrentResourceId, MostTurnawayConcurrentCount, MostTurnawayAccessResourceId, MostTurnawayAccessCount,
MostPopularSpecialtyName, MostPopularSpecialtyCount, LeastPopularSpecialtyName, LeastPopularSpecialtyCount, TotalResourceCount
from
(select top 1 * from (
select top 1 resourceId  as 'MostAccessedResourceId', sum(contentviewcount) as 'MostAccessedCount'
from [dbo].[vDailyContentViewCount]
where institutionId = :InstitutionId and contentViewDate between :StartDate and :EndDate
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) a,

(select top 1 * from (
select top 1 irl.iResourceId  as 'LeastAccessedResourceId', sum(isnull(dcvc.contentviewcount, 0)) as 'LeastAccessedCount'
from tInstitutionResourceLicense irl
left join vDailyContentViewCount dcvc on irl.iInstitutionId = dcvc.institutionId and irl.iResourceId = dcvc.resourceId
where irl.iInstitutionId = :InstitutionId and
 (contentViewDate between :StartDate and :EndDate )  and isnull(dcvc.contentviewcount, 0) > 0
group by irl.iResourceId
order by 2 asc
union select null, null ) x order by 2 desc) b,

(select top 1 * from (
select top 1 resourceId as 'MostTurnawayConcurrentResourceId', sum(contentTurnawayCount) as 'MostTurnawayConcurrentCount'
 from vDailyContentTurnawayCount
where institutionId = :InstitutionId and contentTurnawayDate between :StartDate and :EndDate
and turnawayTypeId = 20
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) c,

(select top 1 * from (
select top 1 resourceId as 'MostTurnawayAccessResourceId', sum(contentTurnawayCount) as 'MostTurnawayAccessCount'
 from vDailyContentTurnawayCount
where institutionId = :InstitutionId and contentTurnawayDate between :StartDate and :EndDate
and turnawayTypeId = 21
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) cc,

(select top 1 * from (
select top 1 s.vchSpecialtyName as 'MostPopularSpecialtyName', sum(dcvc.contentViewCount) as 'MostPopularSpecialtyCount'
from tResourceSpecialty rs
join tSpecialty s on rs.iSpecialtyId = s.iSpecialtyId
join tResource r on rs.iResourceId = r.iResourceId
join vDailyContentViewCount dcvc on r.iResourceId = dcvc.resourceId
where rs.tiRecordStatus = 1 and s.tiRecordStatus = 1 and r.tiRecordStatus = 1
and dcvc.institutionId = :InstitutionId and dcvc.contentViewDate between :StartDate and :EndDate
group by rs.iResourceSpecialtyId, s.vchSpecialtyName, dcvc.institutionId
order by 2 desc
union select null, null) x order by 2 desc) d,

(select top 1 * from (
select top 1  s.vchSpecialtyName as 'LeastPopularSpecialtyName', sum(dcvc.contentViewCount) as 'LeastPopularSpecialtyCount'
from tResourceSpecialty rs
join tSpecialty s on rs.iSpecialtyId = s.iSpecialtyId
join tResource r on rs.iResourceId = r.iResourceId
join vDailyContentViewCount dcvc on r.iResourceId = dcvc.resourceId
where rs.tiRecordStatus = 1 and s.tiRecordStatus = 1 and r.tiRecordStatus = 1 and isnull(dcvc.contentViewCount, 0) > 0
and dcvc.institutionId = :InstitutionId and dcvc.contentViewDate between :StartDate and :EndDate
group by rs.iResourceSpecialtyId, s.vchSpecialtyName, dcvc.institutionId
union select null, null) x order by 2 desc) e,

(select top 1 * from (
select count(*) as 'TotalResourceCount'
from tInstitutionResourceLicense irl
join tResource r on irl.iResourceId = r.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8)
where irl.dtFirstPurchaseDate < :Plus1Month
and irl.iInstitutionId = :InstitutionId and irl.tiRecordStatus = 1
and irl.tiLicenseTypeId = 1 and irl.iLicenseCount > 0
union select null) x order by 1 desc) f

";

        private const string HighlightsFromView = @"

select MostAccessedResourceId, MostAccessedCount, LeastAccessedResourceId, LeastAccessedCount
, MostTurnawayConcurrentResourceId, MostTurnawayConcurrentCount, MostTurnawayAccessResourceId, MostTurnawayAccessCount,
MostPopularSpecialtyName, MostPopularSpecialtyCount, LeastPopularSpecialtyName, LeastPopularSpecialtyCount, TotalResourceCount
from
(select top 1 * from (
select top 1 MostAccessedResourceId, sum(MostAccessedCount) as 'MostAccessedCount'
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
group by MostAccessedResourceId
order by 2 desc
union select null, null) x order by 2 desc) a,

(select top 1 * from (
select top 1 LeastAccessedResourceId, sum(LeastAccessedCount) as 'LeastAccessedCount'
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
and LeastAccessedCount > 0
group by LeastAccessedResourceId
order by 2 asc
union select null, null ) x order by 2 desc) b,

(select top 1 * from (
select top 1 MostTurnawayConcurrentResourceId, sum(MostTurnawayConcurrentCount) as 'MostTurnawayConcurrentCount'
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
group by MostTurnawayConcurrentResourceId
order by 2 desc
union select null, null) x order by 2 desc) c,

(select top 1 * from (
select top 1 MostTurnawayAccessResourceId, sum(MostTurnawayAccessCount) as 'MostTurnawayAccessCount'
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
group by MostTurnawayAccessResourceId
order by 2 desc
union select null, null) x order by 2 desc) cc,

(select top 1 * from (
select top 1 MostPopularSpecialtyName, sum(MostPopularSpecialtyCount) as 'MostPopularSpecialtyCount'
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
group by MostPopularSpecialtyName
order by 2 desc
union select null, null) x order by 2 desc) d,

(select top 1 * from (
select top 1  LeastPopularSpecialtyName as 'LeastPopularSpecialtyName', sum(LeastPopularSpecialtyCount) as 'LeastPopularSpecialtyCount'
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
and LeastPopularSpecialtyCount > 0
group by LeastPopularSpecialtyName
order by 2 asc
union select null, null) x order by 2 desc) e,
(select top 1 * from (
select top 1 TotalResourceCount
from vInstitutionStatistics
where institutionId = :InstitutionId and aggregationDate between :StartDate and :EndDate
order by 1 desc
union select null) x order by 1 desc) f
";

        #endregion
    }

    public class CmsItem
    {
        public string Html { get; set; }
    }
}