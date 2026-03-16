alter table PageView
add authenticationType varchar(50) null;

alter table tResource
add [siDoodyRating] [smallint] NULL

INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber, tiIsSpecialCollection, iSpecialCollectionSequence)
VALUES (22, 'Doody Review Five Stars', 'SquishList #980', getdate(), 1, 0, 31, 0, 0)

INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber, tiIsSpecialCollection, iSpecialCollectionSequence)
VALUES (23, 'Doody Review Four Stars and Above', 'SquishList #980', getdate(), 1, 0, 32, 0, 0)

INSERT INTO tCollection (iCollectionId, vchCollectionName, vchCreatorId, dtCreationDate, tiRecordStatus, tiHideInFilter, iSequenceNumber, tiIsSpecialCollection, iSpecialCollectionSequence)
VALUES (24, 'All Doody Reviewed', 'SquishList #980', getdate(), 1, 0, 33, 0, 0)

