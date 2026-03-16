
ALTER TABLE [dbo].[DailyContentViewCount] DROP CONSTRAINT [UK_DailyContentViewCount_1]
GO

/****** Object:  Index [UK_DailyContentViewCount_1]    Script Date: 8/26/2013 2:01:18 PM ******/
ALTER TABLE [dbo].[DailyContentViewCount] ADD  CONSTRAINT [UK_DailyContentViewCount_1] UNIQUE NONCLUSTERED 
(
	[institutionId] ASC,
	[userId] ASC,
	[ipAddressOctetA] ASC,
	[ipAddressOctetB] ASC,
	[ipAddressOctetC] ASC,
	[ipAddressOctetD] ASC,
	[contentViewDate] ASC,
	[resourceId] ASC,
	[actionTypeId] ASC,
	[chapterSectionId] ASC,
	[foundFromSearch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO


--select count(*) from ContentView where contentViewTimestamp between '8/1/2012' and '8/31/2012'
--select count(*) from PageView where pageViewTimestamp between '8/1/2012' and '8/31/2012'
--select count(*) from Search where searchTimestamp between '8/1/2012' and '8/31/2012'

--select count(*) from ContentView where contentViewTimestamp between '8/19/2012' and '8/31/2012'
--select count(*) from PageView where pageViewTimestamp between '8/19/2012' and '8/31/2012'
--select count(*) from Search where searchTimestamp between '8/19/2012' and '8/31/2012'
--
--select count(*) from ContentView where contentViewTimestamp between '9/1/2012' and '9/30/2012'
--select count(*) from PageView where pageViewTimestamp between '9/1/2012' and '9/30/2012'
--select count(*) from Search where searchTimestamp between '9/1/2012' and '9/30/2012'
--
--select count(*) from DailyContentTurnawayCount where contentTurnawayDate between '8/19/2012' and '8/31/2012'
--select count(*) from DailyContentViewCount where contentViewDate between '8/19/2012' and '8/31/2012'
--select count(*) from DailyPageViewCount where pageViewDate between '8/19/2012' and '8/31/2012'
--select count(*) from DailySearchCount where searchDate between '8/19/2012' and '8/31/2012'
--select count(*) from DailySessionCount where sessionDate between '8/19/2012' and '8/31/2012'
--
--select count(*) from DailyContentTurnawayCount where contentTurnawayDate between '9/1/2012' and '9/30/2012'
--select count(*) from DailyContentViewCount where contentViewDate between '9/1/2012' and '9/30/2012'
--select count(*) from DailyPageViewCount where pageViewDate between '9/1/2012' and '9/30/2012'
--select count(*) from DailySearchCount where searchDate between '9/1/2012' and '9/30/2012'
--select count(*) from DailySessionCount where sessionDate between '9/1/2012' and '9/30/2012'
--
--select count(*) from DailyContentTurnawayCount where contentTurnawayDate between '10/1/2012' and '10/31/2012'
--select count(*) from DailyContentViewCount where contentViewDate between '10/1/2012' and '10/31/2012'
--select count(*) from DailyPageViewCount where pageViewDate between '10/1/2012' and '10/31/2012'
--select count(*) from DailySearchCount where searchDate between '10/1/2012' and '10/31/2012'
--select count(*) from DailySessionCount where sessionDate between '10/1/2012' and '10/31/2012'
--
--select count(*) from DailyContentTurnawayCount where contentTurnawayDate between '11/1/2012' and '11/30/2012'
--select count(*) from DailyContentViewCount where contentViewDate between '11/1/2012' and '11/30/2012'
--select count(*) from DailyPageViewCount where pageViewDate between '11/1/2012' and '11/30/2012'
--select count(*) from DailySearchCount where searchDate between '11/1/2012' and '11/30/2012'
--select count(*) from DailySessionCount where sessionDate between '11/1/2012' and '11/30/2012'
--
--select count(*) from DailyContentTurnawayCount where contentTurnawayDate between '12/1/2012' and '12/31/2012'
--select count(*) from DailyContentViewCount where contentViewDate between '12/1/2012' and '12/31/2012'
--select count(*) from DailyPageViewCount where pageViewDate between '12/1/2012' and '12/31/2012'
--select count(*) from DailySearchCount where searchDate between '12/1/2012' and '12/31/2012'
--select count(*) from DailySessionCount where sessionDate between '12/1/2012' and '12/31/2012'

--Need to update to 1/1/2013 to match production and staging

--Update DailySearchCount to 1/1/2013
Insert Into [Dev_R2Reports]..DailySearchCount(institutionId, userId, searchTypeId, isArchive, isExternal, ipAddressOctetA,
ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, searchDate, searchCount)
select s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA
, s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date) as searchDate, count(*)
from   [Dev_R2Reports]..Search s
where   cast(s.searchTimestamp as date) between '8/19/2012' and '12/31/2012'
group by s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA
, s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date)

