alter table ContentView
add licenseType int not null default ((0));

alter table ContentView
add resourceStatusId int null;

alter table DailyContentViewCount
add licenseType int not null default ((0));

alter table DailyContentViewCount
add resourceStatusId int null;

alter table DailyInstitutionResourceStatisticsCount
add resourceStatusId int null;

alter table DailyInstitutionResourceStatisticsCount
add licenseType int not null default ((0));


--1 purchased
--2 trial
--3 pda

Update ContentView
set licenseType = 1
from ContentView dcvc
join RIT001_Temp..tResource r on dcvc.resourceId = r.iResourceId
join RIT001_Temp..tInstitutionResourceLicense irl on dcvc.institutionId = irl.iInstitutionId and dcvc.resourceId = irl.iResourceId
join RIT001_Temp..tInstitution i on dcvc.institutionId = i.iInstitutionId
where convert(date, dcvc.contentViewTimestamp) >= convert(date, irl.dtFirstPurchaseDate);

go

Update ContentView
set licenseType = 2
from ContentView dcvc
join RIT001_Temp..tResource r on dcvc.resourceId = r.iResourceId
left join RIT001_Temp..tInstitutionResourceLicense irl on dcvc.institutionId = irl.iInstitutionId and dcvc.resourceId = irl.iResourceId
join RIT001_Temp..tInstitution i on dcvc.institutionId = i.iInstitutionId
where 
convert(date, dcvc.contentViewTimestamp) <= convert(date, i.dtTrialAcctEnd) 
and (convert(date, irl.dtPdaAddedDate) is null or convert(date, dcvc.contentViewTimestamp) < convert(date, irl.dtPdaAddedDate))
and (convert(date, irl.dtFirstPurchaseDate) is null or convert(date, dtFirstPurchaseDate) > convert(date, dcvc.contentViewTimestamp));

go


Update ContentView
set licenseType = 3
from ContentView dcvc
join RIT001_Temp..tResource r on dcvc.resourceId = r.iResourceId
join RIT001_Temp..tInstitutionResourceLicense irl on dcvc.institutionId = irl.iInstitutionId and dcvc.resourceId = irl.iResourceId
join RIT001_Temp..tInstitution i on dcvc.institutionId = i.iInstitutionId
where 
convert(date, dcvc.contentViewTimestamp) >= convert(date, dtPdaAddedDate)
and (dtPdaAddedToCartDate is null or (convert(date, dtPdaAddedToCartDate) >= convert(date, dcvc.contentViewTimestamp) --Added To Cart while still viewing
										or (convert(date, dateadd(day, 1, dtPdaAddedToCartDate)) >= convert(date, dcvc.contentViewTimestamp) and dcvc.turnawayTypeId = 0 )))
and (dtPdaDeletedDate is null or (convert(date, dtPdaDeletedDate) >= convert(date, dcvc.contentViewTimestamp) --Added To Cart while still viewing
										or (convert(date, dateadd(day, 1, dtPdaDeletedDate)) >= convert(date, dcvc.contentViewTimestamp) and dcvc.turnawayTypeId = 0 )))
and (dtFirstPurchaseDate is null or convert(date, dtFirstPurchaseDate) > convert(date, dcvc.contentViewTimestamp));

go 

Update DailyContentViewCount
set licenseType = 1
from DailyContentViewCount dcvc
join RIT001_Temp..tResource r on dcvc.resourceId = r.iResourceId
join RIT001_Temp..tInstitutionResourceLicense irl on dcvc.institutionId = irl.iInstitutionId and dcvc.resourceId = irl.iResourceId
join RIT001_Temp..tInstitution i on dcvc.institutionId = i.iInstitutionId
where dcvc.contentViewDate >= convert(date, irl.dtFirstPurchaseDate);

go 

Update DailyContentViewCount
set licenseType = 2
from DailyContentViewCount dcvc
join RIT001_Temp..tResource r on dcvc.resourceId = r.iResourceId
left join RIT001_Temp..tInstitutionResourceLicense irl on dcvc.institutionId = irl.iInstitutionId and dcvc.resourceId = irl.iResourceId
join RIT001_Temp..tInstitution i on dcvc.institutionId = i.iInstitutionId
where 
dcvc.contentViewDate <= convert(date, i.dtTrialAcctEnd) 
and (irl.dtPdaAddedDate is null or dcvc.contentViewDate < convert(date, irl.dtPdaAddedDate))
and (irl.dtFirstPurchaseDate is null or convert(date, irl.dtFirstPurchaseDate) > dcvc.contentViewDate)

go

