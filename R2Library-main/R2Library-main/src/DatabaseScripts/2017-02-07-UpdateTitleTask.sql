

INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ( 'dev'
           , 'R2Utilities'
           , 'MaxResourceId'
           , '8292'
           , 'string value - Any Resource Id less then this, will be processed by UpdateTitleTask.')
