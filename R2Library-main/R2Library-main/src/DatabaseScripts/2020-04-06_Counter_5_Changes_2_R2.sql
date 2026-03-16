

/****** Object:  View [dbo].[vDailyContentViewCount]    Script Date: 4/6/2020 11:13:16 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[vDailyContentViewCount] AS 
    select dcvc.dailyContentViewCountId, dcvc.institutionId, dcvc.userId, dcvc.resourceId, dcvc.chapterSectionId, dcvc.ipAddressOctetA 
         , dcvc.ipAddressOctetB, dcvc.ipAddressOctetC, dcvc.ipAddressOctetD, dcvc.ipAddressInteger, dcvc.contentViewDate 
         , dcvc.contentViewCount, dcvc.actionTypeId, dcvc.foundFromSearch, dcvc.licenseType, dcvc.resourceStatusId, dcvc.uniqueContentViewCount
    from   [DEV_R2Reports].dbo.DailyContentViewCount dcvc 
    union 
    select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
         , cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, cv.foundFromSearch, cv.licenseType, cv.resourceStatusId, count(distinct pv.sessionId)
    from   [DEV_R2Reports].dbo.ContentView cv 
	left join [DEV_R2Reports].dbo.PageView pv on pv.requestId = cv.requestId
    where  turnawayTypeId = 0 
      and  contentViewTimestamp > '10/01/2017 00:00:00' 
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger 
           , cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch, cv.licenseType, cv.resourceStatusId 

GO