Update DailyContentViewCount
set licenseType = 3
from DailyContentViewCount dcvc
join RIT001_Temp..tResource r on dcvc.resourceId = r.iResourceId
join RIT001_Temp..tInstitutionResourceLicense irl on dcvc.institutionId = irl.iInstitutionId and dcvc.resourceId = irl.iResourceId
join RIT001_Temp..tInstitution i on dcvc.institutionId = i.iInstitutionId
where 
dcvc.contentViewDate >= convert(date, dtPdaAddedDate) 
and (dtPdaAddedToCartDate is null or (convert(date, dtPdaAddedToCartDate) >= contentViewDate --Added To Cart while still viewing
										or (convert(date, dateadd(day, 1, dtPdaAddedToCartDate)) >= contentViewDate)))
and (dtPdaDeletedDate is null or (convert(date, dtPdaDeletedDate) >= contentViewDate --Added To Cart while still viewing
										or (convert(date, dateadd(day, 1, dtPdaDeletedDate)) >= contentViewDate)))
and (dtFirstPurchaseDate is null or convert(date, dtFirstPurchaseDate) > dcvc.contentViewDate)


update DailyInstitutionResourceStatisticsCount
set licenseType = dcvc.licenseType
from DailyInstitutionResourceStatisticsCount dirsc
join DailyContentViewCount dcvc on dirsc.resourceId = dcvc.resourceId and dirsc.institutionId = dcvc.institutionId and dcvc.contentViewDate = dirsc.institutionResourceStatisticsDate

update DailyInstitutionResourceStatisticsCount
set licenseType = dcvc.licenseType
from DailyInstitutionResourceStatisticsCount dirsc
join ContentView dcvc on dirsc.resourceId = dcvc.resourceId and dirsc.institutionId = dcvc.institutionId and convert(date, dcvc.contentViewTimestamp) = dirsc.institutionResourceStatisticsDate
where dirsc.licenseType <> dcvc.licenseType

update DailyInstitutionResourceStatisticsCount
set licenseType = tt.licenseType
from R2Reports_Temp..DailyInstitutionResourceStatisticsCount dirsc
join (

 select institutionId, resourceId, ipAddressInteger, institutionResourceStatisticsDate
      , sum(agg.contentCount) as contentRetrievalCount
      , sum(agg.accessCount) as accessTurnawayCount
	  , agg.licenseType
	  , agg.resourceStatusId
from 
 (
  -- accessCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0  as concurrencyCount, count(institutionId) as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv 
  where     turnawayTypeId = 21 and institutionId > 0-- and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- contentCount
  select    cv.institutionId, cv.resourceId, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, count(cv.institutionId) as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv 
  where     cv.institutionId > 0 --and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     cv.chapterSectionId IS NOT NULL and cv.actionTypeId = 0 and turnawayTypeId = 0
  group by cv.institutionId, cv.resourceId, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  ) as agg 
group by institutionId, resourceId, ipAddressInteger, institutionResourceStatisticsDate, licenseType, resourceStatusId


) as tt on 
tt.institutionId = dirsc.institutionId 
and tt.resourceId = dirsc.resourceId
and tt.ipAddressInteger = dirsc.ipAddressInteger
and tt.institutionResourceStatisticsDate = dirsc.institutionResourceStatisticsDate 
and tt.contentRetrievalCount = dirsc.contentRetrievalCount 
and tt.accessTurnawayCount = dirsc.accessTurnawayCount 
where tt.licenseType <> dirsc.licenseType

Update ContentView
set resourceStatusId = r.iResourceStatusId
from RIT001_Temp..tResource r
where resourceId = r.iResourceId

update DailyContentViewCount
set resourceStatusId = r.iResourceStatusId
from RIT001_Temp..tResource r
where resourceId = r.iResourceId

update DailyInstitutionResourceStatisticsCount
set resourceStatusId = r.iResourceStatusId
from RIT001_Temp..tResource r
where resourceId = r.iResourceId


MAKE SURE TO CHANGE THE DATES BASED ON THE CURRENT VIEWS
DROP VIEW [dbo].[vContentView]
GO
CREATE VIEW [dbo].[vContentView]
AS
select contentTurnawayId as [contentViewId], institutionId, userId, resourceId, chapterSectionId
, turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger
, contentViewTimestamp, actionTypeId, foundFromSearch, searchTerm, requestId, licenseType, resourceStatusId
from R2Reports_Temp..ContentView

go

