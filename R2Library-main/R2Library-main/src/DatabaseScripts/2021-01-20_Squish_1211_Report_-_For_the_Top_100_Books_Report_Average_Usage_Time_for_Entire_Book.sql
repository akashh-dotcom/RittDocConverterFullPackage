

DECLARE @StartDate datetime = '12/30/2020 12:00:01 AM'
DECLARE @EndDate datetime = '1/29/2021 11:59:59 PM'
DECLARE @sessionTimeout time = '00:05:00'

USE RIT001

DECLARE @Resources TABLE(rowRank int, isbn varchar(15), title varchar(255), retrievals int)
INSERT INTO @Resources
 select TOP 100
 ROW_NUMBER() OVER(ORDER BY ContentRetrievalCount DESC, LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(r.vchResourceTitle, CHAR(10), CHAR(32)),CHAR(13), CHAR(32)),CHAR(160), CHAR(32)),CHAR(9),CHAR(32))))),
  r.vchIsbn10,
 LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(r.vchResourceTitle, CHAR(10), CHAR(32)),CHAR(13), CHAR(32)),CHAR(160), CHAR(32)),CHAR(9),CHAR(32)))),
 ContentRetrievalCount
 from ( select ResourceId     , sum(contentRetrievalCount) as ContentRetrievalCount     , sum(tocRetrievalCount) as TocRetrievalCount     , sum(sessionCount) as SessionCount     , sum(printCount) as PrintCount     , sum(emailCount) as EmailCount     , sum(accessTurnawayCount) as AccessTurnawayCount     , sum(concurrentTurnawayCount) as ConcurrentTurnawayCount     , case when max(case when FirstPurchaseDate is null then 0 else 1 end ) = 1         then min(FirstPurchaseDate) end as FirstPurchaseDate     , case when max(case when PdaAddedToCartDate is null then 0 else 1 end ) = 1         then min(PdaAddedToCartDate) end as PdaAddedToCartDate     , case when max(case when PdaCreatedDate is null then 0 else 1 end ) = 1         then min(PdaCreatedDate) end as PdaCreatedDate     , max(OriginalSource) as OriginalSource     , sum(LicenseCount) as LicenseCount     , sum(PdaViews) as PdaViews     from (     select dirsc.resourceId         , sum(contentRetrievalCount) as contentRetrievalCount         , sum(tocRetrievalCount) as tocRetrievalCount         , sum(sessionCount) as sessionCount         , sum(printCount) as printCount         , sum(emailCount) as emailCount         , sum(accessTurnawayCount) as accessTurnawayCount         , sum(concurrentTurnawayCount) as concurrentTurnawayCount         , null as FirstPurchaseDate         , null as PdaAddedToCartDate         , null as PdaCreatedDate         , 0 as OriginalSource         , 0 as LicenseCount         , 0 as PdaViews     from vDailyInstitutionResourceStatisticsCount dirsc         join tResource r on r.iResourceId = dirsc.resourceId and r.tiRecordStatus = 1 and r.iResourceStatusId <> 72         left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and irl.iInstitutionId = dirsc.institutionId and irl.tiRecordStatus = 1     where dirsc.institutionResourceStatisticsDate between @StartDate and @EndDate        /*and dirsc.institutionId = 1*/  and dirsc.licenseType <> 2         and irl.tiLicenseTypeId in (1,3)     group by dirsc.resourceId union     select r.iResourceId as resourceId         , 0 as contentRetrievalCount         , 0 as tocRetrievalCount         , 0 as sessionCount         , 0 as printCount         , 0 as emailCount         , 0 as accessTurnawayCount         , 0 as concurrentTurnawayCount         , min(irl.dtFirstPurchaseDate) as FirstPurchaseDate         , min(irl.dtPdaAddedToCartDate) as PdaAddedToCartDate         , min(irl.dtPdaAddedDate) as PdaCreatedDate         , max(irl.tiLicenseOriginalSourceId) as OriginalSource         , sum(irl.iLicenseCount) as LicenseCount         , isnull(sum(irl.iPdaViewCount), 0) as PdaViews     from tResource r         join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and irl.tiRecordStatus = 1     where r.tiRecordStatus = 1 and r.iResourceStatusId <> 72         /*and irl.iInstitutionId = 1*/         and irl.tiLicenseTypeId in (1,3)     group by r.iResourceId ) r  group by r.resourceId  ) rr 
 LEFT JOIN tResource r
	ON r.iResourceId = rr.resourceId
 ORDER BY ContentRetrievalCount DESC, LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(r.vchResourceTitle, CHAR(10), CHAR(32)),CHAR(13), CHAR(32)),CHAR(160), CHAR(32)),CHAR(9),CHAR(32))))



 /*
 SELECT *
FROM @Resources
*/



 USE R2Reports



SELECT t.[Rank by Usage], t.Isbn, t.Title, t.[Successful Content Retrievals], t.[Session Count],
	CONVERT(DECIMAL(10,2), t.[Successful Content Retrievals] * 1.0 / t.[Session Count]) 'Average Session Content Retrievals',
	t.[Average Session Usage Time]/*,
	s.[Average Usage Time (not separated by kickouts)]*/