--Update DailySessionCount to 1/1/2013
Insert Into [DEV_R2Reports]..DailySessionCount (institutionId, userId, ipAddressOctetA, ipAddressOctetB,
ipAddressOctetC, ipAddressOctetD, ipAddressInteger, sessionDate, sessionCount)             
select pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
       , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), count(distinct pv.sessionId) 
 from   [DEV_R2Reports]..PageView pv 
 where  cast(pageViewTimestamp as date) between '8/19/2012' and '12/31/2012'
       group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
       , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) 
order by cast(pageViewTimestamp as date)

--Update DailyPageViewCount to 1/1/2013
Insert Into [DEV_R2Reports]..DailyPageViewCount (institutionId, userId, ipAddressOctetA, ipAddressOctetB, 
ipAddressOctetC, ipAddressOctetD, ipAddressInteger, pageViewDate, pageViewCount)
select pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB 
, pv.ipAddressOctetC, pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) as pageViewDate, count(*) 
from   [DEV_R2Reports]..PageView pv
where  cast(pageViewTimestamp as date) between '8/19/2012' and '12/31/2012'
group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
, pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date)

--Update DailyPageViewCount to 1/1/2013
Insert Into [DEV_R2Reports]..DailyContentViewCount(institutionId, userId, resourceId, chapterSectionId, 
ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewDate, contentViewCount, actionTypeId, foundFromSearch)
select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
, cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, cv.foundFromSearch
 from   [DEV_R2Reports].dbo.ContentView cv 
       where  turnawayTypeId = 0 
 and  cast(contentViewTimestamp as date) between '8/19/2012' and '12/31/2012'
       group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
       , cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch

--Update DailyContentTurnawayCount to 1/1/2013	   
Insert Into [DEV_R2Reports]..DailyContentTurnawayCount(institutionId, userId, resourceId, chapterSectionId, turnawayTypeId, 
ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentTurnawayDate, contentTurnawayCount) 
select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId
, cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD
, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*)
from   [DEV_R2Reports]..ContentView cv 
where  turnawayTypeId <> 0
and  cast(contentViewTimestamp as date) between '8/19/2012' and '12/31/2012'
group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId
, cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD
, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date)
order by cast(contentViewTimestamp as date)


----Update DailyContentViewCount to 1/1/2013 ****DEV ONLY****
--Insert Into [DEV_R2Reports]..DailyContentViewCount(institutionId, userId, resourceId, chapterSectionId, 
--ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewDate, contentViewCount, actionTypeId, foundFromSearch)
-- select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
--        , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, cv.foundFromSearch
--from   [DEV_R2Reports]..ContentView cv
--where  turnawayTypeId = 0
--and  cast(contentViewTimestamp as date) between '8/19/2012' and '12/31/2012'
--group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
--    , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch
	
	
----Update DailyContentViewCount to 1/1/2013 ****STAGE AND PROD ONLY****
--Insert Into [DEV_R2Reports]..DailyContentViewCount(institutionId, userId, resourceId, chapterSectionId, 
--ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewDate, contentViewCount, actionTypeId, foundFromSearch)
-- select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
--        , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, cv.foundFromSearch
--from   [DEV_R2Reports]..ContentView cv
--where  turnawayTypeId = 0
--and  cast(contentViewTimestamp as date) between '12/12/2012' and '12/31/2012'
--group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
--    , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch	
	
	
	
	
	
ALTER VIEW [dbo].[vDailyContentTurnawayCount] AS 
    select dctc.dailyContentTurnawayCountId, dctc.institutionId, dctc.userId, dctc.resourceId, dctc.chapterSectionId, 
    dctc.turnawayTypeId, dctc.ipAddressOctetA, dctc.ipAddressOctetB, dctc.ipAddressOctetC, dctc.ipAddressOctetD, 
    dctc.ipAddressInteger, dctc.contentTurnawayDate, dctc.contentTurnawayCount 
from   [DEV_R2Reports]..DailyContentTurnawayCount dctc 
    union 
    select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId 
    , cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD 
    , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*) 
