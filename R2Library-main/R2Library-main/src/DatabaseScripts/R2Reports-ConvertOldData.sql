
truncate table DailyPageViewCount
truncate table DailySessionCount
truncate table DailyContentViewCount
truncate table DailyContentTurnawayCount
truncate table DailySearchCount

------------------
-- PAGE VIEW DATA
------------------
insert into DailyPageViewCount (institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger , pageViewDate, pageViewCount)
    select iInstitutionId, iUserId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetA
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetB
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int) as iOctetC
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int) as iOctetD
         , iPageViewId * 0
         , cast(dtCreationDate as date) as hitDate
         , count(*) as hitCount
    from   [RIT001_2012-03-21]..tPageViews
    --where  dtCreationDate < '1/1/2010'
    --where  dtCreationDate >= '1/1/2010' and  dtCreationDate < '1/1/2011'
    --where  dtCreationDate >= '1/1/2011' and  dtCreationDate < '1/1/2012'
    --where  dtCreationDate >= '1/1/2012'
    --   and  vchClientIPAddress is not null
    where  vchClientIPAddress is not null
       and vchUrl not like '%public/ping.aspx' 
       and vchUrl not like 'http://192%' 
    group by iInstitutionId, iUserId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int)
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int)
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int)
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int)
         , iPageViewId * 0
         , cast(dtCreationDate as date)
    order by cast(dtCreationDate as date), iInstitutionId, iUserId, count(*)
  
update DailyPageViewCount
set    ipAddressInteger = cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                        + cast((ipAddressOctetB * 256 * 256) as bigint) 
                        + cast((ipAddressOctetC * 256) as bigint) 
                        + cast(ipAddressOctetD as bigint)
  
------------------
-- SESSION DATA
------------------
insert into DailySessionCount(institutionId, userId, ipAddressOctetA, ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, sessionDate, sessionCount)
    select s.iInstitutionId, s.iUserId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetA
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetB
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int) as iOctetC
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int) as iOctetD
         , s.iApplicationSessionId * 0
         , cast(dtSessionStartTime as date) as hitDate
         , count(*) as hitCount         
    from   [RIT001_2012-03-21]..tApplicationSession s 
    where  s.tiRecordStatus = 1  
      and  vchClientIPAddress is not null
--      and  s.dtCreationDate >= '01/01/2009'
--      and  s.dtCreationDate <= '01/31/2009 23:59:59.997'
    group by s.iInstitutionId, s.iUserId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int) 
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) 
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int)
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int)
         , s.iApplicationSessionId * 0
         , cast(dtSessionStartTime as date) 
    order by cast(dtSessionStartTime as date), iInstitutionId, iUserId, count(*)

update DailySessionCount
set    ipAddressInteger = cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                        + cast((ipAddressOctetB * 256 * 256) as bigint) 
                        + cast((ipAddressOctetC * 256) as bigint) 
                        + cast(ipAddressOctetD as bigint)

------------------
-- CONTENT DATA
------------------
insert into DailyContentViewCount(institutionId, userId, resourceId, chapterSectionId, ipAddressOctetA, ipAddressOctetB,
              ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentViewDate, contentViewCount)
    select cr.iInstitutionId, cr.iUserId, cr.iResourceId, cr.vchChapterSectionId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetA
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetB
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int) as iOctetC
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int) as iOctetD
         , cr.iContentRetrievalId * 0
         , cast(dtCreationDate as date) as hitDate
         , count(*)
    from  [RIT001_2012-03-21]..tContentRetrieval cr
    where  cr.tiRecordStatus = 1
      and  vchClientIPAddress is not null
--      and  cr.dtCreationDate >= '01/30/2012'
--      and  cr.dtCreationDate < '01/31/2012 23:59:59.997'
    --where  vchResourceChapterId is not null
    group by cr.iInstitutionId, cr.iUserId, cr.iResourceId, cr.vchChapterSectionId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int)
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) 
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int)
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int)
         , cr.iContentRetrievalId * 0
         , cast(dtCreationDate as date)
    order by cast(dtCreationDate as date), iInstitutionId, iUserId, cr.iResourceId, cr.vchChapterSectionId, count(*)