FROM (
	SELECT r.rowRank 'Rank by Usage', r.Isbn, r.Title, r.retrievals 'Successful Content Retrievals',
		COUNT(DISTINCT sessionId) 'Session Count',
		CONVERT(varchar, DATEADD(s, AVG(sessionResourceTimeInSeconds), 0), 114) 'Average Session Usage Time'
	FROM @Resources r
	LEFT JOIN (
		SELECT sessionId, isbn, SUM(DATEDIFF(s, '0:00:00', viewTime)) sessionResourceTimeInSeconds
		FROM (
			SELECT sessionId, pageViewTimestamp, t.isbn, leadTime, CASE WHEN leadTime > @sessionTimeout THEN @sessionTimeout ELSE leadTime END viewTime, isStart, isEnd, readspanId, [url], referrer
			FROM (
				SELECT sessionId, pageViewTimestamp, isbn,
					COALESCE(CAST(DATEADD(s, DATEDIFF(s, pageViewTimestamp, LEAD(pageViewTimestamp,1) OVER(PARTITION BY sessionId ORDER BY pageViewTimestamp)), 0) as time), @sessionTimeout) leadTime, isStart,
					LEAD(isStart,1) OVER(PARTITION BY sessionId ORDER BY pageViewTimestamp) isEnd, 
					SUM(isStart) OVER(PARTITION BY sessionId ORDER BY pageViewTimestamp) readspanId, 
					[url], referrer
				FROM (
					SELECT RTRIM(LTRIM(
						CASE WHEN CHARINDEX('/', new) > 0 THEN SUBSTRING(new, 0, CHARINDEX('/', new)) 
						WHEN CHARINDEX('#', new) > 0 THEN SUBSTRING(new, 0, CHARINDEX('#', new)) 
						ELSE new END)) isbn, CASE WHEN (referrer NOT LIKE '%/resource/detail/%' AND referrer NOT LIKE '%/resource/title/%') OR [url] LIKE '%/resource/title/%' THEN 1 ELSE 0 END isStart, *
					FROM (
						SELECT REPLACE(REPLACE([url], '/resource/detail/', ''), '/resource/title/', '') new, *
						FROM [PageView]
						WHERE pageViewTimestamp BETWEEN @StartDate and @EndDate
					) t
				) s
				--WHERE isbn = '1451192134'
				--ORDER BY sessionId, pageViewTimestamp
			) t 
			INNER JOIN @Resources r
				ON r.isbn = t.isbn
			WHERE [url] LIKE '%/resource/detail/%'
				--AND r.isbn = '0763761834'
			--ORDER BY sessionId, pageViewTimestamp
		) t
		GROUP BY sessionId, isbn, readspanId
		--ORDER BY sessionResourceTimeInSeconds DESC
	) t
		ON r.isbn = t.isbn
	GROUP BY r.isbn, r.rowRank, r.title, r.retrievals
) t /*
LEFT JOIN (
	SELECT r.rowRank 'Rank by Usage', r.Isbn, r.Title, r.retrievals 'Successful Content Retrievals',
		COUNT(DISTINCT sessionId) 'Session Count',
		CONVERT(varchar, DATEADD(s, AVG(sessionResourceTimeInSeconds), 0), 114) 'Average Usage Time (not separated by kickouts)'
	FROM @Resources r
	LEFT JOIN (
		SELECT sessionId, isbn, SUM(DATEDIFF(s, '0:00:00', viewTime)) sessionResourceTimeInSeconds
		FROM (
			SELECT sessionId, pageViewTimestamp, t.isbn, leadTime, CASE WHEN leadTime > @sessionTimeout THEN @sessionTimeout ELSE leadTime END viewTime, isStart, isEnd, [url], referrer
			FROM (
				SELECT sessionId, pageViewTimestamp, isbn,
					COALESCE(CAST(DATEADD(s, DATEDIFF(s, pageViewTimestamp, LEAD(pageViewTimestamp,1) OVER(PARTITION BY sessionId ORDER BY pageViewTimestamp)), 0) as time), @sessionTimeout) leadTime, isStart,
					LEAD(isStart,1) OVER(PARTITION BY sessionId ORDER BY pageViewTimestamp) isEnd,
					[url], referrer
				FROM (
					SELECT RTRIM(LTRIM(
						CASE WHEN CHARINDEX('/', new) > 0 THEN SUBSTRING(new, 0, CHARINDEX('/', new)) 
						WHEN CHARINDEX('#', new) > 0 THEN SUBSTRING(new, 0, CHARINDEX('#', new)) 
						ELSE new END)) isbn, CASE WHEN (referrer NOT LIKE '%/resource/detail/%' AND referrer NOT LIKE '%/resource/title/%') OR [url] LIKE '%/resource/title/%' THEN 1 ELSE 0 END isStart, *
					FROM (
						SELECT REPLACE(REPLACE([url], '/resource/detail/', ''), '/resource/title/', '') new, *
						FROM [PageView]
						WHERE pageViewTimestamp BETWEEN @StartDate and @EndDate
					) t
				) s
				--WHERE isbn = '1451192134'
				--ORDER BY sessionId, pageViewTimestamp
			) t 
			INNER JOIN @Resources r
				ON r.isbn = t.isbn
			WHERE [url] LIKE '%/resource/detail/%'
				--AND r.isbn = '0763761834'
			--ORDER BY sessionId, pageViewTimestamp
		) t
		GROUP BY sessionId, isbn
		--ORDER BY sessionResourceTimeInSeconds DESC
	) t
		ON r.isbn = t.isbn
	GROUP BY r.isbn, r.rowRank, r.title, r.retrievals
) s
	ON s.isbn = t.isbn */
ORDER BY t.[Rank by Usage]
--ORDER BY t.[Average Usage Time (separated by kickouts)] DESC



/*
SELECT *
FROM PageView
WHERE sessionId = 'm1xjvjiywjs2fas3jvxg30hk'
ORDER BY pageViewTimestamp
--1451192134
*/