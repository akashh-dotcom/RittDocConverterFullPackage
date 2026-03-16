
ALTER TABLE [dbo].[DailyContentViewCount] ADD [actionTypeId] tinyint NULL
GO
ALTER TABLE [dbo].[DailyContentViewCount] ADD CONSTRAINT [D_dbo_DailyContentViewCount_1] DEFAULT 0 FOR [actionTypeId]
GO

update DailyContentViewCount set actionTypeId = 0 where actionTypeId is null


ALTER TABLE [dbo].[DailyContentViewCount]
DROP CONSTRAINT [UQ__DailyCon__3A9EAC9F3FC401E9]
GO
ALTER TABLE [dbo].[DailyContentViewCount] 
ADD  CONSTRAINT [UK_DailyContentViewCount_1]
UNIQUE NONCLUSTERED ([institutionId] ASC, [userId] ASC, [ipAddressOctetA] ASC, [ipAddressOctetB] ASC, [ipAddressOctetC] ASC, [ipAddressOctetD] ASC, [contentViewDate] ASC, [resourceId] ASC, [chapterSectionId] ASC, [actionTypeId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
GO

ALTER TABLE [dbo].[DailyContentViewCount]
DROP CONSTRAINT [UK_DailyContentViewCount_1]
GO
ALTER TABLE [dbo].[DailyContentViewCount] 
ADD  CONSTRAINT [UK_DailyContentViewCount_1]
UNIQUE NONCLUSTERED ([institutionId] ASC, [userId] ASC, [ipAddressOctetA] ASC, [ipAddressOctetB] ASC, [ipAddressOctetC] ASC, [ipAddressOctetD] ASC, [contentViewDate] ASC, [resourceId] ASC, [actionTypeId] ASC, [chapterSectionId] ASC)
WITH ( PAD_INDEX = OFF,
FILLFACTOR = 100,
IGNORE_DUP_KEY = OFF,
STATISTICS_NORECOMPUTE = OFF,
ALLOW_ROW_LOCKS = ON,
ALLOW_PAGE_LOCKS = ON,
DATA_COMPRESSION = NONE )
 ON [PRIMARY]
GO

insert into DailyContentViewCount (institutionId, userId, resourceId, chapterSectionId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD
        , ipAddressInteger, contentViewDate, contentViewCount, actionTypeId) 
    select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD
         , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*), actionTypeId
    from   ContentView cv
    where  cv.turnawayTypeId = 0
      and  cv.contentViewTimestamp > (select dateadd(dd, 1, max(contentViewDate)) from DailyContentViewCount)
      and  cv.contentViewTimestamp < '12/12/2012 00:00:00'
      --and  cv.institutionId = 1 and  cv.userId = 2907
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
         , cast(cv.contentViewTimestamp as date), actionTypeId
    order by cast(cv.contentViewTimestamp as date)

select * from 

-- 5301018
-- select max(dailyContentViewCountId) from DailyContentViewCount

select max(contentViewDate + 1) from DailyContentViewCount

select contentViewDate, dateadd(dd, 1, contentViewDate) from DailyContentViewCount where dailyContentViewCountId = 2629758

USE [RIT001_2012-08-22];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER VIEW [dbo].[vDailyContentViewCount]
AS
select dcvc.dailyContentViewCountId, dcvc.institutionId, dcvc.userId, dcvc.resourceId, dcvc.chapterSectionId, dcvc.ipAddressOctetA
     , dcvc.ipAddressOctetB, dcvc.ipAddressOctetC, dcvc.ipAddressOctetD, dcvc.ipAddressInteger, dcvc.contentViewDate
     , dcvc.contentViewCount, dcvc.actionTypeId
from   R2Reports.dbo.DailyContentViewCount dcvc
union
select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
     , cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId
from   R2Reports.dbo.ContentView cv
where  turnawayTypeId = 0
  and  contentViewTimestamp > '12/12/2012 00:00:00'
group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
     , cast(cv.contentViewTimestamp as date), cv.actionTypeId
GO




