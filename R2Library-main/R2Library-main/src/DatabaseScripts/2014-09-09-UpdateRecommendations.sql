--The following line will extract the UserId from -- user id: 5196, [Reese]
--substring(ci.vchCreatorId, (CHARINDEX(': ', ci.vchCreatorId) + 2), (CHARINDEX(',', ci.vchCreatorId)) - (CHARINDEX(': ', ci.vchCreatorId) + 2)  ),

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

