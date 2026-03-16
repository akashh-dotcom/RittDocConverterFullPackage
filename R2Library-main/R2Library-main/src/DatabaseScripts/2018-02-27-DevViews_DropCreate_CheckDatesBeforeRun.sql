DROP VIEW [dbo].[vContentView]
GO

CREATE VIEW [dbo].[vContentView]
AS
select contentTurnawayId as [contentViewId], institutionId, userId, resourceId, chapterSectionId
, turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger
, contentViewTimestamp, actionTypeId, foundFromSearch, searchTerm, requestId, licenseType, resourceStatusId
from [DEV_R2Reports]..ContentView

GO

DROP VIEW [dbo].[vDailyContentTurnawayCount]
GO

CREATE VIEW [dbo].[vDailyContentTurnawayCount] AS 
    select dctc.dailyContentTurnawayCountId, dctc.institutionId, dctc.userId, dctc.resourceId, dctc.chapterSectionId 
         , dctc.turnawayTypeId, dctc.ipAddressOctetA, dctc.ipAddressOctetB, dctc.ipAddressOctetC, dctc.ipAddressOctetD 
         , dctc.ipAddressInteger, dctc.contentTurnawayDate, dctc.contentTurnawayCount 
    from   [DEV_R2Reports].dbo.DailyContentTurnawayCount dctc 
    union 
    select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId 
         , cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD 
         , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*) 
    from   [DEV_R2Reports].dbo.ContentView cv 
    where  turnawayTypeId <> 0 
      and  contentViewTimestamp > '10/01/2017 00:00:00' 
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId 
           , cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD 
           , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) 

GO

DROP VIEW [dbo].[vDailyContentViewCount]
GO

CREATE VIEW [dbo].[vDailyContentViewCount] AS 
    select dcvc.dailyContentViewCountId, dcvc.institutionId, dcvc.userId, dcvc.resourceId, dcvc.chapterSectionId, dcvc.ipAddressOctetA 
         , dcvc.ipAddressOctetB, dcvc.ipAddressOctetC, dcvc.ipAddressOctetD, dcvc.ipAddressInteger, dcvc.contentViewDate 
         , dcvc.contentViewCount, dcvc.actionTypeId, dcvc.foundFromSearch, dcvc.licenseType, dcvc.resourceStatusId 
    from   [DEV_R2Reports].dbo.DailyContentViewCount dcvc 
    union 
    select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
         , cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, cv.foundFromSearch, cv.licenseType, cv.resourceStatusId 
    from   [DEV_R2Reports].dbo.ContentView cv 
    where  turnawayTypeId = 0 
      and  contentViewTimestamp > '10/01/2017 00:00:00' 
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
           , cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch, cv.licenseType, cv.resourceStatusId 

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
from   [DEV_R2Reports].dbo.DailyInstitutionResourceStatisticsCount dirsc 

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
  from      [DEV_R2Reports].dbo.ContentView cv 
  where     turnawayTypeId = 20 and (institutionId > 0) and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by  institutionId, resourceId, ipAddressInteger, cast(contentViewTimestamp as date), licenseType, resourceStatusId

  union all
  
  -- accessCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0  as concurrencyCount, count(institutionId) as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      [DEV_R2Reports].dbo.ContentView cv 
  where     turnawayTypeId = 21 and institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- sessionCount
  select    pv.institutionId, resourceId, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) as institutionResourceStatisticsDate
          , 0 as concurrencyCount, 0 as accessCount, count(distinct pv.sessionId) as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
		  , licenseType, resourceStatusId
  from      [DEV_R2Reports].dbo.PageView pv
    join    [DEV_R2Reports].dbo.ContentView cv on pv.requestId = cv.requestId 
  where     pv.institutionId > 0 and pageViewTimestamp > cast(cast(getdate() as date) as datetime)
  group by  pv.institutionId, resourceId, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- tocCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, count(institutionId) as tocCount, 0 as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      [DEV_R2Reports].dbo.ContentView cv
  where     institutionId > 0  and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     chapterSectionId IS NULL and actionTypeId = 0 and turnawayTypeId = 0
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- contentCount
  select    cv.institutionId, cv.resourceId, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, count(cv.institutionId) as contentCount, 0 as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      [DEV_R2Reports].dbo.ContentView cv 
  where     cv.institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     cv.chapterSectionId IS NOT NULL and cv.actionTypeId = 0 and turnawayTypeId = 0
  group by cv.institutionId, cv.resourceId, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- printCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, count(institutionId) as printCount, 0 as emailCount
			  , licenseType, resourceStatusId
  from      [DEV_R2Reports].dbo.ContentView cv
  where     institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     actionTypeId = 16 and turnawayTypeId = 0
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  
  union all
  
  -- as emailCount
  select    institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date) as institutionResourceStatisticsDate
              , 0 as concurrencyCount, 0 as accessCount, 0 as sessionCount, 0 as tocCount, 0 as contentCount, 0 as printCount, count(institutionId) as emailCount
			  , licenseType, resourceStatusId
  from      [DEV_R2Reports].dbo.ContentView cv
  where     institutionId > 0 and contentViewTimestamp > cast(cast(getdate() as date) as datetime)
    and     actionTypeId = 17 and turnawayTypeId = 0
  group by institutionId, resourceId, ipAddressInteger, cast(cv.contentViewTimestamp as date), licenseType, resourceStatusId
  ) as agg 
group by institutionId, resourceId, ipAddressInteger, institutionResourceStatisticsDate, licenseType, resourceStatusId

GO

DROP VIEW [dbo].[vDailyPageViewCount]
GO

