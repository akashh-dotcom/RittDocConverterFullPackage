USE [DEV_RIT001]
GO

/****** Object:  View [dbo].[vContentView]    Script Date: 11/6/2017 10:06:45 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vAutomatedCartEvent]
AS

select newid() iAutomatedCartEventId, t.iInstitutionId, t.iTerritoryId, t.iResourceId, t.eventDate, sum(t.NewEdition) 'NewEdition', sum(t.TriggeredPDA) 'TriggeredPDA', sum(t.Turnaway) 'Turnaway' 
, sum(t.Reviewed) 'Reviewed'
from (
--New Editions
select ir.iInstitutionId, i.iTerritoryId, rNew.iResourceId, ISNULL(rnew.dtRISReleaseDate, rNew.dtCreationDate) as eventDate
, 1 as 'NewEdition', 0 as 'TriggeredPDA', 0 as 'Turnaway', 0 as 'Reviewed'
from tInstitutionResourceLicense ir
join tInstitution i on ir.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.tiHouseAcct = 0 and i.iInstitutionAcctStatusId = 1
join tResource r on ir.iResourceId = r.iResourceId and r.iNewEditResourceId is not null and r.tiRecordStatus = 1 
join tResource rNew on r.iNewEditResourceId = rNew.iResourceId  and rNew.iResourceStatusId in (6,8) and rNew.tiRecordStatus = 1 and rNew.dtNotSaleableDate is null
left join tInstitutionResource irNew on rNew.iResourceId = irNew.iResourceId and irNew.iInstitutionId = ir.iInstitutionId and irNew.tiRecordStatus = 1
where irNew.iInstitutionResourceId is null and ir.tiRecordStatus = 1
group by ir.iInstitutionId, i.iTerritoryId, rNew.iResourceId, ISNULL(rnew.dtRISReleaseDate, rNew.dtCreationDate)
union
----Turnaways
select i.iInstitutionId, i.iTerritoryId, r.iResourceId, t.contentTurnawayDate as eventDate
, 0 as 'NewEdition', 0 as 'TriggeredPDA', 1 as 'Turnaway', 0 as 'Reviewed'
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
, 0 as 'NewEdition', 1 as 'TriggeredPDA', 0 as 'Turnaway', 0 as 'Reviewed'
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
, 0 as 'NewEdition', 0 as 'TriggeredPDA', 0 as 'Turnaway', 1 as 'Reviewed'
from tInstitutionRecommendation ir
join tResource r on ir.iResourceId = r.iResourceId and r.iResourceStatusId in (6,8) and r.tiRecordStatus = 1 and r.dtNotSaleableDate is null
join tInstitution i on ir.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 1
left join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId and i.iInstitutionId = irl.iInstitutionId
where i.iInstitutionAcctStatusId = 1 and i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and (irl.dtFirstPurchaseDate is null or irl.iInstitutionResourceLicenseId is null)
and ir.tiRecordStatus = 1
group by i.iInstitutionId, i.iTerritoryId, r.iResourceId, ir.dtCreationDate
) t
group by t.iInstitutionId, t.iTerritoryId, t.iResourceId, t.eventDate


GO

