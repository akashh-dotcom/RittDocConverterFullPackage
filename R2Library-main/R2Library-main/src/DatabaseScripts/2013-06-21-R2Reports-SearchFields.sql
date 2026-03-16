
select * from ContentView

select * from ContentView

select * from DEV_R2Reports.dbo.ContentView
where contentViewTimestamp < '12/1/2012'

select * from DEV_R2Reports..ContentViewTemp

truncate table DEV_R2Reports..ContentViewTemp


SET IDENTITY_INSERT DEV_R2Reports..ContentView ON

insert into DEV_R2Reports..ContentView (contentTurnawayId, institutionId, userId, resourceId, chapterSectionId
    , turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewTimestamp
    , actionTypeId, foundFromSearch, searchTerm)
    select contentTurnawayId, institutionId, userId, resourceId, chapterSectionId
         , turnawayTypeId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewTimestamp
         , actionTypeId, 0, null
    from   DEV_R2Reports..ContentViewTemp
    order by contentTurnawayId

SET IDENTITY_INSERT DEV_R2Reports..ContentView OFF

truncate table DailyContentViewCountTemp

insert into DailyContentViewCountTemp (institutionId, userId, resourceId, chapterSectionId
    , ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewDate
        , contentViewCount, actionTypeId, foundFromSearch) 
    select vc.institutionId, vc.userId, vc.resourceId, vc.chapterSectionId, vc.ipAddressOctetA, vc.ipAddressOctetB, vc.ipAddressOctetC, vc.ipAddressOctetD, vc.ipAddressInteger, vc.contentViewDate, vc.contentViewCount, vc.actionTypeId
         , 0
    from   DailyContentViewCount vc
    where  contentViewDate < '8/17/2012'
    order by vc.contentViewDate, institutionId, userId, resourceId, chapterSectionId

insert into DailyContentViewCountTemp (institutionId, userId, resourceId, chapterSectionId
    , ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewDate
        , contentViewCount, actionTypeId, foundFromSearch) 
    select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
         , cast(cv.contentViewTimestamp as date) as hitDate, count(*), cv.actionTypeId, foundFromSearch
    from   [DEV_R2Reports].dbo.ContentView cv
    where  cv.turnawayTypeId = 0 and cv.institutionId > 0
--        and institutionId = 66
--      and  contentViewTimestamp > '8/17/2012 00:00:00'  and  contentViewTimestamp < '6/1/2013 00:00:00'
--      and  contentViewTimestamp > '8/17/2012 00:00:00'  and  contentViewTimestamp < '10/1/2012 00:00:00'
--      and  contentViewTimestamp > '10/1/2012 00:00:00'  and  contentViewTimestamp < '11/1/2012 00:00:00'
--      and  contentViewTimestamp > '11/1/2012 00:00:00'  and  contentViewTimestamp < '4/1/2013 00:00:00'
--      and  contentViewTimestamp > '4/1/2013 00:00:00'  and  contentViewTimestamp < '6/1/2013 00:00:00'
      and  contentViewTimestamp > '8/17/2012 00:00:00'  and  contentViewTimestamp < '6/1/2013 00:00:00'
    group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD, cv.ipAddressInteger
         , cast(cv.contentViewTimestamp as date), cv.actionTypeId, foundFromSearch
    order by cast(cv.contentViewTimestamp as date), institutionId, userId, resourceId, chapterSectionId

select * from DailyContentViewCountTemp where foundFromSearch = 1

select * from [DEV_R2Reports].dbo.ContentView cv where foundFromSearch = 1

select * from [DEV_R2Reports].dbo.ContentView where institutionId = 0

select * from ContentView cv where institutionId = 66 and  contentViewTimestamp > '10/31/2012 00:00:00'  and  contentViewTimestamp < '11/1/2012 00:00:00' 

select * from ContentView cv where institutionId = 66 and  contentViewTimestamp > '1/31/2013 00:00:00'  and  contentViewTimestamp < '4/1/2013 00:00:00' 

update ContentView 
set    ipAddressInteger = (cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((ipAddressOctetB * 256 * 256) as bigint) 
                     + cast((ipAddressOctetC * 256) as bigint) 
                     + cast(ipAddressOctetD as bigint))
where  ipAddressInteger <> (cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                     + cast((ipAddressOctetB * 256 * 256) as bigint) 
                     + cast((ipAddressOctetC * 256) as bigint) 
                     + cast(ipAddressOctetD as bigint))

EXEC sp_MSforeachtable @command1="print '?' DBCC DBREINDEX ('?', ' ', 80)"
GO
EXEC sp_updatestats
GO

select * from DEV_RIT001.dbo.vDailyContentViewCountTemp where foundFromSearch = 1





