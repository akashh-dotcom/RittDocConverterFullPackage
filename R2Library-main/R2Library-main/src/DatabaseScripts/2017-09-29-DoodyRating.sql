alter table tResource
add siDoodyRating smallint null;

Insert into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, 
iSequenceNumber, tiIsSpecialCollection, iSpecialCollectionSequence)
select 22, 'Doody Review Five Stars', 'SquishList #980', GETDATE(), 1, 0, 31, 0, 0;

Insert into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, 
iSequenceNumber, tiIsSpecialCollection, iSpecialCollectionSequence)
select 23, 'Doody Review Four Stars and Above', 'SquishList #980', GETDATE(), 1, 0, 32, 0, 0;

Insert into tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, 
iSequenceNumber, tiIsSpecialCollection, iSpecialCollectionSequence)
select 24, 'All Doody Reviewed', 'SquishList #980', GETDATE(), 1, 0, 33, 0, 0;

