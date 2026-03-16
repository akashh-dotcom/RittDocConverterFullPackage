#region

using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.DataAccess
{
    public class WebStatisticsService : DataServiceBase
    {
        #region "GetActiveInstitutionsForStatistics Query"

        private const string GetActiveInstitutionsForStatistics = @"
select iInstitutionId, isnull(MAX(DATEADD(mm, 1, im.aggregationDate)), DATEFROMPARTS(year(i.dtCreationDate), month(i.dtCreationDate), 1)) as 'AggregationDate'
from tInstitution i
left join {0}..InstitutionMonthlyStatisticsCount im on i.iInstitutionId = im.InstitutionId
where  i.iInstitutionAcctStatusId = 1 and im.InstitutionMonthlyStatisticsCountId is null or
(
	aggregationDate < @CurrentFormatedDate and iInstitutionId not in 
	(
		select institutionId from {0}..InstitutionMonthlyStatisticsCount 
		where DATEADD(mm, 1, aggregationDate) = DATEFROMPARTS(year(getdate()), month(getdate()), 1)
	)
)
group by i.iInstitutionId, i.dtCreationDate
";

        #endregion

        #region "GetStatisticsForInstitution Query"

        private const string GetStatisticsForInstitution = @"

select MostAccessedResourceId, MostAccessedCount, LeastAccessedResourceId, LeastAccessedCount
, MostTurnawayConcurrentResourceId, MostTurnawayConcurrentCount, MostTurnawayAccessResourceId, MostTurnawayAccessCount,
MostPopularSpecialtyName, MostPopularSpecialtyCount, LeastPopularSpecialtyName, LeastPopularSpecialtyCount, TotalResourceCount
from
(select top 1 * from (
select top 1 resourceId  as 'MostAccessedResourceId', sum(contentviewcount) as 'MostAccessedCount'
from [dbo].[vDailyContentViewCount] 
where institutionId = @InstitutionId and contentViewDate between @StartDate and @EndDate
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) a, 

(select top 1 * from (
select top 1 irl.iResourceId  as 'LeastAccessedResourceId', sum(isnull(dcvc.contentviewcount, 0)) as 'LeastAccessedCount'
from tInstitutionResourceLicense irl
left join vDailyContentViewCount dcvc on irl.iInstitutionId = dcvc.institutionId and irl.iResourceId = dcvc.resourceId
where irl.iInstitutionId = @InstitutionId and 
 (contentViewDate between @StartDate and @EndDate )  and isnull(dcvc.contentviewcount, 0) > 0
group by irl.iResourceId
order by 2 asc
union select null, null ) x order by 2 desc) b,

(select top 1 * from (
select top 1 resourceId as 'MostTurnawayConcurrentResourceId', sum(contentTurnawayCount) as 'MostTurnawayConcurrentCount'
 from vDailyContentTurnawayCount
where institutionId = @InstitutionId and contentTurnawayDate between @StartDate and @EndDate
and turnawayTypeId = 20
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) c,

(select top 1 * from (
select top 1 resourceId as 'MostTurnawayAccessResourceId', sum(contentTurnawayCount) as 'MostTurnawayAccessCount'
 from vDailyContentTurnawayCount
where institutionId = @InstitutionId and contentTurnawayDate between @StartDate and @EndDate
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
and dcvc.institutionId = @InstitutionId and dcvc.contentViewDate between @StartDate and @EndDate
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
and dcvc.institutionId = @InstitutionId and dcvc.contentViewDate between @StartDate and @EndDate
group by rs.iResourceSpecialtyId, s.vchSpecialtyName, dcvc.institutionId
union select null, null) x order by 2 desc) e,

(select top 1 * from (
select count(*) as 'TotalResourceCount'
from tInstitutionResourceLicense irl
where irl.dtFirstPurchaseDate < @Plus1Month
and irl.iInstitutionId = @InstitutionId and tiRecordStatus = 1
union select null) x order by 1 desc) f
";

        #endregion

        #region "GetResourceStatisticsForInstitution Query"

        private const string GetResourceStatisticsForInstitution = @"
select 
--@@@@@@@@@@@@@@@@Successful Content Retreival@@@@@@@@@@@@@@@@
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
where dcvc.contentViewDate between @StartDate and @EndDate
and dcvc.institutionId = @InstitutionId) as 'ContentCount',
--@@@@@@@@@@@@@@@@TOC Retrievals@@@@@@@@@@@@@@@@
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
where dcvc.contentViewDate between @StartDate and @EndDate
and dcvc.institutionId = @InstitutionId and dcvc.chapterSectionId is null) as 'TocCount',
--@@@@@@@@@@@@@@@@Sessions@@@@@@@@@@@@@@@@
(select sum(dpvc.sessionCount) 
from   vDailySessionCount dpvc
left outer join tInstitution i on i.iInstitutionId = dpvc.institutionId
where dpvc.sessionDate between @StartDate and @EndDate
and dpvc.institutionId = @InstitutionId) as 'SessionCount',
--@@@@@@@@@@@@@@@@Print Requests@@@@@@@@@@@@@@@@
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc 
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId 
where dcvc.contentViewDate between @StartDate and @EndDate
and dcvc.institutionId = @InstitutionId and dcvc.actionTypeId = 16) as 'PrintCount',
--@@@@@@@@@@@@@@@@Email Requests@@@@@@@@@@@@@@@@
(select sum(dcvc.contentViewCount)
from   vDailyContentViewCount dcvc 
left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId 
where dcvc.contentViewDate between @StartDate and @EndDate 
and dcvc.institutionId = @InstitutionId and dcvc.actionTypeId = 16) as 'EmailCount',
--@@@@@@@@@@@@@@@@Content Turnaways@@@@@@@@@@@@@@@@
(select sum(dctc.contentTurnawayCount)
from   vDailyContentTurnawayCount dctc
left outer join tInstitution i on i.iInstitutionId = dctc.institutionId
where dctc.contentTurnawayDate between @StartDate and @EndDate
and dctc.institutionId = @InstitutionId and dctc.TurnawayTypeId = 20) as 'TurnawayConcurrencyCount',
--@@@@@@@@@@@@@@@@Access Turnaways@@@@@@@@@@@@@@@@
(select sum(dctc.contentTurnawayCount)
from   vDailyContentTurnawayCount dctc
left outer join tInstitution i on i.iInstitutionId = dctc.institutionId
where dctc.contentTurnawayDate between @StartDate and @EndDate
and dctc.institutionId = @InstitutionId and dctc.TurnawayTypeId = 21) as 'TurnawayAccessCount';
";

        #endregion

        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public WebStatisticsService(IR2UtilitiesSettings r2UtilitiesSettings)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }
    }
}