from   DEV_R2Reports.dbo.ContentView cv 
    where  turnawayTypeId <> 0 
and  contentViewTimestamp > '1/1/2013 00:00:00' 
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId 
    , cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD 
    , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date);

 ALTER VIEW [dbo].[vDailyContentViewCount] AS 
       select dcvc.dailyContentViewCountId, dcvc.institutionId, dcvc.userId, dcvc.resourceId, dcvc.chapterSectionId, dcvc.ipAddressOctetA 
       , dcvc.ipAddressOctetB, dcvc.ipAddressOctetC, dcvc.ipAddressOctetD, dcvc.ipAddressInteger, dcvc.contentViewDate 
       , dcvc.contentViewCount, dcvc.actionTypeId, dcvc.foundFromSearch
 from   [DEV_R2Reports]..DailyContentViewCount dcvc 
       union 
       select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
       , cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, cv.foundFromSearch
 from   [DEV_R2Reports].dbo.ContentView cv 
       where  turnawayTypeId = 0 
 and  contentViewTimestamp > '1/1/2013 00:00:00' 
       group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
       , cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch;

 ALTER VIEW [dbo].[vDailyPageViewCount] AS 
       select dpvc.dailyPageViewCountId, dpvc.institutionId, dpvc.userId, dpvc.ipAddressOctetA, dpvc.ipAddressOctetB, 
       dpvc.ipAddressOctetC, dpvc.ipAddressOctetD, dpvc.ipAddressInteger, dpvc.pageViewDate, dpvc.pageViewCount 
 from   [DEV_R2Reports]..DailyPageViewCount dpvc
       union 
       select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB 
       , pv.ipAddressOctetC, pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) as pageViewDate, count(*) 
 from   [DEV_R2Reports]..PageView pv
 where  pageViewTimestamp > '1/1/2013 00:00:00' 
       group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
       , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date);

 ALTER VIEW [dbo].[vDailySearchCount] AS 
       select dsc.dailySearchCountId, dsc.institutionId, dsc.userId, dsc.searchTypeId, dsc.isArchive, dsc.isExternal, dsc.ipAddressOctetA, 
       dsc.ipAddressOctetB, dsc.ipAddressOctetC, dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.searchDate, dsc.searchCount 
 from   [DEV_R2Reports]..DailySearchCount dsc 
       union 
       select 0, s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA 
       , s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date) as searchDate, count(*) 
 from   [DEV_R2Reports]..Search s
 where  s.searchTimestamp >  '1/1/2013 00:00:00' 
       group by s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA 
       , s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date) ;

 ALTER VIEW [dbo].[vDailySessionCount] AS 
       select dsc.dailySessionCountId, dsc.institutionId, dsc.userId, dsc.ipAddressOctetA, dsc.ipAddressOctetB, dsc.ipAddressOctetC, 
       dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.sessionDate, dsc.sessionCount 
 from   [DEV_R2Reports]..DailySessionCount dsc 
       union 
       select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
       , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date), count(distinct pv.sessionId) 
 from   [DEV_R2Reports]..PageView pv 
 where  pageViewTimestamp > '1/1/2013 00:00:00' 
       group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC 
       , pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) ;	
	
	
--use [DEV_RIT001]
GRANT ALTER 	ON 	[dbo].[vDailyContentViewCountTest] 	TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailyContentViewCountTest] 	TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailyContentTurnawayCount] 	TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailyContentTurnawayCount] 	TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailyPageViewCount] 		TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailyPageViewCount] 		TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailySessionCount] 			TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailySessionCount] 			TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailySearchCount] 			TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailySearchCount] 			TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailyContentViewCount] 		TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailyContentViewCount] 		TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailyContentTurnawayCount2] TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailyContentTurnawayCount2] TO [R2UtilitiesUser];
GRANT ALTER 	ON 	[dbo].[vDailyContentViewCountTemp] 	TO [R2UtilitiesUser];
GRANT SELECT 	ON 	[dbo].[vDailyContentViewCountTemp] 	TO [R2UtilitiesUser];



delete from ContentView where contentViewTimestamp between '8/1/2012' and '8/31/2012'
delete from PageView where pageViewTimestamp between '8/1/2012' and '8/31/2012'
delete from Search where searchTimestamp between '8/1/2012' and '8/31/2012'


select * from PageView where pageViewTimestamp between '1/1/2013' and '3/31/2013'


