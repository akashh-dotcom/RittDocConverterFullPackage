----USE [Dev_R2Reports]
ALTER TABLE DailyContentViewCount
add [foundFromSearch] [bit] NOT NULL DEFAULT ((0));


----USE [Dev_Rit001]
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
 and  contentViewTimestamp > '12/12/2012 00:00:00'
       group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
       , cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch

--Need to run R2Utilities and UpdateContentViewWithPageView
--This will update the content view table with searches found so we can have history back from 8/18/2012