DROP VIEW [dbo].[vDailyContentViewCount]
GO
CREATE VIEW [dbo].[vDailyContentViewCount] AS 
    select dcvc.dailyContentViewCountId, dcvc.institutionId, dcvc.userId, dcvc.resourceId, dcvc.chapterSectionId, dcvc.ipAddressOctetA 
         , dcvc.ipAddressOctetB, dcvc.ipAddressOctetC, dcvc.ipAddressOctetD, dcvc.ipAddressInteger, dcvc.contentViewDate 
         , dcvc.contentViewCount, dcvc.actionTypeId, dcvc.foundFromSearch, dcvc.licenseType, dcvc.resourceStatusId
    from   [R2Reports_Temp].dbo.DailyContentViewCount dcvc 
    union 
    select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC
		, cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId
		, cv.foundFromSearch, cv.licenseType, cv.resourceStatusId 
    from   [R2Reports_Temp].dbo.ContentView cv 
    where  turnawayTypeId = 0 
      and  contentViewTimestamp > '03/01/2016 00:00:00' 
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
           , cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch , cv.licenseType, cv.resourceStatusId

GO		   
DROP VIEW [dbo].[vDailyInstitutionResourceStatisticsCount]
GO
CREATE VIEW [dbo].[vDailyInstitutionResourceStatisticsCount] AS 

select dirsc.dailyInstitutionResourceStatisticsCountId
     , dirsc.institutionId
     , dirsc.resourceId
     , dirsc.ipAddressInteger
     , dirsc.institutionResourceStatisticsDate
     , dirsc.contentRetrievalCount
     , dirsc.tocRetrievalCount
     , dirsc.sessionCount
     , dirsc.printCount
     , dirsc.emailCount
     , dirsc.accessTurnawayCount
     , dirsc.concurrentTurnawayCount
	 , dirsc.licenseType
	 , dirsc.resourceStatusId
from   [R2Reports_Temp].dbo.DailyInstitutionResourceStatisticsCount dirsc 

union 
 
 select 0, institutionId, resourceId, ipAddressInteger, institutionResourceStatisticsDate
      , sum(agg.contentCount) as contentRetrievalCount
      , sum(agg.tocCount) as tocRetrievalCount
      , sum(agg.sessionCount) as sessionCount
      , sum(agg.printCount) as printCount
      , sum(agg.emailCount) as emailCount
      , sum(agg.accessCount) as accessTurnawayCount
      , sum(agg.concurrencyCount) as concurrentTurnawayCount
	  , agg.licenseType
	  , agg.resourceStatusId
from 
 (
  -- concurrencyCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
          , count(institutionId) as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
		  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv 
  where     turnawayTypeId = 20 and (institutionId > 0) and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by  institutionId, resourceId, ipAddressInteger, cast(contentViewTimestamp as date), licenseType, resourceStatusId

  union all
  
  -- accessCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0  as concurrencyCount, count(institutionId) as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv 
  where     turnawayTypeId = 21 and institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- sessionCount
  select    pv.institutionId, resourceId, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) as institutionResourceStatisticsDate
          , 0 as concurrencyCount, 0 as accessCount, count(distinct pv.sessionId) as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
		  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.PageView pv
    join    R2Reports_Temp.dbo.ContentView cv on pv.requestId = cv.requestId 
  where     pv.institutionId > 0 and pageViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by  pv.institutionId, resourceId, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- tocCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, count(institutionId) as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv
  where     institutionId > 0  and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     chapterSectionId IS NULL and actionTypeId = 0 and turnawayTypeId = 0
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- contentCount
  select    cv.institutionId, cv.resourceId, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, count(cv.institutionId) as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv 
  where     cv.institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     cv.chapterSectionId IS NOT NULL and cv.actionTypeId = 0 and turnawayTypeId = 0
  group by cv.institutionId, cv.resourceId, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- printCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, count(institutionId) as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv
  where     institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     actionTypeId = 16 and turnawayTypeId = 0
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- as emailCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, count(institutionId) as emailCount
			  , licenseType, resourceStatusId
  from      R2Reports_Temp.dbo.ContentView cv
  where     institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     actionTypeId = 17 and turnawayTypeId = 0
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  ) as agg 
group by institutionId, resourceId, ipAddressInteger, institutionResourceStatisticsDate, licenseType, resourceStatusId

alter table tSavedReports
add bIncludeTrialStats bit not null default((0));

alter table tSavedReports
add iReportType int not null default((0));

Update tSavedReports
set iReportType = 1
where vchType = 'AppUsage'

Update tSavedReports
set iReportType = 2
where vchType in ('ResUsage', 'ResCost')

alter table tSavedReports
alter column vchType varchar(50) null;