INSERT INTO [dbo].[tCollection]
           ([iCollectionId]
           ,[vchCollectionName]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus]
           ,[tiHideInFilter]
           ,[iSequenceNumber])
     VALUES
           (11
           ,'2015 AJN Books of the Year'
           ,'KenHaberle'
           ,getdate()
           , 1
           ,1
           ,8)
GO

Update tCollection
set iSequenceNumber = 9
where iCollectionId = 8
go
Update tCollection
set iSequenceNumber = 10
where iCollectionId = 9
go
Update tCollection
set iSequenceNumber = 11
where iCollectionId = 10
go

