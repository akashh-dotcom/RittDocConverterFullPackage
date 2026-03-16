alter table tInstitutionResourceLicense
add dtPdaDeletedDate smalldatetime null;

alter table tInstitutionResourceLicense
add vchPdaDeletedById varchar(50) null;


UPDATE tInstitutionResourceLicense
SET 
	dtPdaDeletedDate = isnull(dtLastUpdate, dtCreationDate),
	vchPdaDeletedById = isnull(vchUpdaterId, vchCreatorId),
	tiRecordStatus = 1
--SELECT       isnull(dtLastUpdate, dtCreationDate), isnull(vchUpdaterId, vchCreatorId)
FROM            tInstitutionResourceLicense
WHERE        (tiLicenseTypeId = 3) AND (iLicenseCount = 0) AND (dtFirstPurchaseDate IS NULL) AND (tiRecordStatus = 0)
AND (iPdaViewCount < iPdaMaxViews)

select * from tInstitutionResourceLicense where dtPdaDeletedDate is not null



UPDATE tInstitutionRecommendation
SET
	iAddedToCartByUserId = substring(ci.vchCreatorId, (CHARINDEX(': ', ci.vchCreatorId) + 2), (CHARINDEX(',', ci.vchCreatorId)) - (CHARINDEX(': ', ci.vchCreatorId) + 2)  ),
	dtAddedToCartDate = ci.dtCreationDate
--select ci.dtCreationDate, ci.vchCreatorId --substring(ci.vchCreatorId, (CHARINDEX(': ', ci.vchCreatorId) + 2), (CHARINDEX(',', ci.vchCreatorId)) - (CHARINDEX(': ', ci.vchCreatorId) + 2)  )
from tCart c
join tCartItem ci on c.iCartId = ci.iCartId
join tInstitutionRecommendation ir on c.iInstitutionId = ir.iInstitutionId and ci.iResourceId = ir.iResourceId
where ir.dtCreationDate < ci.dtCreationDate


UPDATE tInstitutionRecommendation
SET
	iPurchasedByUserId = substring(c.vchUpdaterId, (CHARINDEX(': ', c.vchUpdaterId) + 2), (CHARINDEX(',', c.vchUpdaterId)) - (CHARINDEX(': ', c.vchUpdaterId) + 2)  ),
	dtPurchaseDate = c.dtLastUpdate
--select c.dtLastUpdate, substring(c.vchUpdaterId, (CHARINDEX(': ', c.vchUpdaterId) + 2), (CHARINDEX(',', c.vchUpdaterId)) - (CHARINDEX(': ', c.vchUpdaterId) + 2)  )
from tCart c
join tCartItem ci on c.iCartId = ci.iCartId
join tInstitutionRecommendation ir on c.iInstitutionId = ir.iInstitutionId and ci.iResourceId = ir.iResourceId
where c.dtLastUpdate > ir.dtAddedToCartDate and c.tiProcessed = 1 and ci.tiRecordStatus = 1 and ci.tiRecordStatus = 1