CREATE VIEW [dbo].[vDailyPageViewCount] AS 
    select dpvc.dailyPageViewCountId, dpvc.institutionId, dpvc.userId, dpvc.ipAddressOctetA, dpvc.ipAddressOctetB 
         , dpvc.ipAddressOctetC, dpvc.ipAddressOctetD, dpvc.ipAddressInteger, dpvc.pageViewDate, dpvc.pageViewCount 
    from   [DEV_R2Reports].dbo.DailyPageViewCount dpvc
    union 
    select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB 
         , pv.ipAddressOctetC, pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) as pageViewDate, count(*) 
    from   [DEV_R2Reports].dbo.PageView pv
    where  pageViewTimestamp > '10/01/2017 00:00:00' 
    group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
           , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date)

GO

DROP VIEW [dbo].[vDailyResourceSessionCount]
GO

CREATE VIEW [dbo].[vDailyResourceSessionCount] AS 
    select drsc.dailyResourceSessionCountId, drsc.institutionId, drsc.userId, drsc.ipAddressOctetA, drsc.ipAddressOctetB 
         , drsc.ipAddressOctetC, drsc.ipAddressOctetD, drsc.ipAddressInteger, drsc.sessionDate, drsc.sessionCount, drsc.resourceId 
    from   [DEV_R2Reports].dbo.DailyResourceSessionCount drsc 
    union 
    select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
         , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), count(distinct pv.sessionId), cv.resourceId 
    from   [DEV_R2Reports].dbo.PageView pv 
    join [DEV_R2Reports].dbo.ContentView cv on pv.requestId = cv.requestId 
    where  pageViewTimestamp > '10/01/2017 00:00:00' 
    group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
           , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), cv.resourceId 

GO

DROP VIEW [dbo].[vDailySearchCount]
GO

CREATE VIEW [dbo].[vDailySearchCount] AS 
    select dsc.dailySearchCountId, dsc.institutionId, dsc.userId, dsc.searchTypeId, dsc.isArchive, dsc.isExternal, dsc.ipAddressOctetA 
         , dsc.ipAddressOctetB, dsc.ipAddressOctetC, dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.searchDate, dsc.searchCount 
    from   [DEV_R2Reports].dbo.DailySearchCount dsc 
    union 
    select 0, s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA 
         , s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date) as searchDate, count(*) 
    from   [DEV_R2Reports].dbo.Search s
    where  s.searchTimestamp >  '10/01/2017 00:00:00' 
    group by s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA 
           , s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date) 

GO

DROP VIEW [dbo].[vDailySessionCount]
GO

CREATE VIEW [dbo].[vDailySessionCount] AS 
    select dsc.dailySessionCountId, dsc.institutionId, dsc.userId, dsc.ipAddressOctetA, dsc.ipAddressOctetB, dsc.ipAddressOctetC 
         , dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.sessionDate, dsc.sessionCount 
    from   [DEV_R2Reports].dbo.DailySessionCount dsc 
    union 
    select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
         , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), count(distinct pv.sessionId) 
    from   [DEV_R2Reports].dbo.PageView pv 
    where  pageViewTimestamp > '10/01/2017 00:00:00' 
    group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
           , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) 

GO

DROP VIEW [dbo].[vInstitutionResourceStatistics]
GO

CREATE VIEW [dbo].[vInstitutionResourceStatistics] AS 
SELECT institutionId
      , aggregationDate
      , resourceId
      , purchased
      , archivedPurchased
      , newEditionPreviousPurchased
      , pdaAdded
      , pdaAddedToCart
      , pdaNewEdition
      , expertRecommended
  FROM [DEV_R2Reports].dbo.InstitutionMonthlyResourceStatistics

GO

DROP VIEW [dbo].[vInstitutionStatistics]
GO

CREATE VIEW [dbo].[vInstitutionStatistics] AS 
SELECT institutionId
      , aggregationDate
      , mostAccessedResourceId
      , mostAccessedCount
      , leastAccessedResourceId
      , leastAccessedCount
      , mostTurnawayConcurrentResourceId
      , mostTurnawayConcurrentCount
      , mostTurnawayAccessResourceId
      , mostTurnawayAccessCount
      , mostPopularSpecialtyName
      , mostPopularSpecialtyCount
      , leastPopularSpecialtyName
      , leastPopularSpecialtyCount
      , totalResourceCount
      , contentCount
      , tocCount
      , sessionCount
      , printCount
      , emailCount
      , turnawayConcurrencyCount
      , turnawayAccessCount
  FROM [DEV_R2Reports].dbo.InstitutionMonthlyStatisticsCount

GO

DROP VIEW [dbo].[vPageContentView]
GO

CREATE VIEW [dbo].[vPageContentView]
AS
select pv.pageViewId, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC
     , pv.ipAddressOctetD, pv.ipAddressInteger, pv.pageViewTimestamp, pv.pageViewRunTime, pv.sessionId, pv.url
     , pv.requestId, pv.referrer, pv.countryCode, pv.serverNumber
     , cv.contentTurnawayId, cv.resourceId, cv.chapterSectionId, cv.turnawayTypeId, cv.actionTypeId
     , cv.foundFromSearch, cv.searchTerm
     , u.vchFirstName, u.vchLastName, u.vchUserName, u.vchUserEmail
from   DEV_R2Reports..PageView pv
 join  DEV_R2Reports..ContentView cv on cv.requestId = pv.requestId
 left outer join  tUser u on u.iUserId = pv.userId

GO

DROP VIEW [dbo].[vPageView]
GO

CREATE VIEW [dbo].[vPageView]
AS
select pv.pageViewId, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC
     , pv.ipAddressOctetD, pv.ipAddressInteger, pv.pageViewTimestamp, pv.pageViewRunTime, pv.sessionId, pv.url
     , pv.requestId, pv.referrer, pv.countryCode, pv.serverNumber
from   DEV_R2Reports..PageView pv

go 