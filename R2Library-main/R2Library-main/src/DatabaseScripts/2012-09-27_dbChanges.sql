SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
ALTER TABLE [dbo].[tInstitution] ADD [tiAutoAddFreeLicenses] tinyint NULL
GO
ALTER TABLE [dbo].[tInstitution] ADD [dtPrecisionSearchExpireDate] smalldatetime NULL
GO
ALTER TABLE [dbo].[tInstitution] ADD CONSTRAINT [D_dbo_tInstitution_1] DEFAULT 0 FOR [tiAutoAddFreeLicenses]
GO

update tInstitution set tiAutoAddFreeLicenses = 0 
update tInstitution set tiAutoAddFreeLicenses = 1 where iInstitutionId in (1, 50)



-- FROM FeaturedPublisherUpdate.sql
ALTER TABLE tPublisher
ADD 
tiFeaturedPublisher  tinyInt not null default ((0)),
vchFeaturedImageName  varchar(100) null,
vchFeaturedDisplayName   varchar(100) null,
vchFeaturedDescription   varchar(2000) null
;

 update tPublisher
 Set tiFeaturedPublisher = 1, vchFeaturedImageName = 'Springer.jpg', vchFeaturedDisplayName = 'Springer Science+Business Media', vchFeaturedDescription = 'Springer Science+Business Media is a global publishing company publishing books, e-books and peer-reviewed journals in science, technical and medical (STM) publishing. Springer also hosts a number of scientific databases, including SpringerLink, SpringerProtocols, and SpringerImages. Book publications include major reference works, textbooks, monographs and book series; more than 37,000 titles are available as e-books in 13 subject collections. Springer has more than 60 publishing houses, more than 5,000 employees and around 2,000 journals and publishes 6,000 new books each year. Springer has major offices in Berlin, Heidelberg, Dordrecht, and New York City.'
 where iPublisherId = 25
 
-- FROM Cart-ForthcomingTitlesInvoicingMethodColumn.sql
alter table tcart add tiForthcomingTitlesInvoicingMethod tinyint null

alter table tCartItem add dtPurchaseDate datetime null


update tcartitem 
set tcartitem.dtPurchaseDate = c.dtpurchasedate
from tcart c 
where c.icartid = tcartitem.icartid
and c.dtPurchaseDate is not null
 
 
 
 -- FIX REPORT VIEWS
ALTER VIEW [dbo].[vDailyContentTurnawayCount]
AS
select dctc.dailyContentTurnawayCountId, dctc.institutionId, dctc.userId, dctc.resourceId, dctc.chapterSectionId,
       dctc.turnawayTypeId, dctc.ipAddressOctetA, dctc.ipAddressOctetB, dctc.ipAddressOctetC, dctc.ipAddressOctetD,
       dctc.ipAddressInteger, dctc.contentTurnawayDate, dctc.contentTurnawayCount
from   R2Reports.dbo.DailyContentTurnawayCount dctc
union
select 0, cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId
     , cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD
     , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date) as hitDate, count(*)
from   R2Reports.dbo.ContentView cv
where  turnawayTypeId <> 0
  and  contentViewTimestamp > '8/18/2012 00:00:00'
group by cv.institutionId, cv.userId, cv.resourceId, cv.chapterSectionId
     , cv.turnawayTypeId, cv.ipAddressOctetA, cv.ipAddressOctetB, cv.ipAddressOctetC, cv.ipAddressOctetD
     , cv.ipAddressInteger, cast(cv.contentViewTimestamp as date)

GO

 
-- adding forthcoming resources to view 
 
ALTER VIEW [dbo].[vInstitutionResourceLicense]
AS
select i.iInstitutionId, r.iResourceId--, r.iResourceStatusId
     , sum(ril.iNumberLicenses) as iLicenseCount
     , min(ril.dtCreationDate) as [dtFirstPurchaseDate]
from   tInstitution i
 join  dbo.tInstitutionResource ir on ir.iInstitutionId = i.iInstitutionId and ir.tiRecordStatus = 1
 join  dbo.tResourceInstLicense ril on ril.iInstitutionResourceId = ir.iInstitutionResourceId and ril.tiRecordStatus = 1
 join  dbo.tResource r on r.iResourceId = ir.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8)
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 1
group by i.iInstitutionId, r.iResourceId, r.iResourceStatusId
union
select i.iInstitutionId, r.iResourceId --,r.iResourceStatusId
     , 3 as iLicenseCount, '1/1/2000' as [dtFirstPurchaseDate]
from   tInstitution i
 join  tResource r on r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7,8) and r.NotSaleable = 0
where  i.tiRecordStatus = 1
  and  i.iInstitutionAcctStatusId = 2
  and  getdate() between i.dtTrialAcctStart and i.dtTrialAcctEnd
GO

ALTER VIEW [dbo].[vDailyPageViewCount]
AS
select dpvc.dailyPageViewCountId, dpvc.institutionId, dpvc.userId, dpvc.ipAddressOctetA, dpvc.ipAddressOctetB,
       dpvc.ipAddressOctetC, dpvc.ipAddressOctetD, dpvc.ipAddressInteger, dpvc.pageViewDate, dpvc.pageViewCount
from   R2Reports.dbo.DailyPageViewCount dpvc
union
select 0, pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB
     , pv.ipAddressOctetC, pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date) as pageViewDate, count(*)
from   R2Reports.dbo.PageView pv
where  pageViewTimestamp > '8/18/2012 00:00:00'
group by pv.institutionId, pv.userId, pv.ipAddressOctetA, pv.ipAddressOctetB, pv.ipAddressOctetC, pv.ipAddressOctetD, pv.ipAddressInteger, cast(pv.pageViewTimestamp as date)

GO


ALTER VIEW [dbo].[vDailySearchCount]
AS
select dsc.dailySearchCountId, dsc.institutionId, dsc.userId, dsc.searchTypeId, dsc.isArchive, dsc.isExternal, dsc.ipAddressOctetA,
       dsc.ipAddressOctetB, dsc.ipAddressOctetC, dsc.ipAddressOctetD, dsc.ipAddressInteger, dsc.searchDate, dsc.searchCount
from   R2Reports.dbo.DailySearchCount dsc
union
select 0, s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA
     , s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date) as searchDate, count(*)
from   R2Reports.dbo.Search s
where  s.searchTimestamp >  '8/18/2012 00:00:00'
group by s.institutionId, s.userId, s.searchTypeId, s.isArchive, s.isExternal, s.ipAddressOctetA
       , s.ipAddressOctetB, s.ipAddressOctetC, s.ipAddressOctetD, s.ipAddressInteger, cast(s.searchTimestamp as date)
GO
