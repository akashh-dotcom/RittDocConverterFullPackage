alter table tAutomatedCart
add tiRequested tinyint not null default((0));


alter table tAutomatedCartResource
add iRequested int not null default((0));
GO


alter table tUserResourceRequest
add vchComment varchar(1000) null;
GO


/****** Object:  View [dbo].[vAutomatedCartEvent]    Script Date: 2/9/2018 10:11:11 AM ******/
DROP VIEW [dbo].[vAutomatedCartEvent]
GO

/****** Object:  View [dbo].[vAutomatedCartEvent]    Script Date: 2/9/2018 10:11:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE VIEW [dbo].[vAutomatedCartEvent]
AS


select newid() iAutomatedCartEventId, t.iInstitutionId, t.iTerritoryId, t.iResourceId, t.eventDate, sum(t.NewEdition) 'NewEdition', sum(t.TriggeredPDA) 'TriggeredPDA', sum(t.Turnaway) 'Turnaway' 
, sum(t.Reviewed) 'Reviewed', sum(t.Requested) 'Requested'
from (
--New Editions
select irl.iInstitutionId, i.iTerritoryId, rNew.iResourceId, ISNULL(rnew.dtRISReleaseDate, rNew.dtCreationDate) as eventDate
, 1 as 'NewEdition', 0 as 'TriggeredPDA', 0 as 'Turnaway', 0 as 'Reviewed', 0 as 'Requested'
from tInstitutionResourceLicense irl
join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.tiHouseAcct = 0 and i.iInstitutionAcctStatusId = 1
join tResource r on irl.iResourceId = r.iResourceId and r.iNewEditResourceId is not null and r.tiRecordStatus = 1 
join tResource rNew on r.iNewEditResourceId = rNew.iResourceId  and rNew.iResourceStatusId in (6,8) and rNew.tiRecordStatus = 1 and rNew.dtNotSaleableDate is null
left join tInstitutionResourceLicense irNew on rNew.iResourceId = irNew.iResourceId and irNew.iInstitutionId = irl.iInstitutionId and irNew.tiRecordStatus = 1
where irNew.iInstitutionResourceLicenseId is null and irl.tiRecordStatus = 1 
and (irl.iLicenseCount > 0 and irl.tiLicenseTypeId = 1 )
group by irl.iInstitutionId, i.iTerritoryId, rNew.iResourceId, ISNULL(rnew.dtRISReleaseDate, rNew.dtCreationDate)
union
----Turnaways
select i.iInstitutionId, i.iTerritoryId, r.iResourceId, t.contentTurnawayDate as eventDate
, 0 as 'NewEdition', 0 as 'TriggeredPDA', 1 as 'Turnaway', 0 as 'Reviewed', 0 as 'Requested'
from vDailyContentTurnawayCount t 
join tInstitution i on t.institutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1
join tResource r on r.iResourceId = t.resourceId and r.iResourceStatusId in (6,8) and r.tiRecordStatus = 1 and r.dtNotSaleableDate is null
left join tInstitutionResourceAudit ira on i.iInstitutionId = ira.iInstitutionId and r.iResourceId = ira.iResourceId and t.contentTurnawayDate < ira.dtCreationDate and (ira.iInstitutionResourceAuditTypeId = 4  or (ira.iInstitutionResourceAuditTypeId = 6 and ira.iLicenseCount > 0))
where 
ira.iInstitutionResourceAuditId is null
and t.turnawayTypeId in (20,21)
group by i.iInstitutionId, i.iTerritoryId, r.iResourceId,  t.contentTurnawayDate
union
----Triggered PDA not purchased
select i.iInstitutionId, i.iTerritoryId, r.iResourceId, irl.dtPdaCartDeletedDate as eventDate
, 0 as 'NewEdition', 1 as 'TriggeredPDA', 0 as 'Turnaway', 0 as 'Reviewed', 0 as 'Requested'
from tInstitutionResourceLicense irl
join tResource r on r.iResourceId = irl.iResourceId and r.iResourceStatusId in (6,8) and r.tiRecordStatus = 1 and r.dtNotSaleableDate is null
join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1
where 
irl.dtPdaCartDeletedDate is not null 
and irl.dtFirstPurchaseDate is null
and irl.tiRecordStatus = 1
group by i.iInstitutionId, i.iTerritoryId, r.iResourceId, irl.dtPdaCartDeletedDate
union
--ExpertReviewer
select i.iInstitutionId, i.iTerritoryId, r.iResourceId, ir.dtCreationDate as eventDate
, 0 as 'NewEdition', 0 as 'TriggeredPDA', 0 as 'Turnaway', 1 as 'Reviewed', 0 as 'Requested'
from tInstitutionRecommendation ir
join tResource r on ir.iResourceId = r.iResourceId and r.iResourceStatusId in (6,8) and r.tiRecordStatus = 1 and r.dtNotSaleableDate is null
join tInstitution i on ir.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1
left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and i.iInstitutionId = irl.iInstitutionId
where i.iInstitutionAcctStatusId = 1 and i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and (irl.dtFirstPurchaseDate is null or irl.iInstitutionResourceLicenseId is null)
and ir.tiRecordStatus = 1
group by i.iInstitutionId, i.iTerritoryId, r.iResourceId, ir.dtCreationDate
union
--Requested
select i.iInstitutionId, i.iTerritoryId, r.iResourceId, urr.dtCreationDate as eventDate
, 0 as 'NewEdition', 0 as 'TriggeredPDA', 0 as 'Turnaway', 0 as 'Reviewed', 1 as 'Requested'
from tUserResourceRequest urr
join tResource r on urr.iResourceId = r.iResourceId and r.iResourceStatusId in (6,8) and r.tiRecordStatus = 1 and r.dtNotSaleableDate is null
join tInstitution i on urr.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1
left join tInstitutionResourceLicense irl on urr.iInstitutionId = irl.iInstitutionId and urr.iResourceId = irl.iResourceId and irl.tiRecordStatus = 1
where
urr.tiRecordStatus = 1
and (irl.dtFirstPurchaseDate is null or irl.iInstitutionResourceLicenseId is null)
group by i.iInstitutionId, i.iTerritoryId, r.iResourceId, urr.dtCreationDate
) t
group by t.iInstitutionId, t.iTerritoryId, t.iResourceId, t.eventDate

GO


