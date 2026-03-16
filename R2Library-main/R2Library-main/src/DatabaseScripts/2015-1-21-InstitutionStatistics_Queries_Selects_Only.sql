
Declare @InstitutionId int;
Declare @Month int;
Declare @Year int;

Declare @StartDate datetime;
Declare @EndDate datetime;

set @InstitutionId = 50;
set @StartDate = '4/1/2013';
set @EndDate = '5/1/2013';

select MostAccessedResourceId, MostAccessedCount, LeastAccessedResourceId, LeastAccessedCount
, MostTurnawayConcurrentResourceId, MostTurnawayConcurrentCount, MostTurnawayAccessResourceId, MostTurnawayAccessCount,
MostPopularSpecialtyName, MostPopularSpecialtyCount, LeastPopularSpecialtyName, LeastPopularSpecialtyCount, TotalResourceCount
from
(select top 1 * from (
select top 1 resourceId  as 'MostAccessedResourceId', sum(contentviewcount) as 'MostAccessedCount'
from [dbo].[vDailyContentViewCount] 
where institutionId = @InstitutionId and contentViewDate between @StartDate and @EndDate
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) a, 

(select top 1 * from (
select top 1 irl.iResourceId  as 'LeastAccessedResourceId', sum(isnull(dcvc.contentviewcount, 0)) as 'LeastAccessedCount'
from tInstitutionResourceLicense irl
left join vDailyContentViewCount dcvc on irl.iInstitutionId = dcvc.institutionId and irl.iResourceId = dcvc.resourceId
where irl.iInstitutionId = @InstitutionId and 
 (contentViewDate between @StartDate and @EndDate
--or irl.dtFirstPurchaseDate < @EndDate 
)  and isnull(dcvc.contentviewcount, 0) > 0
group by irl.iResourceId
order by 2 asc
union select null, null ) x order by 2 desc) b,

(select top 1 * from (
select top 1 resourceId as 'MostTurnawayConcurrentResourceId', sum(contentTurnawayCount) as 'MostTurnawayConcurrentCount'
 from vDailyContentTurnawayCount
where institutionId = @InstitutionId and contentTurnawayDate between @StartDate and @EndDate
and turnawayTypeId = 20
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) c,

(select top 1 * from (
select top 1 resourceId as 'MostTurnawayAccessResourceId', sum(contentTurnawayCount) as 'MostTurnawayAccessCount'
 from vDailyContentTurnawayCount
where institutionId = @InstitutionId and contentTurnawayDate between @StartDate and @EndDate
and turnawayTypeId = 21
group by resourceId
order by 2 desc
union select null, null) x order by 2 desc) cc,

(select top 1 * from (
select top 1 s.vchSpecialtyName as 'MostPopularSpecialtyName', sum(dcvc.contentViewCount) as 'MostPopularSpecialtyCount'
from tResourceSpecialty rs
join tSpecialty s on rs.iSpecialtyId = s.iSpecialtyId
join tResource r on rs.iResourceId = r.iResourceId
join vDailyContentViewCount dcvc on r.iResourceId = dcvc.resourceId
where rs.tiRecordStatus = 1 and s.tiRecordStatus = 1 and r.tiRecordStatus = 1
and dcvc.institutionId = @InstitutionId and dcvc.contentViewDate between @StartDate and @EndDate
group by rs.iResourceSpecialtyId, s.vchSpecialtyName, dcvc.institutionId
order by 2 desc
union select null, null) x order by 2 desc) d,

(select top 1 * from (
select top 1  s.vchSpecialtyName as 'LeastPopularSpecialtyName', sum(dcvc.contentViewCount) as 'LeastPopularSpecialtyCount'
from tResourceSpecialty rs
join tSpecialty s on rs.iSpecialtyId = s.iSpecialtyId
join tResource r on rs.iResourceId = r.iResourceId
join vDailyContentViewCount dcvc on r.iResourceId = dcvc.resourceId
where rs.tiRecordStatus = 1 and s.tiRecordStatus = 1 and r.tiRecordStatus = 1 and isnull(dcvc.contentViewCount, 0) > 0
and dcvc.institutionId = @InstitutionId and dcvc.contentViewDate between @StartDate and @EndDate
group by rs.iResourceSpecialtyId, s.vchSpecialtyName, dcvc.institutionId
union select null, null) x order by 2 desc) e,

(select top 1 * from (
select count(*) as 'TotalResourceCount'
from tInstitutionResourceLicense irl
where irl.dtFirstPurchaseDate < @EndDate
and irl.iInstitutionId = @InstitutionId and tiRecordStatus = 1
union select null) x order by 1 desc) f

----Resource Count
--select count(*) as 'ResourceCount'
--from tInstitutionResourceLicense irl
--where irl.dtFirstPurchaseDate < cast(cast(@month+1 as varchar(2)) + '/01/' + cast(@year as varchar(4)) as datetime)
--and irl.iInstitutionId = @InstitutionId and tiRecordStatus = 1

------------------------------Account Usage----------------------------
--select 
----(select top 1 resourceId  as 'MostAccessedResourceId', sum(contentviewcount) as 'MostAccessedCount'
----from [dbo].[vDailyContentViewCount] 
----where institutionId = @InstitutionId and month(contentViewDate) = @Month and year(contentviewdate)=@Year
----group by resourceId
----order by 2 desc),
----@@@@@@@@@@@@@@@@Successful Content Retreival@@@@@@@@@@@@@@@@
--(select sum(dcvc.contentViewCount)
--from   vDailyContentViewCount dcvc
--left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
--where  month(dcvc.contentViewDate) = @Month and year(dcvc.contentViewDate)=@Year
--and dcvc.institutionId = @InstitutionId) as [ContentCount],
----@@@@@@@@@@@@@@@@TOC Retrievals@@@@@@@@@@@@@@@@
--(select sum(dcvc.contentViewCount)
--from   vDailyContentViewCount dcvc
--left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId
--where  month(dcvc.contentViewDate) = @Month and year(dcvc.contentViewDate)=@Year
--and dcvc.institutionId = @InstitutionId and dcvc.chapterSectionId is null) as [TOCCount],
----@@@@@@@@@@@@@@@@Sessions@@@@@@@@@@@@@@@@
--(select sum(dpvc.sessionCount) 
--from   vDailySessionCount dpvc
--left outer join tInstitution i on i.iInstitutionId = dpvc.institutionId
--where  month(dpvc.sessionDate) = @Month and year(dpvc.sessionDate)=@Year
--and dpvc.institutionId = @InstitutionId) as [SessionCount],
----@@@@@@@@@@@@@@@@Print Requests@@@@@@@@@@@@@@@@
--(select sum(dcvc.contentViewCount)
--from   vDailyContentViewCount dcvc 
--left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId 
--where  month(dcvc.contentViewDate) = @Month and year(dcvc.contentViewDate)=@Year
--and dcvc.institutionId = @InstitutionId and dcvc.actionTypeId = 16) as [PrintCount],
----@@@@@@@@@@@@@@@@Email Requests@@@@@@@@@@@@@@@@
--(select sum(dcvc.contentViewCount)
--from   vDailyContentViewCount dcvc 
--left outer join tInstitution i on i.iInstitutionId = dcvc.institutionId 
--where  month(dcvc.contentViewDate) = @Month and year(dcvc.contentViewDate)=@Year
--and dcvc.institutionId = @InstitutionId and dcvc.actionTypeId = 16) as [EmailCount],
----@@@@@@@@@@@@@@@@Content Turnaways@@@@@@@@@@@@@@@@
--(select sum(dctc.contentTurnawayCount)
--from   vDailyContentTurnawayCount dctc
--left outer join tInstitution i on i.iInstitutionId = dctc.institutionId
--where  month(dctc.contentTurnawayDate) = @Month and year(dctc.contentTurnawayDate)=@Year
--and dctc.institutionId = @InstitutionId and dctc.TurnawayTypeId = 20) as [TurnawayConcurrencyCount],
----@@@@@@@@@@@@@@@@Access Turnaways@@@@@@@@@@@@@@@@
--(select sum(dctc.contentTurnawayCount)
--from   vDailyContentTurnawayCount dctc
--left outer join tInstitution i on i.iInstitutionId = dctc.institutionId
--where  month(dctc.contentTurnawayDate) = @Month and year(dctc.contentTurnawayDate)=@Year
--and dctc.institutionId = @InstitutionId and dctc.TurnawayTypeId = 21) as [TurnawayAccessCount];








----##############Purchased Resources##############
--select iResourceId as 'PurchasedResourceIds' from tInstitutionResourceLicense irl 
--where tiLicenseTypeId = 1 and tiRecordStatus = 1
--and iInstitutionId = @InstitutionId  and month(dtFirstPurchaseDate) = @Month and year(dtFirstPurchaseDate)=@Year
--group by iResourceId

----##############Archived Purchased Titles##############
--select irl.iResourceId
--from tInstitutionResourceLicense irl 
--join tResource r on irl.iResourceId = r.iResourceId
--join DEV_R2Utilities..ResourceEmails re on r.vchIsbn10 = re.resourceISBN
--where irl.tiLicenseTypeId = 1 and irl.tiRecordStatus = 1
--and irl.iInstitutionId = @InstitutionId and month(re.dateArchivedEmail) = @Month and year(re.dateArchivedEmail)=@Year
----##############New Edition Titles Purchased when previously owned##############
--select irl.iResourceId
--from tInstitutionResourceLicense irl 
--join tResource r on irl.iResourceId = r.iResourceId
--join tResource r2 on r.iPrevEditResourceID = r2.iResourceId
--join tInstitutionResourceLicense irl2 on r2.iResourceId = irl2.iResourceId and irl.iInstitutionId = irl2.iInstitutionId
--where irl.iInstitutionId = @InstitutionId and month(irl.dtFirstPurchaseDate) = @Month and year(irl.dtFirstPurchaseDate)=@Year
----##############PDA Titles Added##############
--select irl.iResourceId
--from tInstitutionResourceLicense irl
--where irl.iInstitutionId = @InstitutionId and month(irl.dtPdaAddedDate) = @Month and year(irl.dtPdaAddedDate) = @Year
--and irl.tiLicenseTypeId = 3
----##############PDA Titles Added to Cart##############
--select irl.iResourceId
--from tInstitutionResourceLicense irl
--where irl.iInstitutionId = @InstitutionId and month(irl.dtPdaAddedToCartDate) = @Month and year(irl.dtPdaAddedToCartDate) = @Year
--and irl.tiLicenseTypeId = 3
----##############New Edition for PDA Titles##############
--select r.iResourceId
--from tResource r
--join DEV_R2Utilities..ResourceEmails re on r.vchIsbn10 = re.resourceISBN
--join tInstitutionResourceLicense irl on r.iPrevEditResourceID = irl.iResourceId and irl.tiLicenseTypeId = 3
--where irl.iInstitutionId = @InstitutionId and month(re.dateNewResourceEmail) = @Month and year(re.dateNewResourceEmail) = @Year








--select irl.iResourceId  as 'LeastAccessedResourceId', sum(isnull(dcvc.contentviewcount, 0)) as 'LeastAccessedCount'
--from tInstitutionResourceLicense irl
--left join vDailyContentViewCount dcvc on irl.iInstitutionId = dcvc.institutionId and irl.iResourceId = dcvc.resourceId
--where irl.iInstitutionId = 1 and 
-- (contentViewDate between '01/01/2009' and '02/01/2009'
--or irl.dtFirstPurchaseDate < '02/01/2009' )  and isnull(dcvc.contentviewcount, 0) > 0
--group by irl.iResourceId
--order by 2 asc