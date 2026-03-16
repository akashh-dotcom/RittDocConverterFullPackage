USE [DEV_RIT001]
GO

INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('dev'
           , 'R2Utilities'
           , 'UpdateTitleTaskWorkingFolder'
           , 'D:\Clients\Rittenhouse\R2Library\Utilities\Working'
           , 'string value - Location of the XML files that are being worked on. Anything still existing after the process errored out.'
		   )
GO


