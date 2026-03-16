

alter table tInstitutionRecommendation
add vchDeletedNote varchar(1024) null;


alter table tUser
add dtLastPasswordChange datetime null;


alter table tUser
add iLoginAttempts int null;



alter table tInstitutionResourceLicense
add decAveragePrice decimal(12,2) null;
go 

update tInstitutionResourceLicense
set decAveragePrice = sub.averagePrice
from tInstitutionResourceLicense irl
join (
select CAST(sum(ci.decDiscountPrice*ci.iNumberOfLicenses)/sum(ci.iNumberOfLicenses) AS DECIMAL(12,2)) averagePrice, irl.iInstitutionResourceLicenseId
from tInstitutionResourceLicense irl
join tInstitution i on irl.iInstitutionId = i.iInstitutionId
join tCart c on irl.iInstitutionId = c.iInstitutionId
join tCartItem ci on c.iCartId = ci.iCartId and irl.iResourceId = ci.iResourceId
where
c.tiRecordStatus = 1  and ci.tiRecordStatus = 1 
and ci.iNumberOfLicenses > 0 and c.tiProcessed = 1
and i.tiHouseAcct = 0
group by irl.iResourceId, irl.iInstitutionId, i.iInstitutionId, irl.iInstitutionResourceLicenseId )  sub on irl.iInstitutionResourceLicenseId = sub.iInstitutionResourceLicenseId

