


INSERT INTO [dbo].[tUserOption]
           ([iUserOptionId]
           ,[vchUserOptionCode]
           ,[vchUserOptionDescription]
           ,[iUserOptionTypeId]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
     VALUES
           ( 17
           , 'AutomatedShoppingCart'
           , 'Automated Shopping Carts<br />Subject: <i>R2 Library Automated Shopping Cart</i>'
           , 1
           , 'Initial Load'
           , getdate()
           , 1)
GO




USE [DEV_RIT001]
GO

INSERT INTO [dbo].[tUserOptionRole]
           ([iRoleId]
           ,[iUserOptionId]
           ,[vchDefaultValue]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
     VALUES
           (1
           , 17
           ,'1'
           , 'Initial Load'
           , getdate()
           , 1)
GO



INSERT INTO [dbo].[tUserOptionValue]
           ([iUserId]
           ,[iUserOptionId]
           ,[vchUserOptionValue]
           ,[vchCreatorId]
           ,[dtCreationDate]
           ,[tiRecordStatus])
Select iUserId, 17, '1', 'Initial Load', getdate(), 1
from tUser
where tiRecordStatus = 1 and iRoleId = 1 and (dtExpirationDate is null or dtExpirationDate > GETDATE())

GO

