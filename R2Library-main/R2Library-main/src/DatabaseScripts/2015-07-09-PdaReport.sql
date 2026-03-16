
Declare @StartDate datetime = '05/01/2014';
Declare @EndDate datetime = '05/01/2015';

select iInstitutionId, vchInstitutionAcctNum, vchInstitutionName
, sum(PdaPurchasedResources) as PdaPurchasedResources, sum(PdaPurchasedLicenses) as PdaPurchasedLicenses
, sum(FirmPurchasedResources) as FirmPurchasedResources, sum(FirmPurchasedLicenses) as FirmPurchasedLicenses
, sum(PdaTitlesAdded) as PdaTitlesAdded
, sum(PdaTitlesDeleted) as PdaTitlesDeleted
, sum(PdaTitlesAccessed) as PdaTitlesViews
from (
select i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName
, count(ci.iNumberOfLicenses) as PdaPurchasedResources, sum(ci.iNumberOfLicenses) as PdaPurchasedLicenses
, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
, 0 as PdaTitlesAdded
, 0 as PdaTitlesDeleted
, 0 as PdaTitlesAccessed
from tInstitution i
join tCart c on i.iInstitutionId = c.iInstitutionId and c.tiProcessed = 1 and c.tiRecordStatus = 1
join tCartItem ci on c.iCartId = ci.iCartId and ci.tiLicenseOriginalSourceId = 2 and ci.tiRecordStatus = 1 and ci.iNumberOfLicenses > 0
where i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and c.dtPurchaseDate between @StartDate and @EndDate
group by i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName

union

select i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName
, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses
, count(ci.iNumberOfLicenses) as FirmPurchasedResources, sum(ci.iNumberOfLicenses) as FirmPurchasedLicenses
, 0 as PdaTitlesAdded
, 0 as PdaTitlesDeleted
, 0 as PdaTitlesAccessed
from tInstitution i
join tCart c on i.iInstitutionId = c.iInstitutionId and c.tiProcessed = 1 and c.tiRecordStatus = 1
join tCartItem ci on c.iCartId = ci.iCartId and ci.tiLicenseOriginalSourceId = 1 and ci.tiRecordStatus = 1 and ci.iNumberOfLicenses > 0
where i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and c.dtPurchaseDate between @StartDate and @EndDate
group by i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName

union

select i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName
, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses
, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
, count(irl.iResourceId) as PdaTitlesAdded
, 0 as PdaTitlesDeleted
, 0 as PdaTitlesAccessed
from tInstitution i
join tInstitutionResourceLicense irl on i.iInstitutionId = irl.iInstitutionId and irl.tiRecordStatus = 1
where i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and  irl.dtPdaAddedToCartDate between @StartDate and @EndDate
group by i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName

union

select i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName
, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses
, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
, 0 as PdaTitlesAdded
, count(irl.iResourceId) as PdaTitlesDeleted
, 0 as PdaTitlesAccessed
from tInstitution i
join tInstitutionResourceLicense irl on i.iInstitutionId = irl.iInstitutionId and irl.tiRecordStatus = 1
where i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and  irl.dtPdaDeletedDate between @StartDate and @EndDate
group by i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName

union

select i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName
, 0 as PdaPurchasedResources, 0 as PdaPurchasedLicenses
, 0 as FirmPurchasedResources, 0 as FirmPurchasedLicenses
, 0 as PdaTitlesAdded
, 0 as PdaTitlesDeleted
, count(ira.iResourceId) as PdaTitlesAccessed
from tInstitution i
join tInstitutionResourceAudit ira on i.iInstitutionId = ira.iInstitutionId and ira.iInstitutionResourceAuditTypeId = 9
where i.tiRecordStatus = 1 and i.tiHouseAcct = 0
and  ira.dtCreationDate between @StartDate and @EndDate
group by i.iInstitutionId, i.vchInstitutionAcctNum, i.vchInstitutionName

) as counts
group by iInstitutionId, vchInstitutionAcctNum, vchInstitutionName