update DailyContentViewCount
set    ipAddressInteger = cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                        + cast((ipAddressOctetB * 256 * 256) as bigint) 
                        + cast((ipAddressOctetC * 256) as bigint) 
                        + cast(ipAddressOctetD as bigint)

------------------
-- TURNAWAY DATA
------------------
insert into DailyContentTurnawayCount(institutionId, userId, resourceId, chapterSectionId, turnawayTypeId, ipAddressOctetA,
              ipAddressOctetB, ipAddressOctetC, ipAddressOctetD, ipAddressInteger, contentTurnawayDate, contentTurnawayCount)
    select ct.iInstitutionId, ct.iUserId, ct.iResourceId, ct.vchChapterSectionId, ct.iTurnawayTypeId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetA
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetB
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int) as iOctetC
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int) as iOctetD
         , ct.iContentTurnawayId * 0
         , cast(dtCreationDate as date) as hitDate
         , count(*)
    from  [RIT001_2012-03-21].dbo.tContentTurnaway ct
    where  ct.tiRecordStatus = 1
      and  ct.vchClientIPAddress is not null
--      and  ct.dtCreationDate >= '01/30/2012'
--      and  ct.dtCreationDate < '01/31/2012 23:59:59.997'
    group by ct.iInstitutionId, ct.iUserId, ct.iResourceId, ct.vchChapterSectionId, ct.iTurnawayTypeId
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int)
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) 
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int)
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int)
         , ct.iContentTurnawayId * 0
         , cast(dtCreationDate as date)
    order by cast(dtCreationDate as date), iInstitutionId, iUserId, iResourceId, vchChapterSectionId, iTurnawayTypeId, count(*)

update DailyContentTurnawayCount
set    ipAddressInteger = cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                        + cast((ipAddressOctetB * 256 * 256) as bigint) 
                        + cast((ipAddressOctetC * 256) as bigint) 
                        + cast(ipAddressOctetD as bigint)
             

------------------
-- SEARCH DATA
------------------
insert into DailySearchCount(institutionId, userId, searchTypeId, isArchive, isExternal, ipAddressOctetA, ipAddressOctetB,
              ipAddressOctetC, ipAddressOctetD, ipAddressInteger, searchDate, searchCount)
    select s.iInstitutionId, s.iUserId, s.iSearchTypeId, cast(s.tiArchiveSearch as bit), cast(s.tiExternalSearch as bit)
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetA
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) as iOctetB
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int) as iOctetC
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int) as iOctetD
         , s.iSearchId * 0
         , cast(dtCreationDate as date) as hitDate
         , count(*)
    from  [RIT001_2012-03-21].dbo.tSearch s
    where  s.tiRecordStatus = 1
      and  s.vchClientIPAddress is not null
--      and  s.dtCreationDate >= '01/30/2012'
--      and  s.dtCreationDate < '01/31/2012 23:59:59.997'
    group by s.iInstitutionId, s.iUserId, s.iSearchTypeId, cast(s.tiArchiveSearch as bit), cast(s.tiExternalSearch as bit)
         , cast(left(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) - 1) as int)
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - charindex('.', vchClientIPAddress, 0) - 1) as int) 
         , cast(substring(vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) + 1) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 0) + 1) - 1) as int)
         , cast(right(vchClientIPAddress, len(vchClientIPAddress) - charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, charindex('.', vchClientIPAddress, 1) + 1 + 1) + 1)) as int)
         , s.iSearchId * 0
         , cast(dtCreationDate as date)
    order by cast(dtCreationDate as date), iInstitutionId, iUserId, s.iSearchTypeId, cast(s.tiArchiveSearch as bit), cast(s.tiExternalSearch as bit), count(*)
              
update DailySearchCount
set    ipAddressInteger = cast((cast(ipAddressOctetA as bigint) * 256 * 256 * 256) as bigint)
                        + cast((ipAddressOctetB * 256 * 256) as bigint) 
                        + cast((ipAddressOctetC * 256) as bigint) 
                        + cast(ipAddressOctetD as bigint)
                           