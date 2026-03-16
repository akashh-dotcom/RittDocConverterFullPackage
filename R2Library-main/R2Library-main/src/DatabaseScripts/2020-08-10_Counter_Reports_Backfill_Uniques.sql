




UPDATE dcvc
SET uniqueContentViewCount = cv.uniqueContentViewCount
FROM DEV_R2Reports..DailyContentViewCount dcvc
INNER JOIN (
    select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
        , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as contentViewDate, count(*) contentViewCount, cv.actionTypeId
		, cv.foundFromSearch, cv.licenseType, isnull(cv.resourceStatusId, 0) resourceStatusId, count(distinct pv.sessionId) uniqueContentViewCount
    from   [DEV_R2Reports]..ContentView cv
    left join   [DEV_R2Reports]..PageView pv on pv.requestId = cv.requestId
	where cv.turnawayTypeId = 0
	group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
           , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch, cv.licenseType, 
		   isnull(cv.resourceStatusId, 0)
) cv on (dcvc.institutionId = cv.institutionId or (dcvc.institutionId IS NULL AND cv.institutionId IS NULL))
	  and (dcvc.userId = cv.userId or (dcvc.userId IS NULL AND cv.userId IS NULL))
	  and (dcvc.resourceId = cv.resourceId or (dcvc.resourceId IS NULL AND cv.resourceId IS NULL))
	  and (dcvc.chapterSectionId = cv.chapterSectionId or (dcvc.chapterSectionId IS NULL AND cv.chapterSectionId IS NULL))
	  and (dcvc.ipAddressOctetA = cv.ipAddressOctetA)
	  and (dcvc.ipAddressOctetB = cv.ipAddressOctetB)
	  and (dcvc.ipAddressOctetC = cv.ipAddressOctetC)
	  and (dcvc.ipAddressOctetD = cv.ipAddressOctetD)
	  and (dcvc.ipAddressInteger = cv.ipAddressInteger)
	  and (dcvc.contentViewDate = cv.contentViewDate)
	  and (dcvc.contentViewCount = cv.contentViewCount or (dcvc.contentViewCount IS NULL AND cv.contentViewCount IS NULL))
	  and (dcvc.actionTypeId = cv.actionTypeId or (dcvc.actionTypeId IS NULL AND cv.actionTypeId IS NULL))
	  and (dcvc.foundFromSearch = cv.foundFromSearch)
	  and (dcvc.licenseType = cv.licenseType)
	  and (dcvc.resourceStatusId = cv.resourceStatusId or (dcvc.resourceStatusId IS NULL AND cv.resourceStatusId IS NULL))
WHERE dcvc.uniqueContentViewCount IS NULL




/*
--Check numbers
SELECT cv.uniqueContentViewCount, dcvc.uniqueContentViewCount, *
FROM (
    select cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
        , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as contentViewDate, count(*) contentViewCount, cv.actionTypeId
		, cv.foundFromSearch, cv.licenseType, isnull(cv.resourceStatusId, 0) resourceStatusId, count(distinct pv.sessionId) uniqueContentViewCount
    from   [DEV_R2Reports]..ContentView cv
    left join   [DEV_R2Reports]..PageView pv on pv.requestId = cv.requestId
	where cv.turnawayTypeId = 0
	group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC 
           , cv.ipAddressOctetD, cv.ipAddressInteger, cast(cv.contentViewTimestamp as date), cv.actionTypeId, cv.foundFromSearch, cv.licenseType, 
		   isnull(cv.resourceStatusId, 0)
) cv
inner join  DEV_R2Reports..DailyContentViewCount dcvc
	  on (dcvc.institutionId = cv.institutionId or (dcvc.institutionId IS NULL AND cv.institutionId IS NULL))
	  and (dcvc.userId = cv.userId or (dcvc.userId IS NULL AND cv.userId IS NULL))
	  and (dcvc.resourceId = cv.resourceId or (dcvc.resourceId IS NULL AND cv.resourceId IS NULL))
	  and (dcvc.chapterSectionId = cv.chapterSectionId or (dcvc.chapterSectionId IS NULL AND cv.chapterSectionId IS NULL))
	  and (dcvc.ipAddressOctetA = cv.ipAddressOctetA)
	  and (dcvc.ipAddressOctetB = cv.ipAddressOctetB)
	  and (dcvc.ipAddressOctetC = cv.ipAddressOctetC)
	  and (dcvc.ipAddressOctetD = cv.ipAddressOctetD)
	  and (dcvc.ipAddressInteger = cv.ipAddressInteger)
	  and (dcvc.contentViewDate = cv.contentViewDate)
	  and (dcvc.contentViewCount = cv.contentViewCount or (dcvc.contentViewCount IS NULL AND cv.contentViewCount IS NULL))
	  and (dcvc.actionTypeId = cv.actionTypeId or (dcvc.actionTypeId IS NULL AND cv.actionTypeId IS NULL))
	  and (dcvc.foundFromSearch = cv.foundFromSearch)
	  and (dcvc.licenseType = cv.licenseType)
	  and (dcvc.resourceStatusId = cv.resourceStatusId or (dcvc.resourceStatusId IS NULL AND cv.resourceStatusId IS NULL))
--WHERE dcvc.uniqueContentViewCount != cv.uniqueContentViewCount AND dcvc.uniqueContentViewCount IS NOT NULL AND cv.uniqueContentViewCount IS NOT NULL
*/