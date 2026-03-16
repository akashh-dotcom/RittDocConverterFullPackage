--select * 
--from tCartItem ci 
--join tCart c on ci.iCartId = c.iCartId and c.tiRecordStatus = 1 and c.tiProcessed = 0 and c.dtPurchaseDate is null
--join tInstitution i on c.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 and i.iInstitutionAcctStatusId = 2
--where ci.iProductId is not null and ci.tiRecordStatus = 1 and ci.dtPurchaseDate is null
--and i.iInstitutionId = 3662

--update tCartItem
--set tiRecordStatus = 0
--where iCartItemId = 138512

update tProduct
set tiRecordStatus = 0, vchUpdaterId = 'KenHaberle', dtLastUpdate = getdate()
where iProductId = 1

update tCartItem
set tiRecordStatus = 0
where iCartItemId in (
	select ci.iCartItemId
	from tCartItem ci 
	where ci.iProductId = 1 and ci.iProductId is not null and ci.tiRecordStatus = 1 and ci.dtPurchaseDate is null
)